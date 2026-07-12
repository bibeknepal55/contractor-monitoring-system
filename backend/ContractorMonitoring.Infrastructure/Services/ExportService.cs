using System.Text;
using System.Text.Json;
using ContractorMonitoring.Application.DTOs.Export;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Infrastructure.Services;

// Export service for generating Excel and PDF reports
public class ExportService : IExportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<byte[]> ExportToExcel<T>(List<T> data, string sheetName)
    {
        // Simple CSV-based Excel export (for production, use ClosedXML or EPPlus)
        var sb = new StringBuilder();

        if (data == null || !data.Any())
            return Encoding.UTF8.GetBytes("No data available");

        // Header row
        var properties = typeof(T).GetProperties();
        sb.AppendLine(string.Join(",", properties.Select(p => $"\"{p.Name}\"")));

        // Data rows
        foreach (var item in data)
        {
            var values = properties.Select(p => {
                var value = p.GetValue(item);
                return value != null ? $"\"{value.ToString()?.Replace("\"", "\"\"")}\"" : "\"\"";
            });
            sb.AppendLine(string.Join(",", values));
        }

        return await Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    public async Task<byte[]> ExportToPdf<T>(List<T> data, string title)
    {
        // Simple HTML-based PDF (for production, use DinkToPdf or IronPDF)
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'>");
        sb.AppendLine($"<title>{title}</title>");
        sb.AppendLine("<style>body{font-family:Arial;margin:20px;} table{border-collapse:collapse;width:100%;} th,td{border:1px solid #ddd;padding:8px;text-align:left;} th{background-color:#4CAF50;color:white;}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine($"<h2>{title}</h2>");
        sb.AppendLine($"<p>Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>");

        if (data != null && data.Any())
        {
            var properties = typeof(T).GetProperties();
            sb.AppendLine("<table><thead><tr>");
            foreach (var prop in properties)
                sb.AppendLine($"<th>{prop.Name}</th>");
            sb.AppendLine("</tr></thead><tbody>");

            foreach (var item in data)
            {
                sb.AppendLine("<tr>");
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item);
                    sb.AppendLine($"<td>{value}</td>");
                }
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</tbody></table>");
        }

        sb.AppendLine("</body></html>");
        return await Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    public async Task<byte[]> GenerateReportExport(ExportRequestDto request, Guid tenantId)
    {
        var projects = await _unitOfWork.Projects.GetAllAsync();
        var tenantProjects = projects.Where(p => p.TenantId == tenantId).ToList();

        var data = tenantProjects.Select(p => new
        {
            p.ProjectCode,
            p.ProjectName,
            p.Status,
            p.Budget,
            p.Location,
            p.StartDate,
            p.EndDate,
            Progress = p.ProgressPercentage ?? 0
        }).ToList();

        return request.Format.ToLower() switch
        {
            "excel" => await ExportToExcel(data, request.ReportType),
            "pdf" => await ExportToPdf(data, request.ReportType),
            _ => throw new ArgumentException("Invalid export format")
        };
    }
}