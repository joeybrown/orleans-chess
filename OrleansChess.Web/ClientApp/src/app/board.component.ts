import { Component, OnInit, Input } from '@angular/core';
import * as $ from 'jquery';
import { BoardService } from "./board.service";
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/pairwise';
import 'rxjs/add/operator/take';
import 'rxjs/add/operator/skip';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { IMove } from './utilities/IMove';

declare var ChessBoard;

@Component({
    selector: 'app-board',
    templateUrl: './board.component.html',
    styleUrls: ['./board.component.css'],
    providers: [BoardService]
})
export class BoardComponent implements OnInit {

    movedPiece = (source: string, target: string) => {
        if (source === target)
            return false;
    
        if (target === 'offboard')
            return false;
    
        return true;
    }
    
    applyFuncToMove = (move: IMove, applyFunc: (squareEl) => void) => {
        const squares = [move.source, move.target];
        squares.forEach(x => {
            const squareEl = $(`#board-${this.boardId}`).find(`.square-${x}`);
            applyFunc(squareEl);
        });
    }
    
    removeSquareClass =
        (move: IMove, cssClass: string) =>
            this.applyFuncToMove(move, (squareEl) =>
                squareEl.removeClass(cssClass));
    
    addSquareClass =
        (move: IMove, cssClass: string) =>
            this.applyFuncToMove(move, (squareEl) => {
                squareEl.addClass(cssClass);
            });
    
    setNewActiveCssClass = ((x: { current: IMove, previous: IMove }, cssClass: string) => {
        if (x.previous) {
            this.removeSquareClass(x.previous, cssClass);
        }
        if (x.current) {
            this.addSquareClass(x.current, cssClass);
        }
    });

    @Input() orientation: 'black' | 'white';
    @Input() size: number;
    @Input() boardId: string;


    private board: any;

    constructor(private readonly boardService: BoardService) {
    }

    private isValidating = new BehaviorSubject<IMove>(null);
    private mostRecentValidMove = new BehaviorSubject<IMove>(null);

    private onDrop = (source, target, piece, newPos, oldPos, orientation) => {
        const moveBack = (moveToUndo: IMove, fen: string) => {
            this.board.position(fen);
            this.addSquareClass(moveToUndo, 'invalidMove');
            setTimeout(()=>this.removeSquareClass(moveToUndo, 'invalidMove'), 500);
        }

        if (!this.movedPiece(source, target))
            return;

        const oldFen = ChessBoard.objToFen(oldPos);
        const newFen = ChessBoard.objToFen(newPos);

        var move = { source: source, target: target };
        this.isValidating.next(move);
        this.boardService.tryMove(oldFen, newFen).subscribe(
            canMove => {
                if (!canMove.successful) {
                    moveBack(move, oldFen);
                    return;
                }
                this.mostRecentValidMove.next(move);
                this.board.position(canMove.fen);
            },
            error => {
                console.log(error)
                moveBack(move, oldFen);
            },
            () => this.isValidating.next(null));
    };

    private onDragStart = (source, piece, position, orientation) => {
        if (this.isValidating.getValue())
            return false;
    }

    private boardConfig = {
        draggable: true,
        onDrop: this.onDrop,
        onDragStart: this.onDragStart,
        start: null,
        pieceTheme: 'chessboardjs-0.3.0/img/chesspieces/wikipedia/{piece}.png'
    }

    ngOnInit(): void {

        this.boardService.initialize().subscribe(fen => {
            const boardConfig = this.boardConfig;
            boardConfig.start = fen;
            this.board = ChessBoard(`board-${this.boardId}`, boardConfig);
            this.board.start();
            if (this.orientation === 'white') {
                this.board.flip();
            }
        });

        this.boardService.fenStream.subscribe(fen => {
            this.board.position(fen);
        });

        this.mostRecentValidMove
            .pairwise()
            .map(x => { return { current: x[1], previous: x[0] }; })
            .subscribe(x => this.setNewActiveCssClass(x, 'mostRecentValidMove'));

        this.isValidating
            .pairwise()
            .map(x => { return { current: x[1], previous: x[0] }; })
            .subscribe(x => this.setNewActiveCssClass(x, 'isValidating'));
    }
}
