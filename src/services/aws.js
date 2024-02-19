import clientReckognition from '@aws-sdk/client-rekognition';
import clientS3 from '@aws-sdk/client-s3';
import { fromIni, fromEnv } from '@aws-sdk/credential-providers';
import { existsSync, readFileSync, writeFileSync } from 'node:fs';
const { DetectTextCommand, RekognitionClient } = clientReckognition;
const { S3Client, PutObjectCommand } = clientS3;

const getCredentials = () => {
    try {
        const credentials = fromIni();
        return credentials;
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
    if (existsSync(cacheFileName)) {
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

    const files = ['result/index.html'];

    const client = new S3Client({
        credentials: getCredentials(),
        region: 'eu-central-2',
    });

    for (const file of files) {
        console.log(`PUT ${file}`);
        const putObjectRequest = {
            Key: 'index.html',
            Bucket: 'debmenuaws',
            ContentType: 'text/html',
            Body: readFileSync(file, { encoding: null })
        };

        const putObjectCommand = new PutObjectCommand(putObjectRequest);
        const response = await client.send(putObjectCommand);
        console.log(response);
    }
}
