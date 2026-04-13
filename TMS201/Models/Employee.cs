namespace TMS201.Models
{
    public class Employee
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Designation { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

}
