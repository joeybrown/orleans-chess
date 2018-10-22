import { Injectable } from '@angular/core';
import { AppHttpService } from "../http/app-http.service";
import { Observable } from 'rxjs/Observable';
import { of } from 'rxjs/observable/of';
import { fromPromise } from 'rxjs/observable/fromPromise';
import { catchError, switchMap } from 'rxjs/operators';
import { HubConnection } from "@aspnet/signalr";
import * as signalR from "@aspnet/signalr";
import { BoardState } from "../models/BoardState";
import { SuccessOrErrors } from "../models/SuccessOrErrors";

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

    playerIJoinGame = gameId => this.gameSeatActivity("PlayerIJoinGame", gameId);

    playerIMove = (gameId, originalPosition, newPosition, eTag) =>
        this.playerMove("PlayerIMove", gameId, originalPosition, newPosition, eTag);

    playerIIMove = (gameId, originalPosition, newPosition, eTag) =>
        this.playerMove("PlayerIIMove", gameId, originalPosition, newPosition, eTag);

    playerIIJoinGame = gameId => this.gameSeatActivity("PlayerIIJoinGame", gameId);

    ensureConnectionInitialized = () => {
        if (this.connection)
            Observable.of(null);

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/chesshub")
            .build();

        // this.connection.on("PlayerIIJoined", () => {
        //     console.log("PlayerII joined.")
        // });

        // this.connection.on("PlayerIJoined", () => {
        //     console.log("PlayerI joined.")
        // });
        return fromPromise(this.connection.start().catch(err => console.error(err.toString())));
    }

    getBoardState: (gameId) => Observable<SuccessOrErrors<BoardState>> = gameId =>
        this.ensureConnectionInitialized()
            .pipe(switchMap(x => fromPromise(this.connection.invoke("GetBoardState", gameId) as Promise<SuccessOrErrors<BoardState>>)))
            .pipe(catchError(error => {
                console.log(error.message);
                var result = new SuccessOrErrors<BoardState>();
                result.wasSuccessful = false;
                result.errors = [error.message];
                return of(result);
            }));
}