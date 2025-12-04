using AIDefCom.Service.Dto.DefenseReport;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.DefenseReportService
{
    public interface IDefenseReportService
    {
        /// <summary>
        /// Generate defense report from defense session ID
        /// </summary>
        Task<DefenseReportResponseDto> GenerateDefenseReportAsync(DefenseReportRequestDto request);
    }
}
