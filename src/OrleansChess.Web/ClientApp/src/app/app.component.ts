import { Component, OnInit } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Color, WhiteBoardOrientation, BlackBoardOrientation, IBoardOrientation } from './models/BoardOrientation';
import { LocationService } from './location.service';

class BoardOrientationFactory {
  static BuildOrientation(orientation: string): IBoardOrientation {
    console.log(orientation);
    if (orientation === 'black')
      return new BlackBoardOrientation();
    return new WhiteBoardOrientation();
  }
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  providers: [LocationService]
})
export class AppComponent implements OnInit {
  constructor(private readonly locationService: LocationService) {
  }

  ngOnInit(): void {
    this.setBoardOrientation();
  }

  private setBoardOrientation() {
    var orientationFromQs = this.locationService.getQueryStringVal('color');
    this.boardOrientation = BoardOrientationFactory.BuildOrientation(orientationFromQs);
    console.log(this.boardOrientation);
  }

  public boardOrientation: IBoardOrientation;
}
