import { NgModule } from '@angular/core';
import { AppAuthService } from './app-auth.service';
import { AppHttpModule } from '../http/app-http.module';

@NgModule({
    imports: [AppHttpModule],
    providers: [
        AppAuthService
    ]
})
export class AppAuthModule {}
