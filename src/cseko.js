import * as cheerio from 'cheerio';
import { spawnSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import imageSize from '@coderosh/image-size';
import { CACHE_FOLDER, cacheOrFetch, RESULT_FOLDER } from './_cache.js';
import { getDateRange } from './_date.js';
import { detectText } from './services/aws.js';

const fetchUrl = 'https://debrecen.csekokavehaz.hu/cms/heti-menu-debrecen/';
const website = 'https://debrecen.csekokavehaz.hu/';
const shortName = 'cseko';
const name = 'Csekő kávéház';

export const fetchCseko = async () => {
    const startedAt = performance.now();
    try {
        // fetch page or load it from cache
        const html = await cacheOrFetch(shortName, fetchUrl, 'html');

        // parse out image src
        const $ = cheerio.load(html);
        const imgSrc = $('img[srcset]').last().attr('srcset').split(',').find((src) => src.includes('2048w')).trim().split(' ')[0];

        // fetch and cache the image
        await cacheOrFetch(shortName, imgSrc, 'jpg');

        const cropFileNames = await createCropImages();

        const weekFileName = cropFileNames.shift();

        const weekDetectResponse = await detectText(weekFileName);
        const weekStrLine = weekDetectResponse?.TextDetections.find((td) => td.Type === 'LINE');
        console.log(weekStrLine?.DetectedText);
        // HETI MENÜ ÁPRILIS 29 - MÁJUS 3.
        const dates = parseDatesFromDateRangeLine(weekStrLine.DetectedText);

        const offers = [];
        let dateIndex = 0;
        const threshold = 0.1;
        for (const filename of cropFileNames) {
            console.log(`Processing ${filename}`);
            const response = await detectText(filename);

            let previousBoxTop = -1;
            const offersForTheDay = [];
            const lines = (response.TextDetections || [])?.filter((td) => td.Type === 'LINE');

            for (let index = 1; index < lines.length; index++) {
                const line = lines[index];
                console.log(
                    line?.Geometry?.BoundingBox?.Top,
                    previousBoxTop,
                    line?.Geometry?.BoundingBox?.Top - previousBoxTop,
                    threshold,
                    (line?.Geometry?.BoundingBox?.Top || 0) - previousBoxTop < threshold,
                );
                if ((line?.Geometry?.BoundingBox?.Top || 0) - previousBoxTop < threshold) {
                    offersForTheDay[offersForTheDay.length - 1] += ' ' + line?.DetectedText.toLowerCase();
                } else {
                    offersForTheDay.push((line?.DetectedText || '').toLowerCase());
                }
                previousBoxTop = line.Geometry?.BoundingBox?.Top || previousBoxTop;
            }

            offers.push({
                date: dates[dateIndex++].toISOString().substring(0, 10).replaceAll('-', '.'),
                day: 'text',
                offers: offersForTheDay.filter((o) => o !== 'levesek' && o !== 'föételek'),
            });
        }

        return {
            shortName,
            fetchedIn: performance.now() - startedAt,
            name,
            fetchUrl,
            offers,
            website,
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
    'január': '01',
    'február': '02',
    'március': '03',
    'április': '04',
    'május': '05',
    'június': '06',
    'július': '07',
    'augusztus': '08',
    'szeptember': '09',
    'október': '10',
    'november': '11',
    'december': '12',
};

function parseDatesFromDateRangeLine(dateRangeLine) {
    // 2024. ÁPRILIS 29. - MÁJUS 03.
    console.log(dateRangeLine);
    const x = dateRangeLine
        .toLowerCase()
        .trim()
        .replaceAll('.', '')
        .replaceAll('-', '')
        .split(' ')
        .filter((s) => !!s);
    // ['2024', 'április', '29', 'május', '03']
    console.log(x);

    // const year = new Date().getFullYear().toString();
    const startDate = `${x[0]}-${MONTH_DICT[x[1]]}-${x[2]}`;
    const endDate = (x.length === 5) ? `${x[0]}-${MONTH_DICT[x[3]]}-${x[4]}` : `${x[0]}-${MONTH_DICT[x[2]]}-${x[3]}`;

    return getDateRange(startDate, endDate);
}

async function createCropImages() {
    const filename = readFileSync(`${CACHE_FOLDER}/${shortName}.cache.jpg`);
    // 2048x1365
    const { width } = await imageSize(filename);
    const edgeWidth = 150;
    const cropWidth = (width - 3 * edgeWidth) / 5;
    const cropStartHeight = 300;
    const cropHeight = 750;
    const generateAdjustment = (step) => cropWidth * step + step * 5 + (step > 3 ? (step - 3) * 15 : 0);
    const crops = [0, 1, 2, 3, 4].map((n) => [edgeWidth + generateAdjustment(n), cropStartHeight, cropWidth, cropHeight]);
    // week header
    crops.unshift([270, 125, 500, 50]);

    const cropFileNames = [];
    for (let i = 0; i < 6; i++) {
        const { cropFileName, cmd } = getImageCropCommand(crops[i], i + 1);
        cropFileNames.push(cropFileName);
        spawnSync(cmd[0], cmd.slice(1));
    }
    return cropFileNames;
}

function getImageCropCommand(bounds, index) {
    const cropFileName = `${RESULT_FOLDER}/${shortName}-${index}-crop.jpg`;
    const cropParams = `${bounds[2]}x${bounds[3]}+${bounds[0]}+${bounds[1]}!`;

    return {
        cmd: `convert ${CACHE_FOLDER}/${shortName}.cache.jpg -crop ${cropParams} ${cropFileName}`
            .split(' '),
        cropFileName,
    };
}
