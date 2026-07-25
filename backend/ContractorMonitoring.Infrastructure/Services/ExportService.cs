using System.Text;
using ClosedXML.Excel;
using ContractorMonitoring.Application.DTOs.Export;
using ContractorMonitoring.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ContractorMonitoring.Infrastructure.Services;

public class ExportService : IExportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExportService> _logger;

    public ExportService(IUnitOfWork unitOfWork, ILogger<ExportService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<byte[]> ExportToExcel<T>(List<T> data, string sheetName)
    {
        return await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();

            // Explicitly set a safe font — prevents ClosedXML from falling back to
            // GDI+ font metrics (which corrupt rendering on Linux/Docker without libgdiplus)
            workbook.Style.Font.FontName = "Calibri";
            workbook.Style.Font.FontSize = 11;

            var sheet = workbook.Worksheets.Add(sheetName.Length > 31 ? sheetName[..31] : sheetName);

            if (data == null || data.Count == 0)
            {
                sheet.Cell(1, 1).Value = "No data available";
                using var emptyStream = new MemoryStream();
                workbook.SaveAs(emptyStream);
                return emptyStream.ToArray();
            }

            var properties = typeof(T).GetProperties();

            // Header row — bold + background
            for (int col = 0; col < properties.Length; col++)
            {
                var cell = sheet.Cell(1, col + 1);
                cell.Value = properties[col].Name;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontName = "Calibri";
                cell.Style.Font.FontSize = 11;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E79");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Data rows
            for (int row = 0; row < data.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    var value = properties[col].GetValue(data[row]);
                    var cell = sheet.Cell(row + 2, col + 1);
                    cell.Style.Font.FontName = "Calibri";
                    cell.Style.Font.FontSize = 11;

                    cell.Value = value switch
                    {
                        null => XLCellValue.FromObject(""),
                        DateTime dt => XLCellValue.FromObject(dt.ToString("yyyy-MM-dd HH:mm:ss")),
                        decimal d => XLCellValue.FromObject(d),
                        double dbl => XLCellValue.FromObject(dbl),
                        int i => XLCellValue.FromObject(i),
                        bool b => XLCellValue.FromObject(b ? "Yes" : "No"),
                        _ => XLCellValue.FromObject(value.ToString() ?? "")
                    };

                    // Alternate row shading
                    if (row % 2 == 1)
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                }
            }

            // Set fixed column widths instead of AdjustToContents() which calls GDI+ font
            // metrics and corrupts rendering on Linux/Docker without libgdiplus
            foreach (var col in sheet.ColumnsUsed())
                col.Width = 20;

            sheet.SheetView.FreezeRows(1);
            sheet.RangeUsed()?.SetAutoFilter();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        });
    }

    public async Task<byte[]> ExportToPdf<T>(List<T> data, string title)
    {
        // HTML-based PDF stub — replace with DinkToPdf/IronPDF in production
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'>");
        sb.AppendLine($"<title>{title}</title>");
        sb.AppendLine("<style>body{font-family:Arial;margin:20px;} table{border-collapse:collapse;width:100%;} th,td{border:1px solid #ddd;padding:8px;text-align:left;} th{background-color:#1F4E79;color:white;} tr:nth-child(even){background:#f2f2f2;}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine($"<h2>{title}</h2>");
        sb.AppendLine($"<p>Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>");

        if (data != null && data.Count > 0)
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
                    sb.AppendLine($"<td>{prop.GetValue(item)}</td>");
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
            Budget = p.Budget,
            p.Location,
            StartDate = p.StartDate.ToString("yyyy-MM-dd"),
            EndDate = p.EndDate?.ToString("yyyy-MM-dd") ?? "",
            Progress = p.ProgressPercentage ?? 0
        }).ToList();

        return request.Format.ToLower() switch
        {
            "excel" or "xlsx" => await ExportToExcel(data, request.ReportType),
            "pdf" => await ExportToPdf(data, request.ReportType),
            _ => throw new ArgumentException($"Invalid export format: {request.Format}")
        };
    }
}
