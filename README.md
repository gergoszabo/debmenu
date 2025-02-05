# DebMenu - Napi menü Debrecenben

## Why

The question in the office always comes up in the morning before: who wants to eat in X or order something from somewhere.

With this little application, no need to try open many sites in a hurry, the daily offers are collected into a single place.

## Which restaurants are checked?

-   Csekő (https://debrecen.csekokavehaz.hu) (disabled at the moment)
-   Forest (https://forestetterem.hu)
-   Govinda (https://www.govindadebrecen.hu)
-   Hüse (http://www.husevendeglo.hu)
-   Manna (https://www.mannaetterem.hu)
-   Viktória (https://www.viktoriaetterem.hu)

## What it does?

-   Crawls local restaurant pages
-   extracts daily offers (text or image)
-   generates a static html output in a temporary folder
-   uploads it to a public S3 bucket
-   site result is available at http://debmenuaws.s3-website.eu-central-2.amazonaws.com

## How it is done

Each restaurant has a page which can be crawled in two ways:

-   Custom html: the restaurant has its own page, not using some catering solution
    -   Fetch the page source
    -   with selectors, the daily offer gets extracted - either text or image
        -   image will be transformed to text with AWS Rekognition service
    -   JSON gets generated with the restaurant's shortened name
-   Catering solution: the restaurant uses some catering solution, it (probably) has an API
    -   Grab the JSON from the API
    -   Parse it
    -   Generate JSON with the restaurant's shortened name

Once all restaurants are crawled and the JSON results are available in `results` folder,
the `index.html` will be generated and it will be uploaded to a public S3 bucket with AWS SDK, website hosting is enabled on that bucket.
Results of each scan will be emailed to the maintainer.

## Used technologies

-   Node.js
-   AWS SDK
    -   S3 to store and serve generated assets
    -   Rekognition to extract text from image
-   ImageMagick
    -   (optionally) enhance image for text extraction

### Install dependencies

#### Linux

-   NVM: curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.7/install.sh | bash
-   Node: nvm i 20
-   sudo apt install imagemagick
-   Hack together ~/.aws/config file or use ENV variables

#### Mac

Same as linux, but use brew to install imagemagick

## Licence

GOTO [LICENSE](./LICENSE)

## TODOs

-   update this readme
-   create `inventory.secret.yaml` with proper host and password
