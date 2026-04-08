using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }
        public DateTime HireDate { get; set; }

        public DateTime IqamaExpiryDate { get; set; }

        public double Salary { get; set; }
        public int VacationDays { get; set; }

        // 🔥 الجديد
        [Required]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required]
        public string PassportNumber { get; set; } = string.Empty;

        [Required]
        public string ResidenceNumber { get; set; } = string.Empty;

        // 🔗 علاقات
        public List<Leave> Leaves { get; set; } = new();
        public List<Insurance> Insurances { get; set; } = new();
    }
}
