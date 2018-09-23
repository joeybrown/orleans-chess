import { Component } from '@angular/core';


@Component({
  selector: 'app-sources',
  templateUrl: './sources.component.html',
})
export class SeatComponent {
    private sources = [
        {text: 'Orleans', href: 'https://dotnet.github.io/orleans/'},
        {text: 'chessboard.js', href: 'http://chessboardjs.com'},
        {text: 'Chess.NET', href: 'https://github.com/ProgramFOX/Chess.NET'}
    ]
}
