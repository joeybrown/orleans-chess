using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Orleans.TestKit;
using OrleansChess.Common;
using OrleansChess.Common.Events;
using OrleansChess.GrainClasses.Chess;
using OrleansChess.GrainInterfaces.Chess;
using OrleansChess.Grains.Tests.Extensions;
using Xunit;

namespace OrleansChess.Grains.Tests.GameTests {
    public class PlayerSeatActionStreamingTests : TestKitBase {
        [Fact]
        public async Task OnGameCreationWhenWhiteJoins_Should_StreamEvent () {
            var mockStreamProvider = new Mock<IPlayerSeatStreamProvider>();
            mockStreamProvider.SetupGet(x=>x.Name).Returns("Default");
            Silo.AddServiceProbe(mockStreamProvider);
            var gameId = Guid.NewGuid ();
            var game = Silo.AddProbe<IGame>(gameId);
            game.Setup(x=>x.GetBoardState()).Returns(new Common.BoardState("fen", "orig", "new", "eTag").ToTask());
            var stream = Silo.AddStreamProbe<WhiteJoinedGame> (gameId, nameof (WhiteJoinedGame));
            var whiteId = Guid.NewGuid ();
            var whiteSeat = Silo.CreateGrain<SeatWhite> (id: gameId);
            var result = await whiteSeat.JoinGame (whiteId);
            stream.Sends.Should ().Be (1);
            stream.VerifySend (x => x.PlayerId == whiteId);
        }

        // [Fact]
        // public async Task OnGameCreationWhenBlackJoins_Should_StreamEvent () {
        //     var gameId = Guid.NewGuid ();
        //     var game = Silo.NewGame (gameId);
        //     var stream = Silo.AddStreamProbe<BlackJoinedGame> (gameId, nameof (BlackJoinedGame));
        //     var blackId = Guid.NewGuid ();
        //     var result = await game.BlackJoinGame (blackId);
        //     stream.Sends.Should ().Be (1);
        //     stream.VerifySend (x => x.PlayerId == blackId);
        // }

        // [Fact]
        // public async Task OnGameCreationAfterBlackJoinsWhenWhiteJoin_Should_StreamEvent () {
        //     var gameId = Guid.NewGuid ();
        //     var game = Silo.NewGame (gameId);
        //     await game.BlackJoinGame (Guid.NewGuid ());
        //     var stream = Silo.AddStreamProbe<WhiteJoinedGame> (gameId, nameof (WhiteJoinedGame));
        //     var whiteId = Guid.NewGuid ();
        //     var result = await game.WhiteJoinGame (whiteId);
        //     stream.Sends.Should ().Be (1);
        //     stream.VerifySend (x => x.PlayerId == whiteId);
        // }

        // [Fact]
        // public async Task OnGameCreationAfterWhiteJoinsWhenBlackJoin_Should_StreamEvent () {
        //     var gameId = Guid.NewGuid ();
        //     var game = Silo.NewGame (gameId);
        //     await game.WhiteJoinGame (Guid.NewGuid ());
        //     var stream = Silo.AddStreamProbe<BlackJoinedGame> (gameId, nameof (BlackJoinedGame));
        //     var blackId = Guid.NewGuid ();
        //     var result = await game.BlackJoinGame (blackId);
        //     stream.Sends.Should ().Be (1);
        //     stream.VerifySend (x => x.PlayerId == blackId);
        // }

        // [Fact]
        // public void OnGameActiveWhenWhiteLeaves_Should_StreamEvent () {

        // }

        // [Fact]
        // public void OnGameActiveWhenBlackLeaves_Should_StreamEvent () {

        // }
    }
}