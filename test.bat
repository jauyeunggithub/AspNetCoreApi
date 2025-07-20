@echo off

REM Clean up any old build artifacts
dotnet clean

REM Ensure that Docker Compose is running and rebuild containers with the test Dockerfile
echo Starting Docker Compose with the test Dockerfile and rebuilding containers...
docker-compose -f docker-compose.test.yml up --build -d

REM Remove the existing SQLite database file (if it exists)
echo Removing existing SQLite database file...
docker-compose -f docker-compose.test.yml exec app-test rm -f /app/db/mydb.sqlite

REM Run the unit tests from the test Dockerfile
echo Running tests...
docker-compose -f docker-compose.test.yml exec app-test dotnet test /src/AspNetCoreApi.Tests.csproj

REM Optionally, stop the containers after tests are done
echo Stopping Docker Compose containers...
docker-compose -f docker-compose.test.yml down
