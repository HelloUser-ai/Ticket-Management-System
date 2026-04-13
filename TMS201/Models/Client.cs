using System.ComponentModel.DataAnnotations;

namespace TMS201.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Client Name is mandatory")]
        public string Name { get; set; }

        // "?" signifies nullable (optional)
        public string? ContactPerson { get; set; }

        public string? Email { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}