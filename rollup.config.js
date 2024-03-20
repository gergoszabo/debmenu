export default {
    input: 'generate.js',
    output: {
        file: 'dist/generate.js',
        format: 'esm'
    },
    external: [
        'node:fs',
        'node:child_process',
        '@aws-sdk/client-rekognition',
        '@aws-sdk/client-s3',
        '@aws-sdk/credential-providers',
        'cheerio'
    ]
};
