FROM node:20 AS build-frontend
WORKDIR /app

COPY frontend ./
RUN npm install \
    && npm run release


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . ./
COPY --from=build-frontend /app/out ./src/wwwroot
RUN dotnet restore \
    && dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/out .
COPY ./src/full_wordlist.tsv .

EXPOSE 8080
VOLUME ["/db"]
ENTRYPOINT ["dotnet", "SpellingBee.dll"]
