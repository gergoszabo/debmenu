import { spawnSync } from 'node:child_process';
import { log } from './_log.js';

export function exec(command, args) {
    log('EXEC', command, args.join(' '));
    spawnSync(command, args, {
        encoding: 'utf8',
        stdio: 'inherit',
        cwd: process.cwd(),
    });
}
