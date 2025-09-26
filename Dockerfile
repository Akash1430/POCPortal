FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Core/Core.csproj", "Core/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
COPY ["Dtos/Dtos.csproj", "Dtos/"]
COPY ["Interfaces/Interfaces.csproj", "Interfaces/"]
COPY ["Models/Models.csproj", "Models/"]
COPY ["WebApi/WebApi.csproj", "WebApi/"]

RUN dotnet restore "WebApi/WebApi.csproj"

COPY . .
WORKDIR "/src/WebApi"
RUN dotnet build "WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "WebApi.dll"]