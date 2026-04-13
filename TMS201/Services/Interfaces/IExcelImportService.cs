using Microsoft.AspNetCore.Http;
using TMS201.Models;

public interface IExcelImportService
{
    Task<(bool success, string message, List<Ticket> data)> ImportTickets(IFormFile? file);
}
