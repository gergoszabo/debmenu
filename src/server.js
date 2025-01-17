import 'temporal-polyfill/global';
import { appendFileSync } from 'node:fs';
import { generate } from './generate.js';

function log(...things) {
    const data = `[${Temporal.Now.zonedDateTimeISO()
        .toString()
        .replace('T', ' ')
        .replace('Z', '')}] ${things.join(' ')}\n`;
    appendFileSync('log.log', data);
    console.log(data.trim());
}

log('Started');
const whenToRun = ['06:01', '07:31', '11:01', '14:15'];
log('When to run', whenToRun.join(', '));

(async () => {
    do {
        const now = Temporal.Now.zonedDateTimeISO()
            .toString()
            .substring(11, 16);

        const shouldRun = whenToRun.includes(now);
        log(now, 'shouldRun:', shouldRun);
        if (shouldRun) {
            log('Fetching');
            generate().catch(log);
        }

        // tick every minute
        await new Promise((resolve) => {
            setTimeout(resolve, 60_000);
        });
    } while (true);
})();
