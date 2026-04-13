namespace TMS201.Models
{
    public class TicketBackup
    {
        public int Id { get; set; }
        public int OriginalTicketId { get; set; }

        public int SerialNo { get; set; }
        public string TicketNo { get; set; }
        public DateTime TicketDate { get; set; }
        public string ClientName { get; set; }
        public string PlantType { get; set; }
        public string PlantName { get; set; }
        public string GivenBy { get; set; }
        public string AssignedTo { get; set; }
        public string TaskDetails { get; set; }
        public string TicketStatus { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public string DeletedBy { get; set; }
        public DateTime DeletedDate { get; set; }
    }
}
