import { readFile } from "../util";

export abstract class AbstractPiece {
    constructor(private assetSrc: string) {}
    abstract srcIdentifier: string;
    public async svgContent() {
        var content = await readFile(`${this.assetSrc}/${this.srcIdentifier}.svg`);
        return content.toString();
    }
}