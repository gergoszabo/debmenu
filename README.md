# DebMenu - Napi menü Debrecenben

## Why

The question in the office always comes up in the morning before: who wants to eat in X or order something from somewhere.

With this little application, no need to try open many sites in a hurry, the daily offers are collected into a single place.

## Which restaurants are checked?

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

## Licence

GOTO [LICENSE](./LICENSE)

## Versions

Check original v1 in [orig](./orig/README.md) folder.
