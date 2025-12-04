using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.DefenseReport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.DefenseReportService
{
    public class DefenseReportService : IDefenseReportService
    {
        private readonly IUnitOfWork _uow;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<DefenseReportService> _logger;

        public DefenseReportService(
            IUnitOfWork uow,
            HttpClient httpClient,
            IConfiguration config,
            ILogger<DefenseReportService> logger)
        {
            _uow = uow;
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<DefenseReportResponseDto> GenerateDefenseReportAsync(DefenseReportRequestDto request)
        {
            try
            {
                _logger.LogInformation("🔵 Starting defense report generation for defense session ID: {DefenseSessionId}", request.DefenseSessionId);

                // 📝 BUOC 1: Lay defense session
                var session = await _uow.DefenseSessions.GetByIdAsync(request.DefenseSessionId);
                if (session == null)
                    throw new KeyNotFoundException($"Defense session with ID {request.DefenseSessionId} not found");

                _logger.LogInformation("✅ Retrieved defense session: {SessionId}", session.Id);

                // 📝 BUOC 2: Lay transcript tu session
                var transcripts = await _uow.Transcripts.GetBySessionIdAsync(session.Id);
                var transcript = transcripts.FirstOrDefault();
                
                if (transcript == null)
                    throw new KeyNotFoundException($"No transcript found for defense session {request.DefenseSessionId}");

                if (string.IsNullOrWhiteSpace(transcript.TranscriptText))
                    throw new InvalidOperationException($"Transcript {transcript.Id} has no text content");

                _logger.LogInformation("✅ Retrieved transcript ID {TranscriptId}: {Length} characters", transcript.Id, transcript.TranscriptText.Length);

                // 📝 BUOC 3: Lay council
                var council = await _uow.Councils.GetByIdAsync(session.CouncilId);
                if (council == null)
                    throw new KeyNotFoundException($"Council with ID {session.CouncilId} not found");

                // 📝 BUOC 4: Lay major
                var major = await _uow.Majors.GetByIdAsync(council.MajorId);
                if (major == null)
                    throw new KeyNotFoundException($"Major with ID {council.MajorId} not found");

                // 📝 BUOC 5: Lay group
                var group = await _uow.Groups.GetByIdAsync(session.GroupId);
                if (group == null)
                    throw new KeyNotFoundException($"Group with ID {session.GroupId} not found");

                // 📝 BUOC 6: Lay semester
                var semester = await _uow.Semesters.GetByIdAsync(group.SemesterId);
                if (semester == null)
                    throw new KeyNotFoundException($"Semester with ID {group.SemesterId} not found");

                // 📝 BUOC 7: Lay committee members
                var committeeAssignments = await _uow.CommitteeAssignments.Query()
                    .Include(ca => ca.Lecturer)
                    .Include(ca => ca.CouncilRole)
                    .Where(ca => ca.CouncilId == council.Id && !ca.IsDeleted)
                    .ToListAsync();

                var councilMembers = committeeAssignments.Select(ca => new CouncilMemberDto
                {
                    LecturerId = ca.LecturerId,
                    FullName = ca.Lecturer?.FullName ?? "N/A",
                    Role = ca.CouncilRole?.RoleName ?? "Member",
                    Email = ca.Lecturer?.Email,
                    Department = ca.Lecturer?.Department,
                    AcademicRank = ca.Lecturer?.AcademicRank,
                    Degree = ca.Lecturer?.Degree
                }).ToList();

                _logger.LogInformation("✅ Retrieved {Count} council members", councilMembers.Count);

                // 📝 BUOC 8: Lay students trong group
                var studentGroups = await _uow.StudentGroups.Query()
                    .Include(sg => sg.Student)
                    .Where(sg => sg.GroupId == group.Id && !sg.IsDeleted)
                    .ToListAsync();

                var students = studentGroups.Select(sg => new StudentInfoDto
                {
                    StudentId = sg.UserId,
                    FullName = sg.Student?.FullName ?? "N/A",
                    Email = sg.Student?.Email,
                    GroupRole = sg.GroupRole
                }).ToList();

                _logger.LogInformation("✅ Retrieved {Count} students", students.Count);

                // 🤖 BUOC 9: Goi AI de phan tich transcript
                var aiAnalysis = await AnalyzeTranscriptWithAIAsync(transcript.TranscriptText, councilMembers, students);

                _logger.LogInformation("✅ AI analysis completed");

                // 📋 BUOC 10: Tao defense report response
                var report = new DefenseReportResponseDto
                {
                    CouncilInfo = new CouncilInfoDto
                    {
                        CouncilId = council.Id,
                        MajorName = major.MajorName,
                        Description = council.Description,
                        Members = councilMembers
                    },
                    SessionInfo = new SessionInfoDto
                    {
                        DefenseDate = session.DefenseDate,
                        StartTime = session.StartTime,
                        EndTime = session.EndTime,
                        Location = session.Location,
                        Status = session.Status
                    },
                    ProjectInfo = new ProjectInfoDto
                    {
                        ProjectCode = group.ProjectCode,
                        TopicTitleEN = group.TopicTitle_EN,
                        TopicTitleVN = group.TopicTitle_VN,
                        SemesterName = semester.SemesterName,
                        Year = semester.Year,
                        Students = students
                    },
                    DefenseProgress = aiAnalysis
                };

                _logger.LogInformation("🎉 Defense report generated successfully for defense session ID: {DefenseSessionId}", request.DefenseSessionId);
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error generating defense report for defense session ID: {DefenseSessionId}", request.DefenseSessionId);
                throw;
            }
        }

        /// <summary>
        /// Goi AI de phan tich transcript va tao phan "Dien bien qua trinh bao ve"
        /// </summary>
        private async Task<DefenseProgressDto> AnalyzeTranscriptWithAIAsync(
            string transcriptText, 
            List<CouncilMemberDto> councilMembers,
            List<StudentInfoDto> students)
        {
            try
            {
                var token = _config["AI:OpenRouterToken"];
                if (string.IsNullOrEmpty(token))
                    throw new InvalidOperationException("AI API token is missing. Please configure 'AI:OpenRouterToken' in appsettings.json.");

                // Chuan bi thong tin context
                var lecturerNames = string.Join(", ", councilMembers.Select(m => $"{m.FullName} ({m.Role})"));
                var studentNames = string.Join(", ", students.Select(s => $"{s.FullName} ({s.GroupRole ?? "Member"})"));

                // Tao prompt cho AI
                var prompt = BuildDefenseReportPrompt(transcriptText, lecturerNames, studentNames);

                // Chuan bi HttpClient
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://openrouter.ai/");
                _httpClient.DefaultRequestHeaders.Add("X-Title", "AIDefCom Defense Report Generator");

                var model = _config["AI:Model"] ?? "gpt-4o-mini";

                // Gioi han do dai transcript de tranh vuot token limit
                var trimmedTranscript = transcriptText.Length > 8000
                    ? transcriptText.Substring(0, 8000)
                    : transcriptText;

                var payload = new
                {
                    model,
                    messages = new[]
                    {
                        new { role = "system", content = "Bạn là AI chuyên phân tích biên bản bảo vệ đồ án tốt nghiệp. Hãy phân tích transcript và tạo báo cáo chi tiết." },
                        new { role = "user", content = prompt },
                        new { role = "user", content = $"Transcript:\n{trimmedTranscript}" }
                    },
                    max_tokens = 16384,
                    temperature = 0.3,
                    top_p = 0.9
                };

                var apiUrl = _config["AI:OpenRouterUrl"] ?? "https://openrouter.ai/api/v1/chat/completions";
                _logger.LogInformation("🤖 Calling AI model: {Model} | URL: {ApiUrl}", model, apiUrl);

                var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ AI API error: {StatusCode} - {Error}", response.StatusCode, error);
                    throw new HttpRequestException($"AI API returned error: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("🔍 Raw AI Response: {Response}", responseContent);

                var result = ParseAIDefenseResponse(responseContent);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error analyzing transcript with AI");
                
                // Tra ve default response neu AI fail
                return new DefenseProgressDto
                {
                    OverallSummary = "AI analysis unavailable. Please review the transcript manually.",
                    StudentPerformance = "N/A",
                    DiscussionFocus = "N/A",
                    StudentPresentations = new List<StudentPresentationDto>(),
                    QuestionsAndAnswers = new List<QuestionAnswerDto>()
                };
            }
        }

        /// <summary>
        /// Tao prompt cho AI de phan tich transcript
        /// </summary>
        private string BuildDefenseReportPrompt(string transcript, string lecturerNames, string studentNames)
        {
            return $@"
Hãy phân tích transcript buổi bảo vệ đồ án dưới đây và tạo báo cáo chi tiết theo định dạng JSON.

**THÔNG TIN NGỮ CẢNH:**
- Giảng viên trong hội đồng: {lecturerNames}
- Sinh viên thực hiện: {studentNames}

**YÊU CẦU PHÂN TÍCH CHI TIẾT:**

1. **Thời gian thực tế**: 
   - Xác định thời gian bắt đầu và kết thúc chính xác từ transcript
   - Nếu không có thông tin rõ ràng, ghi ""N/A""

2. **Phần trình bày của nhóm/sinh viên** (phân tích kỹ từng sinh viên):
   - Xác định rõ sinh viên nào trình bày phần nào
   - Liệt kê các nội dung chính họ trình bày (công nghệ, tính năng, kiến trúc, demo...)
   - Đánh giá độ mạch lạc, rõ ràng của phần trình bày
   - Ghi chú các điểm nổi bật hoặc thiếu sót trong cách trình bày

3. **Câu hỏi và câu trả lời** (phân tích CHI TIẾT):
   
   Với MỖI CÂU HỎI từ giảng viên, hãy ghi chú:
   a) **Nội dung câu hỏi**: Tóm tắt câu hỏi ngắn gọn nhưng đầy đủ
   b) **Sinh viên nào trả lời**: Nếu xác định được tên, ghi tên; nếu không thì ghi ""Nhóm""
   c) **Chất lượng câu trả lời**:
      - ""Trả lời tốt"": Câu trả lời đầy đủ, rõ ràng, chính xác, có dẫn chứng
      - ""Trả lời khá"": Câu trả lời đúng nhưng chưa đầy đủ hoặc thiếu chi tiết
      - ""Trả lời yếu"": Câu trả lời mơ hồ, không rõ ràng, hoặc không đúng trọng tâm
      - ""Không trả lời được"": Sinh viên không biết hoặc không trả lời
      - ""Trả lời sau khi được gợi ý"": Cần giảng viên hỗ trợ mới trả lời được
   d) **Chi tiết câu trả lời**: Tóm tắt nội dung chính sinh viên đã trả lời
   e) **Thái độ khi trả lời**: 
      - Tự tin, lưu loát
      - Do dự, ngập ngừng
      - Cần suy nghĩ lâu
      - Không chắc chắn
   f) **Nếu trả lời không tốt**: Ghi chú lý do (không hiểu rõ, thiếu kiến thức, không chuẩn bị kỹ...)

4. **Tóm tắt tổng quan** (3-5 câu):
   - Đánh giá chung về buổi bảo vệ
   - Điểm mạnh của nhóm
   - Điểm yếu hoặc vấn đề gặp phải
   - Kết quả dự kiến (nếu có thông tin)

5. **Đánh giá phong thái và kỹ năng trình bày**:
   - Cách thức trình bày (slide, demo, giải thích...)
   - Sự tự tin, rõ ràng khi trình bày
   - Khả năng giao tiếp và trả lời câu hỏi
   - Sự phối hợp giữa các thành viên
   - Kỹ năng xử lý tình huống khó

6. **Các chủ đề trọng tâm được thảo luận**:
   - Liệt kê các vấn đề chính mà hội đồng quan tâm
   - Các câu hỏi về công nghệ, kiến trúc, thiết kế
   - Các câu hỏi về tính thực tiễn, khả năng ứng dụng
   - Các góp ý, đề xuất cải tiến từ hội đồng

**FORMAT JSON TRẢ VỀ:**
{{
  ""actualStartTime"": ""HH:mm hoặc N/A"",
  ""actualEndTime"": ""HH:mm hoặc N/A"",
  ""studentPresentations"": [
    {{
      ""studentName"": ""Tên sinh viên hoặc vai trò (Leader/Member)"",
      ""presentationContent"": [
        ""Giới thiệu tổng quan về dự án"",
        ""Trình bày công nghệ sử dụng: React, Node.js...""
        ""Demo tính năng chính...""
        ""Giải thích kiến trúc hệ thống""
      ],
      ""presentationQuality"": ""Đánh giá: Rõ ràng/Khá tốt/Cần cải thiện"",
      ""notes"": ""Ghi chú đặc biệt về phần trình bày (nếu có)""
    }}
  ],
  ""questionsAndAnswers"": [
    {{
      ""lecturer"": ""Tên/vai trò giảng viên (Chủ tịch HĐ, Thư ký, Ủy viên...)""
      ""question"": ""Nội dung câu hỏi cụ thể"",
      ""respondent"": ""Tên sinh viên trả lời hoặc 'Nhóm'"",
      ""answerQuality"": ""Trả lời tốt/Trả lời khá/Trả lời yếu/Không trả lời được/Trả lời sau khi được gợi ý"",
      ""answerContent"": ""Tóm tắt nội dung câu trả lời của sinh viên"",
      ""answerAttitude"": ""Tự tin, lưu loát/Do dự, ngập ngừng/Cần suy nghĩ lâu/Không chắc chắn"",
      ""additionalNotes"": ""Ghi chú thêm (nếu có): ví dụ 'Cần giảng viên gợi ý mới trả lời được', 'Trả lời không đúng trọng tâm'...""
    }}
  ],
  ""overallSummary"": ""Tóm tắt 3-5 câu: Buổi bảo vệ diễn ra..., nhóm đã..., điểm mạnh là..., điểm yếu là..."",
  ""studentPerformance"": ""Đánh giá chi tiết: Sinh viên trình bày [tốt/khá/yếu], phong thái [tự tin/lo lắng], khả năng trả lời câu hỏi [tốt/khá/cần cải thiện], kỹ năng giao tiếp [rõ ràng/chưa rõ ràng]..."",
  ""discussionFocus"": ""Các chủ đề chính: 1) Công nghệ và kiến trúc hệ thống, 2) Tính năng và khả năng mở rộng, 3) Testing và security, 4) Tính thực tiễn và khả năng triển khai...""
}}

**LƯU Ý QUAN TRỌNG:**
- Phân tích KỸ LƯỠNG từng câu hỏi và câu trả lời, không bỏ sót
- Ghi chú RÕ RÀNG chất lượng câu trả lời của sinh viên (tốt/khá/yếu)
- Chú ý THÁI ĐỘ và PHONG CÁCH trả lời (tự tin, do dự, không chắc chắn...)
- Nếu sinh viên trả lời SAI hoặc KHÔNG ĐẦY ĐỦ, phải ghi chú cụ thể
- Nếu giảng viên phải GỢI Ý hoặc HỖ TRỢ, phải ghi rõ
- Phân biệt rõ câu trả lời TỐT và câu trả lời YẾU
- Trả về JSON hợp lệ, KHÔNG thêm markdown formatting (không có ```json)
- Nếu không xác định được thông tin, ghi ""N/A"" hoặc ""Không xác định được từ transcript""
";
        }

        /// <summary>
        /// Parse AI response JSON thanh DefenseProgressDto
        /// </summary>
        private DefenseProgressDto ParseAIDefenseResponse(string responseJson)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                using var doc = JsonDocument.Parse(responseJson);
                string? content = null;

                if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var first = choices[0];
                    if (first.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var cont))
                        content = cont.GetString();
                    else if (first.TryGetProperty("text", out var text))
                        content = text.GetString();
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("⚠️ AI returned empty content.");
                    return new DefenseProgressDto();
                }

                // Clean up content
                content = content
                    .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("```", "")
                    .Replace("\r", "")
                    .Trim();

                int jsonStart = content.IndexOf('{');
                int jsonEnd = content.LastIndexOf('}');
                if (jsonStart < 0 || jsonEnd < 0)
                {
                    _logger.LogWarning("⚠️ No valid JSON found. Raw: {Content}", content);
                    return new DefenseProgressDto();
                }

                var json = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                _logger.LogInformation("✅ Extracted JSON for parsing: {Json}", json);

                var result = JsonSerializer.Deserialize<DefenseProgressDto>(json, options);
                return result ?? new DefenseProgressDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️ Failed to parse AI JSON response. Raw: {Response}", responseJson);
                return new DefenseProgressDto();
            }
        }
    }
}
