import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations'
import { AppComponent } from './app.component';
import { BoardComponent } from "./board/board.component";
import { SourcesComponent } from "./shoutout/sources.component";
import { AppHttpModule } from "./http/app-http.module";
import { ToastrModule } from 'ng6-toastr-notifications';

@NgModule({
  declarations: [
    AppComponent,
    BoardComponent,
    SourcesComponent
  ],
  imports: [
    BrowserModule,
    AppHttpModule,
    BrowserAnimationsModule,
    ToastrModule.forRoot()
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
