FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Step 2: Copy the project file and restore any dependencies
COPY ["AspNetCoreApi.csproj", "AspNetCoreApi/"]
RUN dotnet restore "AspNetCoreApi/AspNetCoreApi.csproj"

# Step 3: Copy the rest of the source code into the container
COPY . .

# Step 4: Set the working directory to the app folder
WORKDIR "/src/AspNetCoreApi"

# Step 5: Build the application
RUN dotnet build "AspNetCoreApi.csproj" -c Release -o /app/build

# Step 6: Publish the app (with dependencies)
FROM build AS publish
RUN dotnet publish "AspNetCoreApi.csproj" -c Release -o /app/publish

# Step 7: Use the official ASP.NET Core 9.0 runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Set the working directory to /app
WORKDIR /app

# Step 8: Copy the app files from the publish stage
COPY --from=publish /app/publish .

# Expose port 80 for the app
EXPOSE 80

# Step 9: Define the entry point to run the app
ENTRYPOINT ["dotnet", "AspNetCoreApi.dll"]
