import { Component, OnInit } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Color, PlayerIBoardOrientation, PlayerIIBoardOrientation, IBoardOrientation } from './models/BoardOrientation';
import { LocationService } from './location.service';

class BoardOrientationFactory {
  static BuildOrientation(orientation: string): IBoardOrientation {
    if (orientation === 'playerI')
      return new PlayerIBoardOrientation();
    return new PlayerIIBoardOrientation();
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
    var orientationFromQs = this.locationService.getQueryStringVal('orientation');
    this.boardOrientation = BoardOrientationFactory.BuildOrientation(orientationFromQs);
  }

  public boardOrientation: IBoardOrientation;
}
