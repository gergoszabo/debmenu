# apt install tesseract-hun curl

rm huse.png huse.html huse.txt

curl "http://husevendeglo.hu" -s -o huse.html

image=$(cat huse.html | grep 'src="/images' -m 1 | xargs | cut -d ' ' -f2 | cut -d '=' -f2)
echo $image

imageUrl="http://husevendeglo.hu$image"
echo $imageUrl

curl $imageUrl -s -o huse.png
echo "huse.png created"

tesseract huse.png huse -l hun
