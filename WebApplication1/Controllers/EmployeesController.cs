using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

namespace EmployeeSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        // 📋 Get All Employees
        [HttpGet]
        public IActionResult GetAll()
        {
            var employees = _context.Employees.ToList();
            return Ok(employees);
        }

        // 🔍 Get Employee By Id
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var emp = _context.Employees.Find(id);
            if (emp == null) return NotFound("Employee not found");
            return Ok(emp);
        }

        // ➕ Add Employee
        [HttpPost]
        public IActionResult Add(Employee emp)
        {
            if (emp == null) return BadRequest("Invalid data");
            _context.Employees.Add(emp);
            _context.SaveChanges();
            return Ok(emp);
        }

        // ✏️ Update Employee
        [HttpPut("{id}")]
        public IActionResult Update(int id, Employee emp)
        {
            var existing = _context.Employees.Find(id);
            if (existing == null) return NotFound("Employee not found");

            existing.Name = emp.Name;
            existing.BirthDate = emp.BirthDate;
            existing.HireDate = emp.HireDate;
            existing.IqamaExpiryDate = emp.IqamaExpiryDate;
            existing.Salary = emp.Salary;
            existing.VacationDays = emp.VacationDays;

            _context.SaveChanges();
            return Ok(existing);
        }

        // ❌ Delete Employee
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var emp = _context.Employees.Find(id);
            if (emp == null) return NotFound("Employee not found");

            _context.Employees.Remove(emp);
            _context.SaveChanges();
            return Ok(new { message = $"Deleted successfully" });
        }

        // ⚠️ Iqama Alerts (expired or about to expire)
        [HttpGet("alerts/iqama")]
        public IActionResult GetIqamaAlerts()
        {
            var today = DateTime.Today;
            var next30Days = today.AddDays(30);

            var employees = _context.Employees
                .Where(e => e.IqamaExpiryDate < today || (e.IqamaExpiryDate >= today && e.IqamaExpiryDate <= next30Days))
                .OrderBy(e => e.IqamaExpiryDate)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.IqamaExpiryDate,
                    DaysLeft = e.IqamaExpiryDate < today
                        ? (int)(today - e.IqamaExpiryDate).TotalDays
                        : (int)(e.IqamaExpiryDate - today).TotalDays
                })
                .ToList();

            return Ok(employees);
        }

        // 💰 End of Service
        [HttpGet("endofservice/{id}")]
        public IActionResult CalculateEndOfService(int id)
        {
            var emp = _context.Employees.Find(id);
            if (emp == null) return NotFound("Employee not found");
            if (emp.HireDate > DateTime.Today) return BadRequest("Invalid hire date");

            var years = (DateTime.Today - emp.HireDate).TotalDays / 365.25;
            double reward = years <= 5 ? years * (emp.Salary / 2) : (5 * (emp.Salary / 2)) + ((years - 5) * emp.Salary);

            return Ok(new
            {
                emp.Id,
                emp.Name,
                YearsOfService = Math.Round(years, 2),
                emp.Salary,
                EndOfServiceReward = Math.Round(reward, 2)
            });
        }

        // 📂 Upload Attendance Excel
        [HttpPost("upload-attendance")]
        public async Task<IActionResult> UploadAttendance(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                // تعيين الترخيص قبل أي استخدام لمكتبة EPPlus
           

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                // إعادة مؤشر القراءة إلى بداية الملف
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];

                if (worksheet == null)
                    return BadRequest("The Excel file is empty or invalid.");

                int rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount < 2)
                    return BadRequest("No data found in the Excel file.");

                var attendances = new List<Attendance>();

                for (int row = 2; row <= rowCount; row++) // Skip header row
                {
                    if (!int.TryParse(worksheet.Cells[row, 1].Text, out int employeeId))
                    {
                        continue; // Skip row if EmployeeId is invalid
                    }

                    if (!DateTime.TryParse(worksheet.Cells[row, 2].Text, out DateTime date))
                    {
                        continue; // Skip row if AttendanceDate is invalid
                    }

                    if (!bool.TryParse(worksheet.Cells[row, 3].Text, out bool isAbsent))
                    {
                        continue; // Skip row if IsAbsent is invalid
                    }

                    var employee = _context.Employees.Find(employeeId);
                    if (employee == null)
                    {
                        continue; // Skip if employee does not exist
                    }

                    attendances.Add(new Attendance
                    {
                        EmployeeId = employeeId,
                        Date = date,
                        IsAbsent = isAbsent
                    });
                }

                if (!attendances.Any())
                {
                    return BadRequest("No valid attendance data found");
                }

                _context.Attendances.AddRange(attendances);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"{attendances.Count} attendance records uploaded successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while processing the file: {ex.Message}");
            }
        }


        // 🧮 Generate Payroll (only absences considered)
        [HttpPost("generate-payroll")]
        public IActionResult GeneratePayroll(int month, int year)
        {
            var employees = _context.Employees.ToList();
            var payrolls = new List<Payroll>();

            foreach (var emp in employees)
            {
                var attendance = _context.Attendances
                    .Where(a => a.EmployeeId == emp.Id && a.Date.Month == month && a.Date.Year == year)
                    .ToList();

                if (attendance.Count == 0)
                    continue; // Skip employee if no attendance data exists for the given month/year

                int absentDays = attendance.Count(a => a.IsAbsent);
                double dailySalary = emp.Salary / 30;
                double deductions = absentDays * dailySalary;

                var payroll = new Payroll
                {
                    EmployeeId = emp.Id,
                    BasicSalary = emp.Salary,
                    Deductions = deductions,
                    NetSalary = emp.Salary - deductions,
                    AbsentDays = absentDays,
                    Month = new DateTime(year, month, 1)
                };

                payrolls.Add(payroll);
            }

            if (!payrolls.Any())
            {
                return BadRequest("No valid payroll data found");
            }

            _context.Payrolls.AddRange(payrolls);
            _context.SaveChanges();

            return Ok(new { message = $"Payroll generated successfully." });
        }

        // 📋 Get All Payroll
        [HttpGet("payroll")]
        public IActionResult GetPayroll()
        {
            var data = _context.Payrolls
                .Include(p => p.Employee)
                .ToList();

            return Ok(data);
        }
    }
}