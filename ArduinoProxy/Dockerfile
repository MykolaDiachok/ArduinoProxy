#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
RUN apt-get update && \
apt-get install -y git 
WORKDIR /src
COPY ["ArduinoProxy/ArduinoProxy.csproj", "ArduinoProxy/"]
RUN dotnet restore "ArduinoProxy/ArduinoProxy.csproj"
COPY . .
WORKDIR "/src/ArduinoProxy"
RUN dotnet build "ArduinoProxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArduinoProxy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArduinoProxy.dll"]