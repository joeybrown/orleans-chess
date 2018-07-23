using System;
using System.Threading.Tasks;
using Orleans.TestKit;
using OrleansChess.Grains.Tests.Extensions;
using Xunit;

namespace OrleansChess.Grains.Tests.GameTests {
    public class PlayerActivityStreamingTests : TestKitBase {
        [Fact]
        public async Task OnGameCreationWhenWhiteJoins_Should_StreamEvent () {
            var whiteId = Guid.NewGuid();
            var game = Silo.NewGame();
            await game.WhiteJoinGame(whiteId);
        }

        [Fact]
        public async Task OnGameCreationWhenBlackJoins_Should_StreamEvent () {
            var blackId = Guid.NewGuid();
            var game = Silo.NewGame();
            await game.BlackJoinGame(blackId);
        }

        [Fact]
        public async Task OnGameActiveWhenWhiteLeaves_Should_StreamEvent () {
            var game = Silo.NewGame();
            await game.BlackJoinGame(Guid.NewGuid());
            await game.WhiteJoinGame(Guid.NewGuid());
            await game.WhiteLeaveGame();
        }

        [Fact]
        public async Task OnGameActiveWhenBlackLeaves_Should_StreamEvent () {
            var game = Silo.NewGame();
            await game.BlackJoinGame(Guid.NewGuid());
            await game.WhiteJoinGame(Guid.NewGuid());
            await game.BlackLeaveGame();
        }
    }
}