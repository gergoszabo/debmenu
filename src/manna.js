import { cacheOrFetch } from './_cache.js';

const fetchUrl = 'https://onemin-prod.herokuapp.com/api/v3/partners/304/restaurants/287/product-categories/with-products?type=web';
const website = 'https://www.mannaetterem.hu/rendeles';
const shortName = 'manna';
const name = 'Manna étterem';
const HETI_MENU = 'Heti menü';

const startedAt = performance.now();
export const fetchManna = async () => {
    try {
        const menu = await cacheOrFetch(shortName, fetchUrl, 'json');

        const hetiMenu = menu.find((m) => m.name === HETI_MENU);

        const result = {
            shortName,
            fetchUrl,
            fetchedIn: 0,
            website,
            name,
            offers: (hetiMenu?.products || []).reduce((acc, product) => {
                let [day, date, ...menu] = product.name.split(' ');
                date = `${new Date().getFullYear()}.${date.substring(1, date.length - 2)}`;
                const offer = acc.find((o) => o.date == date);
                if (!offer) {
                    acc.push({
                        date: date.replaceAll('-', '.'),
                        day,
                        offers: [`${menu.join(' ')}: ${product.description}`],
                    });
                } else {
                    offer.offers.push(`${menu.join(' ')}: ${product.description}`);
                }
                return acc;
            }, []),
        };

        result.fetchedIn = performance.now() - startedAt;

        return result;
    } catch (e) {
        return {
            shortName,
            fetchedIn: performance.now() - startedAt,
            errorReason: e.message,
            name,
            fetchUrl,
            offers: [],
            website,
        };
    }
};
