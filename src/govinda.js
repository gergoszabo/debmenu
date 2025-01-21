import * as cheerio from 'cheerio';
import { log } from './_log.js';
import { CACHE_FOLDER, cacheOrFetch, RESULT_FOLDER } from './_cache.js';
import { detectText } from './services/aws.js';
import { exec } from './_exec.js';

const fetchUrl = 'https://www.govindadebrecen.hu/';
const website = 'https://www.govindadebrecen.hu/';
const shortName = 'govinda';
const name = 'Govinda';

const startedAt = performance.now();
export const fetchGovinda = async () => {
    try {
        // fetch page or load it from cache
        const html = await cacheOrFetch(shortName, fetchUrl, 'html');

        // parse out image src
        const $ = cheerio.load(html);
        const imgSrc = $('.menu-img').attr('src');

        // fetch and cache the image
        await cacheOrFetch(shortName, fetchUrl + imgSrc, 'png');

        const cropFileNames = createCropImages();

        const offers = [];

        const threshold = 0.05;
        for (const filename of cropFileNames) {
            console.log(`Processing ${filename}`);
            const response = await detectText(filename);

            let previousBoxTop = -1;
            const offersForTheDay = [];
            const lines = (response.TextDetections || [])?.filter(
                (td) => td.Type === 'LINE'
            );

            for (let index = 1; index < lines.length; index++) {
                const line = lines[index];
                if (
                    (line?.Geometry?.BoundingBox?.Top || 0) - previousBoxTop <
                    threshold
                ) {
                    offersForTheDay[offersForTheDay.length - 1] +=
                        line?.DetectedText;
                } else {
                    offersForTheDay.push(line?.DetectedText || '');
                }
                previousBoxTop =
                    line.Geometry?.BoundingBox?.Top || previousBoxTop;
            }

            const dateLine = lines[0].DetectedText || '';
            const split = dateLine
                .replaceAll('.', ' ')
                .split(' ')
                .filter((s) => !!s);
            console.log(dateLine, split);
            const date = new Date(
                Date.parse(
                    `${new Date().getFullYear()}-${split[0]}-${
                        split[1]
                    }T00:00:00.000Z`
                )
            )
                .toISOString()
                .substring(0, 10)
                .replaceAll('-', '.');
            const dayStr = split[2];

            offers.push({
                date,
                day: dayStr,
                offers: offersForTheDay,
            });
        }

        return {
            shortName,
            fetchedIn: performance.now() - startedAt,
            fetchUrl,
            website,
            name,
            offers,
        };
    } catch (e) {
        return {
            shortName,
            fetchedIn: performance.now() - startedAt,
            name,
            errorReason: e.message,
            fetchUrl,
            offers: [],
            website,
        };
    }
};

function createCropImages() {
    // 1800×1286
    const originalHeight = 1286;
    const crops = [];
    crops[0] = [0, 0, 171, originalHeight];
    const diff = 325;
    const cropFileNames = [];
    for (let i = 0; i < 5; i++) {
        crops[i + 1] = [crops[i][0] + crops[i][2], 0, diff, originalHeight];
        const { cropFileName, cmd } = getImageCropCommand(
            [crops[i][0] + crops[i][2], 0, diff, originalHeight],
            i + 1
        );
        cropFileNames.push(cropFileName);
        exec(cmd[0], cmd.slice(1));
        // Bun.spawnSync({ cmd });
    }
    return cropFileNames;
}

function getImageCropCommand(bounds, index) {
    const cropFileName = `${RESULT_FOLDER}/${shortName}-${index}-crop.png`;
    const cropParams = `${bounds[2]}x${bounds[3]}+${bounds[0]}+${bounds[1]}!`;

    return {
        cmd: `convert ${CACHE_FOLDER}/${shortName}.cache.png -crop ${cropParams} ${cropFileName}`.split(
            ' '
        ),
        // cmd: `magick ${CACHE_FOLDER}/${shortName}.cache.png -crop ${cropParams} ${cropFileName}`
        //     .split(' '),
        cropFileName,
    };
}
