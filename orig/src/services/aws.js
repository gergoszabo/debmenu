import clientReckognition from '@aws-sdk/client-rekognition';
import clientS3 from '@aws-sdk/client-s3';
import { fromIni } from '@aws-sdk/credential-providers';
import { existsSync, readFileSync, statSync, writeFileSync } from 'node:fs';
import process from 'node:process';
import { log } from '../_log.js';
const { DetectTextCommand, RekognitionClient } = clientReckognition;
const { S3Client, PutObjectCommand } = clientS3;

const CACHE_INTERVAL = 1 * 60 * 60 * 1000; // 1 hour

const detectTextCalled = [];
const uploadResultCalled = [];

export const getStats = () => {
    return {
        detectTextCalled: detectTextCalled.length,
        uploadResultCalled: uploadResultCalled.length,
    };
};

export const clearStats = () => {
    detectTextCalled.length = 0;
    uploadResultCalled.length = 0;
};

const getCredentials = () => {
    return fromIni();
};

export const detectText = async (fileName) => {
    if (!existsSync(fileName)) {
        throw new Error(`The provided ${fileName} does not exists!`);
    }

    const cacheFileName = fileName.split('.')[0] + '.aws.response.json';

    if (
        existsSync(cacheFileName) &&
        statSync(cacheFileName).mtimeMs > Date.now() - CACHE_INTERVAL
    ) {
        log(`Using cached response for ${fileName}`);
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
    detectTextCalled.push(new Date());
    const response = await client.send(command);

    writeFileSync(cacheFileName, JSON.stringify(response, null, 4));

    return response;
};

export const uploadResult = async () => {
    if (process.argv.includes('--skip-upload')) {
        console.log('Skipping upload to S3 bucket');
        return;
    }

    const files = [
        { key: 'index.html', file: 'result/index.html' },
        { key: 'tomorrow.html', file: 'result/tomorrow.html' },
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
            Body: readFileSync(file.file, { encoding: null }),
        };

        const putObjectCommand = new PutObjectCommand(putObjectRequest);
        uploadResultCalled.push(new Date());
        const response = await client.send(putObjectCommand);
        console.log(response);
    }
};
