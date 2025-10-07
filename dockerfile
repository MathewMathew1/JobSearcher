FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build


WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

RUN dotnet tool install --global Microsoft.Playwright.CLI
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN /root/.dotnet/tools/playwright install chromium

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
COPY --from=build /root/.cache/ms-playwright /root/.cache/ms-playwright

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        libnss3 \
        libatk1.0-0 \
        libatk-bridge2.0-0 \
        libcups2 \
        libxcomposite1 \
        libxdamage1 \
        libxrandr2 \
        libxkbcommon0 \
        libgbm1 \
        libasound2 \
        libpango-1.0-0 \
        libglib2.0-0 \
        libcairo2 \
        libdbus-1-3 \
        libnspr4 \
        libatspi2.0-0 \
        xvfb \
        xauth \
        x11-xserver-utils \
        wget \
        ca-certificates \
        fonts-liberation \
        libappindicator3-1 \
        libxss1 \
        lsb-release \
        xdg-utils \
        curl \
        --no-install-recommends && \
    rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "xvfb-run -a dotnet JobSearcher.dll"]