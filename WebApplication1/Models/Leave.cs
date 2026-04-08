namespace WebApplication1.Models
{
    public class Leave
    {
        public int Id { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Type { get; set; } = "Annual";

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}
