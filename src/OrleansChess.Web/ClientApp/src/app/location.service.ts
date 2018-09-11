import { Injectable } from "@angular/core";

@Injectable()
export class LocationService {
    getQueryStringVal(key) {
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
}