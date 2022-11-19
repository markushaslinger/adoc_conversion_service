# base image with dotnet runtime
FROM mcr.microsoft.com/dotnet/aspnet:7.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80

# refresh package lists
RUN apt-get update

# install node
RUN apt-get install -y curl bash apt-utils
RUN curl -sL https://deb.nodesource.com/setup_18.x | bash - 
RUN apt-get install -y nodejs

# install chromium dependencies
RUN apt-get install -y libnss3-dev libgdk-pixbuf2.0-dev libgtk-3-dev libxss-dev libasound2
RUN npm install -g node-html-to-image@3.2.4

ENV NODE_PATH=/usr/lib/node_modules

# install asciidoctor main
RUN apt-get install -y ruby ruby-dev graphviz openjdk-17-jdk build-essential bison flex libffi-dev libxml2-dev libgdk-pixbuf2.0-dev libcairo2-dev libpango1.0-dev fonts-lyx cmake
RUN gem install asciidoctor
RUN gem install asciidoctor-pdf
RUN gem install rouge
RUN gem install asciidoctor-diagram
RUN gem install text-hyphen
RUN gem install bundler
RUN gem install asciidoctor-mathematical
RUN gem install asciidoctor-revealjs

# install imagemagick
RUN apt-get install -y imagemagick

# build image with dotnet sdk
FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim AS build
WORKDIR /src
COPY ["AdocConversionService/AdocConversionService.csproj", "AdocConversionService/"]
RUN dotnet restore "AdocConversionService/AdocConversionService.csproj"
COPY . .
WORKDIR "/src/AdocConversionService"
RUN dotnet build "AdocConversionService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AdocConversionService.csproj" -c Release -o /app/publish

FROM base AS final
ENV ASPNETCORE_URLS="http://0.0.0.0:80"

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdocConversionService.dll"]
