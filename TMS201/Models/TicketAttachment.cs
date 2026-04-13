using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMS201.Models
{
    public class TicketAttachment
    {
        [Key]
        public int Id { get; set; }

        public int TicketId { get; set; }

        public string FileName { get; set; } = "";

        public string FilePath { get; set; } = "";

        public string FileType { get; set; } = "";

        public long FileSize { get; set; }

        public DateTime UploadedDate { get; set; }

        public string UploadedBy { get; set; } = "";

        [ForeignKey("TicketId")]
        public Ticket? Ticket { get; set; }
    }
}
