import { Component } from '@angular/core';


@Component({
    selector: 'app-sources',
    templateUrl: './sources.component.html',
})
export class SourcesComponent {
    sources = [
        { text: 'Orleans', href: 'https://dotnet.github.io/orleans/' },
        { text: 'chessboard.js', href: 'http://chessboardjs.com' },
        { text: 'Chess.NET', href: 'https://github.com/ProgramFOX/Chess.NET' },
        { text: 'Chess Pieces', href: 'https://commons.wikimedia.org/wiki/Category:SVG_chess_pieces' }
    ]
}
