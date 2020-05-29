FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
MAINTAINER LiuliuSoft i66soft@qq.com

WORKDIR /app
EXPOSE 80

COPY ./bin/Release/netcoreapp3.1/publish /app
ENTRYPOINT ["dotnet", "Liuliu.Demo.Web.dll"]