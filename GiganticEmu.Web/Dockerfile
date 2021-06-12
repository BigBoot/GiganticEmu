FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY GiganticEmu.Shared.Backend/GiganticEmu.Shared.Backend.csproj ./GiganticEmu.Shared.Backend/GiganticEmu.Shared.Backend.csproj
COPY GiganticEmu.Shared/GiganticEmu.Shared.csproj ./GiganticEmu.Shared/GiganticEmu.Shared.csproj
COPY GiganticEmu.Web/GiganticEmu.Web.csproj ./GiganticEmu.Web/GiganticEmu.Web.csproj

RUN dotnet restore GiganticEmu.Web/GiganticEmu.Web.csproj

# Copy everything else and build
COPY GiganticEmu.Shared.Backend ./GiganticEmu.Shared.Backend
COPY GiganticEmu.Shared ./GiganticEmu.Shared
COPY GiganticEmu.Web ./GiganticEmu.Web
RUN dotnet publish -c Debug -o out GiganticEmu.Web/GiganticEmu.Web.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "GiganticEmu.Web.dll"]
