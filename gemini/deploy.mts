import {
    InvokeCommand,
    LambdaClient,
    UpdateFunctionCodeCommand,
} from '@aws-sdk/client-lambda';
import { readFileSync } from 'node:fs';
import { spawnSync } from 'node:child_process';
import { join } from 'node:path';

const STAGING_DIRECTORY = 'staging';
const BUNDLE_NAME = 'bundle.zip';

spawnSync('zip', [BUNDLE_NAME, 'index.cjs'], { cwd: `./${STAGING_DIRECTORY}` });

const client = new LambdaClient({ region: 'eu-south-1' });

const zipBuffer = readFileSync(join(STAGING_DIRECTORY, BUNDLE_NAME));

try {
    const updateCommand = new UpdateFunctionCodeCommand({
        FunctionName: 'debmenu_gemini_s3',
        ZipFile: zipBuffer,
    });

    const updateResponse = await client.send(updateCommand);
    console.log('Function updated successfully:', updateResponse);

    console.log('Wating 2 seconds for the update to propagate...');
    await new Promise((resolve) => setTimeout(resolve, 2000));
    console.log('Invoking function to verify deployment...');

    const invokeCommand = new InvokeCommand({
        FunctionName: 'debmenu_gemini_s3',
        Payload: '',
        InvocationType: 'RequestResponse', // Synchronous invocation,
        LogType: 'Tail',
    });
    const invokeResponse = await client.send(invokeCommand);
    console.log('Function invoked successfully:', invokeResponse.$metadata);
    console.log(Buffer.from(invokeResponse.LogResult!, 'base64').toString());
} catch (error) {
    console.error('Error updating function:', error);
}
