using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Youdemy.Data;
using Youdemy.Models;

namespace Youdemy.Controllers
{
    public class HomeController : Controller
    {
        private readonly YoudemyDbContext _context;

        public HomeController(YoudemyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search = null)
        {
            var query = _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(c => c.Title.ToLower().Contains(lowerSearch) || 
                                         c.Description.ToLower().Contains(lowerSearch));
            }

            var courses = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            ViewData["Search"] = search;
            return View(courses);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
