FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/PocketStorage.IdentityServer/PocketStorage.IdentityServer.csproj", "src/PocketStorage.IdentityServer/"]
COPY ["src/PocketStorage.Application/PocketStorage.Application.csproj", "src/PocketStorage.Application/"]
COPY ["src/PocketStorage.Core/PocketStorage.Core.csproj", "src/PocketStorage.Core/"]
COPY ["src/PocketStorage.Data/PocketStorage.Data.csproj", "src/PocketStorage.Data/"]
COPY ["src/PocketStorage.Domain/PocketStorage.Domain.csproj", "src/PocketStorage.Domain/"]
RUN dotnet restore "src/PocketStorage.IdentityServer/PocketStorage.IdentityServer.csproj"
COPY . .
WORKDIR "/src/src/PocketStorage.IdentityServer"
RUN dotnet build "PocketStorage.IdentityServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PocketStorage.IdentityServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PocketStorage.IdentityServer.dll"]
