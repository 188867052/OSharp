#!/bin/bash

containerId="` sudo docker ps | grep "8081->80" | awk  '{print $1}' `"
echo "containerId:$containerId"
if [ -n "$containerId" ]
then
	sudo docker stop $containerId
	sudo docker rm $containerId
fi

imageId="`sudo docker images | grep "api          v1.0" | awk  '{print $3}'`"
echo "imageId:$imageId"
if [ -n "$imageId" ]
then
	sudo docker rmi -f $imageId
fi

sudo docker pull 542153354/api:v1.0 
sudo docker run -d -p 8081:80 542153354/api:v1.0 /bin/sh 

exit
