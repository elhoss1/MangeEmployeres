using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Payroll
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!; // non-nullable EF navigation property

        [Required]
        public double BasicSalary { get; set; }

        [Required]
        public double Deductions { get; set; }

        [Required]
        public double NetSalary { get; set; }

        [Required]
        public int AbsentDays { get; set; }

        [Required]
        public DateTime Month { get; set; }
    }
}