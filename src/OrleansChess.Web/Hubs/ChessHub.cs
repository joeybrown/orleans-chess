using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Orleans;
using OrleansChess.Common;
using OrleansChess.GrainInterfaces.Chess;

public class ChessHub : Hub 
{
    private readonly IClusterClient _orleansClient;

    public ChessHub(IClusterClient orleansClient)
    {
        _orleansClient = orleansClient;
    }

    public override async Task OnConnectedAsync()
    {
    }

    public async Task<ISuccessOrErrors<WhiteJoined>> WhiteJoinGame(Guid gameId){
        var whiteId = Guid.NewGuid();
        var game = _orleansClient.GetGrain<IGame>(gameId);
        var result = await game.WhiteJoinGame(whiteId);
        if (result.WasSuccessful) {
            await WhiteJoined(gameId.ToString());
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
        }
        return result;
    }

    public async Task<ISuccessOrErrors<IBoardStateWithETag>> WhiteMove (Guid gameId, string originalPosition, string newPosition, string eTag) {
        var game = _orleansClient.GetGrain<IGame>(gameId);
        var result = await game.WhiteMove(originalPosition, newPosition, eTag);
        if (result.WasSuccessful) {
            await PositionUpdated(gameId.ToString(), result.Data);
        }
        return result;
    }

    public async Task PositionUpdated(string gameId, IBoardStateWithETag fen) {
        await Clients.Group(gameId).SendAsync(nameof(PositionUpdated), fen);
    }

    public async Task WhiteJoined(string gameId) {
        await Clients.Group(gameId).SendAsync(nameof(WhiteJoined));
    }

    public async Task BlackJoined(string gameId) {
        await Clients.Group(gameId).SendAsync(nameof(BlackJoined));
    }

    public async Task<ISuccessOrErrors<IBoardStateWithETag>> BlackJoinGame(Guid gameId){
        var blackId = Guid.NewGuid();
        var game = _orleansClient.GetGrain<IGame>(gameId);
        var result = await game.BlackJoinGame(blackId);
        if (result.WasSuccessful) {
            await BlackJoined(gameId.ToString());
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
        }
        return result;
    }

    public async Task<ISuccessOrErrors<IBoardStateWithETag>> BlackMove (Guid gameId, string originalPosition, string newPosition, string eTag) {
        var game = _orleansClient.GetGrain<IGame>(gameId);
        var result = await game.BlackMove(originalPosition, newPosition, eTag);
        if (result.WasSuccessful) {
            await PositionUpdated(gameId.ToString(), result.Data);
        }
        return result;
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // todo: leave game
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
        await base.OnDisconnectedAsync(exception);
    }
}