FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/RaceDay.Web/RaceDay.Web.csproj", "src/RaceDay.Web/"]
RUN dotnet restore "src/RaceDay.Web/RaceDay.Web.csproj"
COPY . .
WORKDIR "/src/src/RaceDay.Web"
RUN dotnet publish "RaceDay.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]
