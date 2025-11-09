import { existsSync, mkdirSync, writeFileSync } from 'node:fs';

export const RESULT_FOLDER = 'result';
export const CACHE_FOLDER = 'cache';

export function createCacheFolder() {
    return createFolder(CACHE_FOLDER);
}

export function createResultFolder() {
    return createFolder(RESULT_FOLDER);
}

function createFolder(folder) {
    if (!existsSync(folder)) {
        mkdirSync(folder);
    }
}

export async function cacheOrFetch(filename, fetchUrl, type) {
    const cacheFileName = `${CACHE_FOLDER}/${filename}.cache.${type}`;
    let result;
    console.log(`GET ${fetchUrl}`);
    switch (type) {
        case 'json':
            result = await (await fetch(fetchUrl)).json();
            writeFileSync(cacheFileName, JSON.stringify(result, null, 4));
            break;
        case 'html':
            result = await (await fetch(fetchUrl)).text();
            writeFileSync(cacheFileName, result);
            break;
        case 'jpg':
        case 'png':
            result = await (await fetch(fetchUrl)).arrayBuffer();
            result = Buffer.from(result);
            writeFileSync(cacheFileName, result);
            break;
        default:
            throw new Error(`Not implemented case! ${type}`);
    }
    return result;
}
