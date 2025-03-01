﻿using CAPSTONE_TEAM01_2024.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["page"] = "Index";
            return View();
        }

        public IActionResult HomePage()
        {
            ViewData["page"] = "HomePage";
            return View();
        }

        public IActionResult SchoolYear()
        {
            ViewData["page"] = "SchoolYear";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}