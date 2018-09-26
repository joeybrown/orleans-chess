import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import 'rxjs/add/observable/of';
import 'rxjs/add/operator/delay';

type httpOptions = {
    headers?: HttpHeaders;
    observe?: "body";
    params?: HttpParams;
    reportProgress?: boolean;
    responseType: "arraybuffer";
    withCredentials?: boolean;
}

@Injectable()
export class AppHttpService {

    constructor(private readonly httpClient: HttpClient) {
        console.log('AppHttpService Instance');
    }

    get = (url: string, options?: httpOptions) => {
        return this.httpClient.get(url, options)
    }

}