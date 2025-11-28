using AIDefCom.Service.Dto.Score;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.ScoreNotification
{
    /// <summary>
    /// Interface for sending real-time score notifications
    /// </summary>
    public interface IScoreNotificationService
    {
        Task NotifyScoreCreated(ScoreReadDto score);
        Task NotifyScoreUpdated(ScoreReadDto score);
        Task NotifyScoreDeleted(ScoreReadDto score);
    }
}
