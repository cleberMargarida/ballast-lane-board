FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM node:22-alpine AS spa-build
WORKDIR /spa
COPY src/BallastLaneBoard.ClientApp/package*.json ./
RUN npm ci
COPY src/BallastLaneBoard.ClientApp/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/BallastLaneBoard.WebApi/BallastLaneBoard.WebApi.csproj", "src/BallastLaneBoard.WebApi/"]
COPY ["src/BallastLaneBoard.Application/BallastLaneBoard.Application.csproj", "src/BallastLaneBoard.Application/"]
COPY ["src/BallastLaneBoard.Domain/BallastLaneBoard.Domain.csproj", "src/BallastLaneBoard.Domain/"]
COPY ["src/BallastLaneBoard.Infra/BallastLaneBoard.Infra.csproj", "src/BallastLaneBoard.Infra/"]
COPY ["Directory.Build.props", "."]
RUN dotnet restore "src/BallastLaneBoard.WebApi/BallastLaneBoard.WebApi.csproj"
COPY . .
RUN dotnet build "src/BallastLaneBoard.WebApi/BallastLaneBoard.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/BallastLaneBoard.WebApi/BallastLaneBoard.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:SpaProxySkipBuild=true
COPY --from=spa-build /spa/dist/ballast-lane-board/browser /app/publish/wwwroot

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER app
ENTRYPOINT ["dotnet", "BallastLaneBoard.WebApi.dll"]
