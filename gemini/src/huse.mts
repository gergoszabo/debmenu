import {
    DATE_GROUNDING,
    prompt,
    RESPONSE_EXTRACT_TASK,
    RESPONSE_STRUCTURE,
} from './google.mts';

export const website = 'http://www.husevendeglo.hu';

async function getImageLink() {
    const husehtml = await (await fetch(website)).text();

    console.log(`Extracting image link from html... ${husehtml.length} chars`);
    const response = await prompt([
        {
            text:
                'Extract the image links for below "HETI MENÜ" from this piece of html. Only return the links, nothing else.' +
                husehtml,
        },
    ]);
    console.log(response);

    return (response || '').replaceAll('```', '').trim();
}

export async function getHuseOffers() {
    const imageLink = await getImageLink();

    const arrayBuf = await (await fetch(website + imageLink)).arrayBuffer();
    const base64ImageFile = Buffer.from(arrayBuf).toString('base64');

    const contents = [
        {
            inlineData: {
                mimeType: 'image/jpeg',
                data: base64ImageFile,
            },
        },
        {
            text: `${RESPONSE_EXTRACT_TASK} ${RESPONSE_STRUCTURE}
            There is an offer that is available on everyday.
            instead of putting it a separate group, append it to the regular offers.
            The output should not contain the phrase " minden munkanap elérhető!".
             ${DATE_GROUNDING}`,
        },
    ];
    const response = await prompt(contents);
    console.log(response);

    return JSON.parse(response || '{}');
}
