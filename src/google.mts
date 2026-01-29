import { GoogleGenAI } from '@google/genai';
import type { ContentListUnion } from '@google/genai';
import { createHash } from 'crypto';
import 'dotenv/config';
import { writeFileSync, mkdirSync, existsSync, readFileSync } from 'fs';
import { CACHE_DIR } from '../env.mts';

// The client gets the API key from the environment variable `GEMINI_API_KEY`.
const ai = new GoogleGenAI({});

if (!existsSync(CACHE_DIR)) {
    mkdirSync(CACHE_DIR);
}

export const RESPONSE_EXTRACT_TASK = `
Extract the offers from the image grouped for each day 
day without headers and pricing information. 
it can happen that the daterange contains a typo.
`;

export const RESPONSE_STRUCTURE = `
Respond only with json.
The structure should be like this:

{
  "date": [
    "Offer 1",
    "Offer 2"
  ],
  "date": [
    "Offer 1",
    "Offer 2"
  ]
}

The keys are ISO date strings, and the values are arrays of strings representing the offers for that day.
`;

export const DATE_GROUNDING = `Dates are in the format YYYY-MM-DD.
The current date is ${new Date().toISOString().split('T')[0]}.`;
export const YEAR_GROUNDING = `The current year is ${new Date().getFullYear()}.`;

let modelCallCount = 0;
let totalTokenCount = 0;

export function getStats() {
    return {
        modelCallCount,
        totalTokenCount,
    };
}

export async function prompt(contents: ContentListUnion) {
    const hash = createHash('sha256')
        .update(JSON.stringify(contents))
        .digest('hex');
    const cacheFile = `${CACHE_DIR}/${hash}.json`;

    if (existsSync(cacheFile)) {
        console.log(`Cache hit for ${hash}`);
        return readFileSync(cacheFile, 'utf-8');
    }

    console.log(`Cache miss for ${hash}`);

    modelCallCount++;
    const response = await ai.models.generateContent({
        model: 'gemini-2.5-flash',
        contents,
        config: {
            thinkingConfig: {
                thinkingBudget: 0, // Disables thinking
            },
        },
    });
    totalTokenCount += response.usageMetadata?.totalTokenCount || 0;

    var cleanedResponse = response.text
        ?.replaceAll('```json', '')
        .replaceAll('```', '')
        .trim();
    writeFileSync(cacheFile, cleanedResponse || '', 'utf-8');

    return cleanedResponse;
}
