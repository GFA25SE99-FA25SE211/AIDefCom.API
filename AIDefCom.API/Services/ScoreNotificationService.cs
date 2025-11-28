using AIDefCom.API.Hubs;
using AIDefCom.Service.Dto.Score;
using AIDefCom.Service.Services.ScoreNotification;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AIDefCom.API.Services
{
    /// <summary>
    /// Implementation of IScoreNotificationService using SignalR
    /// </summary>
    public class ScoreNotificationService : IScoreNotificationService
    {
        private readonly IHubContext<ScoreHub> _hubContext;
        private readonly ILogger<ScoreNotificationService> _logger;

        public ScoreNotificationService(IHubContext<ScoreHub> hubContext, ILogger<ScoreNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyScoreCreated(ScoreReadDto score)
        {
            _logger.LogInformation("Sending ScoreCreated notification for score {ScoreId}", score.Id);

            // Notify all subscribers
            await _hubContext.Clients.Group("all_scores").SendAsync("ScoreCreated", score);

            // Notify session-specific subscribers
            await _hubContext.Clients.Group($"session_{score.SessionId}").SendAsync("ScoreCreated", score);

            // Notify student-specific subscribers
            await _hubContext.Clients.Group($"student_{score.StudentId}").SendAsync("ScoreCreated", score);

            // Notify evaluator-specific subscribers
            await _hubContext.Clients.Group($"evaluator_{score.EvaluatorId}").SendAsync("ScoreCreated", score);
        }

        public async Task NotifyScoreUpdated(ScoreReadDto score)
        {
            _logger.LogInformation("Sending ScoreUpdated notification for score {ScoreId}", score.Id);

            // Notify all subscribers
            await _hubContext.Clients.Group("all_scores").SendAsync("ScoreUpdated", score);

            // Notify session-specific subscribers
            await _hubContext.Clients.Group($"session_{score.SessionId}").SendAsync("ScoreUpdated", score);

            // Notify student-specific subscribers
            await _hubContext.Clients.Group($"student_{score.StudentId}").SendAsync("ScoreUpdated", score);

            // Notify evaluator-specific subscribers
            await _hubContext.Clients.Group($"evaluator_{score.EvaluatorId}").SendAsync("ScoreUpdated", score);
        }

        public async Task NotifyScoreDeleted(ScoreReadDto score)
        {
            _logger.LogInformation("Sending ScoreDeleted notification for score {ScoreId}", score.Id);

            // Notify all subscribers
            await _hubContext.Clients.Group("all_scores").SendAsync("ScoreDeleted", score);

            // Notify session-specific subscribers
            await _hubContext.Clients.Group($"session_{score.SessionId}").SendAsync("ScoreDeleted", score);

            // Notify student-specific subscribers
            await _hubContext.Clients.Group($"student_{score.StudentId}").SendAsync("ScoreDeleted", score);

            // Notify evaluator-specific subscribers
            await _hubContext.Clients.Group($"evaluator_{score.EvaluatorId}").SendAsync("ScoreDeleted", score);
        }
    }
}
