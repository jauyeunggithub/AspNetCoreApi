@echo off

dotnet clean

REM Ensure that Docker Compose is running and rebuild containers
echo Starting Docker Compose and rebuilding containers...
docker-compose up --build -d

REM Remove the existing SQLite database file (if it exists)
echo Removing existing SQLite database file...
docker-compose exec app rm -f /app/db/mydb.sqlite

REM Clean any old build artifacts before restoring dependencies
echo Cleaning old build artifacts...
docker-compose exec app rm -rf /app/AspNetCoreApi/bin /app/AspNetCoreApi/obj

REM Install dependencies inside the container (for ASP.NET Core, this is done via 'dotnet restore')
echo Restoring dependencies...
docker-compose exec app dotnet restore /app/AspNetCoreApi/AspNetCoreApi.csproj

REM Automatically run Entity Framework migrations
echo Running migrations...
docker-compose exec app dotnet ef database update --project /app/AspNetCoreApi/AspNetCoreApi.csproj

REM Run the unit tests
echo Running tests...
docker-compose exec app dotnet test /app/AspNetCoreApi.Tests.csproj

REM Optionally, stop the containers after tests are done
echo Stopping Docker Compose containers...
docker-compose down
