import clientReckognition from '@aws-sdk/client-rekognition';
import clientS3 from '@aws-sdk/client-s3';
import { fromIni } from '@aws-sdk/credential-providers';
import { existsSync, readFileSync, writeFileSync } from 'node:fs';
const { DetectTextCommand, RekognitionClient } = clientReckognition;
const { S3Client, PutObjectCommand } = clientS3;

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
        credentials: fromIni(),
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
    const files = ['result/index.html'];

    const client = new S3Client({
        credentials: fromIni(),
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
