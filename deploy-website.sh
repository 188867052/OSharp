#!/bin/bash

containerId="` sudo docker ps | grep "4201->4201" | awk  '{print $1}' `"
echo "containerId:$containerId"
if [ -n "$containerId" ]
then
	sudo docker stop $containerId
	sudo docker rm $containerId
fi

imageId="`sudo docker images | grep "web          v1.0" | awk  '{print $3}'`"
echo "imageId:$imageId"
if [ -n "$imageId" ]
then
	sudo docker rmi  $imageId
fi

sudo docker pull 542153354/web:v1.0 
sudo docker run -it -d   -v /usr/src/app/node_modules -p 4201:4201  542153354/web:v1.0
exit
