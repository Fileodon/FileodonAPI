# Installations guide af Fileodon lokal opsætning
Denne guide er til hvis du bare vil køre projektet uden viden omkring det. Hvis du oplever fejl eller mangler noget læs README.md

### Forudsætninger
* .NET 10 installeret
* En lokal postgres database
* entity framework CLI
* (optional) en lokal smtp klient som mailpit hvis du vil have email validerings mail

## Konfiguration
* Opret .env
```sh
cd Controller
cp .env.example .env
```
* konfigurer database forbindelse, CORS og evt SMTP server.
Eksempel på lokal opsætning
```
DB__HOST="localhost"
# The port number of the database server
DB__PORT="5433"
# The username to connect to the database
DB__USER="postgres"
# The password to connect to the database
DB__PASSWORD=""
# The name of the database to connect to
DB__NAME="fileodon"
### END DATABASE CONFIGURATION ###
JWT__KEY=<auto genereres hvis den ikke findes når projektet køre>
# The allowed origins for CORS requests, separated by commas
CORS__ALLOWED_HOSTS=http://localhost:3000
```

* Naviger ud i root projekt igen
```sh
cd ..
```

## Installer Afhængighedder
* restore dotnet pakker
```sh
dotnet restore
```
* sikre at projektet kan bygges
```sh
dotnet build
```

## Kør API'en
```sh
dotnet run --project controller
```

!Hvis migrations fejler at køre automatisk!
```sh
dotnet ef database update --project Infrastructure --startup-project Controller --context ApplicationDbContext
```