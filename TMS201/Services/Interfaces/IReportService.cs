using TMS201.Models;

namespace TMS201.Services.Interfaces
{
    public interface IReportService
    {
        byte[] ExportToExcel(List<Ticket> tickets);
        byte[] ExportToPdf(List<Ticket> tickets);
    }
}
