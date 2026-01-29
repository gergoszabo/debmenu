import { writeFileSync, rmSync, readdirSync, existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { generateHtml } from './generateTodayMenu.mts';
import { getStats } from './google.mts';
import { uploadResult } from './aws.mts';
import { getForestOffers, website as forestWebsite } from './forest.mts';
import { getGovindaOffers, website as govindaWebsite } from './govinda.mts';
import { getHuseOffers, website as huseWebsite } from './huse.mts';
import { getMannaOffers, website as mannaWebsite } from './manna.mts';
import { getViktoriaOffers, website as viktoriaWebsite } from './viktora.mts';
import { sendEmailSummary } from './email.mts';
import { RESULT_FILE, RESULTS_DIR } from '../env.mts';

export async function handler() {
    const startTime = Date.now();

    if (existsSync(RESULTS_DIR)) {
        readdirSync(RESULTS_DIR).forEach((file) => {
            if (file.endsWith('.html')) {
                console.log(`Removing old html file: ${file}`);
                rmSync(join(RESULTS_DIR, file));
            }
        });
    }

    const forest = await getForestOffers();
    const huse = await getHuseOffers();
    const govinda = await getGovindaOffers();
    const viktoria = await getViktoriaOffers();
    const manna = await getMannaOffers();

    writeFileSync(
        RESULT_FILE,
        JSON.stringify(
            {
                'Forest étterem': { ...forest, website: forestWebsite },
                'Hüse vendéglő': { ...huse, website: huseWebsite },
                'Govinda étterem': { ...govinda, website: govindaWebsite },
                'Viktória étterem': { ...viktoria, website: viktoriaWebsite },
                'Manna étterem': { ...manna, website: mannaWebsite },
            },
            null,
            2
        )
    );

    await generateHtml();

    const stats = getStats();
    console.log(stats);

    await uploadResult();

    const executionTimeMs = Date.now() - startTime;
    const indexHtmlPath = join(RESULTS_DIR, 'index.html');
    if (existsSync(indexHtmlPath)) {
        const htmlContent = readFileSync(indexHtmlPath, 'utf-8');
        await sendEmailSummary(stats.totalTokenCount, htmlContent, executionTimeMs);
    }
}

if (!process.env.AWS_LAMBDA_FUNCTION_NAME) {
    handler();
}
