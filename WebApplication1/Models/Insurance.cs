namespace WebApplication1.Models
{
    public class Insurance
    {
        public int Id { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        // 👇 ده اللي انت عايزه
        public DateTime ExpiryDate { get; set; }

        // 🔗 علاقة
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}
