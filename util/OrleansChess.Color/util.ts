import fs from 'fs';
import util from 'util';

export const readFile = util.promisify(fs.readFile);
export const writeFile = util.promisify(fs.writeFile);
export const exists = util.promisify(fs.exists);
export const mkdir = util.promisify(fs.mkdir);

export async function ensureDirectoryExists(directory: string) {
    var destinationExists = await exists(directory);
    if (!destinationExists) {
        await mkdir(directory);
    }
}