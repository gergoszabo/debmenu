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

// Copy debmenu_run.sh script
spawnSync(
    'scp',
    [
        join(cwd(), 'debmenu_run.sh'),
        `${username}@${target}:/apps/debmenu/debmenu_run.sh`,
    ],
    { stdio: 'inherit' }
);

// todo: no infra setup in this repository yet...
spawnSync(
    'ssh',
    [ `${username}@${target}`, 'pm2 reload debmenu_run' ],
    { stdio: 'inherit' }
);

console.log('Deploy onprem', process.env.ONPREM_DOMAIN);
