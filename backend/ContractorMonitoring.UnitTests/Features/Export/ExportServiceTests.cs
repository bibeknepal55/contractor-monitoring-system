using ClosedXML.Excel;
using FluentAssertions;
using Moq;
using Xunit;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace ContractorMonitoring.UnitTests.Features.Export;

public class ExportServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ILogger<ExportService>> _logger = new();

    private ExportService CreateService() => new(_uow.Object, _logger.Object);

    private record TestRow(string Name, int Value, DateTime Date);

    [Fact]
    public async Task ExportToExcel_WithData_ReturnsValidXlsx()
    {
        var data = new List<TestRow>
        {
            new("Alpha", 10, new DateTime(2024, 1, 1)),
            new("Beta",  20, new DateTime(2024, 6, 1))
        };

        var bytes = await CreateService().ExportToExcel(data, "TestSheet");

        bytes.Should().NotBeNullOrEmpty();

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheets.First();
        ws.Name.Should().Be("TestSheet");

        // Header row
        ws.Cell(1, 1).Value.ToString().Should().Be("Name");
        ws.Cell(1, 2).Value.ToString().Should().Be("Value");

        // Data rows
        ws.Cell(2, 1).Value.ToString().Should().Be("Alpha");
        ws.Cell(3, 1).Value.ToString().Should().Be("Beta");
    }

    [Fact]
    public async Task ExportToExcel_EmptyData_ReturnsNoDataMessage()
    {
        var bytes = await CreateService().ExportToExcel(new List<TestRow>(), "Empty");

        bytes.Should().NotBeNullOrEmpty();
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        wb.Worksheets.First().Cell(1, 1).Value.ToString().Should().Be("No data available");
    }

    [Fact]
    public async Task ExportToExcel_SheetNameOver31Chars_TruncatesName()
    {
        var data = new List<TestRow> { new("X", 1, DateTime.UtcNow) };
        var longName = new string('A', 40);

        var bytes = await CreateService().ExportToExcel(data, longName);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        wb.Worksheets.First().Name.Length.Should().BeLessOrEqualTo(31);
    }

    [Fact]
    public async Task ExportToPdf_WithData_ReturnsHtmlBytes()
    {
        var data = new List<TestRow> { new("Row1", 99, DateTime.UtcNow) };

        var bytes = await CreateService().ExportToPdf(data, "My Report");

        var html = System.Text.Encoding.UTF8.GetString(bytes);
        html.Should().Contain("My Report");
        html.Should().Contain("Row1");
        html.Should().Contain("<table");
    }
}
