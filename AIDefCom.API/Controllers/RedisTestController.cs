using AIDefCom.Service.Services.RedisCache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AIDefCom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisTestController : ControllerBase
    {
        private readonly IRedisCache _redisCache;
        private readonly ILogger<RedisTestController> _logger;

        public RedisTestController(IRedisCache redisCache, ILogger<RedisTestController> logger)
        {
            _redisCache = redisCache;
            _logger = logger;
        }

        /// <summary>
        /// Test Redis connection
        /// </summary>
        [HttpGet("ping")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Test bằng cách set và get một key test
                var testKey = $"test:connection:{DateTime.UtcNow.Ticks}";
                var testValue = "Hello Redis!";
                
                await _redisCache.SetAsync(testKey, testValue, 60);
                var retrievedValue = await _redisCache.GetAsync(testKey);
                await _redisCache.DeleteAsync(testKey);
                
                if (retrievedValue == testValue)
                {
                    return Ok(new 
                    { 
                        success = true, 
                        message = "Redis connection is working!",
                        timestamp = DateTime.UtcNow
                    });
                }
                
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "Redis connection test failed - value mismatch" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis connection test failed");
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = $"Redis connection failed: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Get all keys matching a pattern (e.g., "transcript:*")
        /// </summary>
        [HttpGet("keys")]
        public async Task<IActionResult> GetKeys([FromQuery] string pattern = "*")
        {
            try
            {
                var keys = await _redisCache.GetKeysByPatternAsync(pattern);
                
                return Ok(new 
                { 
                    success = true,
                    pattern,
                    count = keys.Count,
                    keys
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get keys by pattern: {Pattern}", pattern);
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = $"Failed to get keys: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Get transcript by defense session ID
        /// Format: transcript:{defense_session_id}
        /// </summary>
        [HttpGet("transcript/{defenseSessionId}")]
        public async Task<IActionResult> GetTranscriptBySessionId(string defenseSessionId)
        {
            try
            {
                var key = $"transcript:{defenseSessionId}";
                _logger.LogInformation("Fetching transcript with key: {Key}", key);
                
                // Kiểm tra xem key có tồn tại không
                var exists = await _redisCache.ExistsAsync(key);
                if (!exists)
                {
                    return NotFound(new 
                    { 
                        success = false,
                        message = $"Transcript not found for defense session ID: {defenseSessionId}",
                        key
                    });
                }
                
                // Lấy dữ liệu dưới dạng string (JSON)
                var jsonData = await _redisCache.GetAsync(key);
                
                if (string.IsNullOrEmpty(jsonData))
                {
                    return NotFound(new 
                    { 
                        success = false,
                        message = $"Transcript data is empty for key: {key}"
                    });
                }
                
                // Parse JSON để trả về object
                var transcriptData = JsonSerializer.Deserialize<object>(jsonData);
                
                return Ok(new 
                { 
                    success = true,
                    defense_session_id = defenseSessionId,
                    key,
                    data = transcriptData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get transcript for session: {SessionId}", defenseSessionId);
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = $"Failed to get transcript: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Get transcript by defense session ID (Hash format)
        /// Dùng cho trường hợp Redis lưu dưới dạng Hash thay vì JSON string
        /// </summary>
        [HttpGet("transcript-hash/{defenseSessionId}")]
        public async Task<IActionResult> GetTranscriptHashBySessionId(string defenseSessionId)
        {
            try
            {
                var key = $"transcript:{defenseSessionId}";
                _logger.LogInformation("Fetching transcript hash with key: {Key}", key);
                
                // Kiểm tra xem key có tồn tại không
                var exists = await _redisCache.ExistsAsync(key);
                if (!exists)
                {
                    return NotFound(new 
                    { 
                        success = false,
                        message = $"Transcript not found for defense session ID: {defenseSessionId}",
                        key
                    });
                }
                
                // Lấy tất cả fields trong hash
                var hashData = await _redisCache.GetHashAllAsync(key);
                
                if (hashData == null || !hashData.Any())
                {
                    return NotFound(new 
                    { 
                        success = false,
                        message = $"Transcript hash is empty for key: {key}"
                    });
                }
                
                return Ok(new 
                { 
                    success = true,
                    defense_session_id = defenseSessionId,
                    key,
                    data = hashData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get transcript hash for session: {SessionId}", defenseSessionId);
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = $"Failed to get transcript hash: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Get all transcripts (scan all transcript:* keys)
        /// </summary>
        [HttpGet("transcripts")]
        public async Task<IActionResult> GetAllTranscripts()
        {
            try
            {
                var keys = await _redisCache.GetKeysByPatternAsync("transcript:*");
                
                var transcripts = new List<object>();
                
                foreach (var key in keys)
                {
                    try
                    {
                        var jsonData = await _redisCache.GetAsync(key);
                        if (!string.IsNullOrEmpty(jsonData))
                        {
                            var data = JsonSerializer.Deserialize<object>(jsonData);
                            transcripts.Add(new 
                            {
                                key,
                                defense_session_id = key.Replace("transcript:", ""),
                                data
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse transcript for key: {Key}", key);
                    }
                }
                
                return Ok(new 
                { 
                    success = true,
                    count = transcripts.Count,
                    total_keys = keys.Count,
                    transcripts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all transcripts");
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = $"Failed to get transcripts: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Set a test transcript (for testing purposes)
        /// </summary>
        [HttpPost("transcript/test")]
        public async Task<IActionResult> SetTestTranscript([FromBody] SetTestTranscriptRequest request)
        {
            try
            {
                var key = $"transcript:{request.DefenseSessionId}";
                
                var testData = new
                {
                    defense_session_id = request.DefenseSessionId,
                    session_id = request.SessionId ?? "test-session",
                    start_time = request.StartTime ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    lines = request.Lines ?? new List<object>
                    {
                        new 
                        {
                            speaker = "Thư ký",
                            text = "Xin chào mọi người",
                            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")
                        },
                        new 
                        {
                            speaker = "Member A",
                            text = "Dạ em chào thầy",
                            timestamp = DateTime.UtcNow.AddSeconds(5).ToString("yyyy-MM-ddTHH:mm:ss")
                        }
                    }
                };
                
                var jsonData = JsonSerializer.Serialize(testData, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                await _redisCache.SetAsync(key, jsonData, request.TtlSeconds ?? 3600);
                
                return Ok(new 
                { 
                    success = true,
                    message = "Test transcript created successfully",
                    key,
                    data = testData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set test transcript");
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = $"Failed to set transcript: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Delete transcript by defense session ID
        /// </summary>
        [HttpDelete("transcript/{defenseSessionId}")]
        public async Task<IActionResult> DeleteTranscript(string defenseSessionId)
        {
            try
            {
                var key = $"transcript:{defenseSessionId}";
                
                var exists = await _redisCache.ExistsAsync(key);
                if (!exists)
                {
                    return NotFound(new 
                    { 
                        success = false,
                        message = $"Transcript not found for defense session ID: {defenseSessionId}"
                    });
                }
                
                await _redisCache.DeleteAsync(key);
                
                return Ok(new 
                { 
                    success = true,
                    message = $"Transcript deleted successfully for defense session ID: {defenseSessionId}",
                    key
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete transcript for session: {SessionId}", defenseSessionId);
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = $"Failed to delete transcript: {ex.Message}" 
                });
            }
        }
    }

    public class SetTestTranscriptRequest
    {
        public string DefenseSessionId { get; set; } = string.Empty;
        public string? SessionId { get; set; }
        public string? StartTime { get; set; }
        public List<object>? Lines { get; set; }
        public int? TtlSeconds { get; set; }
    }
}
