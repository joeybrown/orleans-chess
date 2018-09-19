import { Config, IColor, LightColors, DarkColors } from "./config";
import { exists, mkdir, writeFile, ensureDirectoryExists } from "./util";
import { WhitePawn } from "./pieces/WhitePawn";

async function createLightPawn(destination: string, color: IColor) {
    var piece = new WhitePawn(Config.source);
    var content = await piece.svgContent();
    
    await writeFile(`${destination}/P.svg`, content);
}

async function createLightPieces() {
    await LightColors.forEach(async color => {
        console.log(`Creating ${color.friendly} pieces`);
        var destination = `${Config.destination}/${color.friendly}`;
        await ensureDirectoryExists(destination);
        await createLightPawn(destination, color);
    });
}

async function createDarkPieces() {
    await DarkColors.forEach(async color => {
        console.log(`Creating ${color.friendly} pieces`);
    });
}

async function main() {
    await createLightPieces();
    await createDarkPieces();
}

main();