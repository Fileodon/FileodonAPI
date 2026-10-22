@echo off
cd /d "%~dp0"
dotnet ef --project ..\Infrastructure --startup-project ..\Controller database update