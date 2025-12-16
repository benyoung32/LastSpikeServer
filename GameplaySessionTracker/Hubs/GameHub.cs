using Microsoft.AspNetCore.SignalR;

namespace GameplaySessionTracker.Hubs
{
    public class GameHub(Services.ISessionService sessionService) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinSession(string sessionId, string playerId)
        {
            if (!Guid.TryParse(sessionId, out var sessionIdGuid) || !Guid.TryParse(playerId, out var playerIdGuid))
            {
                return;
            }

            var session = await sessionService.GetById(sessionIdGuid);
            if (session != null && session.PlayerIds.Contains(playerIdGuid))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            }
        }

        public async Task LeaveSession(string sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
        }
    }
}
