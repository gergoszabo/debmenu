import clientReckognition from '@aws-sdk/client-rekognition';
import clientS3 from '@aws-sdk/client-s3';
import { fromIni, fromEnv } from '@aws-sdk/credential-providers';
import { existsSync, readFileSync, statSync, writeFileSync } from 'node:fs';
const { DetectTextCommand, RekognitionClient } = clientReckognition;
const { S3Client, PutObjectCommand } = clientS3;

const CACHE_INTERVAL = 1 * 60 * 60 * 1000; // 1 hour

const getCredentials = () => {
    try {
        return fromIni();
    } catch (e) {
        console.error(e);
    }
    // let throw, no credentials provided
    return fromEnv();
}

export const detectText = async (fileName) => {
    if (!existsSync(fileName)) {
        throw new Error(`The provided ${fileName} does not exists!`);
    }

    const cacheFileName = fileName.split('.')[0] + '.aws.response.json';

    if (existsSync(cacheFileName) &&
        statSync(cacheFileName).mtimeMs > (Date.now() - CACHE_INTERVAL)) {
        return JSON.parse(readFileSync(cacheFileName));
    }

    const bytes = readFileSync(fileName, { encoding: null });

    const client = new RekognitionClient({
        region: 'eu-central-1',
        credentials: getCredentials(),
    });

    const input = {
        Image: {
            Bytes: bytes,
        },
    };

    const command = new DetectTextCommand(input);
    const response = await client.send(command);

    writeFileSync(cacheFileName, JSON.stringify(response, null, 4));

    return response;
};

export const uploadResult = async () => {
    if (process.argv.includes('--skip-upload')) {
        console.log('Skipping upload to S3 bucket');
        return;
    };

    const files = [
        { key: 'index.html', file: 'result/index.html' },
        { 'key': 'tomorrow.html', file: 'result/tomorrow.html' }
    ];

    const client = new S3Client({
        credentials: getCredentials(),
        region: 'eu-central-2',
    });

    for (const file of files) {
        console.log(`PUT ${file.key} - ${file.file}`);
        const putObjectRequest = {
            Key: file.key,
            Bucket: 'debmenuaws',
            ContentType: 'text/html',
            Body: readFileSync(file.file, { encoding: null })
        };

        const putObjectCommand = new PutObjectCommand(putObjectRequest);
        const response = await client.send(putObjectCommand);
        console.log(response);
    }
}
