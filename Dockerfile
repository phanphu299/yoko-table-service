# SDK build image
FROM dxpprivate.azurecr.io/ahi-build:6.0-alpine3.19 AS build
WORKDIR .
COPY NuGet.Config ./
COPY src/AssetTable.Api/*.csproj         ./src/AssetTable.Api/
COPY src/AssetTable.Application/*.csproj ./src/AssetTable.Application/
COPY src/AssetTable.Domain/*.csproj      ./src/AssetTable.Domain/
COPY src/AssetTable.Persistence/*.csproj ./src/AssetTable.Persistence/
RUN dotnet restore ./src/AssetTable.Api/*.csproj /property:Configuration=Release -nowarn:msb3202,nu1503

COPY src/ ./src
RUN dotnet publish ./src/AssetTable.Api/*.csproj --no-restore -c Release -o /app/out

# Run time image
FROM dxpprivate.azurecr.io/ahi-runtime:6.0-alpine3.19 as final
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS http://+:80
ENTRYPOINT ["dotnet", "AssetTable.Api.dll"]
