using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InsuranceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InsuranceController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Add(Insurance insurance)
        {
            _context.Insurances.Add(insurance);
            _context.SaveChanges();
            return Ok(insurance);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var data = _context.Insurances
                .Include(i => i.Employee)
                .Select(i => new
                {
                    i.Id,
                    i.CompanyName,
                    i.StartDate,
                    i.ExpiryDate,
                    EmployeeName = i.Employee.Name
                }).ToList();

            return Ok(data);
        }

        [HttpGet("expiring-soon")]
        public IActionResult ExpiringSoon()
        {
            var today = DateTime.Today;
            var next30 = today.AddDays(30);

            var data = _context.Insurances
                .Where(i => i.ExpiryDate >= today && i.ExpiryDate <= next30)
                .Select(i => new
                {
                    i.Employee.Name,
                    i.ExpiryDate
                }).ToList();

            return Ok(data);
        }
    }
}
