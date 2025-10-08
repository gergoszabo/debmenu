const fetchUrl =
    'https://onemin-prod.herokuapp.com/api/v3/partners/304/restaurants/287/product-categories/with-products?type=web';
export const website = 'https://www.mannaetterem.hu/rendeles';
const HETI_MENU = 'Heti menü';

type MenuData = Record<string, Record<string, string[]>>;

export async function getMannaOffers() {
    const menu = await (await fetch(fetchUrl)).json();

    const hetiMenu = menu.find((m: any) => m.name === HETI_MENU);
    const offers = (hetiMenu?.products || []).reduce(
        (acc: MenuData, product: any) => {
            // product.name example: 'Hétfő (10.07.)'
            let [day, date, ...menuParts] = product.name.split(' ');
            // Extract date in format (10.07.)
            let dateStr = date?.replace(/[()]/g, ''); // '10.07.'
            if (!dateStr) return acc;
            // Convert to ISO date string for this year
            const [month, dayNum] = dateStr.split('.').filter(Boolean);
            if (!month || !dayNum) return acc;
            const isoDate = new Date(
                `${new Date().getFullYear()}-${month.padStart(
                    2,
                    '0'
                )}-${dayNum.padStart(2, '0')}`
            )
                .toISOString()
                .slice(0, 10);
            const offerText = `${menuParts.join(' ')}: ${
                product.description
            }`.trim();
            if (!acc[isoDate]) acc[isoDate] = {};
            if (!acc[isoDate][day]) acc[isoDate][day] = [];
            acc[isoDate][day].push(offerText);
            return acc;
        },
        {} as MenuData
    );

    // Flatten to { [date]: [offers] }
    const response = Object.fromEntries(
        Object.entries(offers).map(([date, dayObj]) => [
            date,
            Object.values(dayObj as Record<string, string[]>).flat(),
        ])
    );
    console.log(response);
    return response;
}
