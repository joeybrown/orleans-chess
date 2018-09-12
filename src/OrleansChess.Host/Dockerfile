FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
# EXPOSE 80 # Determine port

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["src/OrleansChess.Host/OrleansChess.Host.csproj", "src/OrleansChess.Host/"]
COPY ["src/OrleansChess.GrainInterfaces/OrleansChess.GrainInterfaces.csproj", "src/OrleansChess.GrainInterfaces/"]
COPY ["src/OrleansChess.GrainClasses/OrleansChess.GrainClasses.csproj", "src/OrleansChess.GrainClasses/"]
COPY ["src/OrleansChess.Common/OrleansChess.Common.csproj", "src/OrleansChess.Common/"]
RUN dotnet restore "src/OrleansChess.Host/OrleansChess.Host.csproj" -s https://api.nuget.org/v3/index.json
COPY . .
WORKDIR "/src/src/OrleansChess.Host"
RUN dotnet build "OrleansChess.Host.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "OrleansChess.Host.csproj" -c Release -o /app

FROM base as final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "OrleansChess.Host.dll"]