rm govinda*.png govinda.html govinda.txt

curl "https://www.govindadebrecen.hu" -s -o govinda.html

menuImg=$(cat govinda.html | grep 'class="menu-img' | xargs | cut -d "=" -f3 | cut -d ' ' -f1)
echo $menuImg

curl "https://www.govindadebrecen.hu/$menuImg" -s -o govinda.png

# days
convert govinda.png -crop 1600x70+170+20! govinda.days.png

# monday
convert govinda.png -crop 350x1300+170+110! govinda.monday.png
# tuesday
convert govinda.png -crop 330x1300+490+110! govinda.tuesday.png
# wednesday
convert govinda.png -crop 330x1300+820+110! govinda.wednesday.png
# thursday
convert govinda.png -crop 330x1300+1150+110! govinda.thursday.png
# friday
convert govinda.png -crop 330x1300+1480+110! govinda.friday.png

crops=("days" "monday" "tuesday" "wednesday" "thursday" "friday")
for crop in "${crops[@]}"
do
    echo $crop
    tesseract govinda.$crop.png govinda-$crop -l hun
done