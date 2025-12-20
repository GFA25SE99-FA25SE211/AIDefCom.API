using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.DefenseReport;
using AIDefCom.Service.Services.DefenseReportService;
using AIDefCom.Service.Services.FileStorageService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for generating defense reports (Biên b?n b?o v?)
    /// </summary>
    [Route("api/defense-reports")]
    [ApiController]
    [Authorize] // Tất cả endpoints yêu cầu authenticated
    public class DefenseReportsController : ControllerBase
    {
        private readonly IDefenseReportService _reportService;
        private readonly ILogger<DefenseReportsController> _logger;
        private readonly IFileStorageService _fileStorage; // Added

        public DefenseReportsController(
            IDefenseReportService reportService,
            ILogger<DefenseReportsController> logger,
            IFileStorageService fileStorage) // Injected
        {
            _reportService = reportService;
            _logger = logger;
            _fileStorage = fileStorage;
        }

        /// <summary>
        /// Generate defense report from defense session ID
        /// </summary>
        /// <param name="request">Request containing defense session ID</param>
        /// <returns>Defense report with council info, session info, project info, and AI-analyzed defense progress</returns>
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateDefenseReport([FromBody] DefenseReportRequestDto request)
        {
            _logger.LogInformation("?? Generating defense report for defense session ID: {DefenseSessionId}", request.DefenseSessionId);
            
            var result = await _reportService.GenerateDefenseReportAsync(request);
            
            _logger.LogInformation("? Defense report generated successfully for defense session ID: {DefenseSessionId}", request.DefenseSessionId);

            return Ok(new ApiResponse<DefenseReportResponseDto>
            {
                Code = ResponseCodes.Success,
                Message = "Defense report generated successfully",
                Data = result
            });
        }

        /// <summary>
        /// Upload a defense report file (PDF or Word) to Azure Blob Storage (Admin and Lecturer)
        /// </summary>
        /// <param name="file">Document file (.pdf, .doc, .docx)</param>
        /// <returns>File information with download URL</returns>
        [Authorize(Roles = "Admin,Lecturer")]
        [HttpPost("upload-pdf")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<FileUploadResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadPdf(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Code = ResponseCodes.BadRequest,
                        Message = "File is required",
                        Data = string.Empty
                    });
                }

                var fileUrl = await _fileStorage.UploadPdfAsync(file);
                var downloadUrl = await _fileStorage.GetDownloadUrlAsync(fileUrl, 60);
                
                var response = new FileUploadResponseDto
                {
                    FileUrl = fileUrl,
                    DownloadUrl = downloadUrl,
                    ExpiryMinutes = 60,
                    FileName = file.FileName
                };

                return Ok(new ApiResponse<FileUploadResponseDto>
                {
                    Code = ResponseCodes.Success,
                    Message = "Document uploaded successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload document");
                return BadRequest(new ApiResponse<string>
                {
                    Code = ResponseCodes.BadRequest,
                    Message = ex.Message,
                    Data = string.Empty
                });
            }
        }

        /// <summary>
        /// Get a downloadable URL for an uploaded defense report document
        /// </summary>
        /// <param name="blobUrl">The full URL of the uploaded document</param>
        /// <param name="expiryMinutes">How long the download link should be valid (default: 60 minutes)</param>
        /// <returns>Temporary download URL with SAS token</returns>
        [HttpGet("download")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDownloadUrl([FromQuery] string blobUrl, [FromQuery] int expiryMinutes = 60)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blobUrl))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Code = ResponseCodes.BadRequest,
                        Message = "Blob URL is required",
                        Data = string.Empty
                    });
                }

                var downloadUrl = await _fileStorage.GetDownloadUrlAsync(blobUrl, expiryMinutes);
                
                return Ok(new ApiResponse<string>
                {
                    Code = ResponseCodes.Success,
                    Message = $"Download URL generated successfully (valid for {expiryMinutes} minutes)",
                    Data = downloadUrl
                });
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "File not found: {BlobUrl}", blobUrl);
                return NotFound(new ApiResponse<string>
                {
                    Code = ResponseCodes.NotFound,
                    Message = "File not found",
                    Data = string.Empty
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate download URL");
                return BadRequest(new ApiResponse<string>
                {
                    Code = ResponseCodes.BadRequest,
                    Message = ex.Message,
                    Data = string.Empty
                });
            }
        }
    }
}
