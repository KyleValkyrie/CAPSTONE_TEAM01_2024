using CAPSTONE_TEAM01_2024.Models;
using CAPSTONE_TEAM01_2024.Utilities;
using CAPSTONE_TEAM01_2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Drawing.Printing;

namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class CategoriesController : Controller
    {
// Database Context
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
//ClassList actions
    //Render ClassList view
        public async Task<IActionResult> ClassList(int pageNumber = 1, int pageSize = 10)
        {
			ViewData["page"] = "ClassList";
			var classes = _context.Classes.AsQueryable();

            var paginatedClasses = await PaginatedList<Class>.CreateAsync(classes.OrderBy(c => c.Id), pageNumber, pageSize);

            var advisors = await _context.ProfileManagers
                .Where(pm => pm.VaiTro == "Cố vấn học tập")
                .Select(pm => new SelectListItem
                {
                    Value = pm.Id.ToString(),
                    Text = pm.Email
                }).ToListAsync();

            var viewModel = new ClassListViewModel
            {
                Class = new Class(),
                Advisors = advisors,
                PaginatedClasses = paginatedClasses
            };

            return View(viewModel);
        }
	//Add new Class
		[HttpPost]
		public async Task<IActionResult> AddClass(string className, string advisor, int advisorId, string yearName, string department, int studentCount)
		{
			// Create a new AcademicPeriod object
			var newClass = new Class
			{
				ClassName = className,
				AdvisorName = advisor,
				AdvisorId=advisorId,
				YearName = yearName,
				Department = department,
				StudentCount = studentCount
			};
			// Add the new object to the database
			bool isFound = _context.Classes
					 .Where(r => r.ClassName == newClass.ClassName &&
								 r.AdvisorName == newClass.AdvisorName &&
								 r.AdvisorId == newClass.AdvisorId &&
								 r.YearName == newClass.YearName &&
								 r.Department == newClass.Department)
					 .Any();
			if (!isFound) //Can add
			{
				_context.Classes.Add(newClass);
				await _context.SaveChangesAsync();

				TempData["MessageAddClass"] = "Thêm lớp thành công!";
				TempData["MessageAddClassType"] = "success";

				// Redirect to the Page to see update
				return RedirectToAction("ClassList");
			}
			else
			{
				TempData["MessageAddClass"] = "Thông tin lớp đã tồn tại!";
				TempData["MessageAddClassType"] = "danger";
				return RedirectToAction("ClassList");
			}
		}
	//Edit Class
		[HttpPost]
		public async Task<IActionResult> EditClass(int classId, string className, string advisor, int advisorId, string yearName, string department, int studentCount)
		{
			var modifiedClass = await _context.Classes.FindAsync(classId);
			// Check if another record with same info exist
			var existingClass = await _context.Classes
				.FirstOrDefaultAsync(cl => cl.ClassName == className &&
										   cl.AdvisorName == advisor &&
										   cl.AdvisorId == advisorId &&
										   cl.YearName == yearName &&
										   cl.Department == department &&
										   cl.StudentCount == studentCount);
			if (modifiedClass != null && existingClass == null)
			{
				TempData["MessageEditClass"] = "Cập nhật lớp thành công!";
				TempData["MessageEditClassType"] = "success";
				modifiedClass.ClassName = className;
				modifiedClass.AdvisorName = advisor;
				modifiedClass.AdvisorId = advisorId;
				modifiedClass.YearName = yearName;
				modifiedClass.Department = department;
				modifiedClass.StudentCount = studentCount;
				await _context.SaveChangesAsync();
				return RedirectToAction("ClassList");
			}
			else
			{
				TempData["MessageEditClass"] = "Trùng thời gian, cập nhật đã bị hủy!";
				TempData["MessageEditClassType"] = "danger";
				return RedirectToAction("ClassList");
			}
		}
	//Delete Class
		[HttpPost]
		public async Task<IActionResult> DeleteClass(int classId)
		{
			var classToDelete = await _context.Classes.FindAsync(classId);
			if (classToDelete != null)
			{
				_context.Classes.Remove(classToDelete);
				await _context.SaveChangesAsync();
				// Redirect to the Page to see update
				TempData["MessageDeleteClass"] = "Xóa lớp thành công!";
				TempData["MessageDeleteClassType"] = "success";
				return RedirectToAction("ClassList");
			}
			else
			{
				TempData["MessageDeleteClass"] = "Xảy ra lỗi khi xóa!";
				TempData["MessageDeleteClassType"] = "danger";
				return RedirectToAction("ClassList");
			}
		}
	//Import xls
		//Get All Advisors from the DB
		public List<string> GetAdvisors()
		{
			var advisors = new List<string?>();
			// Assuming you have a DbContext named 'YourDbContext'
			using (var context = _context)
			{
				advisors = context.ProfileManagers
								  .Where(p => p.VaiTro == "Cố vấn học tập")
								  .Select(p => p.Email) 
								  .ToList();
			}
			return advisors;
		}
		//Download Template
		[HttpGet]
		public IActionResult DownloadClassTemplate()
		{
			try
			{
				ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

				using (var stream = new MemoryStream())
				{
					using (var package = new ExcelPackage(stream))
					{
						var worksheet = package.Workbook.Worksheets.Add("Template");
						// Manually adjust row height to fit content
						for (int row = 1; row <= 100; row++)
						{
							worksheet.Row(row).Height = worksheet.Row(row).Height * 2; // Adjust multiplier as needed
						}
						worksheet.Cells[6, 1, 100, 5].Style.Font.Size = 14;
						worksheet.Cells[1, 1, 100, 5].Style.Font.Name = "Times New Roman";
						worksheet.Cells[6, 1, 100, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

						// Organization name
						worksheet.Cells[1, 1].Value = "TRƯỜNG ĐẠI HỌC VĂN LANG";
						worksheet.Cells[1, 1].Style.Font.Size = 14;
						
						//Department Name
						worksheet.Cells[2, 1].Value = "KHOA CÔNG NGHỆ THÔNG TIN";
						worksheet.Cells[2, 1].Style.Font.Size = 14;
						worksheet.Cells[2, 1].Style.Font.Bold = true;

						// Document Title
						worksheet.Cells[3, 1, 3, 5].Merge = true;
						worksheet.Cells[3, 1].Value = "DANH SÁCH CỐ VẤN HỌC TẬP";
						worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
						worksheet.Cells[3, 1].Style.Font.Bold = true;
						worksheet.Cells[3, 1].Style.Font.Size = 20;

						//Document Period
						worksheet.Cells[4, 1, 4, 5].Merge = true;
						worksheet.Cells[4, 1].Value = "NĂM HỌC: ";
						worksheet.Cells[4, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
						worksheet.Cells[4, 1].Style.Font.Bold = true;
						worksheet.Cells[4, 1].Style.Font.Size = 18;

						// Define headers
						worksheet.Cells[5, 1].Value = "Mã Lớp";
						worksheet.Cells[5, 2].Value = "Cố Vấn Học Tập";
						worksheet.Cells[5, 3].Value = "Niên Khóa";
						worksheet.Cells[5, 4].Value = "Ngành";
						worksheet.Cells[5, 5].Value = "Số lượng SV";

						// Apply some styling to the headers
						using (var range = worksheet.Cells[5, 1, 5, 5])
						{
							range.Style.Font.Bold = true;
							range.Style.Fill.PatternType = ExcelFillStyle.Solid;
							range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
							range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
							range.Style.Font.Size = 16;
						}

						// Add sample data
						worksheet.Cells[6, 1].Value = "71K29CNTT1";
						worksheet.Cells[6, 2].Value = "quang.187pm20551@vanlanguni.vn";
						worksheet.Cells[6, 3].Value = "2018 - 2022";
						worksheet.Cells[6, 4].Value = "7480102 - Mạng máy tính và truyền thông dữ liệu (CTTC)";
						worksheet.Cells[6, 5].Value = "30";

						// Add dropdown list (data validation) to the "Ngành" column
						var validation1 = worksheet.DataValidations.AddListValidation("D6:D100");
						validation1.Formula.Values.Add("7480201 - Công nghệ Thông tin (CTTC)");
						validation1.Formula.Values.Add("7480201 - Công nghệ Thông tin (CTĐB)");
						validation1.Formula.Values.Add("7480104 - Hệ thống Thông tin (CTTC)");
						validation1.Formula.Values.Add("7480102 - Mạng máy tính và truyền thông dữ liệu (CTTC)");

						// Add data validation to limit the value of the "Số lượng SV" column
						var validation2 = worksheet.DataValidations.AddDecimalValidation("E6:E100");
						validation2.Formula.Value = 0; // Minimum value
						validation2.Formula2.Value = 30; // Maximum value
						validation2.ShowErrorMessage = true;
						validation2.ErrorTitle = "Thông tin không hợp lệ";
						validation2.Error = "Sinh viên trong khoảng từ 0 tới 30.";

						// Add dropdown list (data validation) to the "Cố Vấn Học Tập" column
						var validation3 = worksheet.DataValidations.AddListValidation("B6:B100");
						var advisors = GetAdvisors();
						foreach (var advisor in advisors)
						{
							validation3.Formula.Values.Add(advisor);
						}

						// wrap text for entire table
						//worksheet.Cells[1, 1, 100, 5].Style.WrapText = true;
						// Auto-fit columns to adjust width based on content
						worksheet.Cells.AutoFitColumns();

						package.Save();
					}

					stream.Position = 0;
					var content = stream.ToArray();
					return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mẫu Danh Sách Lớp.xlsx");
				}
			}
			catch (Exception ex)
			{
				// Log the exception details
				//_logger.LogError(ex, "Error generating Excel template");;
				//return RedirectToAction("ClassList");
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}
		//Import Records
		[HttpPost]
		public async Task<IActionResult> ImportClassFromExcel(IFormFile file)
		{
			if (file == null || file.Length == 0)
				return BadRequest("File not selected");

			var successfulImports = 0;
			var duplicateRecords = 0;

			// Fetch advisor data
			var advisors = _context.ProfileManagers.ToDictionary(a => a.Email, a => a.Id);

			using (var stream = new MemoryStream())
			{
				await file.CopyToAsync(stream);
				using (var package = new ExcelPackage(stream))
				{
					var worksheet = package.Workbook.Worksheets[0];
					var rowCount = worksheet.Dimension.Rows;

					for (int row = 2; row <= rowCount; row++)
					{
						var className = worksheet.Cells[row, 1].Value?.ToString();
						var advisorName = worksheet.Cells[row, 2].Value?.ToString();
						var yearName = worksheet.Cells[row, 3].Value?.ToString();
						var department = worksheet.Cells[row, 4].Value?.ToString();
						var studentCountStr = worksheet.Cells[row, 5].Value?.ToString();

						if (string.IsNullOrWhiteSpace(className) ||
							string.IsNullOrWhiteSpace(advisorName) ||
							string.IsNullOrWhiteSpace(yearName) ||
							string.IsNullOrWhiteSpace(department) ||
							string.IsNullOrWhiteSpace(studentCountStr))
						{
							continue; // Skip this row if any required cell is null or empty
						}

						if (!advisors.TryGetValue(advisorName, out var advisorId))
						{
							// Handle case where advisor is not found
							continue;
						}

						int studentCount = int.Parse(studentCountStr);

						var record = new Class
						{
							ClassName = className,
							AdvisorId = advisorId,
							AdvisorName = advisorName,
							YearName = yearName,
							Department = department,
							StudentCount = studentCount
						};

						if (!_context.Classes.Any(r => r.ClassName == record.ClassName &&
														r.AdvisorName == record.AdvisorName &&
														r.AdvisorId == record.AdvisorId &&
														r.YearName == record.YearName &&
														r.Department == record.Department))
						{
							_context.Classes.Add(record);
							successfulImports++;
						}
						else
						{
							duplicateRecords++;
						}
					}
				}
			}

			await _context.SaveChangesAsync();

			TempData["MessageImportClass"] = $"Nhập dữ liệu thành công! Đã nhập {successfulImports} bản ghi. Có {duplicateRecords} bản ghi bị trùng.";
			TempData["MessageImportClassType"] = "success";

			return RedirectToAction("ClassList");
		}
		//Export Records
		[HttpGet]
		public async Task<IActionResult> ExportClassToExcel()
		{
			// Fetch records from the database
			var records = await _context.Classes.ToListAsync();

			using (var package = new ExcelPackage())
			{
				var worksheet = package.Workbook.Worksheets.Add("Classes");
				// Manually adjust row height to fit content
				for (int exportRow = 1; exportRow <= 100; exportRow++)
				{
					worksheet.Row(exportRow).Height = worksheet.Row(exportRow).Height * 2; // Adjust multiplier as needed
				}

				worksheet.Cells[6, 1, 100, 5].Style.Font.Size = 14;
				worksheet.Cells[1, 1, 100, 5].Style.Font.Name = "Times New Roman";
				worksheet.Cells[6, 1, 100, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

				// Organization name
				worksheet.Cells[1, 1].Value = "TRƯỜNG ĐẠI HỌC VĂN LANG";
				worksheet.Cells[1, 1].Style.Font.Size = 14;

				//Department Name
				worksheet.Cells[2, 1].Value = "KHOA CÔNG NGHỆ THÔNG TIN";
				worksheet.Cells[2, 1].Style.Font.Size = 14;
				worksheet.Cells[2, 1].Style.Font.Bold = true;

				// Document Title
				worksheet.Cells[3, 1, 3, 5].Merge = true;
				worksheet.Cells[3, 1].Value = "DANH SÁCH CỐ VẤN HỌC TẬP";
				worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				worksheet.Cells[3, 1].Style.Font.Bold = true;
				worksheet.Cells[3, 1].Style.Font.Size = 20;

				//Document Period
				worksheet.Cells[4, 1, 4, 5].Merge = true;
				worksheet.Cells[4, 1].Value = "NĂM HỌC: ";
				worksheet.Cells[4, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				worksheet.Cells[4, 1].Style.Font.Bold = true;
				worksheet.Cells[4, 1].Style.Font.Size = 18;

				// Define headers
				worksheet.Cells[5, 1].Value = "Mã Lớp";
				worksheet.Cells[5, 2].Value = "Cố Vấn Học Tập";
				worksheet.Cells[5, 3].Value = "Niên Khóa";
				worksheet.Cells[5, 4].Value = "Ngành";
				worksheet.Cells[5, 5].Value = "Số lượng SV";

				// Apply some styling to the headers
				using (var range = worksheet.Cells[5, 1, 5, 5])
				{
					range.Style.Font.Bold = true;
					range.Style.Fill.PatternType = ExcelFillStyle.Solid;
					range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
					range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					range.Style.Font.Size = 16;
				}

				// Add dropdown list (data validation) to the "Ngành" column
				var validation1 = worksheet.DataValidations.AddListValidation("D6:D100");
				validation1.Formula.Values.Add("7480201 - Công nghệ Thông tin (CTTC)");
				validation1.Formula.Values.Add("7480201 - Công nghệ Thông tin (CTĐB)");
				validation1.Formula.Values.Add("7480104 - Hệ thống Thông tin (CTTC)");
				validation1.Formula.Values.Add("7480102 - Mạng máy tính và truyền thông dữ liệu (CTTC)");

				// Add data validation to limit the value of the "Số lượng SV" column
				var validation2 = worksheet.DataValidations.AddDecimalValidation("E6:E100");
				validation2.Formula.Value = 0; // Minimum value
				validation2.Formula2.Value = 30; // Maximum value
				validation2.ShowErrorMessage = true;
				validation2.ErrorTitle = "Thông tin không hợp lệ";
				validation2.Error = "Sinh viên trong khoảng từ 0 tới 30.";

				// Add dropdown list (data validation) to the "Cố Vấn Học Tập" column
				var validation3 = worksheet.DataValidations.AddListValidation("B6:B100");
				var advisors = GetAdvisors();
				foreach (var advisor in advisors)
				{
					validation3.Formula.Values.Add(advisor);
				}

				// Add data
				int row = 6;
				foreach (var record in records)
				{
					worksheet.Cells[row, 1].Value = record.ClassName;
					worksheet.Cells[row, 2].Value = record.AdvisorName;
					worksheet.Cells[row, 3].Value = record.YearName;
					worksheet.Cells[row, 4].Value = record.Department;
					worksheet.Cells[row, 5].Value = record.StudentCount;
					row++;
				}

				// Auto-fit columns
				worksheet.Cells.AutoFitColumns();

				var bytes = package.GetAsByteArray();
				return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Danh sách lớp.xlsx");
			}
		}
//StudentList actions
    //Render StudentList view
        public async Task<IActionResult> StudentList(int Id, int pageNumber = 1, int pageSize = 10)
        {
            ViewData["page"] = "StudentList";

            var studentsInCurrentClass = _context.Students
                .Include(s => s.Class)
                .Where(s => s.ClassId == Id)
                .AsQueryable();

            var paginatedStudents = await PaginatedList<Student>.CreateAsync(studentsInCurrentClass.OrderBy(c => c.Id), pageNumber, pageSize);

            // Get IDs of all students assigned to any class
            var assignedStudentIds = await _context.Students
                                                  .Where(s => s.ClassId != null)
                                                  .Select(s => s.ProfileManagerId)
                                                  .ToListAsync();

            // Fetch profile managers who are not assigned to any class
            var profileManagers = await _context.ProfileManagers
                .Where(pm => pm.VaiTro == "Sinh viên" && !assignedStudentIds.Contains(pm.Id))
                .ToListAsync();

            var studentList = profileManagers
                .Select(pm => new SelectListItem
                {
                    Value = pm.Id.ToString(),
                    Text = pm.Email
                }).ToList();

            var classInfo = await _context.Classes.FirstOrDefaultAsync(c => c.Id == Id);

            var viewModel = new StudentListViewModel
            {
                Student = new Student(),
                StudentList = studentList,
                PaginatedStudents = paginatedStudents,
                ProfileManagers = profileManagers,
                Class = classInfo
            };

            return View(viewModel);
        }
    //Add new Student
        public async Task<IActionResult> AddStudent(string studentId, string selectedEmail, string studentFullName, DateTime studentDateOfBirth, int classId, int profileManagerId, string studentStatus)
        {
            // Check current student count for the class
            var currentStudentCount = await _context.Students.CountAsync(s => s.ClassId == classId);

            if (currentStudentCount >= 30)
            {
                TempData["MessageAddStudent"] = "Lớp đã đầy, không thể thêm sinh viên!";
                TempData["MessageAddStudentType"] = "danger";
                return RedirectToAction("StudentList", new { Id = classId });
            }
            // Create a new Student object
            var newStudent = new Student
            {
                Id = studentId,
                Email = selectedEmail,
                FullName = studentFullName,
                DateOfBirth = studentDateOfBirth,
                ClassId = classId,
                Status = studentStatus,
                ProfileManagerId = profileManagerId
            };

            // Add the new object to the database
            bool isFound = _context.Students
                .Where(r => r.Id == newStudent.Id &&
                            r.Email == newStudent.Email &&
                            r.FullName == newStudent.FullName &&
                            r.DateOfBirth == newStudent.DateOfBirth &&
                            r.ClassId == newStudent.ClassId &&
                            r.ProfileManagerId == newStudent.ProfileManagerId)
                .Any();

            if (!isFound) // Can add
            {
                _context.Students.Add(newStudent);
                await _context.SaveChangesAsync();

                TempData["MessageAddStudent"] = "Thêm sinh viên thành công!";
                TempData["MessageAddStudentType"] = "success";

                // Redirect to the Page to see update
                return RedirectToAction("StudentList", new { Id = classId });
            }
            else
            {
                TempData["MessageAddStudent"] = "Thông tin sinh viên đã tồn tại!";
                TempData["MessageAddStudentType"] = "danger";
                return RedirectToAction("StudentList", new { Id = classId });
            }
        }
        //Edit Student
        public async Task<IActionResult> EditStudent(string studentId, string studentEmail, string studentFullName, DateTime studentDateOfBirth, int classId, int profileManagerId, string studentStatus)
		{
            var modifiedStudent = await _context.Students.FindAsync(studentId);
                TempData["MessageEditStudent"] = "Cập nhật sinh viên thành công!";
                TempData["MessageEditStudentType"] = "success";
                modifiedStudent.DateOfBirth = studentDateOfBirth;
                modifiedStudent.Status = studentStatus;
                await _context.SaveChangesAsync();
                return RedirectToAction("StudentList", new { Id = classId });    
        }
    //Delete Student
        public async Task<IActionResult> DeleteStudent(string studentId, int classId)
        {
            var studentToDelete = await _context.Students.FindAsync(studentId);
            if (studentToDelete != null)
            {
                _context.Students.Remove(studentToDelete);
                await _context.SaveChangesAsync();
                // Redirect to the Page to see update
                TempData["MessageDeleteStudent"] = "Xóa Sinh Viên thành công!";
                TempData["MessageDeleteStudentType"] = "success";
                return RedirectToAction("StudentList", new { Id = classId });
            }
            else
            {
                TempData["MessageDeleteStudent"] = "Xảy ra lỗi khi xóa!";
                TempData["MessageDeleteStudentType"] = "danger";
                return RedirectToAction("StudentList", new { Id = classId });
            }
        }
    //Import xls
        //Download Template
        [HttpGet]
        public async Task<IActionResult> DownloadStudentTemplate(string className)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var stream = new MemoryStream())
                {
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Template");
                        var dataSheet = package.Workbook.Worksheets.Add("Data");
                        dataSheet.Hidden = eWorkSheetHidden.VeryHidden; // Hide the data sheet

                        // Fetch available students (not assigned to any class)
                        var availableStudents = _context.ProfileManagers
                                                        .Where(pm => pm.VaiTro == "Sinh viên" && !_context.Students.Any(s => s.ProfileManagerId == pm.Id))
                                                        .Select(pm => pm.Email)
                                                        .ToList();

                        var dropdownList = string.Join(",", availableStudents);

                        // Fetch all student profile managers
                        var allProfileManagers = await _context.ProfileManagers
                            .Where(pm => pm.VaiTro == "Sinh viên")
                            .ToListAsync();

                        // Filter out those already assigned to a class
                        var unassignedStudents = allProfileManagers
                            .Where(pm => !_context.Students.Any(s => s.ProfileManagerId == pm.Id && s.ClassId != null))
                            .ToList();

                        // Add headers to the data sheet
                        dataSheet.Cells["A1"].Value = "Email";
                        dataSheet.Cells["B1"].Value = "Id";
                        dataSheet.Cells["C1"].Value = "FullName";

                        // Populate data sheet with filtered student data
                        for (int i = 0; i < unassignedStudents.Count; i++)
                        {
                            dataSheet.Cells[i + 2, 1].Value = unassignedStudents[i].Email;
                            dataSheet.Cells[i + 2, 2].Value = unassignedStudents[i].MaSo;
                            dataSheet.Cells[i + 2, 3].Value = unassignedStudents[i].TenDayDu;
                        }

                        // Manually adjust row height to fit content
                        for (int row = 1; row <= 100; row++)
                        {
                            worksheet.Row(row).Height = worksheet.Row(row).Height * 2; // Adjust multiplier as needed
                        }
                        worksheet.Cells[6, 1, 100, 6].Style.Font.Size = 14;
                        worksheet.Cells[1, 1, 100, 6].Style.Font.Name = "Times New Roman";
                        worksheet.Cells[6, 1, 100, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // Organization name
                        worksheet.Cells[1, 1].Value = "TRƯỜNG ĐẠI HỌC VĂN LANG";
                        worksheet.Cells[1, 1].Style.Font.Size = 14;

                        //Department Name
                        worksheet.Cells[2, 1].Value = "KHOA CÔNG NGHỆ THÔNG TIN";
                        worksheet.Cells[2, 1].Style.Font.Size = 14;
                        worksheet.Cells[2, 1].Style.Font.Bold = true;

                        // Document Title
                        worksheet.Cells[3, 1, 3, 5].Merge = true;
                        worksheet.Cells[3, 1].Value = "DANH SÁCH SINH VIÊN";
                        worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[3, 1].Style.Font.Bold = true;
                        worksheet.Cells[3, 1].Style.Font.Size = 20;

                        //Document Period
                        worksheet.Cells[4, 1, 4, 5].Merge = true;
                        worksheet.Cells[4, 1].Value = "LỚP " + className;
                        worksheet.Cells[4, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[4, 1].Style.Font.Bold = true;
                        worksheet.Cells[4, 1].Style.Font.Size = 18;

                        // Define headers
                        worksheet.Cells[5, 1].Value = "Email";
                        worksheet.Cells[5, 2].Value = "Mã Số SV";
                        worksheet.Cells[5, 3].Value = "Họ và Tên";
                        worksheet.Cells[5, 4].Value = "Ngày Tháng Năm Sinh";
                        worksheet.Cells[5, 5].Value = "Mã Lớp";
                        worksheet.Cells[5, 6].Value = "Tình Trạng";

                        // Apply some styling to the headers
                        using (var range = worksheet.Cells[5, 1, 5, 6])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            range.Style.Font.Size = 16;
                        }

                        // Add sample data
                        worksheet.Cells[6, 1].Value = "bao.187pm06548@vanlanguni.vn";
                        worksheet.Cells[6, 2].Value = "187pm06548";
                        worksheet.Cells[6, 3].Value = "Nguyễn Đăng Hồng Bảo";
                        worksheet.Cells[6, 4].Value = "08/01/1999";
                        worksheet.Cells[6, 5].Value = className;
                        worksheet.Cells[6, 6].Value = "Còn học";


                        // Add Data Validation for dropdown list
                        var validation = worksheet.DataValidations.AddListValidation("A6:A36");
                        validation.Formula.Values.Add(dropdownList); // Apply your available students' emails

						// Add VLOOKUP formulas for first 10 rows
						for (int row = 6; row <= 36; row++)
						{
							worksheet.Cells[row, 2].Formula = $"IF(A{row}=\"\", \"\", VLOOKUP(A{row}, Data!A:C, 2, FALSE))";
							worksheet.Cells[row, 3].Formula = $"IF(A{row}=\"\", \"\", VLOOKUP(A{row}, Data!A:C, 3, FALSE))";
						}

                        // Add dropdown list (data validation) to the "Ngành" column
                        var validation1 = worksheet.DataValidations.AddListValidation("F6:F36");
                        validation1.Formula.Values.Add("Đang học");
                        validation1.Formula.Values.Add("Bảo lưu");
                        validation1.Formula.Values.Add("Nghỉ học");

                        // Auto-fit columns to adjust width based on content
                        worksheet.Cells.AutoFitColumns();

                        package.Save();
                    }

                    stream.Position = 0;
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mẫu Danh Sách SV.xlsx");
                }
            }
            catch (Exception ex)
            {
                // Log the exception details
                //_logger.LogError(ex, "Error generating Excel template");;
                //return RedirectToAction("ClassList");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //Import Records
        [HttpPost]
        public async Task<IActionResult> ImportStudentFromExcel(IFormFile file, int classId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File not selected");

            var successfulImports = 0;
            var duplicateRecords = 0;

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 6; row <= rowCount; row++)
                    {
                        var email = worksheet.Cells[row, 1].Text;
                        var id = worksheet.Cells[row, 2].Text;
                        var fullName = worksheet.Cells[row, 3].Text;
                        var dobText = worksheet.Cells[row, 4].Text;
                        var className = worksheet.Cells[row, 5].Text;
                        var status = worksheet.Cells[row, 6].Text;

                        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(id) ||
                            string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(dobText) ||
                            string.IsNullOrWhiteSpace(className) || string.IsNullOrWhiteSpace(status))
                        {
                            continue; // Skip this row if any required cell is null or empty
                        }

                        if (!DateTime.TryParse(dobText, out var dateOfBirth))
                        {
                            continue; // Invalid date format
                        }

                        // Find ProfileManagerId based on email
                        var profileManagerId = await _context.ProfileManagers
                            .Where(pm => pm.Email == email)
                            .Select(pm => pm.Id)
                            .FirstOrDefaultAsync();

                        if (profileManagerId == 0)
                        {
                            continue; // Skip if profile manager not found
                        }

                        var record = new Student
                        {
                            Email = email,
                            Id = id,
                            FullName = fullName,
                            DateOfBirth = dateOfBirth,
                            ClassId = classId,
                            Status = status,
                            ProfileManagerId = profileManagerId
                        };

                        if (!_context.Students.Any(r => r.Email == record.Email && r.Id == record.Id &&
                                                        r.FullName == record.FullName && r.DateOfBirth == record.DateOfBirth &&
                                                        r.ClassId == record.ClassId && r.Status == record.Status))
                        {
                            _context.Students.Add(record);
                            successfulImports++;
                        }
                        else
                        {
                            duplicateRecords++;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            TempData["MessageImportStudent"] = $"Nhập dữ liệu thành công! Đã nhập {successfulImports} bản ghi. Có {duplicateRecords} bản ghi bị trùng.";
            TempData["MessageImportStudentType"] = "success";

            return RedirectToAction("StudentList",new { Id = classId });
        }
        //Export Records
        [HttpGet]
        public async Task<IActionResult> ExportStudentToExcel(int classId)
        {
            var students = await _context.Students
                .Where(s => s.ClassId == classId)
                .ToListAsync();

            var classInfo = await _context.Classes.FindAsync(classId);
            var className = classInfo?.ClassName ?? "Class";

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Student Data");

                // Add headers
                worksheet.Cells["A1"].Value = "Email";
                worksheet.Cells["B1"].Value = "Student Id";
                worksheet.Cells["C1"].Value = "Full Name";
                worksheet.Cells["D1"].Value = "Date Of Birth";
                worksheet.Cells["E1"].Value = "Status";

                // Add student data
                for (int i = 0; i < students.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = students[i].Email;
                    worksheet.Cells[i + 2, 2].Value = students[i].Id;
                    worksheet.Cells[i + 2, 3].Value = students[i].FullName;
                    worksheet.Cells[i + 2, 4].Value = students[i].DateOfBirth.ToShortDateString();
                    worksheet.Cells[i + 2, 5].Value = students[i].Status;
                }

                // Format the worksheet for better readability
                worksheet.Cells.AutoFitColumns(0);
                worksheet.Cells[1, 1, 1, 5].Style.Font.Bold = true;

                var fileContent = package.GetAsByteArray();

                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Danh sách SV lớp {className}.xlsx");
            }
        }

        //SchoolYear actions
        //Render SchoolYear view
        public async Task<IActionResult> SchoolYear(int pageNumber = 1, int pageSize = 10)
		{
			ViewData["page"] = "SchoolYear";
			var periods = _context.AcademicPeriods.AsQueryable();

			var paginatedPeriods = await PaginatedList<AcademicPeriod>.CreateAsync(periods.OrderBy(p => p.PeriodStart), pageNumber, pageSize);

			var model = new SchoolYearViewModel
			{
				PaginatedPeriods = paginatedPeriods
			};

			return View(model);
		}
	//Edit Academic Year
		[HttpPost]
		public async Task<IActionResult> EditPeriod(int periodId, string periodName, DateTime periodStart, DateTime periodEnd)
		{
			var modifiedPeriod = await _context.AcademicPeriods.FindAsync(periodId);
			// Check if another record with the same year and semester exists
			var existingPeriod = await _context.AcademicPeriods
				.FirstOrDefaultAsync(ap => ap.PeriodName == periodName && 
										   ap.PeriodStart == periodStart && 
										   ap.PeriodEnd == periodEnd &&
										   ap.PeriodId != periodId);
			if (modifiedPeriod != null && existingPeriod == null)
			{
				TempData["MessageEditYear"] = "Cập nhật thành công!";
				TempData["MessageEditType"] = "success";
				modifiedPeriod.PeriodName = periodName;
				modifiedPeriod.PeriodStart = periodStart;
				modifiedPeriod.PeriodEnd = periodEnd;
				await _context.SaveChangesAsync();
				return RedirectToAction("SchoolYear");
			}
			else
			{
				TempData["MessageEditYear"] = "Trùng thời gian, cập nhật đã bị hủy!";
				TempData["MessageEditType"] = "danger";
				return RedirectToAction("SchoolYear");
			}
		}
	//Delete Academic Year
		[HttpPost]
		public async Task<IActionResult> DeletePeriod(int periodId)
		{
			var periodToDelete = await _context.AcademicPeriods.FindAsync(periodId);
			if (periodToDelete != null)
			{
				_context.AcademicPeriods.Remove(periodToDelete);
				await _context.SaveChangesAsync();
				// Redirect to the Page to see update
				TempData["MessageDeleteYear"] = "Xóa năm học thành công!";
				TempData["MessageDeleteType"] = "success";
				return RedirectToAction("SchoolYear");				
			}
			else
			{
				TempData["MessageDeleteYear"] = "Xảy ra lỗi khi xóa!";
				TempData["MessageDeleteType"] = "danger";
				return RedirectToAction("SchoolYear");
			}
			
		}
	//Add new Academic Year
		[HttpPost]
		public async Task<IActionResult> AddPeriod(string periodName, DateTime periodStart, DateTime periodEnd)
		{
			// Create a new AcademicPeriod object
			var newPeriod = new AcademicPeriod
			{
				PeriodName = periodName,
				PeriodStart = periodStart,
				PeriodEnd = periodEnd
			};
			// Add the new object to the database
			bool isFound = _context.AcademicPeriods
					 .Where(r => r.PeriodName == newPeriod.PeriodName && 
								 r.PeriodStart == newPeriod.PeriodStart &&
								 r.PeriodEnd == newPeriod.PeriodEnd)
					 .Any();
			if (!isFound) //Can add
			{
				_context.AcademicPeriods.Add(newPeriod);
				await _context.SaveChangesAsync();

				TempData["MessageAddYear"] = "Thêm năm học thành công!";
				TempData["MessageAddType"] = "success";

				// Redirect to the Page to see update
				return RedirectToAction("SchoolYear");
			}
			else
			{
				TempData["MessageAddYear"] = "Thông tin năm học đã tồn tại!";
				TempData["MessageAddType"] = "danger";
				return RedirectToAction("SchoolYear");
			}
		}
//SemesterPlan actions
		public IActionResult SemesterPlan()
		{
			ViewData["page"] = "SemesterPlan";
			return View();
		}
	}
}


