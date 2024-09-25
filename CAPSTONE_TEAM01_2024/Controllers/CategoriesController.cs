using CAPSTONE_TEAM01_2024.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class CategoriesController:Controller
    {
// Database
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }
//EndSemesterReport actions
        public IActionResult EndSemesterReport()
        {
            ViewData["page"] = "EndSemesterReport";
            return View();
        }
//SchoolYear actions
        
        }
//SemesterPlan actions
    }


