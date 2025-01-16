using CAPSTONE_TEAM01_2024.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CAPSTONE_TEAM01_2024.Utilities
{
    public class NotificationHub:Hub
    {
        private readonly ApplicationDbContext _context;  // Your database context

        public NotificationHub(ApplicationDbContext context)
        {
            _context = context;
        }
        // Method when a user connects
        public override async Task OnConnectedAsync()
        {
            var userEmail = Context.User.Identity.Name; // Assuming the email is stored as the user identity

            if (!string.IsNullOrEmpty(userEmail))
            {
                // Fetch unread notifications from the database for this user
                var unreadNotifications = await GetUnreadNotificationsForUser(userEmail);

                // Send unread notifications to the user
                foreach (var notification in unreadNotifications)
                {
                    await Clients.User(userEmail).SendAsync("ReceiveNotification", notification.Message);
                    // Mark the notification as read in the database
                    await MarkNotificationAsRead(notification.Id);
                }
            }

            await base.OnConnectedAsync();
        }

        // Method to send a notification to a specific user
        public async Task SendNotification(string message, string targetEmail)
        {
            var notification = new Notification
            {
                Message = message,
                CreatedAt = DateTime.UtcNow,
                TargetedUserEmail = targetEmail,
                IsRead = false
            };

            // Save the notification to the database
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Check if the user is online
            if (Context.User?.Identity?.Name == targetEmail)
            {
                // If the user is online, send the notification immediately
                await Clients.User(targetEmail).SendAsync("ReceiveNotification", message);
            }
        }

        // Get unread notifications from the database for a specific user
        private async Task<List<Notification>> GetUnreadNotificationsForUser(string userEmail)
        {
            return await _context.Notifications
                .Where(n => n.TargetedUserEmail == userEmail && !n.IsRead)
                .ToListAsync();
        }

        // Mark a notification as read in the database
        private async Task MarkNotificationAsRead(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();
            }
        }
    }
}
