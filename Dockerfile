FROM mono:latest
EXPOSE 9000
COPY ./ /src
WORKDIR /src
RUN nuget restore && \
    find . -name "*RuntimeInformation.dll" -delete && \
    xbuild /p:Configuration=Release && \
    find . -name "*RuntimeInformation.dll" -delete && \
    cp WebApp/WebAppConfig_dockerized.json WebApp/bin/Release/WebAppConfig.json
WORKDIR /src/WebApp/bin/Release
CMD mono WebApp.exe
