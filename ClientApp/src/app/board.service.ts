import { Injectable } from '@angular/core';
import { AppHttpService } from "./http/app-http.service";
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/of';
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import { Subject } from "rxjs/Subject";


export class TryMoveResult {
    constructor(public successful: boolean, public fen:string){}
}

@Injectable()
export class BoardService{
    private initialFen = `rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR`;
    private count: number = 0;

    public fen = new Subject<string>();
    
    constructor(private appHttpService: AppHttpService) {
    }

    tryMove: (oldFen: string, newFen: string) => Observable<TryMoveResult> = (oldFen, newFen) => {
        const url = encodeURI(`api/tryMove/${newFen}`);
        console.log(url);
        this.count += 1;
        const treatAsSuccess = this.count % 3 !== 0;
        const success = new TryMoveResult(true, newFen);
        const failure = new TryMoveResult(false, oldFen)
        return Observable.of(treatAsSuccess ? success : failure).delay(500);
        // return this.appHttpService.get(url);
    }

    initialize: () => Observable<string> = () =>{
        return Observable.of(this.initialFen);
    }

}