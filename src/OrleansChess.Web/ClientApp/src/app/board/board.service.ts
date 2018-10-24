import { Injectable } from '@angular/core';
import { AppHttpService } from "../http/app-http.service";
import { Observable } from 'rxjs/Observable';
import { of } from 'rxjs/observable/of';
import { fromPromise } from 'rxjs/observable/fromPromise';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { HubConnection } from "@aspnet/signalr";
import * as signalR from "@aspnet/signalr";
import { BoardState } from "../models/BoardState";
import { SuccessOrErrors } from "../models/SuccessOrErrors";
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

@Injectable()
export class BoardService {
    private connection: HubConnection = null;
    private eTag: BehaviorSubject<string> = new BehaviorSubject(null);

    constructor(private appHttpService: AppHttpService) {
    }

    private gameSeatActivity: (method: string, gameId: string) => Observable<SuccessOrErrors<BoardState>> =
        (method: string, gameId: string) =>
            this.ensureConnectionInitialized()
                .pipe(switchMap(()=>fromPromise(this.connection.invoke(method, gameId))));
            

    private playerMove: (method: string, gameId: string, originalPosition: string, newPosition: string) => Observable<SuccessOrErrors<BoardState>> =
        (method, gameId, originalPosition, newPosition) =>
             this.eTag
                .pipe(switchMap(eTag => fromPromise(this.connection.invoke(method, gameId, originalPosition, newPosition, eTag))))
                .pipe(tap(x => {
                    if (x.wasSuccessful) {
                        this.eTag.next(x.data.eTag);
                    }
                }));

    playerIJoinGame = gameId => this.gameSeatActivity("PlayerIJoinGame", gameId);

    playerIMove = (gameId, originalPosition, newPosition) =>
        this.playerMove("PlayerIMove", gameId, originalPosition, newPosition);

    playerIIMove = (gameId, originalPosition, newPosition) =>
        this.playerMove("PlayerIIMove", gameId, originalPosition, newPosition);

    playerIIJoinGame = gameId => this.gameSeatActivity("PlayerIIJoinGame", gameId);

    ensureConnectionInitialized = () => {
        if (this.connection)
            return of(null);

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
            }))
            .pipe(tap(x => {
                if (x.wasSuccessful) {
                    this.eTag.next(x.data.eTag);
                }
            }));
}