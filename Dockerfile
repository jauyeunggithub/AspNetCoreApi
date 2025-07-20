# Use the official .NET SDK image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["asp-net-core-api/asp-net-core-api.csproj", "asp-net-core-api/"]
RUN dotnet restore "asp-net-core-api/asp-net-core-api.csproj"
COPY . .
WORKDIR "/src/asp-net-core-api"
RUN dotnet build "asp-net-core-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "asp-net-core-api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "asp-net-core-api.dll"]
