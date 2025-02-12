FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY src/*.csproj ./
RUN dotnet restore

COPY src/. ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS final
WORKDIR /app
COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "medscanner-notificacao.dll"]