# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src


COPY ["AspNetCoreApi.csproj", "."]
COPY ["AspNetCoreApi.Tests.csproj", "."]


RUN dotnet restore "AspNetCoreApi.csproj"
RUN dotnet restore "AspNetCoreApi.Tests.csproj"

COPY . .

WORKDIR "/src"
RUN dotnet build "AspNetCoreApi.csproj" -c Release -o /app/build
RUN dotnet build "AspNetCoreApi.Tests.csproj" -c Release -o /app/tests_build

FROM build AS publish
WORKDIR "/src"
RUN dotnet publish "AspNetCoreApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

RUN apt-get update && apt-get install -y curl --no-install-recommends && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .


COPY --from=build /app/tests_build /app/bin/Release/net9.0/


COPY ["AspNetCoreApi.Tests.csproj", "."]

COPY ["Tests/", "Tests/"]

EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "AspNetCoreApi.dll"]
