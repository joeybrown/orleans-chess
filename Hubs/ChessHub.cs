using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public class ChessHub : Hub 
{
    public async Task UpdateFen(string fen)
    {
        await Clients.All.SendAsync("UpdateFen", fen);
    }
}