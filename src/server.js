import 'temporal-polyfill/global';
import { log } from './_log.js';
import { generate } from './generate.js';

log('Started');
const whenToRun = ['06:01', '07:31', '11:01', '14:15'];
log('When to run', whenToRun.join(', '));

const shouldRunOnStart = process.argv.includes('--run-on-start');
let ranOnStart = false;

(async () => {
    do {
        try {
            const now = Temporal.Now.zonedDateTimeISO()
                .toString()
                .substring(11, 16);

            const shouldRun = whenToRun.includes(now);
            log(now, 'shouldRun:', shouldRun);
            if (shouldRun || (shouldRunOnStart && !ranOnStart)) {
                ranOnStart = true;
                log('Fetching');
                generate().catch(log);
            }

            // tick every minute
            await new Promise((resolve) => {
                setTimeout(resolve, 60_000);
            });
        } catch (e) {
            log('Error:', e);
        }
    } while (true);

    log('Ended');
})();
