# apt install tesseract-hun curl imagemagick

rm forest.html forest*.jpg forest*.txt

curl "https://forestetterem.hu" -s -o forest.html

imageHref=$(cat forest.html | grep "et_pb_lightbox_image" -m 2 | grep '<a href' | cut -d '=' -f2 | cut -d '"' -f2)
echo $imageHref

imageHref=${imageHref/http/https}

curl "$imageHref" -s -o forest.jpg

# header
convert forest.jpg -crop 2000x150+0+0! forest.header.jpg

# monday
convert forest.jpg -crop 388x800+80+225! forest.monday.jpg
# tuesday
convert forest.jpg -crop 388x800+448+225! forest.tuesday.jpg
# wednesday
convert forest.jpg -crop 388x800+816+225! forest.wednesday.jpg
# thursday
convert forest.jpg -crop 388x800+1184+225! forest.thursday.jpg
# friday
convert forest.jpg -crop 388x800+1552+225! forest.friday.jpg

# weekly soup
convert forest.jpg -crop 400x120+120+1120! forest.soup.jpg

# weekly grill
convert forest.jpg -crop 520x170+520+1120! forest.grill.jpg

# side dishes
convert forest.jpg -crop 420x170+1020+1120! forest.sidedish.jpg

# salad
convert forest.jpg -crop 520x210+1420+1120! forest.salad.jpg

# chef
convert forest.jpg -crop 520x90+400+1400! forest.chef.jpg

crops=("header" "monday" "tuesday" "wednesday" "thursday" "friday" "soup" "grill" "sidedish" "salad" "chef")
for crop in "${crops[@]}"
do
    echo $crop
    tesseract forest.$crop.jpg forest-$crop -l hun
done
