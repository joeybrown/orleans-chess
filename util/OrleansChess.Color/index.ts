import { Config, IColor, LightColors, DarkColors } from "./config";
import { writeFile, ensureDirectoryExists } from "./util";
import { WhitePawn } from "./pieces/WhitePawn";
import { WhiteKnight } from "./pieces/WhiteKnight";
import { join } from "path";

async function createLightPawn(destination: string, color: IColor) {
    var piece = new WhitePawn(Config.source);
    var content = await piece.svgContent();
    var newContent = content.split("#ffffff").join(color.hex);
    await writeFile(`${destination}/P.svg`, newContent);
}

async function createLightKnight(destination: string, color: IColor) {
    var piece = new WhiteKnight(Config.source);
    var content = await piece.svgContent();
    var newContent = content.split("#ffffff").join(color.hex);
    await writeFile(`${destination}/N.svg`, newContent);
}

async function createLightPieces() {
    await LightColors.forEach(async color => {
        console.log(`Creating ${color.friendly} pieces`);
        var destination = `${Config.destination}/${color.friendly}`;
        await ensureDirectoryExists(destination);
        await createLightPawn(destination, color);
        await createLightKnight(destination, color);
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