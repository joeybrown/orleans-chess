using System;
using System.Threading.Tasks;
using FluentAssertions;
using Orleans.TestKit;
using OrleansChess.Common;
using OrleansChess.GrainClasses.Chess;
using OrleansChess.GrainInterfaces.Chess;
using Xunit;

namespace OrleansChess.Grains.Tests.GameTests
{
    public class FenTests : TestKitBase
    {
        [Fact]
        public async Task OnGameCreation_Fen_Should_BeInit()
        {
            var grain = Silo.CreateGrain<Game>(Guid.NewGuid());
            var beforeAnyPlayers = await grain.GetShortFen();
            var white = await grain.WhiteJoinGame(Guid.NewGuid());
            var black = await grain.BlackJoinGame(Guid.NewGuid());
            var playersActive = await grain.GetShortFen();

            var expected = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

            beforeAnyPlayers.Should().Be(expected);
            white.Data.Should().Be(expected);
            black.Data.Should().Be(expected);
            playersActive.Should().Be(expected);
        }
    }
}