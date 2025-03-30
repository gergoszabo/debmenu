import * as cheerio from 'cheerio';
import { log } from './_log.js';
import { readFileSync } from 'node:fs';
import imageSize from '@coderosh/image-size';
import { CACHE_FOLDER, cacheOrFetch, RESULT_FOLDER } from './_cache.js';
import { getDateRange } from './_date.js';
import { detectText } from './services/aws.js';
import { exec } from './_exec.js';

const fetchUrl = 'https://forestetterem.hu/';
const website = 'https://forestetterem.hu';
const shortName = 'forest';
const name = 'Forest étterem';

const startedAt = performance.now();
export const fetchForest = async () => {
    try {
        const today = new Date().toISOString().substring(0, 10);
        const tomorrow = new Date(new Date().getTime() + 24 * 60 * 60 * 1000)
            .toISOString()
            .substring(0, 10);
        // fetch page or load it from cache
        const html = await cacheOrFetch(shortName, fetchUrl, 'html');

        // parse out image src
        const $ = cheerio.load(html);
        const imgSrc = $('.et_pb_lightbox_image').attr('href');

        // fetch and cache the image
        await cacheOrFetch(shortName, imgSrc, 'jpg');

        const offers = [];
        // adjust huse image to make the background image and text disappear

        // brew install imagemagick
        // magick huse.cache.png -evaluate Add 10% huse.magicked.png
        const adjustedImageFileName = `${RESULT_FOLDER}/${shortName}.magick.jpg`;
        exec('convert', [
            `${CACHE_FOLDER}/${shortName}.cache.jpg`,
            '-evaluate',
            'Add',
            '-20%',
            adjustedImageFileName,
        ]);
        // spawnSync(
        //     'convert',
        //     [
        //         `${CACHE_FOLDER}/${shortName}.cache.jpg`,
        //         '-evaluate',
        //         'Add',
        //         '-20%',
        //         adjustedImageFileName,
        //     ],
        //     {
        //         encoding: 'utf8',
        //         stdio: 'inherit',
        //         cwd: process.cwd(),
        //     }
        // );

        const cropFileNames = await createCropImages();

        const weekFileName = cropFileNames.shift();

        const weekDetectResponse = await detectText(weekFileName);
        const weekStrLine = weekDetectResponse?.TextDetections.find(
            (td) => td.Type === 'LINE'
        );
        console.log(weekStrLine?.DetectedText);
        // НЕТІ MENÜ ÁPRILIS 15 - 19.
        // HETI MENÜ ÁPRILIS 28 - MÁJUS 3.
        const week = weekStrLine.DetectedText.split(' ')
            .slice(2)
            .filter((s) => !!s)
            .join(' ');
        const dates = parseDatesFromDateRangeLine(week);

        let dateIndex = 0;
        const threshold = 0.035;
        for (const filename of cropFileNames) {
            const dateForFile =
                (dateIndex < dates.length &&
                    dates[dateIndex].toISOString().substring(0, 10)) ||
                '';

            if (![today, tomorrow].includes(dateForFile)) {
                dateIndex++;
                continue;
            }

            console.log(`Processing ${filename}`);
            const response = await detectText(filename);

            let previousBoxTop = -1;
            const offersForTheDay = [];
            const lines = (response.TextDetections || [])?.filter(
                (td) => td.Type === 'LINE'
            );

            // skip days when venue is closed
            if (lines.some((l) => l.DetectedText === 'ZÁRVA')) {
                continue;
            }

            for (let index = 1; index < lines.length; index++) {
                const line = lines[index];
                if (
                    (line?.Geometry?.BoundingBox?.Top || 0) - previousBoxTop <
                    threshold
                ) {
                    offersForTheDay[offersForTheDay.length - 1] +=
                        ' ' + line?.DetectedText.toLowerCase();
                } else {
                    offersForTheDay.push(
                        (line?.DetectedText || '').toLowerCase()
                    );
                }
                previousBoxTop =
                    line.Geometry?.BoundingBox?.Top || previousBoxTop;
            }

            offers.push({
                date: dates[dateIndex++]
                    .toISOString()
                    .substring(0, 10)
                    .replaceAll('-', '.'),
                day: 'text',
                offers: offersForTheDay.filter(
                    (o) => o !== 'levesek' && o !== 'föételek'
                ),
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
            errorReason: e.message + '\n' + e.stack,
            fetchUrl,
            offers: [],
            website,
        };
    }
};

const MONTH_DICT = {
    január: '01',
    januar: '01',
    február: '02',
    februar: '02',
    március: '03',
    marcius: '03',
    április: '04',
    aprilis: '04',
    május: '05',
    majus: '05',
    június: '06',
    junius: '06',
    július: '07',
    julius: '07',
    augusztus: '08',
    szeptember: '09',
    október: '10',
    oktober: '10',
    november: '11',
    december: '12',
};

function parseDatesFromDateRangeLine(dateRangeLine) {
    log('parseDatesFromDateRangeLine()', dateRangeLine);
    // ÁPRILIS 15 - 19.
    const x = dateRangeLine
        .toLowerCase()
        .trim()
        .replaceAll('.', '')
        .replaceAll('-', ' ')
        .split(' ')
        .filter((s) => !!s);
    // ['április', '15', '19']
    // ['április', '28', 'május', '3']

    const year = new Date().getFullYear().toString();
    const startDate = `${year}-${MONTH_DICT[x[0]]}-${x[1]}`;
    const endDate =
        x.length === 3
            ? `${year}-${MONTH_DICT[x[0]]}-${x[2]}`
            : `${year}-${MONTH_DICT[x[2]]}-${x[3]}`;

    log('getDateRange()', startDate, endDate);
    return getDateRange(startDate, endDate);
}

async function createCropImages() {
    const filename = readFileSync(`${RESULT_FOLDER}/${shortName}.magick.jpg`);
    // used to be 800x573, current it is 2000x1700
    // now it is 900x765 :)
    // lets try relative values instead of hardcoded values
    // the values were correct on 2000x1700 so lets adjust based on that
    const { width, height } = await imageSize(filename);
    const edgeWidth = (30 / 1700) * height;
    const cropWidth = (width - 2 * edgeWidth) / 5;
    const cropStartHeight = (150 / 1700) * height;
    const cropHeight = (900 / 1700) * height;
    const generateAdjustment = (step) =>
        (50 / 2000) * width - ((step * 20) / 2000) * width;
    const crops = [0, 1, 2, 3, 4].map((n) => [
        edgeWidth + cropWidth * n + generateAdjustment(n),
        cropStartHeight,
        cropWidth,
        cropHeight,
    ]);
    crops.unshift([0, 0, width, cropStartHeight]);

    const cropFileNames = [];
    for (let i = 0; i < 6; i++) {
        const { cropFileName, cmd } = getImageCropCommand(crops[i], i + 1);
        cropFileNames.push(cropFileName);
        exec(cmd[0], cmd.slice(1));
        // log('EXEC', cmd.join(' '));
        // spawnSync(cmd[0], cmd.slice(1));
    }
    return cropFileNames;
}

function getImageCropCommand(bounds, index) {
    const cropFileName = `${RESULT_FOLDER}/${shortName}-${index}-crop.jpg`;
    const cropParams = `${bounds[2]}x${bounds[3]}+${bounds[0]}+${bounds[1]}!`;

    return {
        cmd: `convert ${RESULT_FOLDER}/${shortName}.magick.jpg -crop ${cropParams} ${cropFileName}`.split(
            ' '
        ),
        cropFileName,
    };
}
