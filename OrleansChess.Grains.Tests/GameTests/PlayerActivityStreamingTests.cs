using System;
using System.Threading.Tasks;
using FluentAssertions;
using Orleans.TestKit;
using OrleansChess.Common.Events;
using OrleansChess.GrainClasses.Chess;
using OrleansChess.Grains.Tests.Extensions;
using Xunit;

namespace OrleansChess.Grains.Tests.GameTests {
    public class PlayerActivityStreamingTests : TestKitBase {
        [Fact]
        public async Task OnGameCreationWhenWhiteJoins_Should_StreamEvent () {
            var gameId = Guid.NewGuid ();
            var game = Silo.NewGame (gameId);
            var stream = Silo.AddStreamProbe<WhiteJoinedGame> (gameId, null);
            var whiteId = Guid.NewGuid ();
            var result = await game.WhiteJoinGame (whiteId);
            stream.Sends.Should ().Be (1);
            stream.VerifySend (x => x.GetType () == typeof (WhiteJoinedGame));
            stream.VerifySend (x => x.PlayerId == whiteId);
        }

        [Fact]
        public async Task OnGameCreationWhenBlackJoins_Should_StreamEvent () {
            var gameId = Guid.NewGuid ();
            var game = Silo.NewGame (gameId);
            var stream = Silo.AddStreamProbe<BlackJoinedGame> (gameId, null);
            var blackId = Guid.NewGuid ();
            var result = await game.BlackJoinGame (blackId);
            stream.Sends.Should ().Be (1);
            stream.VerifySend (x => x.GetType () == typeof (BlackJoinedGame));
            stream.VerifySend (x => x.PlayerId == blackId);
        }

        [Fact]
        public void OnGameActiveWhenWhiteLeaves_Should_StreamEvent () {

        }

        [Fact]
        public void OnGameActiveWhenBlackLeaves_Should_StreamEvent () {

        }
    }
}