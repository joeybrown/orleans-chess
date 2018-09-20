import { Config, LightColors, DarkColors } from "./config";
import { writeFile, ensureDirectoryExists } from "./util";
import { readdir, readFile } from "./util";

async function createLightPieces() {
    await LightColors.forEach(async color => {
        console.log(`Creating ${color.friendly} pieces`);
        var destination = `${Config.destination}/${color.friendly}`;
        await ensureDirectoryExists(destination);
        var files = (await readdir(Config.source)).filter((x: string)=>x.startsWith('w'));
        await files.forEach(async sourceFile => {
            var piece = `${Config.source}/${sourceFile}`;
            var content = (await readFile(piece)).toString();
            var newContent = content.split("#ffffff").join(color.hex);
            var fileName = sourceFile.substring(1);
            await writeFile(`${destination}/${fileName}`, newContent);
        });
    });
}

async function createDarkPieces() {
    await DarkColors.forEach(async color => {
        console.log(`Creating ${color.friendly} pieces`);
    });
}

async function main() {
    await createLightPieces();
    // await createDarkPieces();
}

main();