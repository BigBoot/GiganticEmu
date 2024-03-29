FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY GiganticEmu.Shared.Backend/GiganticEmu.Shared.Backend.csproj ./GiganticEmu.Shared.Backend/GiganticEmu.Shared.Backend.csproj
COPY GiganticEmu.Shared/GiganticEmu.Shared.csproj ./GiganticEmu.Shared/GiganticEmu.Shared.csproj
COPY GiganticEmu.Mice/GiganticEmu.Mice.csproj ./GiganticEmu.Mice/GiganticEmu.Mice.csproj
COPY Directory.Build.props ./Directory.Build.props

RUN dotnet restore GiganticEmu.Mice/GiganticEmu.Mice.csproj

# Copy everything else and build
COPY GiganticEmu.Shared.Backend ./GiganticEmu.Shared.Backend
COPY GiganticEmu.Shared ./GiganticEmu.Shared
COPY GiganticEmu.Mice ./GiganticEmu.Mice
COPY .git ./.git
RUN dotnet publish -c Release -o out -p:DebugType=None -p:DebugSymbols=false GiganticEmu.Mice/GiganticEmu.Mice.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "GiganticEmu.Mice.dll"]
