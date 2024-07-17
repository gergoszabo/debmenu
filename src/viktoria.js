import { cacheOrFetch } from './_cache.js';
import { getDateRange } from './_date.js';
import * as cheerio from 'cheerio';

const fetchUrl = 'https://www.viktoriaetterem.hu/menu';
const website = 'https://www.viktoriaetterem.hu/menu';
const shortName = 'viktoria';
const name = 'Viktória étterem';

const startedAt = performance.now();
export const fetchViktoria = async () => {
    try {
        const menu = await cacheOrFetch(shortName, fetchUrl, 'html');

        const $ = cheerio.load(menu);
        const dates = getDateRangeFromHtml(menu);

        const $divs = $('.content-tab div');

        const offers = [];

        let currendDate = '';
        for (let index = 0; index < $divs.length; index++) {
            const div = $divs[index];

            if (!div.attribs['class']) {
                const date = dates.shift();
                if (date) {
                    currendDate = date.toISOString().substring(0, 10).replaceAll('-', '.');
                } else {
                    break;
                }

                if (div.firstChild?.nodeType === 3) {
                    offers.push({
                        date: currendDate,
                        day: div.firstChild.data.trim(),
                        offers: [],
                    });
                }
            } else if (div.attribs['class'] === 'featured-title') {
                const day = offers.find((d) => d.date === currendDate);
                const el = div.firstChild?.nextSibling;
                const data = (el && el.type === 'tag' && el.firstChild?.type === 'text' &&
                    el.firstChild.data.trim()) || '';
                if (day) {
                    day.offers.push(data.replaceAll('"', ''));
                }
            } // Handle special case, one per month
            else if (div.attribs['class'] === 'featured-desc') {
                const day = offers.find((d) => d.date === currendDate);

                if (div.children.length > 6) {
                    const strs = [];
                    for (let index = 0; index < div.children.length; index++) {
                        const element = div.children[index];
                        const text = $(element).text();
                        if (text.trim().length) {
                            strs.push(text.trim().replaceAll('"', ''));
                        }
                    }
                    const zsmenu = day?.offers.pop();
                    strs.unshift(zsmenu);
                    day?.offers.push(strs.join('<br>'));
                    // day?.offers.push(...strs);
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
            errorReason: e.message,
            name,
            fetchUrl,
            offers: [],
            website,
        };
    }
};

const MONTH_DICT = {
    'Január': '01',
    'Február': '02',
    'Március': '03',
    'Április': '04',
    'Május': '05',
    'Június': '06',
    'Július': '07',
    'Augusztus': '08',
    'Szeptember': '09',
    'Október': '10',
    'November': '11',
    'December': '12',
};

function getDateRangeFromHtml(html) {
    const $ = cheerio.load(html);
    const $p = $('p>strong').contents();
    const [start, end] = $p[0]['data']
        .replaceAll('..', '')
        .replaceAll('.', '')
        .replaceAll('\u00a0', ' ')
        .split('-')
        .map((s) => s.trim().split(' '));

    end.unshift(new Date().getFullYear().toString());

    const s = `${start[0]}-${MONTH_DICT[start[1]]}-${start[2]}`;
    const e = `${end[0]}-${MONTH_DICT[end[1]]}-${end[2]}`;
    return getDateRange(s, e);
}
