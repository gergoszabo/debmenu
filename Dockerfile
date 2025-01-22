FROM node:22-slim
RUN apt update && apt install tzdata imagemagick -y
ENV TZ="Europe/Budapest"
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone && dpkg-reconfigure -f noninteractive tzdata
RUN useradd -m user
USER user
WORKDIR /home/user
COPY debmenu.js debmenu.js
COPY .aws/credentials /home/user/.aws/credentials
CMD node debmenu.js