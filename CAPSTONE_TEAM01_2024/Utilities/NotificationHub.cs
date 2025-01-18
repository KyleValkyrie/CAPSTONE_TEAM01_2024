using CAPSTONE_TEAM01_2024.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CAPSTONE_TEAM01_2024.Utilities
{
    public class NotificationHub : Hub
    {
        private readonly ApplicationDbContext _context;  // Your database context

        public NotificationHub(ApplicationDbContext context)
        {
            _context = context;
        }
       
    }
}
