using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public class ChessHub : Hub 
{
    public override async Task OnConnectedAsync()
    {
        var fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
        await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
        await Clients.Caller.SendAsync("InitializeFen", fen);
        await base.OnConnectedAsync();
    }

    public async Task<bool> TryMove(string newFen)
    {
        await Clients.Group("SignalR Users").SendAsync("UpdateFen", newFen);
        return true;
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
        await base.OnDisconnectedAsync(exception);
    }
}