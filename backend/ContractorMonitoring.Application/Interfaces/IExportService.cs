using ContractorMonitoring.Application.DTOs.Export;

namespace ContractorMonitoring.Application.Interfaces;

// Export service interface for PDF and Excel generation
public interface IExportService
{
    Task<byte[]> ExportToExcel<T>(List<T> data, string sheetName);
    Task<byte[]> ExportToPdf<T>(List<T> data, string title);
    Task<byte[]> GenerateReportExport(ExportRequestDto request, Guid tenantId);
}