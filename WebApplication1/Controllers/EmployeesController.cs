using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            if (emp == null)
                return NotFound("Employee not found");

            return Ok(emp);
        }

        // ➕ Add Employee
        [HttpPost]
        public IActionResult Add(Employee emp)
        {
            if (emp == null)
                return BadRequest("Invalid data");

            _context.Employees.Add(emp);
            _context.SaveChanges();

            return Ok(emp);
        }

        // ✏️ Update Employee
        [HttpPut("{id}")]
        public IActionResult Update(int id, Employee emp)
        {
            var existing = _context.Employees.Find(id);
            if (existing == null)
                return NotFound("Employee not found");

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
            if (emp == null)
                return NotFound("Employee not found");

            _context.Employees.Remove(emp);
            _context.SaveChanges();

            return Ok("Deleted successfully");
        }

        // ⚠️ Iqama Alerts (مرتبة + دقة أعلى)
        [HttpGet("alerts/iqama")]
        public IActionResult GetIqamaAlerts()
        {
            var today = DateTime.Today;
            var next30Days = today.AddDays(30);

            var employees = _context.Employees
                .Where(e => e.IqamaExpiryDate >= today && e.IqamaExpiryDate <= next30Days)
                .OrderBy(e => e.IqamaExpiryDate)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.IqamaExpiryDate,
                    DaysLeft = (int)(e.IqamaExpiryDate - today).TotalDays
                })
                .ToList();

            return Ok(employees);
        }

        // 💰 End of Service (نسخة احترافية)
        [HttpGet("endofservice/{id}")]
        public IActionResult CalculateEndOfService(int id)
        {
            var emp = _context.Employees.Find(id);

            if (emp == null)
                return NotFound("Employee not found");

            if (emp.HireDate > DateTime.Today)
                return BadRequest("Invalid hire date");

            var totalDays = (DateTime.Today - emp.HireDate).TotalDays;
            var years = totalDays / 365.25; // دقة أعلى

            double reward = 0;

            // 🔥 نظام أقرب للقانون السعودي
            if (years <= 5)
            {
                reward = years * (emp.Salary / 2);
            }
            else
            {
                reward = (5 * (emp.Salary / 2)) + ((years - 5) * emp.Salary);
            }

            return Ok(new
            {
                emp.Id,
                emp.Name,
                YearsOfService = Math.Round(years, 2),
                emp.Salary,
                EndOfServiceReward = Math.Round(reward, 2)
            });
        }



    }
}