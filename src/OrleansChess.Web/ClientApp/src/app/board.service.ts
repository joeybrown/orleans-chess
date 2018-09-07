import { Injectable } from '@angular/core';
import { AppHttpService } from "./http/app-http.service";
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/of';
import { of } from 'rxjs/observable/of';
import { fromPromise } from 'rxjs/observable/fromPromise';
import { catchError, map } from 'rxjs/operators';
import { Subject } from "rxjs/Subject";
import { HubConnection } from "@aspnet/signalr";
import * as signalR from "@aspnet/signalr";

export interface ISuccessOrErrors<T> {
    readonly data: T;
    readonly wasSuccessful: boolean;
    readonly errors: string[];
}

export interface IValueWithETag<T> {
    readonly value: T;
    readonly eTag: string;
}

export interface IFenWithETag extends IValueWithETag<string> {

}

@Injectable()
export class BoardService {
    private connection: HubConnection;

    private fen = new Subject<IFenWithETag>();

    constructor(private appHttpService: AppHttpService) {
    }

    private gameSeatActivity: (method: string, gameId: string) => Observable<ISuccessOrErrors<IFenWithETag>> =
        (method: string, gameId: string) =>
            fromPromise(this.connection.invoke(method, gameId))
                .pipe(catchError(error => of(`Error: ${error}`)));

    private playerMove: (method: string, gameId: string, originalPosition: string, newPosition: string, eTag: string) => Observable<ISuccessOrErrors<IFenWithETag>> =
        (method, gameId, originalPosition, newPosition, eTag) =>
            fromPromise(this.connection.invoke(method, gameId, originalPosition))
                .pipe(catchError(error => of(`Error: ${error}`)));

    whiteJoinGame = gameId => this.gameSeatActivity("WhiteJoinGame", gameId);

    whiteMove = (gameId, originalPosition, newPosition, eTag) =>
        this.playerMove("WhiteMove", gameId, originalPosition, newPosition, eTag);

    blackMove = (gameId, originalPosition, newPosition, eTag) =>
        this.playerMove("BlackMove", gameId, originalPosition, newPosition, eTag);

    blackJoinGame = gameId => this.gameSeatActivity("BlackJoinGame", gameId);

    fenStream = this.fen.asObservable();

    initialize: () => Observable<string> = () => {

        var fen = new Subject<string>();

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/chesshub")
            .build();

        this.connection.on("PositionUpdated", (newFen: IFenWithETag) => {
            this.fen.next(newFen);
        });

        this.connection.on("BlackJoined", () => {
            console.log("Black joined.")
        });

        this.connection.on("WhiteJoined", () => {
            console.log("White joined.")
        });

        this.connection.start().catch(err => console.error(err.toString()));

        return fen;
    }
}