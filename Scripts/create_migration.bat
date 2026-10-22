@echo off
set /p migrationName="Enter the name of the migration: "
cd /d "%~dp0"
dotnet ef --project Infrastructure --startup-project Controller migrations add "%migrationName%"