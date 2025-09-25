using Microsoft.AspNetCore.SignalR;
using Crit.Server.Hubs;

namespace Crit.Server.Services
{
    public interface INotificationService
    {
        Task NotifyNewComplaint(string clientName, string title, string type);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyNewComplaint(string clientName, string title, string type)
        {
            await _hubContext.Clients.Group("Admins").SendAsync("NewComplaint", new
            {
                ClientName = clientName,
                Title = title,
                Type = type,
                Time = DateTime.Now
            });
        }
    }
}