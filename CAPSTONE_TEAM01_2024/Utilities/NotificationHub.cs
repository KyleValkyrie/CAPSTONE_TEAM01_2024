using Microsoft.AspNetCore.SignalR;

namespace CAPSTONE_TEAM01_2024.Utilities
{
    public class NotificationHub:Hub
    {
        // Method that clients can call
        public async Task SendNotification(string user, string message)
        {
            // Send the notification to a specific user
            await Clients.User(user).SendAsync("ReceiveNotification", message);
        }
    }
}
