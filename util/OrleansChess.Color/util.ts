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

export function readdir(path: string): Promise<string[]>{
    return new Promise(function (resolve, reject) {
        fs.readdir(path, function (error, result) {
            if (error) {
                reject(error);
            } else {
                resolve(result);
            }
        });
    });
}

export async function asyncForEach(array: any[], callback: (item: any, index?: number, original?: any[]) => any) {
    for (let index = 0; index < array.length; index++) {
      await callback(array[index], index, array)
    }
  }