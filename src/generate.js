import { writeFileSync } from 'node:fs';
import { fetchManna } from './manna.js';
import { fetchViktoria } from './viktoria.js';
import { fetchHuse } from './huse.js';
import {
    createCacheFolder,
    createResultFolder,
    RESULT_FOLDER,
} from './_cache.js';
import { fetchGovinda } from './govinda.js';
import { TODAY, toHtml, TOMORROW } from './_template.js';
import { uploadResult } from './services/aws.js';
import { fetchForest } from './forest.js';
import { inspect } from 'node:util';
import { log } from './_log.js';

export async function generate() {
    createCacheFolder();
    createResultFolder();

    const fetchFunctions = [
        fetchForest,
        fetchHuse,
        fetchManna,
        fetchViktoria,
        fetchGovinda,
    ];

    const results = [];

    for (let index = 0; index < fetchFunctions.length; index++) {
        const res = await fetchFunctions[index]();
        console.log(res);
        log(inspect(res, false, null, true));
        results.push(res);
        writeFileSync(
            `${RESULT_FOLDER}/${res.shortName}.json`,
            JSON.stringify(res, null, 4)
        );
    }

    writeFileSync(`${RESULT_FOLDER}/index.html`, toHtml(results, TODAY));
    writeFileSync(`${RESULT_FOLDER}/tomorrow.html`, toHtml(results, TOMORROW));

    await uploadResult();
}
