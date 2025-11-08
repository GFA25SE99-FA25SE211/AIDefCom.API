using AIDefCom.Service.Dto.TranscriptAnalysis;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.TranscriptAnalysisService
{
    public interface ITranscriptAnalysisService
    {
        Task<TranscriptAnalysisResponseDto> AnalyzeTranscriptAsync(TranscriptAnalysisRequestDto request);
    }
}
