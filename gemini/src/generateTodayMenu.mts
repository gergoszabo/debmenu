import { mkdir, readFile, writeFile } from 'fs/promises';
import { dirname, join } from 'path';
import { RESULT_FILE, RESULTS_DIR } from '../env.mts';

type ProviderMenu = {
    website: string;
    [date: string]: string[] | string;
};
type MenuData = Record<string, ProviderMenu>;

function localDateYYYYMMDD(d = new Date()): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
}

export async function generateHtml(): Promise<string[]> {
    const dataRaw = await readFile(RESULT_FILE, 'utf8');
    const data: MenuData = JSON.parse(dataRaw);

    const providers = Object.keys(data).sort();

    // Collect all unique date keys across providers (excluding 'website' key)
    const dateSet = new Set<string>();
    for (const provider of providers) {
        const providerData = data[provider] || {};
        for (const k of Object.keys(providerData)) {
            if (k !== 'website') dateSet.add(k);
        }
    }

    const allDates = Array.from(dateSet).sort();
    const todayKey = localDateYYYYMMDD(new Date());

    const filenameFor = (dateKey: string, justName: boolean = false) =>
        justName ? `${dateKey}.html` : join(RESULTS_DIR, `${dateKey}.html`);

    const buildHeader = (current: string) => {
        const parts: string[] = [];
        parts.push(
            `<header style="margin-bottom:12px;padding:6px;border-bottom:1px solid rgba(255,255,255,0.06)">`
        );
        parts.push(`<nav>`);
        parts.push(`<strong>Dates:</strong> `);
        const links: string[] = [];
        for (const d of allDates) {
            const fn = filenameFor(d, true);
            if (d === current) {
                links.push(
                    `<span style="margin-right:8px;font-weight:600">${d}</span>`
                );
            } else {
                links.push(
                    `<a href="${fn}" style="margin-right:8px;color:inherit;text-decoration:underline">${d}</a>`
                );
            }
        }
        parts.push(links.join(''));
        parts.push(`</nav>`);
        parts.push(`</header>`);
        return parts.join('\n');
    };

    const buildHtmlFor = (dateKey: string): string => {
        const parts: string[] = [];
        parts.push(`<!doctype html>`);
        parts.push(`<html lang="en">`);
        parts.push(`<head>`);
        parts.push(`<meta charset="utf-8"/>`);
        parts.push(
            `<meta name="viewport" content="width=device-width,initial-scale=1"/>`
        );
        parts.push(`<title>Menus — ${dateKey}</title>`);
        parts.push(
            `<style>body{background-color:black;color:white;font-family:system-ui,-apple-system,Segoe UI,Roboto,Ubuntu,'Helvetica Neue',Arial;padding:12px;}h1{margin-top:0}.sections-wrapper{display:grid;grid-template-columns:1fr;gap:12px}section{margin:0;padding:12px;border-radius:6px;box-shadow:0 1px 0 rgba(0,0,0,0.04)}ul{margin:8px 0 0 16px}li{text-transform:capitalize;}a{color:inherit}@media (min-width:1440px) and (hover:hover) and (pointer:fine){.sections-wrapper{grid-template-columns:1fr 1fr}}</style>`
        );
        parts.push(`</head>`);
        parts.push(`<body>`);

        // header with links to all date files
        parts.push(buildHeader(dateKey));

        parts.push(`<h1>${dateKey}</h1>`);
        parts.push(`<p>Generated: ${new Date().toLocaleString()}</p>`);

        parts.push(`<div class="sections-wrapper">`);
        for (const provider of providers) {
            parts.push(`<section id="${provider}">`);
            const providerData = data[provider] || {};
            const website =
                typeof providerData.website === 'string'
                    ? providerData.website
                    : '';
            // h2 with anchor to website if present
            if (website) {
                parts.push(
                    `<h2><a href="${escapeHtml(
                        website
                    )}" target="_blank" rel="noopener noreferrer">${provider}</a></h2>`
                );
            } else {
                parts.push(`<h2>${provider}</h2>`);
            }

            const menuForDate = providerData[dateKey];
            if (Array.isArray(menuForDate) && menuForDate.length > 0) {
                parts.push(`<ul>`);
                for (const item of menuForDate) {
                    parts.push(`<li>${escapeHtml(item.toLowerCase())}</li>`);
                }
                parts.push(`</ul>`);
            } else {
                parts.push(`<p><em>No offerings for this date.</em></p>`);
            }

            parts.push(`</section>`);
        }
        parts.push(`</div>`);

        parts.push(`</body>`);
        parts.push(`</html>`);

        return parts.join('\n');
    };

    const written: string[] = [];

    // Write one HTML file per date key
    for (const dateKey of allDates) {
        const fp = filenameFor(dateKey);
        const html = buildHtmlFor(dateKey);
        await mkdir(dirname(fp), { recursive: true });
        await writeFile(fp, html, 'utf8');
        written.push(fp);
    }

    // Create index.html that points to today's file (copy today's page if present)
    if (allDates.includes(todayKey)) {
        const todayFile = filenameFor(todayKey);
        // copy content of today's file into index.html
        const todayHtml = buildHtmlFor(todayKey);
        await writeFile(join(RESULTS_DIR, 'index.html'), todayHtml, 'utf8');
        written.unshift(join(RESULTS_DIR, 'index.html'));
    } else {
        // create a simple index with links
        const header = buildHeader('');
        const parts = [
            `<!doctype html>`,
            `<html lang="en">`,
            `<head>`,
            `<meta charset="utf-8"/>`,
            `<meta name="viewport" content="width=device-width,initial-scale=1"/>`,
            `<title>Menus</title>`,
            `<style>body{background-color:black;color:white;font-family:system-ui,-apple-system,Segoe UI,Roboto,Ubuntu,'Helvetica Neue',Arial;padding:12px;}a{color:inherit;text-decoration:underline}.sections-wrapper{display:grid;grid-template-columns:1fr;gap:12px}section{margin:0;padding:12px;border-radius:6px;box-shadow:0 1px 0 rgba(0,0,0,0.04)}@media (min-width:1440px) and (hover:hover) and (pointer:fine){.sections-wrapper{grid-template-columns:1fr 1fr}}</style>`,
            `</head>`,
            `<body>`,
            header,
            `<h1>Menus</h1>`,
            `<p>No menu for today. Pick a date:</p>`,
            `</body>`,
            `</html>`,
        ].join('\n');
        await writeFile(join(RESULTS_DIR, 'index.html'), parts, 'utf8');
        written.unshift(join(RESULTS_DIR, 'index.html'));
    }

    return written;
}

function escapeHtml(s: string): string {
    return s
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');
}
