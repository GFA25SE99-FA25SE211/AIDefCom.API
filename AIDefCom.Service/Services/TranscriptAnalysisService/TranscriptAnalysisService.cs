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
        }

        public async Task<TranscriptAnalysisResponseDto> AnalyzeTranscriptAsync(TranscriptAnalysisRequestDto request)
        {
            try
            {
                var token = _config["AI:OpenRouterToken"];
                if (string.IsNullOrEmpty(token))
                    throw new InvalidOperationException("AI API token is missing. Please configure 'AI:OpenRouterToken' in appsettings.json.");

                // ?? B1: L?y transcript t? Redis cache
                var transcriptKey = $"partial_transcript:{request.DefenseSessionId}";
                _logger.LogInformation("?? Fetching transcript from Redis with key: {Key}", transcriptKey);
                
                var transcript = await _redisCache.GetAsync(transcriptKey);
                if (string.IsNullOrWhiteSpace(transcript))
                {
                    throw new Exception($"Transcript not found in Redis cache for session ID: {request.DefenseSessionId}");
                }

                _logger.LogInformation("? Retrieved transcript from Redis: {Length} characters", transcript.Length);

                // ?? B2: L?y bu?i b?o v?
                var defense = await _uow.DefenseSessions.GetByIdAsync(request.DefenseSessionId);
                if (defense == null)
                    throw new Exception("Defense session not found.");

                // ?? B3: L?y h?i ??ng
                var council = await _uow.Councils.GetByIdAsync(defense.CouncilId);
                if (council == null)
                    throw new Exception("Council not found.");

                // ?? B4: L?y rubric th?t c?a ngành
                var rubrics = await _uow.MajorRubrics.GetRubricsByMajorIdAsync(council.MajorId);
                var rubricNames = rubrics.Select(r => r.RubricName).ToList();

                // ?? B5: T?o prompt ??n gi?n hóa
                var prompt = BuildSimplifiedPrompt(transcript, rubricNames);

                // ?? B6: Chu?n b? HttpClient
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://openrouter.ai/");
                _httpClient.DefaultRequestHeaders.Add("X-Title", "AIDefCom Transcript Analyzer");

                // ?? Ch?n model m?nh h?n
                var model = _config["AI:Model"] ?? "gpt-4o-mini";

                // ?? Gi?i h?n ?? dài transcript
                var trimmedTranscript = transcript.Length > 5000
                    ? transcript.Substring(0, 5000)
                    : transcript;

                var payload = new
                {
                    model,
                    messages = new[]
                    {
                        new { role = "system", content = "B?n là AI chuyên ch?m lu?n v?n và ?ánh giá bu?i b?o v? khóa lu?n t?t nghi?p." },
                        new { role = "user", content = prompt },
                        new { role = "user", content = trimmedTranscript }
                    },
                    max_tokens = 16384,
                    temperature = 0.2,
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

                var result = ParseAIResponse(responseContent);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error analyzing transcript with AI");
                throw;
            }
        }

        private string BuildSimplifiedPrompt(string transcript, IEnumerable<string> rubricNames)
        {
            var rubricList = string.Join(", ", rubricNames);
            return $@"
D??i ?ây là các tiêu chí ch?m ?i?m th?c t? cho ngành:

?? Tiêu chí Rubric: {rubricList}

Hãy phân tích transcript bu?i b?o v? d??i ?ây, ?ánh giá theo t?ng tiêu chí rubric.
Tr? v? k?t qu? d??i d?ng JSON h?p l? theo m?u:

{{
  ""summary"": {{
    ""overallSummary"": ""Tóm t?t 3–5 câu"",
    ""studentPerformance"": ""Phân tích phong thái và kh? n?ng"",
    ""discussionFocus"": ""Các ch? ?? tr?ng tâm""
  }},
  ""lecturerFeedbacks"": [
    {{
      ""lecturer"": ""Tên gi?ng viên ho?c vai trò"",
      ""mainComments"": ""Nhân xét t?ng quát"",
      ""positivePoints"": [""Ít nh?t 2 ?i?m m?nh""],
      ""improvementPoints"": [""Ít nh?t 2 ?i?m c?n c?i thi?n""],
      ""rubricScores"": {{
        ""{rubricList}"": 8.0
      }}
    }}
  ],
  ""aiInsight"": {{
    ""analysis"": ""Phân tích t?ng h?p"",
    ""rubricAverages"": {{
      ""{rubricList}"": 8.0
    }},
    ""toneAnalysis"": ""Phân tích gi?ng ?i?u""
  }},
  ""aiSuggestion"": {{
    ""forStudent"": ""G?i ý c?i thi?n"",
    ""forAdvisor"": ""G?i ý cho gi?ng viên"",
    ""forSystem"": ""G?i ý cho h? th?ng AI""
  }}
}}";
        }

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
                    _logger.LogWarning("?? AI returned empty content.");
                    return new TranscriptAnalysisResponseDto();
                }

                content = content
                    .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("```", "")
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Trim();

                int jsonStart = content.IndexOf('{');
                int jsonEnd = content.LastIndexOf('}');
                if (jsonStart < 0 || jsonEnd < 0)
                {
                    _logger.LogWarning("?? No valid JSON found. Raw: {Content}", content);
                    return new TranscriptAnalysisResponseDto();
                }

                var json = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                _logger.LogInformation("? Extracted JSON for parsing: {Json}", json);

                var result = JsonSerializer.Deserialize<TranscriptAnalysisResponseDto>(json, options);
                NormalizeEmptyFields(result);
                return result ?? new TranscriptAnalysisResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? Failed to parse AI JSON response. Raw: {Response}", responseJson);
                return new TranscriptAnalysisResponseDto();
            }
        }

        private static void NormalizeEmptyFields(TranscriptAnalysisResponseDto dto)
        {
            if (dto == null) return;

            dto.Summary ??= new SummaryDto
            {
                OverallSummary = "N/A",
                StudentPerformance = "N/A",
                DiscussionFocus = "N/A"
            };

            dto.LecturerFeedbacks ??= new List<LecturerFeedbackDto>
            {
                new LecturerFeedbackDto
                {
                    Lecturer = "N/A",
                    MainComments = "N/A",
                    PositivePoints = new List<string> { "N/A" },
                    ImprovementPoints = new List<string> { "N/A" },
                    RubricScores = new Dictionary<string, double?>()
                }
            };

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
    }
}
