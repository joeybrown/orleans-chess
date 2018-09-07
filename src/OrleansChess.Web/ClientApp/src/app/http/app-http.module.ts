import { NgModule } from '@angular/core';
import { AppHttpService } from "./app-http.service";
import { HttpClientModule } from "@angular/common/http";


@NgModule({
    imports: [HttpClientModule],
    providers: [
        AppHttpService
    ]
})
export class AppHttpModule { 
}
