using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Orleans;
using OrleansChess.Common;
using OrleansChess.Common.Events;
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

    public async Task<ISuccessOrErrors<IBoardState>> GetBoardState(string gameId) {
        var gameGuid = Guid.Parse(gameId);
        var game = _orleansClient.GetGrain<IGame>(gameGuid);
        var boardState = await game.GetBoardState();
        return new Success<IBoardState>(boardState);
    }

    public async Task<ISuccessOrErrors<BoardState>> WhiteJoinGame(Guid gameId){
        var identity = (ClaimsIdentity) Context.User.Identity;
        var playerId = Guid.Parse (identity.FindFirst("userId").Value);
        var seat = _orleansClient.GetGrain<ISeatWhite>(gameId);
        var result = await seat.JoinGame(playerId);
        if (result.WasSuccessful) {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
        }
        return result;
    }

    public async Task<ISuccessOrErrors<WhiteMoved>> WhiteMove (Guid gameId, string originalPosition, string newPosition, string eTag) {
        var board = _orleansClient.GetGrain<IBoard>(gameId);
        var result = await board.WhiteMove(originalPosition, newPosition, eTag);
        return result;
    }

    public async Task<ISuccessOrErrors<BoardState>> BlackJoinGame(Guid gameId){
        var playerId = Guid.NewGuid(); // todo: user should have guid
        var seat = _orleansClient.GetGrain<ISeatBlack>(gameId);
        var result = await seat.JoinGame(playerId);
        if (result.WasSuccessful) {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
        }
        return result;
    }

    public async Task<ISuccessOrErrors<BlackMoved>> BlackMove (Guid gameId, string originalPosition, string newPosition, string eTag) {
        var board = _orleansClient.GetGrain<IBoard>(gameId);
        var result = await board.BlackMove(originalPosition, newPosition, eTag);
        return result;
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // todo: leave game
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
        await base.OnDisconnectedAsync(exception);
    }
}