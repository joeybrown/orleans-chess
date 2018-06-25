import { Injectable } from '@angular/core';
import { AppHttpService } from "./http/app-http.service";
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/of';
import { of } from 'rxjs/observable/of';
import { fromPromise } from 'rxjs/observable/fromPromise';
import { catchError, map } from 'rxjs/operators';
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import { Subject } from "rxjs/Subject";
import { HubConnection } from "@aspnet/signalr";
import * as signalR from "@aspnet/signalr";

export class TryMoveResult {
    constructor(public successful: boolean, public fen:string){}
}

@Injectable()
export class BoardService{
    private connection: HubConnection;

    private count: number = 0;

    private fen = new Subject<string>();
    
    constructor(private appHttpService: AppHttpService) {
    }

    tryMove: (oldFen: string, newFen: string) => Observable<TryMoveResult> = (oldFen, newFen) => {

        this.count += 1;
        const treatAsSuccess = this.count % 3 !== 0;

        if (!treatAsSuccess) {
            return Observable.of(new TryMoveResult(false, oldFen)).delay(500)
        }

        return fromPromise(this.connection.invoke("TryMove", newFen))
            .pipe(catchError(error => of(`Error: ${error}`)))
            .pipe(map(x => new TryMoveResult(true, newFen)));
    }

    fenStream = this.fen.asObservable();

    initialize: () => Observable<string> = () => {

        var fen = new Subject<string>();

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/chesshub")
            .build();

        this.connection.on("UpdateFen", (newFen) => {
            this.fen.next(newFen);
        })

        this.connection.on("InitializeFen", (initialFen) => {
            fen.next(initialFen);
            fen.complete();
        });

        this.connection.start().catch(err => console.error(err.toString()));        
        
        return fen;
    }
}