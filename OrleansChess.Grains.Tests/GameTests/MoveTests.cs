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
    public class MoveTests : TestKitBase
    {
        [Fact]
        public async Task OnGameCreation_Should_BeWhiteTurn()
        {
            var game = Silo.CreateGrain<Game>(Guid.NewGuid());
            var white = await game.WhiteJoinGame(Guid.NewGuid());
            var black = await game.BlackJoinGame(Guid.NewGuid());
            
        }

        [Fact]
        public async Task OnGameCreation_ShouldNot_BeBlackTurn()
        {
            var game = Silo.CreateGrain<Game>(Guid.NewGuid());
            var white = await game.WhiteJoinGame(Guid.NewGuid());
            var black = await game.BlackJoinGame(Guid.NewGuid());

        }
    }
}