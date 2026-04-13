using System;
using System.ComponentModel.DataAnnotations;

namespace TMS201.Models
{
    public class Ticket
    {
        public int Id { get; set; }


        public int SerialNo { get; set; }

        public string TicketNo { get; set; } = "";

        [Required]
        public DateTime TicketDate { get; set; }

        [Required]
        public string ClientName { get; set; } = "";

        [Required]
        public string PlantName { get; set; } = "";

        [Required]
        public string PlantType { get; set; } = "";

        public string GivenBy { get; set; } = "";
        public string AssignedTo { get; set; } = "";

        public string TaskDetails { get; set; } = "";

        [Required]
        public string TicketStatus { get; set; } = "";

        // 🔹 Use SRN instead of SerialNo
        //public string SRN { get; set; } = "";          // Unique serial for ticket

        // 🔹 Optional backup / tracking fields
        public string CreatedBy { get; set; } = "";    // User who created
        public DateTime? CreatedDate { get; set; }     // When created
        public string UpdatedBy { get; set; } = "";    // Last updated by
        public DateTime? UpdatedDate { get; set; }     // Last updated on

        public ICollection<TicketAttachment>? Attachments { get; set; }
    }
}