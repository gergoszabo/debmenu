import {
    DATE_GROUNDING,
    prompt,
    RESPONSE_EXTRACT_TASK,
    RESPONSE_STRUCTURE,
} from './google.mts';

export const website = 'https://www.govindadebrecen.hu/';

async function getImageLink() {
    const html = await (await fetch(website)).text();

    console.log(`Extracting image link from html... ${html.length} chars`);
    const response = await prompt([
        {
            text:
                'Extract the img tag with menu-img class from this piece of html. Only return the link, nothing else.' +
                html,
        },
    ]);
    console.log(response);

    return (response || '').replaceAll('```', '').trim();
}

export async function getGovindaOffers() {
    const imageLink = await getImageLink();

    const arrayBuf = await (await fetch(website + imageLink)).arrayBuffer();
    const base64ImageFile = Buffer.from(arrayBuf).toString('base64');

    const text = `${RESPONSE_EXTRACT_TASK} ${RESPONSE_STRUCTURE} don't create sub-categories  ${DATE_GROUNDING}`;
    const contents = [
        {
            inlineData: {
                mimeType: 'image/jpeg',
                data: base64ImageFile,
            },
        },
        {
            text,
        },
    ];
    const response = await prompt(contents);
    console.log(response);

    return JSON.parse(response || '{}');
}
