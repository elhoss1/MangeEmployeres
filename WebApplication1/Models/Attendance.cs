namespace WebApplication1.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }

        public bool IsAbsent { get; set; } // ✅ غياب بس

        public Employee Employee { get; set; }
    }
}
