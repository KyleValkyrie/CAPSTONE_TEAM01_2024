using CAPSTONE_TEAM01_2024.Models;
using Microsoft.AspNetCore.Mvc;

namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class ProfileManagerController: Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileManagerController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult ProfileManager()
        {
            var listprofilemanager = _context.ProfileManagers.ToList();
            ViewData["page"] = "ProfileManager";

            return View(listprofilemanager);
        }
      
        [Route("AddProfileManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProfileManager(ProfileManagerModel profile)
        {
            if (ModelState.IsValid)
            {
                _context.ProfileManagers.Add(profile);
                _context.SaveChanges();
                return RedirectToAction("ProfileManager");
            }
            return View(profile);
        }
    }
}
