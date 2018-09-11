export class Color {
    constructor(friendlyName: string) {
        this.friendlyName = friendlyName;
    }
    friendlyName: string;
}

export interface IBoardOrientation {
    color: Color;
    opposite: () => IBoardOrientation;
    shouldFlipBoard: boolean;
}

export class PlayerIIBoardOrientation implements IBoardOrientation {
    color: Color = new Color("black"); //todo: pass in
    opposite: () => IBoardOrientation = () => new PlayerIBoardOrientation();
    shouldFlipBoard: boolean = false;
}

export class PlayerIBoardOrientation implements IBoardOrientation {
    color: Color = new Color("white"); //todo: pass in
    opposite: () => IBoardOrientation = () => new PlayerIIBoardOrientation();
    shouldFlipBoard: boolean = true;
}
