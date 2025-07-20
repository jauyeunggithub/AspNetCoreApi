# Stage 1: Build the main application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy the .csproj files for both the main app and tests
COPY ["AspNetCoreApi.csproj", "."]
COPY ["AspNetCoreApi.Tests.csproj", "."]

# Restore dependencies for the main app and tests (although tests are not needed in runtime)
RUN dotnet restore "AspNetCoreApi.csproj"

# Copy the rest of the source code
COPY . .

# Build the main application only
RUN dotnet build "AspNetCoreApi.csproj" -c Release -o /app/build

# Stage 2: Publish the main application
FROM build AS publish
WORKDIR /src

# Publish the main application
RUN dotnet publish "AspNetCoreApi.csproj" -c Release -o /app/publish

# Stage 3: Final runtime image for the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

# Install necessary system dependencies (e.g., curl)
RUN apt-get update && apt-get install -y curl --no-install-recommends && rm -rf /var/lib/apt/lists/*

# Copy the published API from the 'publish' stage
COPY --from=publish /app/publish .

# Expose the port the app will run on
EXPOSE 80

# Set the environment variable for ASP.NET Core to listen on port 80
ENV ASPNETCORE_URLS=http://+:80

# Set the entry point to run the application
ENTRYPOINT ["dotnet", "AspNetCoreApi.dll"]
