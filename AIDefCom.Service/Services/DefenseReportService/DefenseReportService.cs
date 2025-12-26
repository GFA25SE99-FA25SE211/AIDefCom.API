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
                        new { role = "system", content = "Bạn là AI chuyên phân tích biên bản bảo vệ đồ án tốt nghiệp. Hãy phân tích TOÀN BỘ transcript và tạo báo cáo chi tiết." },
                        new { role = "user", content = prompt },
                        new { role = "user", content = $"TOÀN BỘ TRANSCRIPT:\n\n{trimmedTranscript}" }
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
                var prompt = new System.Text.StringBuilder();
            
                prompt.AppendLine("Phân tích transcript buổi bảo vệ đồ án và tạo báo cáo theo định dạng JSON.");
                prompt.AppendLine();
                prompt.AppendLine("**THÔNG TIN:**");
                prompt.AppendLine($"- Giảng viên: {lecturerNames}");
                prompt.AppendLine($"- Sinh viên: {studentNames}");
                prompt.AppendLine();
                prompt.AppendLine("**NGUYÊN TẮC BẮT BUỘC:**");
                prompt.AppendLine("- TUYỆT ĐỐI KHÔNG BỊA thông tin không có trong transcript");
                prompt.AppendLine("- CHỈ trích xuất thông tin THỰC SỰ CÓ trong transcript");
                prompt.AppendLine("- Nếu KHÔNG CÓ thì ghi 'Không có trong transcript'");
                prompt.AppendLine();
                prompt.AppendLine("**YÊU CẦU:**");
                prompt.AppendLine();
                prompt.AppendLine("1. **Thời gian**: Lấy từ transcript, không có thì ghi 'Không có trong transcript'");
                prompt.AppendLine();
                prompt.AppendLine("2. **Tóm tắt phần trình bày của sinh viên** (studentPresentations):");
                prompt.AppendLine("   - CHỈ ghi sinh viên THỰC SỰ trình bày trong transcript");
                prompt.AppendLine("   - Mỗi sinh viên: liệt kê các ý chính thành danh sách bullet points");
                prompt.AppendLine("   - Mỗi ý chính là 1 câu ngắn gọn");
                prompt.AppendLine();
                prompt.AppendLine("3. **Câu hỏi và trả lời** (questionsAndAnswers):");
                prompt.AppendLine("   - lecturerName: Tên giảng viên đặt câu hỏi");
                prompt.AppendLine("   - question: Nội dung câu hỏi (tóm tắt ngắn gọn)");
                prompt.AppendLine("   - respondentName: Tên sinh viên trả lời (nếu không xác định ghi 'Nhóm')");
                prompt.AppendLine("   - answerPoints: Danh sách các ý chính trong câu trả lời (bullet points)");
                prompt.AppendLine("   - councilDiscussion: Nhận xét và thảo luận của Hội đồng - PHẢI GHI RÕ:");
                prompt.AppendLine("     + Tên giảng viên + phản ứng (đồng ý/không đồng ý/bổ sung)");
                prompt.AppendLine("     + Nếu có tranh luận giữa các giảng viên: ghi rõ ai nói gì");
                prompt.AppendLine("     + Kết luận cuối cùng mà hội đồng thống nhất");
                prompt.AppendLine("     + Nếu không có thảo luận: ghi 'Không có thảo luận'");
                prompt.AppendLine();
                prompt.AppendLine("4. **Tóm tắt tổng quan** (overallSummary): 2-3 câu đánh giá chung");
                prompt.AppendLine();
                prompt.AppendLine("**FORMAT JSON:**");
                prompt.AppendLine("{");
                prompt.AppendLine("  \"actualStartTime\": \"HH:mm\",");
                prompt.AppendLine("  \"actualEndTime\": \"HH:mm\",");
                prompt.AppendLine("  \"studentPresentations\": [");
                prompt.AppendLine("    {");
                prompt.AppendLine("      \"studentName\": \"Tên sinh viên\",");
                prompt.AppendLine("      \"presentationPoints\": [\"Ý chính 1\", \"Ý chính 2\"]");
                prompt.AppendLine("    }");
                prompt.AppendLine("  ],");
                prompt.AppendLine("  \"questionsAndAnswers\": [");
                prompt.AppendLine("    {");
                prompt.AppendLine("      \"lecturerName\": \"Tên giảng viên\",");
                prompt.AppendLine("      \"question\": \"Câu hỏi\",");
                prompt.AppendLine("      \"respondentName\": \"Tên sinh viên\",");
                prompt.AppendLine("      \"answerPoints\": [\"Ý trả lời 1\", \"Ý trả lời 2\"],");
                prompt.AppendLine("      \"councilDiscussion\": \"[Tên GV]: [ý kiến]. Kết luận: [kết quả]\"");
                prompt.AppendLine("    }");
                prompt.AppendLine("  ],");
                prompt.AppendLine("  \"overallSummary\": \"Tóm tắt\"");
                prompt.AppendLine("}");
                prompt.AppendLine();
                prompt.AppendLine("**VÍ DỤ councilDiscussion:**");
                prompt.AppendLine("- Có tranh luận: 'TS. Mai: KHÔNG ĐỒNG Ý, chỉ ra lỗi về message persistence. ThS. Nam: ĐỒNG Ý với cô Mai. Kết luận: Cần bổ sung durable queue.'");
                prompt.AppendLine("- Có phản hồi tích cực: 'ThS. Hùng: Nhận xét câu trả lời ổn/đúng/hợp lý.'");
                prompt.AppendLine("- Hỏi thêm rồi đồng ý: 'ThS. Nam: Hỏi thêm về mock. Sau khi sinh viên giải thích, thầy Nam nhận xét câu trả lời rõ ràng.'");
                prompt.AppendLine("- Giảng viên im lặng (không nhận xét gì): 'Không có thảo luận'");
                prompt.AppendLine();
                prompt.AppendLine("**LƯU Ý QUAN TRỌNG về councilDiscussion:**");
                prompt.AppendLine("- CHỈ ghi ý kiến khi giảng viên THỰC SỰ NÓI/NHẬN XÉT trong transcript");
                prompt.AppendLine("- Nếu giảng viên im lặng hoặc không có phản hồi rõ ràng → ghi 'Không có thảo luận'");
                prompt.AppendLine("- ĐỒNG Ý chỉ ghi khi giảng viên XÁC NHẬN rõ ràng (ổn, đúng, hợp lý, good, correct, etc.)");
                prompt.AppendLine("- TUYỆT ĐỐI KHÔNG suy đoán ý kiến của giảng viên nếu không có trong transcript");
            
                return prompt.ToString();
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
                    _logger.LogWarning("AI returned empty content.");
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
                    _logger.LogWarning("No valid JSON found. Raw: {Content}", content);
                    return new DefenseProgressDto();
                }

                var json = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                _logger.LogInformation("Extracted JSON for parsing: {Json}", json);

                var result = JsonSerializer.Deserialize<DefenseProgressDto>(json, options);
                return result ?? new DefenseProgressDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse AI JSON response. Raw: {Response}", responseJson);
                return new DefenseProgressDto();
            }
        }
    }
}
