FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["EmployeeManagementSystem.API/EmployeeManagementSystem.API.csproj", "EmployeeManagementSystem.API/"]
COPY ["EmployeeManagementSystem.Application/EmployeeManagementSystem.Application.csproj", "EmployeeManagementSystem.Application/"]
COPY ["EmployeeManagementSystem.Infrastructure/EmployeeManagementSystem.Infrastructure.csproj", "EmployeeManagementSystem.Infrastructure/"]
COPY ["EmployeeManagementSystem.Domain/EmployeeManagementSystem.Domain.csproj", "EmployeeManagementSystem.Domain/"]

RUN dotnet restore "EmployeeManagementSystem.API/EmployeeManagementSystem.API.csproj"

COPY . .
WORKDIR "/src/EmployeeManagementSystem.API"
RUN dotnet build "EmployeeManagementSystem.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmployeeManagementSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "EmployeeManagementSystem.API.dll"]