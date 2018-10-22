using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Orleans;
using OrleansChess.Common;
using OrleansChess.Common.Events;
using OrleansChess.GrainInterfaces.Chess;

[Authorize]
public class ChessHub : Hub, IChessHub
{
    private readonly IClusterClient _orleansClient;

    public ChessHub(IClusterClient orleansClient)
    {
        _orleansClient = orleansClient;
    }

    public override Task OnConnectedAsync()
    {
        return Task.CompletedTask;
    }

    public async Task<ISuccessOrErrors<IBoardState>> GetBoardState(string gameId) {
        var gameGuid = Guid.Parse(gameId);
        var game = _orleansClient.GetGrain<IGame>(gameGuid);
        var boardState = await game.GetBoardState();
        return new Success<IBoardState>(boardState);
    }

    public async Task<ISuccessOrErrors<BoardState>> PlayerIJoinGame(string gameId){
        var playerId = Context.User.Claims.First(x=>x.Type.Equals("userId")).Value;
        var seat = _orleansClient.GetGrain<ISeatI>(Guid.Parse(gameId));
        var result = await seat.JoinGame(Guid.Parse(playerId));
        if (result.WasSuccessful) {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
        }
        return result;
    }

    public async Task<ISuccessOrErrors<PlayerIMoved>> PlayerIMove (Guid gameId, string originalPosition, string newPosition, string eTag) {
        var board = _orleansClient.GetGrain<IBoard>(gameId);
        var result = await board.PlayerIMove(originalPosition, newPosition, eTag);
        return result;
    }

    public async Task<ISuccessOrErrors<BoardState>> PlayerIIJoinGame(string gameId){
        var playerId = Context.User.Claims.First(x=>x.Type.Equals("userId")).Value;
        var seat = _orleansClient.GetGrain<ISeatII>(Guid.Parse(gameId));
        var result = await seat.JoinGame(Guid.Parse(playerId));
        if (result.WasSuccessful) {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
        }
        return result;
    }

    public async Task<ISuccessOrErrors<PlayerIIMoved>> PlayerIIMove (Guid gameId, string originalPosition, string newPosition, string eTag) {
        var board = _orleansClient.GetGrain<IBoard>(gameId);
        var result = await board.PlayerIIMove(originalPosition, newPosition, eTag);
        return result;
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // todo: leave game
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
        await base.OnDisconnectedAsync(exception);
    }
}