import { existsSync, mkdirSync, readFileSync, rmSync, statSync, writeFileSync } from 'node:fs';

export const RESULT_FOLDER = 'result';
export const CACHE_FOLDER = 'cache';
const CACHE_INTERVAL = 1 * 60 * 60 * 1000; // 1 hour

const noCache = process.argv.includes('--no-cache');

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
    if (
        !noCache &&
        existsSync(cacheFileName) &&
        statSync(cacheFileName).mtimeMs > (Date.now() - CACHE_INTERVAL)
    ) {
        console.log(`READ ${cacheFileName}`);
        switch (type) {
            case 'json':
                result = JSON.parse(readFileSync(cacheFileName, { encoding: 'utf8' }));
                break;
            case 'html':
                result = readFileSync(cacheFileName, { encoding: 'utf8' });
                break;
            case 'png':
                result = readFileSync(cacheFileName, { encoding: null });
                break;
            default:
                throw new Error(`Not implemented case! ${type}`);
        }
        return result;
    } else {
        if (existsSync(cacheFileName)) {
            rmSync(cacheFileName);
        }
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
}
