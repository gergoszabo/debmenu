export const toHtml = (results) =>
    `<!DOCTYPE html>
<html lang="hu">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta http-equiv="Cache-Control" content="max-age=3600">
    <title>DebMenu</title>
    <style>
        ul, li {
            margin: 5px;
        }
    </style>
</head>
<body>
${resultsToHtml(results)}
Generated at ${
        new Date(
            Date.now() + 3600000,
        ).toISOString().replaceAll('T', ' ').replaceAll('Z', '')
    }
</body>
</html>`;

const resultsToHtml = (results) => {
    const today = new Date(Date.now() + 3600000).toISOString().substring(0, 10).replaceAll('-', '.');
    return results
        .map((result) => {
            let html = `<li><a href="${result.website}">${result.name}</a><ul>`;

            result.offers
                .forEach((o) => {
                    if (o.date === today) {
                        html += o.offers.map((offer) => `<li>${offer}</li>`).join('\n');
                    }
                });

            return html + '</ul></li>';
        }).join('\n');
};
