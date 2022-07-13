FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["orleans-simple.csproj", "./"]
RUN dotnet restore "orleans-simple.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "orleans-simple.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "orleans-simple.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "orleans-simple.dll"]
