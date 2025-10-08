import { build } from 'esbuild';
import { existsSync, rmSync } from 'node:fs';

if (existsSync('staging')) {
    rmSync('staging', { recursive: true });
}

build({
    entryPoints: ['src/index.mts'],
    bundle: true,
    platform: 'node',
    target: ['node22'],
    metafile: true,
    outfile: 'staging/index.cjs',
    format: 'cjs',
    minify: false,
    external: [
        '@aws-sdk/client-s3',
        '@aws-sdk/credential-providers',
        'child_process',
    ],
})
    .then((result) => {
        console.log(
            'Build succeeded:',
            Object.values(result.metafile.outputs)[0].bytes.toLocaleString(
                undefined,
                {
                    useGrouping: true,
                }
            ),
            'bytes'
        );
    })
    .catch(() => process.exit(1));
