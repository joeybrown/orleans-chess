using System;
using System.Threading.Tasks;
using FluentAssertions;
// using Moq;
using Orleans.TestKit;
using OrleansChess.Common;
using OrleansChess.GrainClasses.Chess;
using OrleansChess.GrainInterfaces.Chess;
using Xunit;

namespace OrleansChess.Grains.Tests
{
    public class GameTests : TestKitBase
    {
        [Fact]
        public async Task OnGameCreation_Should_AllowWhite()
        {
            var grain = Silo.CreateGrain<Game>(Guid.NewGuid());
            var result = await grain.WhiteJoinGame(Guid.NewGuid());
            result.WasSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task OnGameCreation_Should_AllowBlack()
        {
            var grain = Silo.CreateGrain<Game>(Guid.NewGuid());
            var result = await grain.BlackJoinGame(Guid.NewGuid());
            result.WasSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task AfterWhiteJoin_ShouldNot_AllowWhite()
        {
            var grain = Silo.CreateGrain<Game>(Guid.NewGuid());
            var firstWhite = await grain.WhiteJoinGame(Guid.NewGuid());
            var secondWhite = await grain.WhiteJoinGame(Guid.NewGuid());
            firstWhite.WasSuccessful.Should().BeTrue();
            secondWhite.WasSuccessful.Should().BeFalse();
        }

        [Fact]
        public async Task AfterBlackJoin_ShouldNot_AllowBlack()
        {
            var grain = Silo.CreateGrain<Game>(Guid.NewGuid());
            var firstBlack = await grain.BlackJoinGame(Guid.NewGuid());
            var secondBlack = await grain.BlackJoinGame(Guid.NewGuid());
            firstBlack.WasSuccessful.Should().BeTrue();
            secondBlack.WasSuccessful.Should().BeFalse();
        }

        
        [Fact]
        public async Task AfterWhiteJoin_Shoul_AllowBlack()
        {
            var grain = Silo.CreateGrain<Game>(Guid.NewGuid());
            var white = await grain.WhiteJoinGame(Guid.NewGuid());
            var black = await grain.BlackJoinGame(Guid.NewGuid());
            white.WasSuccessful.Should().BeTrue();
            black.WasSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task AfterBlackJoin_Should_AllowWhite()
        {
            var grain = Silo.CreateGrain<Game>(Guid.NewGuid());
            var black = await grain.BlackJoinGame(Guid.NewGuid());
            var white = await grain.WhiteJoinGame(Guid.NewGuid());
            black.WasSuccessful.Should().BeTrue();
            white.WasSuccessful.Should().BeTrue();
        }
    }
}
