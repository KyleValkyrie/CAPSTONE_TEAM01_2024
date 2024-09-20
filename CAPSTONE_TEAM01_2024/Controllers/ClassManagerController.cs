﻿using CAPSTONE_TEAM01_2024.Models;
using CAPSTONE_TEAM01_2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class ClassManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassManagerController(ApplicationDbContext context)
        {
            _context = context;
        }
//for Class view
        //Add new Class
        [HttpPost]
        public async Task<IActionResult> AddClass(int classId, string className, string advisor, int studentCount, int enrollmentNumber, int academicPeriodId)
        {
            // Create a new Class object
            var newClass = new Class
            {
                ClassName = className,
                Advisor = advisor,
                StudentCount = studentCount,
                EnrollmentNumber = enrollmentNumber,
                AcademicPeriodId = academicPeriodId
            };

            // Add the new object to the database
            // Check if the class already exists
            bool isFound = await _context.Classes
                .AnyAsync(r => r.ClassName == newClass.ClassName &&
                               r.Advisor == newClass.Advisor &&
                               r.StudentCount == newClass.StudentCount &&
                               r.EnrollmentNumber == newClass.EnrollmentNumber &&
                               r.AcademicPeriodId == newClass.AcademicPeriodId);
            if (!isFound) //Can add
            {
                _context.Classes.Add(newClass);
                await _context.SaveChangesAsync();

                TempData["MessageClassOverview"] = "Thêm lớp thành công!";
                TempData["MessageType"] = "success";

                // Redirect to the Page to see update
                return RedirectToAction("ClassList", new { id = academicPeriodId });
            }
            else
            {
                TempData["MessageClassOverview"] = "Lớp đã tồn tại!";
                TempData["MessageType"] = "danger";
                return RedirectToAction("ClassList", new { id = academicPeriodId });
            }
        }

        public async Task<IActionResult> ClassList(int id)
        {
            // Fetch the academic period data
            var academicPeriod = await _context.AcademicPeriods.FindAsync(id);

            // Fetch the classes data related to the academic period
            var classes = await _context.Classes
                                        .Where(c => c.AcademicPeriodId == id)
                                        .ToListAsync();

            // Create the ViewModel
            var viewModel = new ClassListViewModel
            {
                AcademicPeriod = academicPeriod,
                Classes = classes
            };

            return View(viewModel);
        }
    }
}