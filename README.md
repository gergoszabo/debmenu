# DebMenu - Napi menü Debrecenben

## Why

The question in the office always comes up in the morning before: who wants to eat in X or order something from somewhere.

With this little application, no need to try open many sites in a hurry, the daily offers are collected into a single place.

## Which restaurants are checked?

- Campus (https://campusettermek.hu)
- Govinda (https://www.govindadebrecen.hu)
- Hüse (http://www.husevendeglo.hu)
- Manna (https://www.mannaetterem.hu)
- Melange Kávéház (https://melangekavehaz.hu)
- Pálma Étterem (https://www.palmaetterem.hu/)
- Viktória (https://www.viktoriaetterem.hu)

## What it does?

- Crawls local restaurant pages
- extracts daily offers (text or image)
- generates a static html output in a temporary folder
- uploads it to a public S3 bucket
- site result is available at http://debmenuaws.s3-website.eu-central-2.amazonaws.com

## How it is done

Each restaurant has a page which can be crawled in two ways:
- Custom html: the restaurant has its own page, not using some catering solution
    - Selenium opens the restaurant url
    - with selectors, the daily offer gets extracted - either text or image
        - image might be transformed to text with AWS Rekognition service
    - HTML page gets generate with the restaurant's shortened name
- Catering solution: the restaurant uses some catering solution, it (probably) has an API
    - Grab the JSON from the API
    - Parse it
    - Generate html page with the restaurant's shortened name

Once all restaurants are crawled and the HTML pages are available in `results` folder,
AWS SDK will upload it to a public S3 bucket, website hosting is enabled on that bucket.
Results of each scan will be emailed to the maintainer.

## Used technologies

- .NET 8.0 - C#
- Selenium
- AWS SDK
    - S3 to store and serve generated assets
    - Rekognition to extract text from image
- MailKit
- Magick.NET
    - (optionally) enhance image for text extraction

## Licence

GOTO [LICENSE](./LICENSE)
