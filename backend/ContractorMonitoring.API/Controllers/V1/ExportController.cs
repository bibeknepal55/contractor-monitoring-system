using Asp.Versioning;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Export;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContractorMonitoring.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/export")]
[ApiController]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;

    public ExportController(IExportService exportService)
    {
        _exportService = exportService;
    }

    [HttpPost("generate")]
    [Authorize(Policy = Permissions.Reports.Export)]
    public async Task<IActionResult> Export([FromBody] ExportRequestDto request)
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? Guid.Empty.ToString());

        var fileBytes = await _exportService.GenerateReportExport(request, tenantId);

        var fileName = $"{request.ReportType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{request.Format}";
        var contentType = request.Format.ToLower() switch
        {
            "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "pdf" => "application/pdf",
            _ => "application/octet-stream"
        };

        return File(fileBytes, contentType, fileName);
    }
}