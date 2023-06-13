FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CFM/CFM.csproj", "CFM/"]
RUN dotnet restore "CFM/CFM.csproj"
COPY . .
WORKDIR "/src/CFM"
RUN dotnet build "CFM.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CFM.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CFM.dll"]
