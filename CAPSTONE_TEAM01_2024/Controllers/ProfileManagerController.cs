using CAPSTONE_TEAM01_2024.Models;
using CAPSTONE_TEAM01_2024.Utilities;
using CAPSTONE_TEAM01_2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
			var users = await _context.Users.Select(u => new SelectListItem
			{
				Value = u.Id,
				Text = u.Email
			}).ToListAsync();
			var viewmodel = new ProfileManagerViewModel
			{
				ProfileManagerModel = new ProfileManagerModel(),
				UserEmail = users,
				PaginatedProfile = paginatedProfile,

			};
		
            return View(viewmodel);
        }

        public async Task<IActionResult> Create()
        {
			var emails = await _context.Users
					.Select(u => new SelectListItem
					{
						Value = u.Id,
						Text = u.Email
					})
					.ToListAsync();

			var viewModel = new ProfileManagerViewModel
			{
				UserEmail = emails
			};

			return PartialView("AddProfileManager", viewModel);
		}

        // Save New ProfileManager to database and return Index_ProfileManager
        [HttpPost]

        public async Task <IActionResult> Create(ProfileManagerViewModel model)
        {

			/*bool isFound = _context.ProfileManagers.Where(x => x.Email == model.ProfileManagerModel.Email && x.MaSo == model.ProfileManagerModel.MaSo && x.SoDienThoai == model.ProfileManagerModel.SoDienThoai && x.VaiTro == model.ProfileManagerModel.VaiTro).Any();*/
			bool isFound = _context.ProfileManagers
						   .Any(x => x.UserId == model.ProfileManagerModel.UserId &&
                                     x.Email == model.ProfileManagerModel.Email &&
									 x.MaSo == model.ProfileManagerModel.MaSo &&
									 x.SoDienThoai == model.ProfileManagerModel.SoDienThoai &&
									 x.VaiTro == model.ProfileManagerModel.VaiTro);
			if (!isFound)
			{
				var profile = new ProfileManagerModel
				{
					UserId = model.SelectedEmail,
					Email = _context.Users.FirstOrDefault(u => u.Id == model.SelectedEmail)?.Email,
					MaSo = model.ProfileManagerModel.MaSo,
					TenDayDu = model.ProfileManagerModel.TenDayDu,
					SoDienThoai = model.ProfileManagerModel.SoDienThoai,
					VaiTro = model.ProfileManagerModel.VaiTro
				};

				_context.ProfileManagers.Add(profile);
				await _context.SaveChangesAsync();

				TempData["Message"] = "Thêm Tài Khoản Thành Công!";
				TempData["MessageType"] = "success";
				return RedirectToAction("Index_ProfileManager");
			}
			else
			{
				TempData["Message"] = "Email hoặc Mã Số hoặc Số Điện Thoại Đã Tồn Tại!";
				TempData["MessageType"] = "danger";
				return RedirectToAction("Index_ProfileManager");
			}
			//if (!isFound)
			//         {
			//             _context.ProfileManagers.Add(model);
			//             _context.SaveChanges();rt

			//             TempData["Message"] = "Thêm Tài Khoản Thành Công !";
			//             TempData["MessageType"] = "success";
			//             return RedirectToAction("Index_ProfileManager", "ProfileManager");
			//         }
			//         else
			//         {
			//             TempData["Message"] = "Email hoặc Mã Số hoặc Số Điện Thoại Đã Tồn Tại !";
			//             TempData["MessageType"] = "danger";
			//             return RedirectToAction("Index_ProfileManager", "ProfileManager");
			//         }

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
