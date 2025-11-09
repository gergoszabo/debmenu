import clientS3 from '@aws-sdk/client-s3';
import { fromIni } from '@aws-sdk/credential-providers';
import { readdirSync, readFileSync } from 'node:fs';
import { RESULTS_DIR } from '../env.mts';
import { join } from 'node:path';

const { S3Client, PutObjectCommand } = clientS3;

const getCredentials = () => {
    // lambda env variables
    if (process.env.AAKID && process.env.AAKSK) {
        return {
            accessKeyId: process.env.AAKID,
            secretAccessKey: process.env.AAKSK,
        };
    }
    // local env variables
    if (process.env.AWS_ACCESS_KEY_ID && process.env.AWS_SECRET_ACCESS_KEY) {
        return {
            accessKeyId: process.env.AWS_ACCESS_KEY_ID,
            secretAccessKey: process.env.AWS_SECRET_ACCESS_KEY,
        };
    }
    return fromIni();
};

export const uploadResult = async () => {
    const files = readdirSync(RESULTS_DIR).reduce((acc, file) => {
        if (file.endsWith('.html')) {
            acc.push({ key: file, path: join(RESULTS_DIR, file) });
        }
        return acc;
    }, [] as { key: string; path: string }[]);

    const client = new S3Client({
        credentials: getCredentials(),
        region: 'eu-central-2',
    });

    for (const file of files) {
        console.log(`PUT ${file.key} - ${file.path}`);
        const putObjectRequest = {
            Key: file.key,
            Bucket: 'debmenuaws',
            ContentType: 'text/html',
            Body: readFileSync(file.path, { encoding: null }),
        };

        const putObjectCommand = new PutObjectCommand(putObjectRequest);
        const response = await client.send(putObjectCommand);
        console.log(response);
    }
};
