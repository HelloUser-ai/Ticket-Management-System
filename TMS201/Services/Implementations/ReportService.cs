using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Kernel.Geom;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using TMS201.Models;
using TMS201.Services.Interfaces;

namespace TMS201.Services.Implementations
{
    public class ReportService : IReportService
    {
        // ✅ EXCEL: Professional Layout with Text Wrap
        public byte[] ExportToExcel(List<Ticket> tickets)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Tickets List");

                var headers = new[] { "SR", "Ticket No", "Date", "Client", "Plant Name", "Type", "Given By", "Assigned To", "Task Details", "Status", "Updated" };

                // --- HEADER STYLING ---
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F2937"); // Dark Slate
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                int row = 2;
                int sr = 1;

                foreach (var t in tickets)
                {
                    ws.Cell(row, 1).Value = sr;   // instead of t.SerialNo
                    ws.Cell(row, 2).Value = t.TicketNo;
                    ws.Cell(row, 3).Value = t.TicketDate.ToString("dd-MM-yyyy");
                    ws.Cell(row, 4).Value = t.ClientName;
                    ws.Cell(row, 5).Value = t.PlantName;
                    ws.Cell(row, 6).Value = t.PlantType;
                    ws.Cell(row, 7).Value = t.GivenBy;
                    ws.Cell(row, 8).Value = t.AssignedTo;

                    var taskCell = ws.Cell(row, 9);
                    taskCell.Value = t.TaskDetails;
                    taskCell.Style.Alignment.WrapText = true;

                    ws.Cell(row, 10).Value = t.TicketStatus;
                    ws.Cell(row, 11).Value = t.UpdatedDate?.ToString("dd-MM-yyyy") ?? "-";

                    row++;
                    sr++;
                }

                // --- COLUMN & ROW FORMATTING ---
                ws.Columns().AdjustToContents();
                ws.Column(9).Width = 45; // Task details ko fixed badi width di hai
                ws.Rows().AdjustToContents(); // Auto height for wrapped text

                var fullRange = ws.Range(1, 1, row - 1, headers.Length);
                fullRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                fullRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        // ✅ PDF: Landscape + Auto-Wrapping + Safe Fonts
        public byte[] ExportToPdf(List<Ticket> tickets)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
                var pdf = new PdfDocument(writer);
                var doc = new Document(pdf, PageSize.A4.Rotate()); // Landscape mode
                doc.SetMargins(15, 15, 15, 15);

                // Safe Bold Fonts (Solving your SetBold error)
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont regFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Title
                doc.Add(new Paragraph("TICKET MANAGEMENT SYSTEM - REPORT")
                    .SetFont(boldFont)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16)
                    .SetFontColor(ColorConstants.DARK_GRAY));

                doc.Add(new Paragraph($"Generated on: {DateTime.Now:dd MMM yyyy HH:mm}")
                    .SetFontSize(9)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetFontColor(ColorConstants.GRAY));

                // Column Ratios: Task Details (Col 9) gets 28% width
                float[] columnWidths = { 3, 10, 8, 10, 10, 5, 8, 8, 28, 7, 5 };
                Table table = new Table(UnitValue.CreatePercentArray(columnWidths)).UseAllAvailableWidth();

                string[] headers = { "SR", "T-No", "Date", "Client", "Plant", "Type", "Given", "Assign", "Task Details", "Status", "Updated" };

                // Header Generation
                foreach (var h in headers)
                {
                    table.AddHeaderCell(new Cell()
                        .Add(new Paragraph(h).SetFont(boldFont).SetFontSize(9).SetFontColor(ColorConstants.WHITE))
                        .SetBackgroundColor(new DeviceRgb(31, 41, 55))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetPadding(5));
                }

                // Data Rows
                int sr = 1;

                foreach (var t in tickets)
                {
                    table.AddCell(CreatePdfCell(sr.ToString(), regFont, TextAlignment.CENTER));
                    table.AddCell(CreatePdfCell(t.TicketNo, regFont, TextAlignment.CENTER));
                    table.AddCell(CreatePdfCell(t.TicketDate.ToString("dd/MM/yy"), regFont, TextAlignment.CENTER));
                    table.AddCell(CreatePdfCell(t.ClientName, regFont, TextAlignment.LEFT));
                    table.AddCell(CreatePdfCell(t.PlantName, regFont, TextAlignment.LEFT));
                    table.AddCell(CreatePdfCell(t.PlantType, regFont, TextAlignment.CENTER));
                    table.AddCell(CreatePdfCell(t.GivenBy, regFont, TextAlignment.LEFT));
                    table.AddCell(CreatePdfCell(t.AssignedTo, regFont, TextAlignment.LEFT));

                    table.AddCell(new Cell().Add(new Paragraph(t.TaskDetails ?? "-")
                        .SetFont(regFont)
                        .SetFontSize(8)));

                    table.AddCell(CreatePdfCell(t.TicketStatus, regFont, TextAlignment.CENTER));
                    table.AddCell(CreatePdfCell(t.UpdatedDate?.ToString("dd/MM/yy") ?? "-", regFont, TextAlignment.CENTER));

                    sr++;
                }

                doc.Add(table);
                doc.Close();
                return ms.ToArray();
            }
        }

        // Helper method to keep code clean
        private Cell CreatePdfCell(string text, PdfFont font, TextAlignment align)
        {
            return new Cell().Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(8))
                .SetPadding(3)
                .SetTextAlignment(align)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }
    }
}