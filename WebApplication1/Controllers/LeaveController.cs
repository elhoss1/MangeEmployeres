using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LeaveController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Add(Leave leave)
        {
            _context.Leaves.Add(leave);
            _context.SaveChanges();
            return Ok(leave);
        }

        [HttpGet("current")]
        public IActionResult Current()
        {
            var today = DateTime.Today;

            var data = _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.StartDate <= today && l.EndDate >= today)
                .Select(l => new
                {
                    l.Employee.Name,
                    l.StartDate,
                    l.EndDate
                }).ToList();

            return Ok(data);
        }

        [HttpGet("returning-soon")]
        public IActionResult ReturningSoon()
        {
            var today = DateTime.Today;
            var next7 = today.AddDays(7);

            var data = _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.EndDate >= today && l.EndDate <= next7)
                .Select(l => new
                {
                    l.Employee.Name,
                    l.EndDate
                }).ToList();

            return Ok(data);
        }
    }
}
