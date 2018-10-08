import { Component, OnInit, Input } from '@angular/core';
import * as $ from 'jquery';
import { BoardService } from "./board.service";
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/pairwise';
import 'rxjs/add/operator/take';
import 'rxjs/add/operator/skip';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { IMove } from '../utilities/IMove';
import { curry } from 'lodash';
import { Observable } from 'rxjs/Observable';
import { BoardState } from '../models/BoardState';
import { SuccessOrErrors } from '../models/SuccessOrErrors';
import { IBoardOrientation, PlayerIBoardOrientation, PlayerIIBoardOrientation } from '../models/BoardOrientation';
import { ToastrManager } from 'ng6-toastr-notifications';
import { AppAuthService } from '../auth/app-auth.service';
import { switchMap } from 'rxjs/operators';

declare var ChessBoard;

export class BoardComponentHelpers {
    static movedPiece = (source: string, target: string) => {
        if (source === target)
            return false;

        if (target === 'offboard')
            return false;

        return true;
    }

    static applyFuncToMove = (gameId: string, move: IMove, applyFunc: (squareEl) => void) => {
        const squares = [move.source, move.target];
        squares.forEach(x => {
            const squareEl = $(`#gameId-${gameId}`).find(`.square-${x}`);
            applyFunc(squareEl);
        });
    }

    static removeSquareClass = (gameId: string, move: IMove, cssClass: string) =>
        BoardComponentHelpers.applyFuncToMove(gameId, move, (squareEl) =>
            squareEl.removeClass(cssClass));

    static addSquareClass = (gameId: string, move: IMove, cssClass: string) =>
        BoardComponentHelpers.applyFuncToMove(gameId, move, (squareEl) =>
            squareEl.addClass(cssClass));

    static setNewActiveCssClass = (gameId: string, x: { current: IMove, previous: IMove }, cssClass: string) => {
        if (x.previous)
            BoardComponentHelpers.removeSquareClass(gameId, x.previous, cssClass);
        if (x.current)
            BoardComponentHelpers.addSquareClass(gameId, x.current, cssClass);
    }

    static onDragStart = (isValidating: IMove, source, piece, position, orientation) => !isValidating;
}

interface ISeatBehavior {
    joinGame(boardService: BoardService, gameId: string): Observable<SuccessOrErrors<BoardState>>;
    movePiece(boardService: BoardService, gameId: string, originalPosition: string, newPosition: string, eTag: string): Observable<SuccessOrErrors<BoardState>>;
    shouldFlipOrientation: boolean;
}

class SeatPlayerIBehavior implements ISeatBehavior {
    joinGame(boardService: BoardService, gameId: string): Observable<SuccessOrErrors<BoardState>> {
        return boardService.playerIJoinGame(gameId)
    }
    movePiece(boardService: BoardService, gameId: string, originalPosition: string, newPosition: string, eTag: string): Observable<SuccessOrErrors<BoardState>> {
        return boardService.playerIMove(gameId, originalPosition, newPosition, eTag)
    }
    shouldFlipOrientation = true;
}

class SeatPlayerIIBehavior implements ISeatBehavior {
    joinGame(boardService: BoardService, gameId: string): Observable<SuccessOrErrors<BoardState>> {
        return boardService.playerIIJoinGame(gameId)
    }
    movePiece(boardService: BoardService, gameId: string, originalPosition: string, newPosition: string, eTag: string): Observable<SuccessOrErrors<BoardState>> {
        return boardService.playerIIMove(gameId, originalPosition, newPosition, eTag)
    }
    shouldFlipOrientation = false;
}

class SeatBehaviorFactory {
    static buildSeatBehavior(orientation: IBoardOrientation) {
        switch (typeof (orientation)) {
            case (typeof (PlayerIIBoardOrientation)):
                return new SeatPlayerIIBehavior();
            case (typeof (PlayerIBoardOrientation)):
                return new SeatPlayerIBehavior();
            default:
                throw ('Unknown orientation');
        }
    }
}

@Component({
    selector: 'app-board',
    templateUrl: './board.component.html',
    styleUrls: ['./board.component.css'],
    providers: [BoardService]
})
export class BoardComponent implements OnInit {
    @Input() orientation: IBoardOrientation;
    @Input() size: number;
    @Input() gameId: string;

    private board: any;
    private seatBehavior: ISeatBehavior;

    constructor(private readonly boardService: BoardService,
        private readonly toastr: ToastrManager,
        private readonly authService: AppAuthService) {
    }

    private isValidating = new BehaviorSubject<IMove>(null);
    private mostRecentValidMove = new BehaviorSubject<IMove>(null);

    private movedPiece = BoardComponentHelpers.movedPiece;
    private removeSquareClass = curry(BoardComponentHelpers.removeSquareClass)(this.gameId);
    private addSquareClass = curry(BoardComponentHelpers.addSquareClass)(this.gameId);
    private setNewActiveCssClass = curry(BoardComponentHelpers.setNewActiveCssClass)(this.gameId);

    private onDrop = (source, target, piece, newPos, oldPos, orientation) => {
        // const moveBack = (moveToUndo: IMove, fen: string) => {
        //     this.board.position(fen);
        //     this.addSquareClass(moveToUndo, 'invalidMove');
        //     setTimeout(()=>this.removeSquareClass(moveToUndo, 'invalidMove'), 500);
        // }

        // if (!this.movedPiece(source, target))
        //     return;

        // const oldFen = ChessBoard.objToFen(oldPos);
        // const newFen = ChessBoard.objToFen(newPos);

        // var move = { source: source, target: target };
        // this.isValidating.next(move);
        // this.boardService.tryMove(oldFen, newFen).subscribe(
        //     canMove => {
        //         if (!canMove.successful) {
        //             moveBack(move, oldFen);
        //             return;
        //         }
        //         this.mostRecentValidMove.next(move);
        //         this.board.position(canMove.fen);
        //     },
        //     error => {
        //         console.log(error)
        //         moveBack(move, oldFen);
        //     },
        //     () => this.isValidating.next(null));
    };

    private pieceTheme = (piece: string) => {
        var isPlayerIPiece = piece.search(/w/) !== -1;
        var justPieceNoColor = piece[1];
        var color = isPlayerIPiece ? 'yellow' : 'lightblue';
        return `assets/images/chesspieces/${color}/${justPieceNoColor}.svg`;
    }

    private boardConfig = {
        draggable: true,
        onDrop: this.onDrop,
        onDragStart: curry(BoardComponentHelpers.onDragStart)(this.isValidating.getValue()),
        start: null,
        pieceTheme: this.pieceTheme
    }

    ngOnInit(): void {
        const setBoardOrientation = (board: any, boardOrientation: IBoardOrientation) => {
            if (this.orientation.shouldFlipBoard) {
                this.board.flip();
            }
        }

        var getBoardState = this.authService.ensureUserHasPlayerId()
            .pipe(switchMap((x => this.boardService.getBoardState(this.gameId))))
            .subscribe(x => {
                if (x.wasSuccessful) {
                    const boardConfig = this.boardConfig;
                    boardConfig.start = x.data.fen;
                    this.board = ChessBoard(`gameId-${this.gameId}`, boardConfig);
                    this.board.start();
                    setBoardOrientation(this.board, this.orientation);
                    return;
                }
                x.errors.forEach(x => {
                    this.toastr.errorToastr(x);
                });
            });
        // this.seatBehavior.joinGame(this.boardService, this.boardId).subscribe(x => {
        //     if (x.wasSuccessful) {
        //         const boardConfig = this.boardConfig;
        //         boardConfig.start = x.data.fen;
        //         this.board = ChessBoard(`board-${this.boardId}`, boardConfig);
        //         this.board.start();
        //         if (this.seatBehavior.shouldFlipOrientation) {
        //             this.board.flip();
        //         }
        //     }
        // });

        // var fenStream = this.boardService.initialize().subscribe(fen => {
        //     const boardConfig = this.boardConfig;
        //     boardConfig.start = fen;
        //     this.board = ChessBoard(`board-${this.boardId}`, boardConfig);
        //     this.board.start();
        //     if (this.orientation === 'playerI') {
        //         this.board.flip();
        //     }
        // });

        // this.boardService.fenStream.subscribe(fen => {
        //     this.board.position(fen);
        // });

        // this.mostRecentValidMove
        //     .pairwise()
        //     .map(x => { return { current: x[1], previous: x[0] }; })
        //     .subscribe(x => this.setNewActiveCssClass(x, 'mostRecentValidMove'));

        // this.isValidating
        //     .pairwise()
        //     .map(x => { return { current: x[1], previous: x[0] }; })
        //     .subscribe(x => this.setNewActiveCssClass(x, 'isValidating'));
    }
}
