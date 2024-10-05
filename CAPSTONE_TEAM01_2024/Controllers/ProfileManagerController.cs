using CAPSTONE_TEAM01_2024.Models;
using CAPSTONE_TEAM01_2024.Utilities;
using CAPSTONE_TEAM01_2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class ProfileManagerController: Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        //public IActionResult Index_ProfileManager()
        //{
        //    var listprofilemanager = _context.ProfileManagers.ToList();
        //    ViewData["page"] = "Index_ProfileManager";

        //    return View(listprofilemanager);
        //}
        public async Task<IActionResult> Index_ProfileManager(int pageNumber = 1, int pageSize = 10)
        {
			ViewData["page"] = "Index_ProfileManager";
			var periods = _context.ProfileManagers.AsQueryable();

            var paginatedProfile = await PaginatedList<ProfileManagerModel>.CreateAsync(periods.OrderBy(p => p.Email + p.MaSo + p.TenDayDu + p.SoDienThoai + p.VaiTro), pageNumber, pageSize);

            var model = new ProfileManagerViewModel
            {
                PaginatedProfile = paginatedProfile,
            };
            return View(model);
        }

        public IActionResult Create()
        {
            ProfileManagerModel model = new ProfileManagerModel(); 
            return PartialView("AddProfileManager",model); 
        }

        // Save New ProfileManager to database and return Index_ProfileManager
        [HttpPost]
      
        public IActionResult Create(ProfileManagerModel model)
        {

           bool isFound = _context.ProfileManagers.Where(x => x.Email == model.Email && x.MaSo == model.MaSo && x.SoDienThoai == model.SoDienThoai && x.VaiTro == model.VaiTro ).Any();
            if(!isFound) {
                _context.ProfileManagers.Add(model);
                _context.SaveChanges();

                TempData["Message"] = "Thêm Tài Khoản Thành Công !";
                TempData["MessageType"] = "success";
                return RedirectToAction("Index_ProfileManager", "ProfileManager");
            }
            else
            {
                TempData["Message"] = "Email hoặc Mã Số hoặc Số Điện Thoại Đã Tồn Tại !";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index_ProfileManager", "ProfileManager");
            }
             
            
           
        }

        // => Edit ProfileManager
        // Find Id when seclect in Index_ProfileManager  and Display ModalPopup Edit for item selected
        public IActionResult Edit(int id) 
        {
            ProfileManagerModel model = _context.ProfileManagers.Where(p=> p.Id == id).FirstOrDefault();
            return PartialView("EditProfileManager", model);
        }
        // Save  Item edited to database and return Index_ProfileManager
        [HttpPost]
        public IActionResult Edit(ProfileManagerModel model) 
        {
            bool isFound = _context.ProfileManagers.Where(x => x.Email == model.Email && x.MaSo == model.MaSo && x.SoDienThoai == model.SoDienThoai && x.VaiTro == model.VaiTro).Any();
            if (!isFound)
            {
                _context.ProfileManagers.Update(model);
                _context.SaveChanges();

                TempData["Message"] = "Chỉnh Sửa Thành Công!";
                TempData["MessageType"] = "success";
                return RedirectToAction("Index_ProfileManager", "ProfileManager");
            }
            else
            {
                TempData["Message"] = "Email hoặc Mã Số hoặc Số Điện Thoại Đã Tồn Tại";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index_ProfileManager", "ProfileManager");
            }
            
            
            return RedirectToAction("Index_ProfileManager");
        }
        // => Delete ProfileManager
        // Find Id when seclect in Index_ProfileManager  and Display ModalPopup Deleted for item selected
        public IActionResult Delete(int id) 
        {
            ProfileManagerModel model = _context.ProfileManagers.Where(p => p.Id == id).FirstOrDefault();
            return PartialView("DeleteProfileManager", model);
        }

        // Delete Item  out database and return Index_ProfileManager
        [HttpPost]
        public IActionResult Delete(ProfileManagerModel model)
        {
            _context.ProfileManagers.Remove(model);
            _context.SaveChanges();
            return RedirectToAction("Index_ProfileManager");
        }

        public IActionResult Detail(int id)
        {
            ProfileManagerModel model = _context.ProfileManagers.Where(p => p.Id == id).FirstOrDefault();
            return PartialView("DetailProfileManager", model);
        }

    }
}
