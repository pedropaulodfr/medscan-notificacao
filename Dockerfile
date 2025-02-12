FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copia o arquivo .csproj e restaura as dependências
COPY *.csproj ./
RUN dotnet restore

# Copia todo o código-fonte e publica a aplicação
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS final
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "medscanner-notificacao.dll"]