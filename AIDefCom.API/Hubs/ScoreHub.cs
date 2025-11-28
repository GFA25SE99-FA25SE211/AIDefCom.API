using Microsoft.AspNetCore.SignalR;

namespace AIDefCom.API.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time score updates
    /// </summary>
    public class ScoreHub : Hub
    {
        private readonly ILogger<ScoreHub> _logger;

        public ScoreHub(ILogger<ScoreHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Subscribe to score updates for a specific session
        /// </summary>
        public async Task SubscribeToSession(int sessionId)
        {
            var groupName = $"session_{sessionId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} subscribed to session {SessionId}", 
                Context.ConnectionId, sessionId);
        }

        /// <summary>
        /// Unsubscribe from score updates for a specific session
        /// </summary>
        public async Task UnsubscribeFromSession(int sessionId)
        {
            var groupName = $"session_{sessionId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} unsubscribed from session {SessionId}", 
                Context.ConnectionId, sessionId);
        }

        /// <summary>
        /// Subscribe to score updates for a specific student
        /// </summary>
        public async Task SubscribeToStudent(string studentId)
        {
            var groupName = $"student_{studentId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} subscribed to student {StudentId}", 
                Context.ConnectionId, studentId);
        }

        /// <summary>
        /// Unsubscribe from score updates for a specific student
        /// </summary>
        public async Task UnsubscribeFromStudent(string studentId)
        {
            var groupName = $"student_{studentId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} unsubscribed from student {StudentId}", 
                Context.ConnectionId, studentId);
        }

        /// <summary>
        /// Subscribe to score updates for a specific evaluator
        /// </summary>
        public async Task SubscribeToEvaluator(string evaluatorId)
        {
            var groupName = $"evaluator_{evaluatorId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} subscribed to evaluator {EvaluatorId}", 
                Context.ConnectionId, evaluatorId);
        }

        /// <summary>
        /// Unsubscribe from score updates for a specific evaluator
        /// </summary>
        public async Task UnsubscribeFromEvaluator(string evaluatorId)
        {
            var groupName = $"evaluator_{evaluatorId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} unsubscribed from evaluator {EvaluatorId}", 
                Context.ConnectionId, evaluatorId);
        }

        /// <summary>
        /// Subscribe to all score updates (admin/monitoring)
        /// </summary>
        public async Task SubscribeToAllScores()
        {
            var groupName = "all_scores";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} subscribed to all scores", Context.ConnectionId);
        }

        /// <summary>
        /// Unsubscribe from all score updates
        /// </summary>
        public async Task UnsubscribeFromAllScores()
        {
            var groupName = "all_scores";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} unsubscribed from all scores", Context.ConnectionId);
        }
    }
}
