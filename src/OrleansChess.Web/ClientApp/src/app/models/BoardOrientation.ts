export type Color = 'white' | 'black';

export interface IBoardOrientation {
    color: Color;
    opposite: () => IBoardOrientation;
    shouldFlipBoard: boolean;
}

export class BlackBoardOrientation implements IBoardOrientation {
    color: Color = 'black';
    opposite: () => IBoardOrientation = () => new WhiteBoardOrientation();
    shouldFlipBoard: boolean = false;
}

export class WhiteBoardOrientation implements IBoardOrientation {
    color: Color = 'white';
    opposite: () => IBoardOrientation = () => new BlackBoardOrientation();
    shouldFlipBoard: boolean = true;
}
