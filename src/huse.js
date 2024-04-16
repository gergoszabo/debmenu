import * as cheerio from 'cheerio';
import { CACHE_FOLDER, cacheOrFetch, RESULT_FOLDER } from './_cache.js';
import { getDateRange } from './_date.js';
import { detectText } from './services/aws.js';
import { spawnSync } from 'node:child_process';

const fetchUrl = 'http://www.husevendeglo.hu';
const website = 'http://www.husevendeglo.hu';
const shortName = 'huse';
const name = 'Hüse vendéglő';

const startedAt = performance.now();
export const fetchHuse = async () => {
    try {
        // fetch page or load it from cache
        const html = await cacheOrFetch(shortName, fetchUrl, 'html');

        // parse out image src
        const $ = cheerio.load(html);
        const imgSrc = $('div > div > p > img').attr('src');

        // fetch and cache the image
        await cacheOrFetch(shortName, fetchUrl + imgSrc, 'png');

        // adjust huse image to make the background image and text disappear

        // brew install imagemagick
        // magick huse.cache.png -evaluate Add 10% huse.magicked.png
        const adjustedImageFileName = `${RESULT_FOLDER}/${shortName}.magick.png`;
        spawnSync('convert', [
            // const proc = spawnSync('magick', [
            `${CACHE_FOLDER}/${shortName}.cache.png`,
            '-evaluate',
            'Add',
            '10%',
            adjustedImageFileName,
        ], {
            encoding: 'utf8',
            stdio: 'inherit',
            cwd: process.cwd(),
        });

        // call aws detectText on the adjusted image
        const response = await detectText(adjustedImageFileName);
        const dates = parseDatesFromDateRangeLine(
            (response?.TextDetections ||
                [{ DetectedText: '' }, { DetectedText: '' }])[1].DetectedText || '',
        );

        console.log(response?.TextDetections[1]);
        console.log(dates);

        const offers = [];
        let selectedDate = '';
        let dateIndex = 0;
        // 0: Heti menü
        // 1: DateRange
        // 2: 2000 Ft
        // 3: days

        let superMenuLinesProcessed = 0;
        const superMenuLines = [];
        for (
            let index = 0;
            index < (response?.TextDetections?.length || 0);
            index++
        ) {
            const td = (response.TextDetections || [])[index];
            const text = td.DetectedText?.toLowerCase() || '';

            if (superMenuLinesProcessed) {
                // supermenu takes two line, needs to be added to every day
                superMenuLines.push(text);
                if (superMenuLinesProcessed > 1) {
                    offers.forEach((offer) => {
                        offer.offers.push(superMenuLines.join(' '));
                    });
                    break;
                }
                superMenuLinesProcessed++;
            } else if (DAYS.includes(text)) {
                const date = dates[dateIndex++];
                if (!date) {
                    break;
                }
                selectedDate = date
                    .toISOString()
                    .substring(0, 10)
                    .replaceAll('-', '.');

                offers.push({
                    date: selectedDate,
                    day: text,
                    offers: [],
                });
            } else if (text === '***') {
                superMenuLinesProcessed = 1;
            } else {
                const offer = offers.find((o) => o.date === selectedDate);
                if (offer?.offers?.length) {
                    (offer?.offers || [])[0] += ' ' + text;
                } else {
                    offer?.offers.push(text);
                }
            }
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

const DAYS = ['hétfö', 'kedd', 'szerda', 'csütörtök', 'péntek'];

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
    let dateRange = dateRangeLine
        .toLowerCase()
        .replace('között', '')
        .trim()
        .split('és')
        .map((str) =>
            new Date().getFullYear().toString() + '.' +
            str.split(' ')
                .map((s) => Object.keys(MONTH_DICT).includes(s) ? MONTH_DICT[s] : s)
                .filter((s) => s)
                .join('.')
                .replaceAll('..', '.')
        )
        .map((s) => s.substring(0, s.length - 1));

    // [2024.01.29,2024.02.2]
    // [2024.02.12,16]
    if (dateRange[1].split('').filter((c) => c === '.').length === 1) {
        // middle of month, second part does not contain the month
        const lastIndexOfDot = dateRange[0].lastIndexOf('.');
        const str = dateRange[0].substring(0, lastIndexOfDot) +
            dateRange[1].substring(dateRange[1].indexOf('.'));
        dateRange[1] = str;
    }
    dateRange = dateRange.map((dr) => dr.replaceAll('.', '-'));
    return getDateRange(dateRange[0], dateRange[1]);
}
