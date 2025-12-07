using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.TranscriptAnalysis;
using AIDefCom.Service.Services.RedisCache;
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

namespace AIDefCom.Service.Services.TranscriptAnalysisService
{
    public class TranscriptAnalysisService : ITranscriptAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<TranscriptAnalysisService> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IRedisCache _redisCache;
        private readonly string _openRouterToken;
        private readonly string _openRouterUrl;
        private readonly string _aiModel;

        public TranscriptAnalysisService(
            HttpClient httpClient,
            IConfiguration config,
            ILogger<TranscriptAnalysisService> logger,
            IUnitOfWork uow,
            IRedisCache redisCache)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
            _uow = uow;
            _redisCache = redisCache;

            // ✅ Validate AI configuration in constructor (best practice)
            _openRouterToken = config["AI:OpenRouterToken"]
                ?? throw new InvalidOperationException("AI:OpenRouterToken is missing. Please configure it in appsettings.json.");
            _openRouterUrl = config["AI:OpenRouterUrl"]
                ?? "https://openrouter.ai/api/v1/chat/completions";
            _aiModel = config["AI:Model"] ?? "gpt-4o-mini";

            _logger.LogInformation("✅ TranscriptAnalysis AI Service initialized with model: {Model}", _aiModel);
        }

        public async Task<TranscriptAnalysisResponseDto> AnalyzeTranscriptAsync(TranscriptAnalysisRequestDto request)
        {
            try
            {
                // 📝 B1: Lấy transcript từ Redis cache với validation toàn diện
                var transcriptKey = $"transcript:defense:{request.DefenseSessionId}";
                _logger.LogInformation("🔍 Fetching transcript from Redis with key: {Key}", transcriptKey);

                var transcript = await _redisCache.GetAsync(transcriptKey);

                // ❌ Kiểm tra transcript có tồn tại không
                if (string.IsNullOrWhiteSpace(transcript))
                {
                    _logger.LogWarning("⚠️ Transcript not found in Redis cache for session {SessionId}", request.DefenseSessionId);
                    return CreateNotFoundResponse(request.DefenseSessionId,
                        "Transcript chưa có trong hệ thống. Vui lòng đảm bảo buổi bảo vệ đã được ghi âm và transcript đã được tạo.");
                }

                _logger.LogInformation("✅ Retrieved transcript from Redis: {Length} characters", transcript.Length);

                // ❌ Kiểm tra độ dài transcript có hợp lệ không (tối thiểu 100 ký tự để có nội dung phân tích)
                if (transcript.Length < 100)
                {
                    _logger.LogWarning("⚠️ Transcript too short ({Length} chars) for analysis. Session: {SessionId}",
                        transcript.Length, request.DefenseSessionId);
                    return CreateInvalidContentResponse(request.DefenseSessionId,
                        $"Transcript quá ngắn ({transcript.Length} ký tự) để phân tích. Có thể buổi bảo vệ chưa diễn ra hoặc ghi âm bị lỗi.");
                }

                // ❌ Kiểm tra transcript có chứa nội dung có ý nghĩa không (không phải chỉ toàn ký tự đặc biệt)
                var meaningfulChars = transcript.Count(c => char.IsLetterOrDigit(c));
                var meaningfulRatio = (double)meaningfulChars / transcript.Length;

                if (meaningfulRatio < 0.3)
                {
                    _logger.LogWarning("⚠️ Transcript contains too few meaningful characters ({Ratio:P}) for session {SessionId}",
                        meaningfulRatio, request.DefenseSessionId);
                    return CreateInvalidContentResponse(request.DefenseSessionId,
                        "Transcript có nội dung không hợp lệ (quá nhiều ký tự đặc biệt hoặc nhiễu). Vui lòng kiểm tra lại file ghi âm.");
                }

                // ❌ Kiểm tra transcript có chứa từ khóa liên quan đến buổi bảo vệ không
                var hasRelevantContent = ContainsDefenseRelatedKeywords(transcript);
                if (!hasRelevantContent)
                {
                    _logger.LogWarning("⚠️ Transcript does not contain defense-related keywords. Possible incorrect content. Session: {SessionId}",
                        request.DefenseSessionId);
                    // Không return lỗi, chỉ warning vì có thể là buổi bảo vệ đặc biệt
                }

                // 📝 B2: Lấy buổi bảo vệ
                var defense = await _uow.DefenseSessions.GetByIdAsync(request.DefenseSessionId);
                if (defense == null)
                {
                    _logger.LogWarning("⚠️ Defense session {SessionId} not found in database", request.DefenseSessionId);
                    throw new KeyNotFoundException($"Defense session {request.DefenseSessionId} not found.");
                }

                // 📝 B3: Lấy hội đồng
                var council = await _uow.Councils.GetByIdAsync(defense.CouncilId);
                if (council == null)
                {
                    _logger.LogWarning("⚠️ Council {CouncilId} not found for session {SessionId}",
                        defense.CouncilId, request.DefenseSessionId);
                    throw new KeyNotFoundException($"Council {defense.CouncilId} not found.");
                }

                // 📝 B4: Lấy rubric thật của ngành
                var rubrics = await _uow.MajorRubrics.GetRubricsByMajorIdAsync(council.MajorId);
                var rubricDetails = rubrics.Select(r => new
                {
                    r.RubricName,
                    r.Description
                }).ToList();

                if (!rubricDetails.Any())
                {
                    _logger.LogWarning("⚠️ No rubrics found for major ID: {MajorId}", council.MajorId);
                    return CreateNoRubricsResponse(request.DefenseSessionId, council.MajorId,
                        "Không tìm thấy tiêu chí đánh giá (rubric) cho ngành này. Vui lòng cấu hình rubric trước khi phân tích.");
                }

                _logger.LogInformation("✅ Retrieved {Count} rubrics for analysis", rubricDetails.Count);

                // 📝 B5: Tạo prompt nâng cao
                var prompt = BuildAdvancedAnalysisPrompt(rubricDetails);

                // 📝 B6: Chuẩn bị HttpClient
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openRouterToken);
                _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://openrouter.ai/");
                _httpClient.DefaultRequestHeaders.Add("X-Title", "AIDefCom Transcript Analyzer");

                // 📝 B7: Giới hạn độ dài transcript để tránh vượt token limit
                var trimmedTranscript = transcript.Length > 8000
                    ? transcript.Substring(0, 8000) + "\n\n[... Transcript đã được rút gọn để phù hợp token limit ...]"
                    : transcript;

                var payload = new
                {
                    model = _aiModel,
                    messages = new[]
                    {
                        new { role = "system", content = "Bạn là AI chuyên gia chấm điểm và đánh giá buổi bảo vệ đồ án tốt nghiệp. Hãy phân tích transcript một cách khách quan, chi tiết và chính xác theo tiêu chí rubric." },
                        new { role = "user", content = prompt },
                        new { role = "user", content = $"**TRANSCRIPT BUỔI BẢO VỆ:**\n\n{trimmedTranscript}" }
                    },
                    max_tokens = 16384,
                    temperature = 0.2, // Giảm temperature để kết quả ổn định hơn
                    top_p = 0.9
                };

                _logger.LogInformation("🤖 Calling AI model: {Model} | URL: {ApiUrl} | Transcript length: {Length} chars",
                    _aiModel, _openRouterUrl, trimmedTranscript.Length);

                var response = await _httpClient.PostAsJsonAsync(_openRouterUrl, payload);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ AI API error: {StatusCode} - {Error}", response.StatusCode, error);
                    throw new HttpRequestException($"AI API returned error: {response.StatusCode} - {error}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("🔍 Raw AI Response: {Response}", responseContent);

                var result = ParseAIResponse(responseContent);

                // 📊 Tính điểm trung bình tổng thể
                CalculateOverallAverages(result);

                _logger.LogInformation("✅ Transcript analysis completed successfully for session {SessionId}", request.DefenseSessionId);
                return result;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "❌ Resource not found during analysis for session {SessionId}", request.DefenseSessionId);
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ AI API request failed for session {SessionId}", request.DefenseSessionId);
                throw new InvalidOperationException($"AI service is currently unavailable. Please try again later. Details: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected error analyzing transcript for session {SessionId}", request.DefenseSessionId);
                throw;
            }
        }

        /// <summary>
        /// Kiểm tra transcript có chứa từ khóa liên quan đến buổi bảo vệ không
        /// </summary>
        private bool ContainsDefenseRelatedKeywords(string transcript)
        {
            var keywords = new[]
            {
                // Tiếng Việt
                "bảo vệ", "đồ án", "dự án", "trình bày", "hội đồng", "giảng viên",
                "sinh viên", "câu hỏi", "giải thích", "chủ tịch", "ủy viên", "thư ký",
                "đánh giá", "nhận xét", "phản biện", "demo", "tính năng", "hệ thống",
                "công nghệ", "thiết kế", "kiến trúc", "code", "database", "testing",
                
                // Tiếng Anh (trường hợp transcript bằng tiếng Anh)
                "defense", "project", "presentation", "committee", "lecturer",
                "student", "question", "explain", "chairman", "member", "secretary",
                "evaluation", "feedback", "review", "demo", "feature", "system",
                "technology", "design", "architecture", "database"
            };

            var lowerTranscript = transcript.ToLowerInvariant();
            var foundKeywords = keywords.Count(keyword => lowerTranscript.Contains(keyword.ToLowerInvariant()));

            // Cần ít nhất 3 từ khóa liên quan
            return foundKeywords >= 3;
        }

        /// <summary>
        /// Tạo response khi không tìm thấy transcript
        /// </summary>
        private TranscriptAnalysisResponseDto CreateNotFoundResponse(int sessionId, string reason)
        {
            return new TranscriptAnalysisResponseDto
            {
                Summary = new SummaryDto
                {
                    OverallSummary = $"❌ Không tìm thấy transcript cho buổi bảo vệ #{sessionId}.",
                    StudentPerformance = "Không có dữ liệu để phân tích.",
                    DiscussionFocus = "N/A"
                },
                LecturerFeedbacks = new List<LecturerFeedbackDto>
                {
                    new LecturerFeedbackDto
                    {
                        Lecturer = "System",
                        MainComments = reason,
                        PositivePoints = new List<string> { "Không có dữ liệu" },
                        ImprovementPoints = new List<string>
                        {
                            "Đảm bảo buổi bảo vệ đã được ghi âm",
                            "Kiểm tra transcript đã được tạo và lưu vào Redis",
                            "Liên hệ quản trị viên nếu vấn đề vẫn tiếp diễn"
                        },
                        RubricScores = new Dictionary<string, double?>()
                    }
                },
                AiInsight = new AiInsightDto
                {
                    Analysis = $"Không thể phân tích do thiếu transcript. Session ID: {sessionId}",
                    RubricAverages = new Dictionary<string, double?>(),
                    ToneAnalysis = "N/A"
                },
                AiSuggestion = new AiSuggestionDto
                {
                    ForStudent = "Không có gợi ý (thiếu dữ liệu phân tích)",
                    ForAdvisor = "Không có gợi ý (thiếu dữ liệu phân tích)",
                    ForSystem = "Kiểm tra hệ thống ghi âm và chuyển đổi speech-to-text"
                }
            };
        }

        /// <summary>
        /// Tạo response khi transcript không hợp lệ (quá ngắn, nhiễu, không có nội dung)
        /// </summary>
        private TranscriptAnalysisResponseDto CreateInvalidContentResponse(int sessionId, string reason)
        {
            return new TranscriptAnalysisResponseDto
            {
                Summary = new SummaryDto
                {
                    OverallSummary = $"⚠️ Transcript của buổi bảo vệ #{sessionId} không hợp lệ để phân tích.",
                    StudentPerformance = "Không thể đánh giá do nội dung transcript không đầy đủ.",
                    DiscussionFocus = "N/A"
                },
                LecturerFeedbacks = new List<LecturerFeedbackDto>
                {
                    new LecturerFeedbackDto
                    {
                        Lecturer = "System",
                        MainComments = reason,
                        PositivePoints = new List<string> { "Không có dữ liệu hợp lệ" },
                        ImprovementPoints = new List<string>
                        {
                            "Kiểm tra chất lượng file ghi âm (âm thanh rõ ràng, không nhiễu)",
                            "Đảm bảo buổi bảo vệ đủ dài (tối thiểu 10-15 phút)",
                            "Kiểm tra cấu hình Azure Speech Service",
                            "Thử ghi âm và chuyển đổi lại transcript"
                        },
                        RubricScores = new Dictionary<string, double?>()
                    }
                },
                AiInsight = new AiInsightDto
                {
                    Analysis = $"Không thể phân tích do transcript không hợp lệ hoặc quá ngắn. Session ID: {sessionId}",
                    RubricAverages = new Dictionary<string, double?>(),
                    ToneAnalysis = "N/A"
                },
                AiSuggestion = new AiSuggestionDto
                {
                    ForStudent = "Đảm bảo buổi bảo vệ được ghi âm rõ ràng và đầy đủ",
                    ForAdvisor = "Kiểm tra thiết bị ghi âm trước khi bắt đầu buổi bảo vệ",
                    ForSystem = "Cải thiện thuật toán lọc nhiễu và chuyển đổi speech-to-text"
                }
            };
        }

        /// <summary>
        /// Tạo response khi không có rubric để đánh giá
        /// </summary>
        private TranscriptAnalysisResponseDto CreateNoRubricsResponse(int sessionId, int majorId, string reason)
        {
            return new TranscriptAnalysisResponseDto
            {
                Summary = new SummaryDto
                {
                    OverallSummary = $"⚠️ Không thể phân tích buổi bảo vệ #{sessionId} do thiếu tiêu chí đánh giá.",
                    StudentPerformance = "Không thể chấm điểm do chưa cấu hình rubric cho ngành.",
                    DiscussionFocus = "N/A"
                },
                LecturerFeedbacks = new List<LecturerFeedbackDto>
                {
                    new LecturerFeedbackDto
                    {
                        Lecturer = "System",
                        MainComments = reason,
                        PositivePoints = new List<string> { "Không có tiêu chí đánh giá" },
                        ImprovementPoints = new List<string>
                        {
                            $"Cấu hình rubric cho ngành (Major ID: {majorId})",
                            "Liên kết rubric với ngành thông qua MajorRubric",
                            "Đảm bảo mỗi ngành có ít nhất 3-5 tiêu chí đánh giá"
                        },
                        RubricScores = new Dictionary<string, double?>()
                    }
                },
                AiInsight = new AiInsightDto
                {
                    Analysis = $"Không thể phân tích do thiếu rubric. Major ID: {majorId}, Session ID: {sessionId}",
                    RubricAverages = new Dictionary<string, double?>(),
                    ToneAnalysis = "N/A"
                },
                AiSuggestion = new AiSuggestionDto
                {
                    ForStudent = "Liên hệ khoa/bộ môn để cấu hình tiêu chí đánh giá cho ngành",
                    ForAdvisor = "Đề xuất bộ môn thiết lập rubric chuẩn cho ngành",
                    ForSystem = "Tạo rubric mẫu cho các ngành phổ biến: Công nghệ thông tin, Khoa học máy tính, Kỹ thuật phần mềm..."
                }
            };
        }

        /// <summary>
        /// Tạo prompt nâng cao cho AI phân tích transcript
        /// </summary>
        private string BuildAdvancedAnalysisPrompt(IEnumerable<dynamic> rubricDetails)
        {
            var rubricDescriptions = string.Join("\n", rubricDetails.Select((r, index) =>
                $"{index + 1}. **{r.RubricName}**: {r.Description ?? "Không có mô tả"}"));

            return $@"
# 🎯 NHIỆM VỤ: PHÂN TÍCH TRANSCRIPT BUỔI BẢO VỆ ĐỒ ÁN TỐT NGHIỆP

Bạn sẽ nhận được transcript (bản ghi văn bản) của một buổi bảo vệ đồ án tốt nghiệp. Hãy phân tích KỸ LƯỠNG và TRẢ VỀ JSON theo format bên dưới.

---

## 📋 CÁC TIÊU CHÍ ĐÁNH GIÁ (RUBRIC)

{rubricDescriptions}

---

## ⚠️ QUY TẮC PHÂN TÍCH TRANSCRIPT

### 🔴 **BƯỚC 1: PHÂN BIỆT VAI TRÒ TRONG TRANSCRIPT**
Trước tiên, hãy PHÂN TÍCH KỸ LƯỠNG transcript để xác định:

1. **Ai là GIẢNG VIÊN / HỘI ĐỒNG?**
   - Thường được gọi: ""Chủ tịch"", ""Thư ký"", ""Ủy viên"", ""Phản biện"", ""Giảng viên"", ""Thầy"", ""Cô"", ""Tiến sĩ"", ""TS."", ""PGS."", ""GS.""
   - Người đặt câu hỏi, yêu cầu giải thích
   - Người nhận xét, đánh giá, góp ý

2. **Ai là SINH VIÊN?**
   - Thường được gọi: ""Em"", ""Nhóm em"", ""Sinh viên"", tên cụ thể (VD: ""Nguyễn Văn A"")
   - Người trả lời câu hỏi
   - Người trình bày, demo dự án
   - Người giải thích, bảo vệ quan điểm

### 🔴 **BƯỚC 2: KIỂM TRA CHẤT LƯỢNG TRANSCRIPT**
Nếu transcript có các đặc điểm sau, **BẮT BUỘC TRẢ VỀ JSON LỖI**:

❌ **Trường hợp 1: Transcript LAN MAN, KHÔNG RÕ RÀNG**
- Không phân biệt được ai là giảng viên, ai là sinh viên
- Nội dung lộn xộn, không có cấu trúc câu hỏi - trả lời
- Nhiều câu vô nghĩa, bị cắt ngang liên tục

❌ **Trường hợp 2: CÂU HỎI KHÔNG LIÊN QUAN ĐẾN DỰ ÁN**
- Hội đồng hỏi về chuyện cá nhân, gia đình
- Câu hỏi chung chung không liên quan đến công nghệ, kỹ thuật
- Nội dung chủ yếu là trò chuyện phiếm, không có tính học thuật

❌ **Trường hợp 3: THIẾU THÔNG TIN QUAN TRỌNG**
- Không có phần trình bày dự án
- Không có câu hỏi từ hội đồng
- Không có câu trả lời từ sinh viên

**📌 Khi gặp các trường hợp trên, TRẢ VỀ JSON LỖI như sau:**
{{
  ""summary"": {{
    ""overallSummary"": ""⚠️ Transcript không đủ điều kiện phân tích. [NÊU RÕ LÝ DO: lan man/không liên quan/thiếu thông tin]"",
    ""studentPerformance"": ""Không thể đánh giá do transcript không hợp lệ."",
    ""discussionFocus"": ""N/A""
  }},
  ""lecturerFeedbacks"": [
    {{
      ""lecturer"": ""System"",
      ""mainComments"": ""Transcript không đủ rõ ràng để phân tích. Vui lòng kiểm tra lại file ghi âm hoặc hệ thống chuyển đổi speech-to-text."",
      ""positivePoints"": [""Không có dữ liệu hợp lệ""],
      ""improvementPoints"": [""Cải thiện chất lượng ghi âm"", ""Đảm bảo buổi bảo vệ có cấu trúc rõ ràng""],
      ""rubricScores"": {{}}
    }}
  ],
  ""aiInsight"": {{
    ""analysis"": ""Không thể phân tích do transcript không đạt chuẩn. [NÊU RÕ VẤN ĐỀ CỤ THỂ]"",
    ""rubricAverages"": {{}},
    ""toneAnalysis"": ""N/A""
  }},
  ""aiSuggestion"": {{
    ""forStudent"": ""Đảm bảo buổi bảo vệ được tổ chức chuyên nghiệp với cấu trúc rõ ràng"",
    ""forAdvisor"": ""Hướng dẫn sinh viên chuẩn bị kỹ lưỡng cho buổi bảo vệ"",
    ""forSystem"": ""Cải thiện chất lượng ghi âm và hệ thống nhận diện giọng nói""
  }}
}}

---

## 🔍 YÊU CẦU PHÂN TÍCH CHI TIẾT (CHỈ KHI TRANSCRIPT HỢP LỆ)

### **1. TÓM TẮT TỔNG QUAN (Summary)**
- **overallSummary**: Tóm tắt 4-6 câu về diễn biến buổi bảo vệ:
  - Nhóm sinh viên trình bày về dự án gì?
  - Công nghệ chính được sử dụng là gì?
  - Hội đồng đặt những câu hỏi về lĩnh vực nào (công nghệ, thiết kế, tính năng, bảo mật...)?
  - Kết quả chung: buổi bảo vệ diễn ra tốt/khá/cần cải thiện?

- **studentPerformance**: Đánh giá chi tiết (5-7 câu) **DỰA TRÊN CÂU TRẢ LỜI CỦA SINH VIÊN**:
  - Sinh viên trình bày rõ ràng, tự tin hay chưa?
  - Sinh viên trả lời câu hỏi nhanh nhạy, chính xác hay lúng túng?
  - Kiến thức nền tảng (lý thuyết) có vững không?
  - Kỹ năng thực hành (code, demo, giải quyết vấn đề)?
  - Thái độ: cởi mở, tiếp thu góp ý hay phòng thủ?

- **discussionFocus**: Liệt kê 5-7 chủ đề chính mà hội đồng quan tâm (CHỈ LIỆT KÊ CÂU HỎI TỪ GIẢNG VIÊN/HỘI ĐỒNG):
  - VD: ""1) Kiến trúc hệ thống và scalability, 2) Xử lý bảo mật và authentication, 3) Testing strategy và code quality, 4) Tính thực tiễn của dự án, 5) Công nghệ AI/ML được áp dụng""

---

### **2. ĐÁNH GIÁ TỪ TỪNG GIẢNG VIÊN (LecturerFeedbacks)**

⚠️ **CHỈ PHÂN TÍCH GIẢNG VIÊN, KHÔNG BAO GỒM SINH VIÊN**

Với MỖI GIẢNG VIÊN trong hội đồng (Chủ tịch, Thư ký, Ủy viên, Phản biện...), hãy:

#### **a) Xác định giảng viên**
- **lecturer**: Tên hoặc vai trò (VD: ""TS. Nguyễn Văn A - Chủ tịch HĐ"", ""PGS. Trần Thị B - Phản biện"")

#### **b) Phân tích câu hỏi của giảng viên**
- **mainComments**: Tóm tắt 3-5 câu về câu hỏi/nhận xét của giảng viên này:
  - Giảng viên tập trung vào khía cạnh nào của dự án?
  - Câu hỏi có chất lượng không? Có liên quan đến tiêu chí rubric không?
  - Phong cách đánh giá: khắt khe, khích lệ, trung lập?

#### **c) Đánh giá dựa trên câu TRẢ LỜI của SINH VIÊN**
- **positivePoints**: Liệt kê 3-4 điểm mạnh **DỰA VÀO CÂU TRẢ LỜI CỦA SINH VIÊN** cho câu hỏi của giảng viên này:
  - VD: ""Trả lời rõ ràng về kiến trúc hệ thống"", ""Demo thành công tính năng bảo mật"", ""Giải thích thuật toán logic và thuyết phục""

- **improvementPoints**: Liệt kê 3-4 điểm yếu **DỰA VÀO CÂU TRẢ LỜI CỦA SINH VIÊN**:
  - VD: ""Không trả lời được về unit test"", ""Giải thích chưa rõ về caching strategy"", ""Lúng túng khi hỏi về edge case""

#### **d) Chấm điểm theo Rubric**
- **rubricScores**: **CHỈ CHẤM DỰA VÀO CÂU TRẢ LỜI CỦA SINH VIÊN**

🔴 **QUY TẮC CHẤM ĐIỂM:**
1. **ĐỌC KỸ DESCRIPTION CỦA TỪNG RUBRIC** (đã cung cấp ở trên)
2. **SO SÁNH câu trả lời của sinh viên với yêu cầu trong Description**
3. **Sinh viên trả lời XUẤT SẮC** (đáp ứng đầy đủ Description) → 8.5 - 10.0
4. **Sinh viên trả lời TốT** (đáp ứng phần lớn Description) → 7.0 - 8.4
5. **Sinh viên trả lời KHÁ** (đáp ứng một phần Description) → 6.0 - 6.9
6. **Sinh viên trả lời YẾU** (không đáp ứng Description) → 4.0 - 5.9
7. **Không có thông tin liên quan** → null

**Ví dụ:**
""rubricScores"": {{
  ""Kiến thức lý thuyết"": 8.5,    // Sinh viên giải thích rõ ràng các khái niệm
  ""Kỹ năng lập trình"": 9.0,      // Demo code chạy tốt, logic rõ ràng
  ""Thiết kế hệ thống"": 7.0,      // Vẽ được diagram nhưng thiếu chi tiết
  ""Testing & QA"": null           // Không có câu hỏi/trả lời về testing
}}

---

### **3. PHÂN TÍCH VÀ GỢI Ý CỦA AI (AiInsight)**

#### **a) Phân tích tổng hợp**
- **analysis**: Viết 5-7 câu phân tích sâu:
  - So sánh đánh giá của các giảng viên (có thống nhất không?)
  - Điểm mạnh NỔI BẬT nhất của sinh viên
  - Điểm yếu NGHIÊM TRỌNG nhất cần khắc phục
  - Xu hướng chung: dự án thiên về lý thuyết hay thực hành?

#### **b) Điểm trung bình theo Rubric**
- **rubricAverages**: Tính điểm trung bình của TẤT CẢ giảng viên cho từng rubric:
  - Lấy điểm từ tất cả giảng viên → tính trung bình cộng
  - Nếu rubric nào không ai chấm → ghi null

#### **c) Phân tích giọng điệu**
- **toneAnalysis**: Đánh giá thái độ của hội đồng và sinh viên:
  - Hội đồng: khắt khe, ủng hộ, trung lập, thân thiện?
  - Sinh viên: tự tin, lo lắng, phòng thủ, cầu thị?
  - Không khí buổi bảo vệ: căng thẳng, thoải mái, chuyên nghiệp?

---

### **4. GỢI Ý CẢI THIỆN (AiSuggestion)**

#### **a) Gợi ý cho Sinh viên**
- **forStudent**: Đưa ra 5-7 lời khuyên CỤ THỂ dựa trên điểm yếu đã phát hiện:
  - VD: ""Tìm hiểu thêm về design pattern (Factory, Singleton) để giải thích kiến trúc rõ hơn""
  - VD: ""Viết thêm unit test với code coverage > 70%""

#### **b) Gợi ý cho Giảng viên hướng dẫn**
- **forAdvisor**: Đề xuất 3-5 hướng dẫn thêm:
  - VD: ""Hướng dẫn sinh viên tìm hiểu về CI/CD pipeline""
  - VD: ""Đề xuất sinh viên refactor code để cải thiện maintainability""

#### **c) Gợi ý câu hỏi bổ sung**
- **forSystem**: Đề xuất 7-10 câu hỏi MỚI dựa trên nội dung dự án:
  - VD: ""Nếu hệ thống có 10,000 người dùng đồng thời, bạn sẽ xử lý như thế nào?""
  - VD: ""Giải thích sự khác biệt giữa SQL Injection và XSS, và cách phòng chống?""

---

## ✅ FORMAT JSON TRẢ VỀ

{{
  ""summary"": {{
    ""overallSummary"": ""Tóm tắt 4-6 câu..."",
    ""studentPerformance"": ""Đánh giá 5-7 câu dựa trên câu trả lời..."",
    ""discussionFocus"": ""1) Chủ đề 1, 2) Chủ đề 2...""
  }},
  ""lecturerFeedbacks"": [
    {{
      ""lecturer"": ""Tên/vai trò giảng viên"",
      ""mainComments"": ""Phân tích câu hỏi của giảng viên..."",
      ""positivePoints"": [""Điểm mạnh dựa trên câu trả lời của SV""],
      ""improvementPoints"": [""Điểm yếu dựa trên câu trả lời của SV""],
      ""rubricScores"": {{
        ""Rubric 1"": 8.5,
        ""Rubric 2"": 7.0,
        ""Rubric 3"": null
      }}
    }}
  ],
  ""aiInsight"": {{
    ""analysis"": ""Phân tích tổng hợp..."",
    ""rubricAverages"": {{
      ""Rubric 1"": 8.2,
      ""Rubric 2"": 7.5
    }},
    ""toneAnalysis"": ""Đánh giá thái độ...""
  }},
  ""aiSuggestion"": {{
    ""forStudent"": ""1) Gợi ý 1, 2) Gợi ý 2..."",
    ""forAdvisor"": ""1) Đề xuất 1, 2) Đề xuất 2..."",
    ""forSystem"": ""1) Câu hỏi 1?\n2) Câu hỏi 2?...""
  }}
}}

---

## ⚠️ LƯU Ý QUAN TRỌNG

1. ✅ **Trả về JSON HỢP LỆ**, KHÔNG thêm markdown (```json)
2. ✅ **PHÂN BIỆT RÕ vai trò**: Giảng viên (người hỏi) vs Sinh viên (người trả lời)
3. ✅ **CHỈ CHẤM ĐIỂM dựa trên CÂU TRẢ LỜI CỦA SINH VIÊN**
4. ✅ **ĐỌC KỸ Description của Rubric** trước khi chấm điểm
5. ✅ **NẾU transcript LAN MAN, KHÔNG RÕ RÀNG** → Trả về JSON lỗi như hướng dẫn
6. ✅ **NẾU câu hỏi KHÔNG LIÊN QUAN đến dự án** → Trả về JSON lỗi
7. ✅ **Điểm số phải hợp lý** (4.0 - 10.0), dựa trên Description của Rubric

Hãy bắt đầu phân tích transcript bên dưới! 🚀
";
        }

        /// <summary>
        /// Parse AI response JSON thành TranscriptAnalysisResponseDto
        /// </summary>
        private TranscriptAnalysisResponseDto ParseAIResponse(string responseJson)
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
                    return CreateFallbackResponse();
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
                    _logger.LogWarning("⚠️ No valid JSON found. Raw: {Content}", content.Substring(0, Math.Min(500, content.Length)));
                    return CreateFallbackResponse();
                }

                var json = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                _logger.LogDebug("✅ Extracted JSON for parsing (length: {Length})", json.Length);

                var result = JsonSerializer.Deserialize<TranscriptAnalysisResponseDto>(json, options);
                if (result == null)
                {
                    _logger.LogWarning("⚠️ Deserialization returned null");
                    return CreateFallbackResponse();
                }

                NormalizeEmptyFields(result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️ Failed to parse AI JSON response. Raw length: {Length}", responseJson?.Length ?? 0);
                return CreateFallbackResponse();
            }
        }

        /// <summary>
        /// Tính điểm trung bình tổng thể từ tất cả giảng viên
        /// </summary>
        private void CalculateOverallAverages(TranscriptAnalysisResponseDto dto)
        {
            if (dto?.LecturerFeedbacks == null || !dto.LecturerFeedbacks.Any())
                return;

            dto.AiInsight ??= new AiInsightDto();
            dto.AiInsight.RubricAverages ??= new Dictionary<string, double?>();

            // Lấy tất cả rubric names từ tất cả giảng viên
            var allRubricNames = dto.LecturerFeedbacks
                .Where(f => f.RubricScores != null)
                .SelectMany(f => f.RubricScores.Keys)
                .Distinct()
                .ToList();

            foreach (var rubricName in allRubricNames)
            {
                var scores = dto.LecturerFeedbacks
                    .Where(f => f.RubricScores != null && f.RubricScores.ContainsKey(rubricName))
                    .Select(f => f.RubricScores[rubricName])
                    .Where(score => score.HasValue)
                    .Select(score => score!.Value)
                    .ToList();

                if (scores.Any())
                {
                    var average = Math.Round(scores.Average(), 2);
                    dto.AiInsight.RubricAverages[rubricName] = average;
                    _logger.LogInformation("📊 Rubric '{Rubric}': Average = {Average} (from {Count} scores)",
                        rubricName, average, scores.Count);
                }
                else
                {
                    dto.AiInsight.RubricAverages[rubricName] = null;
                }
            }
        }

        /// <summary>
        /// Chuẩn hóa các field rỗng
        /// </summary>
        private static void NormalizeEmptyFields(TranscriptAnalysisResponseDto dto)
        {
            if (dto == null) return;

            dto.Summary ??= new SummaryDto
            {
                OverallSummary = "N/A",
                StudentPerformance = "N/A",
                DiscussionFocus = "N/A"
            };

            dto.LecturerFeedbacks ??= new List<LecturerFeedbackDto>();

            if (!dto.LecturerFeedbacks.Any())
            {
                dto.LecturerFeedbacks.Add(new LecturerFeedbackDto
                {
                    Lecturer = "N/A",
                    MainComments = "Không có dữ liệu phản hồi từ giảng viên",
                    PositivePoints = new List<string> { "N/A" },
                    ImprovementPoints = new List<string> { "N/A" },
                    RubricScores = new Dictionary<string, double?>()
                });
            }

            dto.AiInsight ??= new AiInsightDto
            {
                Analysis = "N/A",
                RubricAverages = new Dictionary<string, double?>(),
                ToneAnalysis = "N/A"
            };

            dto.AiSuggestion ??= new AiSuggestionDto
            {
                ForStudent = "N/A",
                ForAdvisor = "N/A",
                ForSystem = "N/A"
            };
        }

        /// <summary>
        /// Tạo response mặc định khi AI fail
        /// </summary>
        private static TranscriptAnalysisResponseDto CreateFallbackResponse()
        {
            return new TranscriptAnalysisResponseDto
            {
                Summary = new SummaryDto
                {
                    OverallSummary = "AI analysis unavailable. Please review the transcript manually.",
                    StudentPerformance = "Unable to analyze",
                    DiscussionFocus = "N/A"
                },
                LecturerFeedbacks = new List<LecturerFeedbackDto>
                {
                    new LecturerFeedbackDto
                    {
                        Lecturer = "System",
                        MainComments = "AI service encountered an error during analysis.",
                        PositivePoints = new List<string> { "Analysis unavailable" },
                        ImprovementPoints = new List<string> { "Please retry or analyze manually" },
                        RubricScores = new Dictionary<string, double?>()
                    }
                },
                AiInsight = new AiInsightDto
                {
                    Analysis = "Analysis failed. Please check logs for details.",
                    RubricAverages = new Dictionary<string, double?>(),
                    ToneAnalysis = "N/A"
                },
                AiSuggestion = new AiSuggestionDto
                {
                    ForStudent = "N/A",
                    ForAdvisor = "N/A",
                    ForSystem = "N/A"
                }
            };
        }
    }
}
