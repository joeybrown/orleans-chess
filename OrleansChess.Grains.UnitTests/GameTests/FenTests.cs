using System;
using System.Threading.Tasks;
using FluentAssertions;
using Orleans.TestKit;
using OrleansChess.Common;
using OrleansChess.GrainClasses.Chess;
using OrleansChess.GrainInterfaces.Chess;
using Xunit;

namespace OrleansChess.Grains.UnitTests.GameTests {
    public class FenTests : TestKitBase {
        [Fact]
        public async Task OnGameCreation_Fen_Should_BeInit () {
            var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
            const string expected = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
            var state = await game.GetBoardState();
            state.Fen.Should().Be(expected);
        }
    }
}