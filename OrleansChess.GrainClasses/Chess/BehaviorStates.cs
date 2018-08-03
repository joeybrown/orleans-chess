namespace OrleansChess.GrainClasses.Chess {
    public enum TurnBehaviorStateOption {
        Black,
        White
    }

    public enum GameBehaviorStateOption {
        NoPlayersActive,
        WaitingForWhite,
        WaitingForBlack,
        GameIsActive
    }

    public enum SeatBehaviorStateOption {
        Occupied,
        Unoccupied
    }

}