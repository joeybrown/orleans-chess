import { AppHttpService } from "../http/app-http.service";
import { Injectable } from "@angular/core";

@Injectable()
export class AppAuthService {
    constructor(private appHttpService: AppHttpService) {
        console.log('AppAuthService Instantiated');
    }

    ensureUserHasPlayerId = () => {
        return this.appHttpService.get('api/user/ensureUserHasPlayerId');
    }
}