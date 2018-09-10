import { Injectable } from '@angular/core';
import { AppHttpService } from "./http/app-http.service";
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/of';
import { of } from 'rxjs/observable/of';
import { fromPromise } from 'rxjs/observable/fromPromise';
import { catchError, map, switchMap } from 'rxjs/operators';
import { Subject } from "rxjs/Subject";
import { HubConnection } from "@aspnet/signalr";
import * as signalR from "@aspnet/signalr";
import { BoardState } from "./models/BoardState";
import { SuccessOrErrors } from "./models/SuccessOrErrors";

@Injectable()
export class BoardService {
    private connection: HubConnection;

    constructor(private appHttpService: AppHttpService) {
    }

    private gameSeatActivity: (method: string, gameId: string) => Observable<SuccessOrErrors<BoardState>> =
        (method: string, gameId: string) =>
            fromPromise(this.connection.invoke(method, gameId))
                .pipe(catchError(error => of(`Error: ${error}`)));

    private playerMove: (method: string, gameId: string, originalPosition: string, newPosition: string, eTag: string) => Observable<SuccessOrErrors<BoardState>> =
        (method, gameId, originalPosition, newPosition, eTag) =>
            fromPromise(this.connection.invoke(method, gameId, originalPosition))
                .pipe(catchError(error => of(`Error: ${error}`)));

    whiteJoinGame = gameId => this.gameSeatActivity("WhiteJoinGame", gameId);

    whiteMove = (gameId, originalPosition, newPosition, eTag) =>
        this.playerMove("WhiteMove", gameId, originalPosition, newPosition, eTag);

    blackMove = (gameId, originalPosition, newPosition, eTag) =>
        this.playerMove("BlackMove", gameId, originalPosition, newPosition, eTag);

    blackJoinGame = gameId => this.gameSeatActivity("BlackJoinGame", gameId);

    ensureConnectionInitialized = () => {
        if (this.connection)
            Observable.of(null);

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/chesshub")
            .build();

        // this.connection.on("BlackJoined", () => {
        //     console.log("Black joined.")
        // });

        // this.connection.on("WhiteJoined", () => {
        //     console.log("White joined.")
        // });
        return fromPromise(this.connection.start().catch(err => console.error(err.toString())));
    }

    getBoardState = gameId =>
        this.ensureConnectionInitialized()
            .pipe(switchMap(x=>fromPromise(this.connection.invoke("GetBoardState", gameId))))
            .pipe(catchError(error => of(`Error: ${error}`)));
}