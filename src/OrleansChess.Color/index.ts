import { Config, IColor, LightColors, DarkColors } from "./config";

import fs from 'fs';
import util from 'util';

const readFile = util.promisify(fs.readFile);
const writeFile = util.promisify(fs.writeFile);
const exists = util.promisify(fs.exists);
const mkdir = util.promisify(fs.mkdir);

abstract class AbstractPiece {
    constructor(private assetSrc: string) {}
    abstract srcIdentifier: string;
    public async svgContent() {
        var content = await readFile(`${this.assetSrc}/${this.srcIdentifier}.svg`);
        return content.toString();
    }
}

class WhitePawn extends AbstractPiece {
    srcIdentifier = 'wP';
}

async function createLightPawn(assetDir: string, destinationDir: string, color: IColor) {
    var piece = new WhitePawn(Config.source);
    var content = await piece.svgContent();

    var destination = `${destinationDir}/${color.friendly}`;
    var destinationExists = await exists(destination);

    if (!destinationExists) {
        await mkdir(destination);
    }
    await writeFile(`${destination}/P.svg`, content);
}

async function createLightPieces() {
    await LightColors.forEach(async color => {
        console.log(`Creating ${color.friendly} pieces`);
        await createLightPawn(Config.source, Config.destination, color);
    });
}

async function createDarkPieces() {
    await DarkColors.forEach(async color => {
        console.log(`Creating ${color.friendly} pieces`);
    });
}

async function main()  {
    await createLightPieces();
    await createDarkPieces();
}

main();