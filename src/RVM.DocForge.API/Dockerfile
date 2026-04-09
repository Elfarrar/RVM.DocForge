FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/RVM.DocForge.API/RVM.DocForge.API.csproj", "src/RVM.DocForge.API/"]
COPY ["src/RVM.DocForge.Domain/RVM.DocForge.Domain.csproj", "src/RVM.DocForge.Domain/"]
COPY ["src/RVM.DocForge.Infrastructure/RVM.DocForge.Infrastructure.csproj", "src/RVM.DocForge.Infrastructure/"]
RUN dotnet restore "./src/RVM.DocForge.API/RVM.DocForge.API.csproj"
COPY . .
WORKDIR "/src/src/RVM.DocForge.API"
RUN dotnet build "./RVM.DocForge.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RVM.DocForge.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RVM.DocForge.API.dll"]
