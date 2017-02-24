FROM mono:latest
EXPOSE 9000
COPY ./ /src
WORKDIR /src
RUN xbuild /p:Configuration=Release
WORKDIR /src/WebApp/bin/Release
CMD mono WebApp.exe
