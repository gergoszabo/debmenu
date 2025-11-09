import { spawnSync } from 'node:child_process';
import { readFileSync, writeFileSync } from 'node:fs';

const version = spawnSync('git', ['log', '-1', '--format=%H'])
    .stdout.toString()
    .trim();

let content = readFileSync('./src/_template.js', { encoding: 'utf-8' });
content = content.replace(
    `const GIT_REV = '';`,
    `const GIT_REV = '${version}';`
);
writeFileSync('./src/_template.js', content, { encoding: 'utf-8' });
