FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MapperApp/MapperApp.csproj", "MapperApp/"]
RUN dotnet restore "MapperApp/MapperApp.csproj"
COPY . .
WORKDIR "/src/MapperApp"
RUN dotnet build "MapperApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MapperApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MapperApp.dll"]
