export interface IBoardState {
    fen: string;
    originalPosition: string;
    newPosition: string;
}

export class BoardState implements IBoardState {
    fen: string;
    originalPosition: string;
    newPosition: string;
}