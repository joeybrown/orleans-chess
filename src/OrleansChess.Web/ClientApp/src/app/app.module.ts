import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { BoardComponent } from "./board.component";
import { SourcesComponent } from "./sources.component";
import { AppHttpModule } from "./http/app-http.module";


@NgModule({
  declarations: [
    AppComponent,
    BoardComponent,
    SourcesComponent
  ],
  imports: [
    BrowserModule,
    AppHttpModule
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
