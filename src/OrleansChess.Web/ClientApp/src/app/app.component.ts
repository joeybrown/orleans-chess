import { Component, OnInit } from '@angular/core';
import { HttpParams } from '@angular/common/http';

const getQueryStringVal = (key) => {
  var keyValuePairs = location.search.substring(1).split('&').map(x => {
    const split = x.split('=');
    return {
      key: split[0],
      value: split[1]
    }
  });
  var kvp = keyValuePairs.find(x => x.key === key);
  if (!kvp)
    return;
  return kvp.value;
}

const defaultColor = 'black';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  ngOnInit(): void {
    this.setBoardOrientation();
  }

  private setBoardOrientation() {
    var orientationFromQs = getQueryStringVal('color');
    this.boardOrientation = orientationFromQs || defaultColor;
  }

  private boardOrientation: string;


}
