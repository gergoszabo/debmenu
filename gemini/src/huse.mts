import {
    prompt,
    RESPONSE_EXTRACT_TASK,
    RESPONSE_STRUCTURE,
} from './google.mts';

export const website = 'https://husevendeglodebrecen.hu/napi-ajanlat/';

export async function getHuseOffers() {
    const html = await (await fetch(website)).text();

    const contents = [
        {
            text: `${RESPONSE_EXTRACT_TASK} ${RESPONSE_STRUCTURE} ${html}`,
        },
    ];
    const response = await prompt(contents);
    console.log(response);

    return JSON.parse(response || '{}');
}
