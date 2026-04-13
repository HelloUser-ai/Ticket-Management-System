using ClosedXML.Excel;
using TMS201.Models;

public class ExcelImportService : IExcelImportService
{
    public async Task<(bool success, string message, List<Ticket> data)> ImportTickets(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return (false, "File not selected", new List<Ticket>());

        var tickets = new List<Ticket>();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        using var workbook = new XLWorkbook(stream);
        if (!workbook.Worksheets.Any())
            return (false, "Excel file has no worksheet.", new List<Ticket>());

        var sheet = workbook.Worksheet(1);

        // ✅ Dictionary setup: Key = Excel Header, Value = Ticket Model Property Name
        var expectedHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
           
        { "Ticket No", "TicketNo" },
        { "Date", "TicketDate" },
        { "Client", "ClientName" },
        { "Plant", "PlantName" },
        { "Type", "PlantType" },
        { "Given By", "GivenBy" },
        { "Assigned", "AssignedTo" },
        { "Task", "TaskDetails" },
        { "Status Visual", "TicketStatus" },
        { "Created By", "CreatedBy" }
    };

        var actualHeaders = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var firstRow = sheet.FirstRowUsed();
        if (firstRow == null || firstRow.LastCellUsed() == null)
            return (false, "Excel sheet is empty or header row is missing.", new List<Ticket>());

        int colCount = firstRow.LastCellUsed().Address.ColumnNumber;

        for (int col = 1; col <= colCount; col++)
        {
            var header = firstRow.Cell(col).GetString().Trim();
            if (!string.IsNullOrEmpty(header))
                actualHeaders[header] = col;
        }

        // ❌ VALIDATION: Check if mandatory columns exist
        foreach (var header in expectedHeaders.Keys)
        {
            if (!actualHeaders.ContainsKey(header))
                return (false, $"Missing column: {header}", new List<Ticket>());
        }

        // ✅ DATA READ
        var lastRow = sheet.LastRowUsed();
        if (lastRow == null || lastRow.RowNumber() < 2)
            return (false, "No data rows found in Excel.", new List<Ticket>());

        int rowCount = lastRow.RowNumber();

        for (int row = 2; row <= rowCount; row++)
        {
            var ticket = new Ticket();
            var currentRow = sheet.Row(row);

            foreach (var header in expectedHeaders)
            {
                if (!actualHeaders.TryGetValue(header.Key, out int colIndex)) continue;

                var cell = currentRow.Cell(colIndex);
                var value = cell.GetString()?.Trim();

                // Switch on the Value from our dictionary (the property name)
                switch (header.Value)
                {
                    case "TicketNo": ticket.TicketNo = value; break;
                    case "ClientName": ticket.ClientName = value; break;
                    case "PlantName": ticket.PlantName = value; break;
                    case "PlantType": ticket.PlantType = value; break;
                    case "GivenBy": ticket.GivenBy = value; break;
                    case "AssignedTo": ticket.AssignedTo = value; break;
                    case "TaskDetails": ticket.TaskDetails = value; break;
                    case "TicketStatus": ticket.TicketStatus = value; break;
                    case "CreatedBy": ticket.CreatedBy = value; break;
                    case "TicketDate":
                        if (cell.TryGetValue<DateTime>(out DateTime dt))
                            ticket.TicketDate = dt;
                        else
                            ticket.TicketDate = DateTime.Now;
                        break;
                }
            }

            // Default values
            if (string.IsNullOrEmpty(ticket.TicketStatus)) ticket.TicketStatus = "Pending";
            ticket.CreatedDate = DateTime.Now;
            ticket.UpdatedDate = DateTime.Now;

            tickets.Add(ticket);
        }

        return (true, "Excel processed successfully", tickets);
    }
}
