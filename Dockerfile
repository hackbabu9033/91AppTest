FROM mcr.microsoft.com/dotnet/sdk:6.0
COPY . ./
RUN dotnet restore

ENTRYPOINT "/bin/sh"