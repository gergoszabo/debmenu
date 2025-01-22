import { appendFileSync } from 'node:fs';

export function log(...things) {
    const data = `[${Temporal.Now.zonedDateTimeISO()
        .toString()
        .replace('T', ' ')
        .replace('Z', '')}] ${things.join(' ')}\n`;
    appendFileSync('log.log', data);
    console.log(data.trim());
}
