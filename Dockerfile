FROM asciidoctor/docker-asciidoctor AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src
COPY ["AdocLangService.sln", "./"]
COPY ["AdocLangService/AdocLangService.csproj", "./AdocLangService/"]
COPY ["AdocLangService.Processing/AdocLangService.Processing.csproj", "./AdocLangService.Processing/"]
RUN dotnet restore
COPY . .
WORKDIR "/src/AdocLangService.Processing"
RUN dotnet build -c Release -o /app/build
WORKDIR "/src/AdocLangService"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final

RUN apk add bash icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib wget
RUN apk add libgdiplus --repository https://dl-3.alpinelinux.org/alpine/edge/testing/

RUN wget https://dot.net/v1/dotnet-install.sh
RUN chmod +x ./dotnet-install.sh
RUN ./dotnet-install.sh --version latest --runtime aspnetcore --channel 7.0
RUN rm dotnet-install.sh
ENV PATH="${PATH}:/root/.dotnet"

ENV ASPNETCORE_URLS="http://0.0.0.0:80"

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdocLangService.dll"]
