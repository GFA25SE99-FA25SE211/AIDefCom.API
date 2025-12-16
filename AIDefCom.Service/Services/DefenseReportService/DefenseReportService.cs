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
                _logger.LogInformation("?? Starting defense report generation for defense session ID: {DefenseSessionId}", request.DefenseSessionId);

                // ?? BUOC 1: Lay defense session
                var session = await _uow.DefenseSessions.GetByIdAsync(request.DefenseSessionId);
                if (session == null)
                    throw new KeyNotFoundException($"Defense session with ID {request.DefenseSessionId} not found");

                _logger.LogInformation("? Retrieved defense session: {SessionId}", session.Id);

                // ?? BUOC 2: Lay TAT CA transcript tu session
                var transcripts = await _uow.Transcripts.GetBySessionIdAsync(session.Id);

                if (transcripts == null || !transcripts.Any())
                    throw new KeyNotFoundException($"No transcript found for defense session {request.DefenseSessionId}");

                // ?? SỬA CHÍNH: Chỉ lấy những transcript có Status = Completed
                var completedTranscripts = transcripts
                    .Where(t => string.Equals(t.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!completedTranscripts.Any())
                    throw new InvalidOperationException($"No transcript with status 'Completed' found for defense session {request.DefenseSessionId}");

                _logger.LogInformation("? Retrieved {Count} completed transcript(s) for defense session {SessionId}",
                    completedTranscripts.Count, session.Id);

                // Loc chi lay nhung transcript co noi dung (an toàn thêm)
                var validTranscripts = completedTranscripts
                    .Where(t => !string.IsNullOrWhiteSpace(t.TranscriptText))
                    .ToList();

                if (!validTranscripts.Any())
                    throw new InvalidOperationException($"No valid completed transcript content found for defense session {request.DefenseSessionId}");

                _logger.LogInformation("? Retrieved {Count} valid completed transcript(s) with content for defense session {SessionId}",
                    validTranscripts.Count, session.Id);

                // Gop tat ca transcript text thanh 1 chuoi de phan tich
                var combinedTranscriptText = string.Join("\n\n--- TRANSCRIPT TIẾP THEO ---\n\n",
                    validTranscripts.Select(t => t.TranscriptText));

                _logger.LogInformation("? Combined transcript total length: {Length} characters",
                    combinedTranscriptText.Length);

                // ?? BUOC 3: Lay council
                var council = await _uow.Councils.GetByIdAsync(session.CouncilId);
                if (council == null)
                    throw new KeyNotFoundException($"Council with ID {session.CouncilId} not found");

                // ?? BUOC 4: Lay major
                var major = await _uow.Majors.GetByIdAsync(council.MajorId);
                if (major == null)
                    throw new KeyNotFoundException($"Major with ID {council.MajorId} not found");

                // ?? BUOC 5: Lay group
                var group = await _uow.Groups.GetByIdAsync(session.GroupId);
                if (group == null)
                    throw new KeyNotFoundException($"Group with ID {session.GroupId} not found");

                // ?? BUOC 6: Lay semester
                var semester = await _uow.Semesters.GetByIdAsync(group.SemesterId);
                if (semester == null)
                    throw new KeyNotFoundException($"Semester with ID {group.SemesterId} not found");

                // ?? BUOC 7: Lay committee members
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

                _logger.LogInformation("? Retrieved {Count} council members", councilMembers.Count);

                // ?? BUOC 8: Lay students trong group
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

                _logger.LogInformation("? Retrieved {Count} students", students.Count);

                // ?? BUOC 9: Goi AI de phan tich TAT CA transcript
                var aiAnalysis = await AnalyzeTranscriptWithAIAsync(combinedTranscriptText, councilMembers, students);
                _logger.LogInformation("? AI analysis completed");

                // ?? BUOC 10: Tao defense report response
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

                _logger.LogInformation("?? Defense report generated successfully for defense session ID: {DefenseSessionId}", request.DefenseSessionId);
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error generating defense report for defense session ID: {DefenseSessionId}", request.DefenseSessionId);
                throw;
            }
        }

        /// <summary>
        /// Goi AI de phan tich TAT CA transcript va tao phan "Dien bien qua trinh bao ve"
        /// </summary>
        private async Task<DefenseProgressDto> AnalyzeTranscriptWithAIAsync(
            string combinedTranscriptText,
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
                var prompt = BuildDefenseReportPrompt(combinedTranscriptText, lecturerNames, studentNames);

                // Chuan bi HttpClient
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://openrouter.ai/");
                _httpClient.DefaultRequestHeaders.Add("X-Title", "AIDefCom Defense Report Generator");

                var model = _config["AI:Model"] ?? "gpt-4o-mini";

                // Gioi han do dai transcript de tranh vuot token limit
                var trimmedTranscript = combinedTranscriptText.Length > 20000
                    ? combinedTranscriptText.Substring(0, 20000)
                    : combinedTranscriptText;

                _logger.LogInformation("?? Analyzing combined transcript: Original={OriginalLength}, Trimmed={TrimmedLength} characters",
                    combinedTranscriptText.Length, trimmedTranscript.Length);

                var payload = new
                {
                    model,
                    messages = new[]
                    {
                        new { role = "system", content = "B?n là AI chuyên phân tích biên b?n b?o v? d? án t?t nghi?p. Hãy phân tích TOÀN B? transcript (có th? g?m nhi?u doãn) và t?o báo cáo chi ti?t t?ng h?p." },
                        new { role = "user", content = prompt },
                        new { role = "user", content = $"TOÀN B? TRANSCRIPT (có th? g?m nhi?u ph?n):\n\n{trimmedTranscript}" }
                    },
                    max_tokens = 16384,
                    temperature = 0.3,
                    top_p = 0.9
                };

                var apiUrl = _config["AI:OpenRouterUrl"] ?? "https://openrouter.ai/api/v1/chat/completions";
                _logger.LogInformation("?? Calling AI model: {Model} | URL: {ApiUrl}", model, apiUrl);

                var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("? AI API error: {StatusCode} - {Error}", response.StatusCode, error);
                    throw new HttpRequestException($"AI API returned error: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("?? Raw AI Response: {Response}", responseContent);

                var result = ParseAIDefenseResponse(responseContent);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error analyzing combined transcript with AI");

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
        /// Tao prompt cho AI de phan tich TAT CA transcript
        /// </summary>
        private string BuildDefenseReportPrompt(string combinedTranscript, string lecturerNames, string studentNames)
        {
            return $@"
Hãy phân tích TOÀN BỘ transcript buổi bảo vệ dự án dưới đây (có thể gồm NHIỀU ĐOẠN transcript được gộp lại) và tạo báo cáo chi tiết TỔNG HỢP theo định dạng JSON.

**THÔNG TIN NGỮ CẢNH:**
- Giảng viên trong hội đồng: {lecturerNames}
- Sinh viên trong nhóm: {studentNames}

**NGUYÊN TẮC PHÂN TÍCH QUAN TRỌNG:**
✅ Đây là TOÀN BỘ transcript của buổi bảo vệ, có thể gồm NHIỀU PHẦN được gộp lại
✅ Hãy đọc và phân tích TOÀN BỘ nội dung, KHÔNG BỎ SÓT bất kỳ phần nào
✅ CHỈ phân tích và ghi nhận thông tin của sinh viên NÀO THỰC SỰ THAM GIA trình bày hoặc trả lời câu hỏi trong TOÀN BỘ transcript
✅ KHÔNG được tự động tạo thông tin cho sinh viên không xuất hiện trong transcript
✅ Nếu sinh viên KHÔNG có phản hồi hoặc KHÔNG được đề cập trong TOÀN BỘ transcript, GHI RÕ: ""Không nhận được phản hồi từ sinh viên này trong transcript""
✅ Nếu KHÔNG XÁC ĐỊNH được sinh viên nào trả lời, ghi ""Nhóm"" hoặc ""Không xác định được""
✅ Tổng hợp thông tin từ TẤT CẢ các phần transcript để có cái nhìn toàn diện

**YÊU CẦU PHÂN TÍCH CHI TIẾT:**

1. **Thời gian thực tế**:
   - Xác định thời gian bắt đầu và kết thúc chính xác từ TOÀN BỘ transcript
   - Nếu có nhiều phần transcript, lấy thời gian bắt đầu sớm nhất và kết thúc muộn nhất
   - Nếu không có thông tin rõ ràng, ghi ""N/A""

2. **Phần trình bày của sinh viên** (CHỈ phân tích sinh viên THỰC SỰ TRÌNH BÀY trong TOÀN BỘ transcript):
   - XÁC ĐỊNH RÕ sinh viên nào trình bày phần nào DỰA VÀO TOÀN BỘ TRANSCRIPT
   - TỔNG HỢP tất cả nội dung họ trình bày từ TẤT CẢ các phần transcript (công nghệ, tính năng, kiến trúc, demo...)
   - Đánh giá độ mạch lạc, rõ ràng của phần trình bày
   - Ghi chú các điểm nổi bật hoặc thiếu sót trong cách trình bày
   - ❌ KHÔNG tạo thông tin trình bày cho sinh viên không xuất hiện trong TOÀN BỘ transcript
   - ❌ Nếu sinh viên nào KHÔNG trình bày trong TOÀN BỘ transcript, KHÔNG thêm vào danh sách ""studentPresentations""

3. **Câu hỏi và câu trả lời** (phân tích CHI TIẾT DỰA VÀO TOÀN BỘ TRANSCRIPT):
   
   Với MỖI CÂU HỎI từ giảng viên trong TOÀN BỘ transcript, hãy ghi chú:
   
   a) **Nội dung câu hỏi**: 
      - Tóm tắt câu hỏi ngắn gọn nhưng đầy đủ
      - Ghi rõ giảng viên nào hỏi (nếu xác định được từ transcript)
   
   b) **Sinh viên nào trả lời**:
      - Nếu XÁC ĐỊNH ĐƯỢC TÊN từ transcript, ghi tên cụ thể
      - Nếu KHÔNG XÁC ĐỊNH được, ghi ""Nhóm"" hoặc ""Không xác định được""
      - ❌ KHÔNG GÁN tên sinh viên nếu không chắc chắn
   
   c) **Nội dung câu trả lời chi tiết**:
      - Tóm tắt ĐẦY ĐỦ nội dung sinh viên ĐÃ TRẢ LỜI trong transcript
      - Ghi rõ các luận điểm, dẫn chứng mà sinh viên đưa ra
      - Nếu KHÔNG CÓ câu trả lời trong TOÀN BỘ transcript, ghi ""Không có phản hồi trong transcript""
   
   d) **Phản ứng và thảo luận của hội đồng về câu trả lời**:
      - Ghi chú cụ thể các giảng viên NHẬN XÉT, ĐÁNH GIÁ về câu trả lời (nếu có trong transcript)
      - Ghi rõ nội dung thảo luận giữa các giảng viên về câu trả lời đó
      - Các ý kiến đồng tình, phản biện, hoặc bổ sung từ hội đồng
      - Nếu không có phản ứng từ hội đồng, ghi ""Không có thảo luận""
   
   e) **Phản hồi lại từ giảng viên** (nếu có):
      - Giảng viên có giải thích thêm, bổ sung, hoặc sửa chữa cho sinh viên không?
      - Giảng viên có đưa ra gợi ý, hướng dẫn thêm không?
      - Giảng viên có đặt câu hỏi tiếp theo liên quan không?
      - Nếu không có, ghi ""Không có phản hồi thêm""
   
   f) **Chất lượng câu trả lời** (đánh giá SAU KHI phân tích toàn bộ tương tác):
      - ""Trả lời xuất sắc"": Câu trả lời đầy đủ, chính xác, có dẫn chứng, được hội đồng đánh giá cao
      - ""Trả lời tốt"": Câu trả lời đúng, rõ ràng, hội đồng chấp nhận
      - ""Trả lời khá"": Câu trả lời đúng nhưng chưa đầy đủ, cần bổ sung thêm
      - ""Trả lời yếu"": Câu trả lời mơ hồ, không rõ ràng, hội đồng chưa đồng ý
      - ""Trả lời sai"": Câu trả lời sai hoặc không đúng trọng tâm
      - ""Không trả lời được"": Sinh viên không biết hoặc không trả lời
      - ""Trả lời sau khi được gợi ý"": Cần giảng viên hỗ trợ mới trả lời được
      - ""Không nhận được phản hồi"": Không có câu trả lời trong TOÀN BỘ transcript
   
   g) **Thái độ khi trả lời** (CHỈ ghi nếu CÓ THÔNG TIN từ transcript):
      - Tự tin, lưu loát
      - Do dự, ngập ngừng
      - Cần suy nghĩ lâu
      - Không chắc chắn
      - ""N/A"" nếu không xác định được
   
   h) **Kết quả cuối cùng của câu hỏi**:
      - Sinh viên có trả lời thỏa đáng được hội đồng chấp nhận không?
      - Câu hỏi có được giải quyết hoàn toàn hay vẫn còn tồn đọng?
      - Có cần theo dõi hoặc cải thiện thêm không?

4. **Tóm tắt tổng quan** (3-5 câu - TỔNG HỢP từ TOÀN BỘ TRANSCRIPT):
   - Đánh giá chung về buổi bảo vệ DỰA VÀO TOÀN BỘ TRANSCRIPT
   - Điểm mạnh của nhóm (CHỈ ghi những gì THỰC SỰ XUẤT HIỆN trong TOÀN BỘ transcript)
   - Điểm yếu hoặc vấn đề gặp phải (CHỈ ghi những gì THỰC SỰ XUẤT HIỆN trong TOÀN BỘ transcript)
   - Kết quả dự kiến (nếu có thông tin trong transcript)

5. **Đánh giá phong thái và kỹ năng trình bày** (CHỈ DỰA VÀO TOÀN BỘ TRANSCRIPT):
   - Cách thức trình bày (slide, demo, giải thích...)
   - Sự tự tin, rõ ràng khi trình bày
   - Khả năng giao tiếp và trả lời câu hỏi
   - Sự phối hợp giữa các thành viên (nếu có thông tin)
   - Kỹ năng xử lý tình huống khó
   - ❌ KHÔNG đánh giá sinh viên không xuất hiện trong TOÀN BỘ transcript

6. **Các chủ đề trọng tâm được thảo luận** (CHỈ DỰA VÀO TOÀN BỘ TRANSCRIPT):
   - TỔNG HỢP tất cả các vấn đề chính mà hội đồng quan tâm từ TẤT CẢ các phần transcript
   - Các câu hỏi về công nghệ, kiến trúc, thiết kế
   - Các câu hỏi về tính thực tiễn, khả năng ứng dụng
   - Các góp ý, đề xuất cải tiến từ hội đồng

**FORMAT JSON TRẢ VỀ:**
{{
  ""actualStartTime"": ""HH:mm hoặc N/A"",
  ""actualEndTime"": ""HH:mm hoặc N/A"",
  ""studentPresentations"": [
    {{
      ""studentName"": ""Tên sinh viên THỰC SỰ TRÌNH BÀY (từ TOÀN BỘ transcript) hoặc vai trò nếu không xác định được tên"",
      ""presentationContent"": [
        ""TỔNG HỢP nội dung THỰC SỰ ĐƯỢC TRÌNH BÀY từ TẤT CẢ các phần transcript"",
        ""Ví dụ: Giới thiệu tổng quan về dự án"",
        ""Ví dụ: Trình bày công nghệ sử dụng: React, Node.js...""
      ],
      ""presentationQuality"": ""Đánh giá DỰA VÀO TOÀN BỘ TRANSCRIPT: Rõ ràng/Khá tốt/Cần cải thiện"",
      ""notes"": ""Ghi chú đặc biệt DỰA VÀO TOÀN BỘ TRANSCRIPT (nếu có)""
    }}
  ],
  ""questionsAndAnswers"": [
    {{
      ""lecturer"": ""Tên/vai trò giảng viên (từ transcript hoặc 'Giảng viên' nếu không xác định)"",
      ""question"": ""Nội dung câu hỏi cụ thể từ TOÀN BỘ transcript"",
      ""respondent"": ""Tên sinh viên THỰC SỰ TRẢ LỜI (từ transcript) hoặc 'Nhóm'/'Không xác định được'"",
      ""answerContent"": ""Tóm tắt ĐẦY ĐỦ nội dung câu trả lời THỰC SỰ XUẤT HIỆN trong transcript, bao gồm các luận điểm và dẫn chứng. Nếu không có: 'Không có phản hồi trong transcript'"",
      ""councilDiscussion"": ""Ghi chú CHI TIẾT về phản ứng và thảo luận của hội đồng về câu trả lời này: các giảng viên nhận xét gì, đánh giá như thế nào, có ý kiến đồng tình/phản biện gì. Nếu không có: 'Không có thảo luận'"",
      ""lecturerFollowUp"": ""Phản hồi lại từ giảng viên (nếu có): giải thích thêm, bổ sung, sửa chữa, gợi ý, hoặc câu hỏi tiếp theo. Nếu không có: 'Không có phản hồi thêm'"",
      ""answerQuality"": ""Trả lời xuất sắc/Trả lời tốt/Trả lời khá/Trả lời yếu/Trả lời sai/Không trả lời được/Trả lời sau khi được gợi ý/Không nhận được phản hồi"",
      ""answerAttitude"": ""Tự tin, lưu loát/Do dự, ngập ngừng/Cần suy nghĩ lâu/Không chắc chắn/N/A"",
      ""finalOutcome"": ""Kết quả cuối cùng: Câu trả lời được chấp nhận/Cần bổ sung thêm/Chưa thỏa đáng/Vấn đề vẫn còn tồn đọng"",
      ""additionalNotes"": ""Ghi chú thêm DỰA VÀO TRANSCRIPT (nếu có)""
    }}
  ],
  ""overallSummary"": ""Tóm tắt 3-5 câu TỔNG HỢP DỰA VÀO TOÀN BỘ TRANSCRIPT: Buổi bảo vệ diễn ra..., nhóm đã..., điểm mạnh là..., điểm yếu là..."",
  ""studentPerformance"": ""Đánh giá chi tiết TỔNG HỢP CHỈ DỰA VÀO TOÀN BỘ TRANSCRIPT: Sinh viên trình bày [tốt/khá/yếu], phong thái [tự tin/lo lắng], khả năng trả lời câu hỏi [tốt/khá/cần cải thiện], kỹ năng giao tiếp [rõ ràng/chưa rõ ràng]..."",
  ""discussionFocus"": ""Các chủ đề chính TỔNG HỢP DỰA VÀO TOÀN BỘ TRANSCRIPT: 1) Công nghệ và kiến trúc hệ thống, 2) Tính năng và khả năng mở rộng, 3) Testing và security, 4) Tính thực tiễn và khả năng triển khai...""
}}

**LƯU Ý QUAN TRỌNG - ĐỌC KỸ:**
✅ ĐỌC và PHÂN TÍCH TOÀN BỘ transcript (có thể gồm NHIỀU PHẦN)
✅ TỔNG HỢP thông tin từ TẤT CẢ các phần transcript
✅ CHỈ phân tích những gì THỰC SỰ XUẤT HIỆN trong TOÀN BỘ transcript
✅ Với MỖI CÂU HỎI, phải ghi đầy đủ:
   - Nội dung câu hỏi
   - Câu trả lời của sinh viên (đầy đủ, chi tiết)
   - Phản ứng và thảo luận của hội đồng về câu trả lời đó
   - Phản hồi lại từ giảng viên (nếu có)
   - Đánh giá chất lượng câu trả lời
   - Kết quả cuối cùng
✅ CHỈ ghi nhận thông tin của sinh viên NÀO THỰC SỰ THAM GIA trình bày hoặc trả lời trong TOÀN BỘ transcript
✅ Nếu sinh viên KHÔNG xuất hiện trong TOÀN BỘ transcript, KHÔNG thêm vào danh sách
✅ Nếu KHÔNG XÁC ĐỊNH được sinh viên nào trả lời, ghi ""Nhóm"" hoặc ""Không xác định được""
✅ Phân tích KỸ LƯỠNG tương tác giữa sinh viên và hội đồng
✅ Ghi chú RÕ RÀNG các thảo luận, tranh luận, góp ý của hội đồng
✅ Trả về JSON hợp lệ, KHÔNG thêm markdown formatting (không có ```json)
✅ Nếu không xác định được thông tin từ TOÀN BỘ transcript, ghi ""N/A"" hoặc ""Không xác định được từ transcript""

❌ KHÔNG bỏ sót bất kỳ phần transcript nào
❌ KHÔNG tự động tạo thông tin cho sinh viên không xuất hiện trong TOÀN BỘ transcript
❌ KHÔNG đoán mò hoặc suy đoán thông tin không có trong TOÀN BỘ transcript
❌ KHÔNG gán tên sinh viên nếu không chắc chắn
❌ KHÔNG tạo câu trả lời giả định cho sinh viên
❌ KHÔNG bỏ qua phần thảo luận và phản hồi của hội đồng
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
                    _logger.LogWarning("?? AI returned empty content.");
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
                    _logger.LogWarning("?? No valid JSON found. Raw: {Content}", content);
                    return new DefenseProgressDto();
                }

                var json = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                _logger.LogInformation("? Extracted JSON for parsing: {Json}", json);

                var result = JsonSerializer.Deserialize<DefenseProgressDto>(json, options);
                return result ?? new DefenseProgressDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? Failed to parse AI JSON response. Raw: {Response}", responseJson);
                return new DefenseProgressDto();
            }
        }
    }
}
