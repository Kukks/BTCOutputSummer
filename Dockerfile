FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["outputsummer.csproj", "./"]
RUN dotnet restore "outputsummer.csproj"
COPY . .
WORKDIR "/src/outputsummer"
RUN dotnet build "outputsummer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "outputsummer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "outputsummer.dll"]
