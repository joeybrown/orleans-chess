export type Color = 'white' | 'black';

export interface IBoardOrientation {
    color: Color;
    opposite: Color;
}

export class WhiteBoardOrientation implements IBoardOrientation {
    color: Color = 'white';
    opposite: Color = 'black';
}

export class BlackBoardOrientation implements IBoardOrientation {
    color: Color = 'black';
    opposite: Color = 'white';
}