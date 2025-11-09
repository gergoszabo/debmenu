import {
    prompt,
    RESPONSE_EXTRACT_TASK,
    RESPONSE_STRUCTURE,
} from './google.mts';

export const website = 'https://www.viktoriaetterem.hu/menu';

export async function getViktoriaOffers() {
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
