import { AppHttpService } from "../http/app-http.service";
import { Injectable } from "@angular/core";

@Injectable()
export class AppAuthService {
    constructor(private appHttpService: AppHttpService) {
    }

    ensureUserIsAuthenticated = () => {
        return this.appHttpService.get('api/user/ensureAuthenticated');
    }
}