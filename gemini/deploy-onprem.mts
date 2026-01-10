import { spawnSync } from 'node:child_process';
import 'dotenv/config';
import { join } from 'node:path';
import { cwd } from 'node:process';

const target = process.env.ONPREM_DOMAIN;
const username = process.env.ONPREM_USER;

if (!target) {
    console.error('Missing ONPREM_DOMAIN env variable!');
    process.exit(1);
}

spawnSync(
    'scp',
    [
        join(cwd(), 'staging', 'debmenu.cjs'),
        `${username}@${target}:/apps/debmenu/debmenu.cjs`,
    ],
    { stdio: 'inherit' }
);

console.log('Deploy onprem', process.env.ONPREM_DOMAIN);
