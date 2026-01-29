import {
    DATE_GROUNDING,
    prompt,
    RESPONSE_EXTRACT_TASK,
    RESPONSE_STRUCTURE,
} from './google.mts';

export const website = 'https://forestetterem.hu';

async function getImageLink() {
    const foresthtml = await (await fetch(website)).text();

    console.log(
        `Extracting image link from html... ${foresthtml.length} chars`
    );
    const response = await prompt([
        {
            text:
                'Extract the image links for below "heti étlap" from this piece of html. Only return the links, nothing else. ' +
                foresthtml,
        },
    ]);
    console.log(response);

    return (response || '').replaceAll('```', '').trim();
}

export async function getForestOffers() {
    const imageLink = await getImageLink();

    const arrayBuf = await (
        await fetch(imageLink.replace('http://', 'https://'))
    ).arrayBuffer();
    const base64ImageFile = Buffer.from(arrayBuf).toString('base64');

    const contents = [
        {
            inlineData: {
                mimeType: 'image/jpeg',
                data: base64ImageFile,
            },
        },
        {
            text: `${RESPONSE_EXTRACT_TASK} ${RESPONSE_STRUCTURE} ${DATE_GROUNDING}`,
        },
    ];
    const response = await prompt(contents);
    console.log(response);
    return JSON.parse(response || '{}');
}
