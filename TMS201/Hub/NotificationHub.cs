using Microsoft.AspNetCore.SignalR;

namespace TMS201.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            var username = httpContext?.Session.GetString("Username");

            Console.WriteLine("Connected User: " + username); // ✅ debug

            if (!string.IsNullOrWhiteSpace(username))
            {
                var cleanUser = username.Trim().ToLower();

                await Groups.AddToGroupAsync(Context.ConnectionId, cleanUser);

                Console.WriteLine($"User added to group: {cleanUser}");
            }
            else
            {
                Console.WriteLine("⚠ Username not found in session");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            var username = httpContext?.Session.GetString("Username");

            if (!string.IsNullOrWhiteSpace(username))
            {
                var cleanUser = username.Trim().ToLower();

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, cleanUser);

                Console.WriteLine($"User removed from group: {cleanUser}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
