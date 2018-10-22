import { Config, LightColors, DarkColors, IColor } from "./config";
import { writeFile, ensureDirectoryExists } from "./util";
import { readdir, readFile } from "./util";

async function createPiece(pieceFilter: (x: string) => boolean, hexToReplace: string, color: IColor) {
    console.log(`Creating ${color.friendly.toLowerCase()} pieces`);
    var destination = `${Config.destination}/${color.location}`;
    await ensureDirectoryExists(destination);
    var files = (await readdir(Config.source)).filter(pieceFilter);
    await files.forEach(async sourceFile => {
        var piece = `${Config.source}/${sourceFile}`;
        var content = (await readFile(piece)).toString();
        var newContent = content.split(hexToReplace).join(color.hex);
        var fileName = sourceFile.substring(1);
        await writeFile(`${destination}/${fileName}`, newContent);
    });
}

async function main() {
    await LightColors.forEach(async color => {
        const pieceFilter = (x: string) => x.startsWith('w');
        const hexToReplace = "#ffffff";
        await createPiece(pieceFilter, hexToReplace, color);
    });

    await DarkColors.forEach(async color => {
        const pieceFilter = (x: string) => x.startsWith('b');
        const hexToReplace = "#000000";
        await createPiece(pieceFilter, hexToReplace, color);
    });
}

main();