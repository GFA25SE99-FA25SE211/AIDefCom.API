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

## 🔍 YÊU CẦU PHÂN TÍCH CHI TIẾT

### **1. TÓM TẮT TỔNG QUAN (Summary)**
Hãy đọc toàn bộ transcript và viết:
- **overallSummary**: Tóm tắt 4-6 câu về diễn biến buổi bảo vệ:
  - Nhóm sinh viên trình bày về dự án gì?
  - Công nghệ chính được sử dụng là gì?
  - Hội đồng đặt những câu hỏi về lĩnh vực nào (công nghệ, thiết kế, tính năng, bảo mật...)?
  - Kết quả chung: buổi bảo vệ diễn ra tốt/khá/cần cải thiện?

- **studentPerformance**: Đánh giá chi tiết (5-7 câu):
  - Sinh viên trình bày rõ ràng, tự tin hay chưa?
  - Sinh viên trả lời câu hỏi nhanh nhạy, chính xác hay lúng túng?
  - Kiến thức nền tảng (lý thuyết) có vững không?
  - Kỹ năng thực hành (code, demo, giải quyết vấn đề)?
  - Thái độ: cởi mở, tiếp thu góp ý hay phòng thủ?

- **discussionFocus**: Liệt kê 5-7 chủ đề chính mà hội đồng quan tâm:
  - VD: ""1) Kiến trúc hệ thống và scalability, 2) Xử lý bảo mật và authentication, 3) Testing strategy và code quality, 4) Tính thực tiễn của dự án, 5) Công nghệ AI/ML được áp dụng, 6) Hiệu năng và tối ưu hóa, 7) Quy trình triển khai (deployment)""

---

### **2. ĐÁNH GIÁ TỪ TỪNG GIẢNG VIÊN (LecturerFeedbacks)**

Với MỖI GIẢNG VIÊN trong hội đồng (Chủ tịch, Thư ký, Ủy viên, Phản biện...), hãy phân tích:

#### **a) Thông tin giảng viên**
- **lecturer**: Tên hoặc vai trò giảng viên (VD: ""TS. Nguyễn Văn A - Chủ tịch HĐ"")

#### **b) Nhận xét tổng quát**
- **mainComments**: Tóm tắt 3-5 câu về ý kiến chính của giảng viên này:
  - Giảng viên tập trung vào khía cạnh nào của dự án?
  - Giảng viên hỏi nhiều hay ít? Phong cách hỏi như thế nào?

#### **c) Điểm mạnh sinh viên thể hiện**
- **positivePoints**: Liệt kê ít nhất 3-4 điểm mạnh mà giảng viên này đánh giá cao:
  - VD: ""Trình bày rõ ràng, có cấu trúc"", ""Demo sản phẩm mượt mà"", ""Hiểu rõ công nghệ đang dùng"", ""Trả lời câu hỏi tự tin""

#### **d) Điểm cần cải thiện**
- **improvementPoints**: Liệt kê ít nhất 3-4 điểm yếu hoặc lời khuyên:
  - VD: ""Thiếu unit test"", ""Chưa xử lý edge case"", ""Giải thích thuật toán chưa rõ"", ""Cần tìm hiểu thêm về security""

#### **e) Chấm điểm theo từng tiêu chí Rubric**
- **rubricScores**: Đối với MỖI rubric, hãy đưa ra điểm số (số thực) dựa trên:
  - Câu trả lời của sinh viên có liên quan đến tiêu chí này không?
  - Sinh viên trả lời tốt → điểm cao (8-10)
  - Sinh viên trả lời khá → điểm trung bình (6-7.5)
  - Sinh viên trả lời yếu hoặc không trả lời được → điểm thấp (4-5.5)
  - Không có thông tin đánh giá tiêu chí này → ghi null

**Ví dụ:**
```json
""rubricScores"": {{
  ""Kiến thức lý thuyết"": 8.5,
  ""Kỹ năng lập trình"": 9.0,
  ""Thiết kế hệ thống"": 7.0,
  ""Testing & Quality Assurance"": 6.5,
  ""Tính sáng tạo"": 8.0
}}
```

---

### **3. PHÂN TÍCH VÀ GỢI Ý CỦA AI (AiInsight)**

#### **a) Phân tích tổng hợp**
- **analysis**: Viết 5-7 câu phân tích sâu:
  - So sánh đánh giá của các giảng viên (có thống nhất không?)
  - Điểm mạnh NỔI BẬT nhất của nhóm sinh viên
  - Điểm yếu NGHIÊM TRỌNG nhất cần khắc phục
  - Xu hướng chung: dự án thiên về lý thuyết hay thực hành?

#### **b) Điểm trung bình theo từng Rubric**
- **rubricAverages**: Tính điểm trung bình của TẤT CẢ giảng viên cho từng rubric:
  - Lấy điểm từ tất cả giảng viên → tính trung bình cộng
  - Nếu rubric nào không ai chấm → ghi null

**Ví dụ:**
```json
""rubricAverages"": {{
  ""Kiến thức lý thuyết"": 8.2,
  ""Kỹ năng lập trình"": 8.8,
  ""Thiết kế hệ thống"": 7.3,
  ""Testing & Quality Assurance"": 6.8,
  ""Tính sáng tạo"": 7.5
}}
```

#### **c) Phân tích giọng điệu và thái độ**
- **toneAnalysis**: Đánh giá thái độ của hội đồng và sinh viên:
  - Hội đồng: khắt khe, ủng hộ, trung lập, thân thiện?
  - Sinh viên: tự tin, lo lắng, phòng thủ, cầu thị?
  - Không khí buổi bảo vệ: căng thẳng, thoải mái, chuyên nghiệp?

---

### **4. GỢI Ý CẢI THIỆN VÀ CÂU HỎI BỔ SUNG (AiSuggestion)**

#### **a) Gợi ý cho Sinh viên**
- **forStudent**: Đưa ra 5-7 lời khuyên CỤ THỂ để cải thiện:
  - VD: ""Tìm hiểu thêm về design pattern (Factory, Singleton, Observer) để giải thích kiến trúc rõ hơn""
  - VD: ""Viết thêm unit test với code coverage > 70% để chứng minh chất lượng code""
  - VD: ""Học thêm về JWT, OAuth2, OWASP Top 10 để cải thiện bảo mật""

#### **b) Gợi ý cho Giảng viên hướng dẫn**
- **forAdvisor**: Đề xuất 3-5 hướng dẫn thêm cho GVHD:
  - VD: ""Hướng dẫn sinh viên tìm hiểu về CI/CD pipeline (GitHub Actions, Docker)""
  - VD: ""Đề xuất sinh viên refactor code để cải thiện maintainability""

#### **c) Gợi ý câu hỏi bổ sung cho Hội đồng**
- **forSystem**: Đề xuất 7-10 câu hỏi MỚI mà hội đồng CÓ THỂ HỎI THÊM để đánh giá sâu hơn:
  
**Ví dụ:**
```
1. ""Nếu hệ thống có 10,000 người dùng đồng thời, bạn sẽ xử lý như thế nào? (Scalability)""
2. ""Giải thích sự khác biệt giữa SQL Injection và XSS, và cách bạn phòng chống? (Security)""
3. ""Tại sao bạn chọn MongoDB thay vì PostgreSQL cho dự án này? (Architecture decision)""
4. ""Nếu API bị lỗi 500, bạn sẽ debug như thế nào? (Troubleshooting)""
5. ""Code coverage của project là bao nhiêu? Unit test đã cover những case nào? (Testing)""
6. ""Nếu phải deploy lên AWS/Azure, bạn sẽ dùng service nào? (Cloud deployment)""
7. ""Giải thích cách bạn implement caching strategy? (Performance optimization)""
8. ""Code của bạn có tuân theo SOLID principles không? Cho ví dụ. (Code quality)""
9. ""Nếu yêu cầu thêm tính năng X, bạn sẽ thiết kế database schema như thế nào? (Extensibility)""
10. ""So sánh REST API và GraphQL, tại sao bạn chọn REST? (Technology choice)""
```

---

## ✅ FORMAT JSON TRẢ VỀ

```json
{{
  ""summary"": {{
    ""overallSummary"": ""Tóm tắt 4-6 câu về buổi bảo vệ..."",
    ""studentPerformance"": ""Đánh giá chi tiết 5-7 câu..."",
    ""discussionFocus"": ""1) Chủ đề 1, 2) Chủ đề 2, 3) Chủ đề 3...""
  }},
  ""lecturerFeedbacks"": [
    {{
      ""lecturer"": ""Tên/vai trò giảng viên"",
      ""mainComments"": ""Nhận xét 3-5 câu"",
      ""positivePoints"": [""Điểm mạnh 1"", ""Điểm mạnh 2"", ""Điểm mạnh 3""],
      ""improvementPoints"": [""Cần cải thiện 1"", ""Cần cải thiện 2"", ""Cần cải thiện 3""],
      ""rubricScores"": {{
        ""Rubric 1"": 8.5,
        ""Rubric 2"": 7.0,
        ""Rubric 3"": null
      }}
    }}
  ],
  ""aiInsight"": {{
    ""analysis"": ""Phân tích tổng hợp 5-7 câu..."",
    ""rubricAverages"": {{
      ""Rubric 1"": 8.2,
      ""Rubric 2"": 7.5
    }},
    ""toneAnalysis"": ""Đánh giá thái độ và không khí buổi bảo vệ...""
  }},
  ""aiSuggestion"": {{
    ""forStudent"": ""1) Gợi ý 1, 2) Gợi ý 2, 3) Gợi ý 3..."",
    ""forAdvisor"": ""1) Đề xuất 1, 2) Đề xuất 2..."",
    ""forSystem"": ""1) Câu hỏi gợi ý 1?\n2) Câu hỏi gợi ý 2?\n3) Câu hỏi gợi ý 3?...""
  }}
}}
```

---

## ⚠️ LƯU Ý QUAN TRỌNG

1. ✅ **Trả về JSON HỢP LỆ**, KHÔNG thêm markdown formatting (```json)
2. ✅ **Chấm điểm KHÁCH QUAN** dựa trên transcript, không đoán mò
3. ✅ **Nếu không có thông tin** về rubric nào → ghi **null** trong rubricScores
4. ✅ **Phân tích KỸ LƯỠNG** từng câu hỏi và câu trả lời trong transcript
5. ✅ **Gợi ý câu hỏi** phải LIÊN QUAN đến nội dung dự án (không hỏi chung chung)
6. ✅ **Điểm số** phải hợp lý (4.0 - 10.0), không chấm quá cao hoặc quá thấp vô căn cứ

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
