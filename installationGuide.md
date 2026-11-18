# Installationsguide til Fileodon API

Denne guide hjælper dig med at køre Fileodon API lokalt. Den er skrevet til nye
udviklere og beskriver den anbefalede opsætning fra en frisk kopi af projektet.

Hvis du arbejder på funktionalitet eller har brug for mere teknisk baggrund, kan du
se [README.md](README.md).

## Forudsætninger

Installer følgende:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 14 eller nyere](https://www.postgresql.org/download/)
- Git
- Docker Desktop, hvis du vil bruge Mailpit til lokale e-mails (valgfrit)

## 1. Hent projektet

```bash
git clone https://github.com/Fileodon/FileodonAPI.git
cd FileodonAPI
```

## 2. Opret databasen

Start PostgreSQL, og opret derefter en lokal database. Det kan for eksempel gøres
fra `psql`:

```sql
CREATE DATABASE fileodon;
```

Notér PostgreSQL-brugernavn, adgangskode, port og databasenavn. Du skal bruge dem i
konfigurationen i næste trin.

## 3. Opret lokal konfiguration

Programmet læser miljøfilen fra `Controller/data/.env`.

### Windows PowerShell

```powershell
Copy-Item Controller\.env.example Controller\data\.env
```

### macOS/Linux

```bash
cp Controller/.env.example Controller/data/.env
```

Åbn derefter `Controller/data/.env`, og udfyld databaseoplysningerne:

```env
DB__HOST=localhost
DB__PORT=5432
DB__USER=postgres
DB__PASSWORD=din-postgres-adgangskode
DB__NAME=fileodon
```

`__` bruges til at angive indlejrede ASP.NET Core-konfigurationsværdier. For eksempel
bliver `DB__HOST` til `DB:HOST`.

Du kan også angive en komplet forbindelsesstreng med
`ConnectionStrings__DefaultConnection` i stedet for de fem `DB__*`-værdier.

Programmet opretter automatisk mappen `Controller/data` samt krypteringsnøgler,
JWT-nøgle, SFTP PAM-nøgler og logmappe ved første opstart. Disse filer er lokale og
må ikke committes.

## 4. Installer pakker og byg projektet

Kør kommandoerne fra projektets rodmappe:

```bash
dotnet restore
dotnet build
```

## 5. Start API'et

```bash
dotnet run --project Controller
```

Ved opstart kontrollerer API'et forbindelsen til PostgreSQL og anvender automatisk
alle ventende EF Core-migrationer.

Når udviklingsprofilen bruges, kan du åbne:

- Swagger UI: <https://localhost:7248/api/>
- Swagger UI via HTTP: <http://localhost:5283/api/>
- Helbredstjek: <https://localhost:7248/api/system/health>

Hvis HTTPS-certifikatet ikke er godkendt endnu, kan du gøre det én gang:

```bash
dotnet dev-certs https --trust
```

## Valgfrit: lokale e-mails med Mailpit

Mailpit er nyttig til registrering og nulstilling af adgangskoder, fordi den modtager
e-mails lokalt uden at sende dem videre.

Start Mailpit fra projektets rodmappe:

```bash
docker compose up -d mailpit
```

Tilføj derefter følgende til `Controller/data/.env`:

```env
SMTP__HOST=localhost
SMTP__PORT=1025
SMTP__STARTTLS=false
SMTP__USER=
SMTP__PASSWORD=
SMTP__FROM_ADDRESS=no-reply@localhost
```

Mailpit-inboxen findes på <http://localhost:8025>.

`docker-compose.yml` starter kun Mailpit. PostgreSQL skal stadig være installeret og
køre separat.

## Migrationer

Du behøver normalt ikke selv at køre migrationer, fordi API'et anvender ventende
migrationer automatisk ved opstart.

Hvis du ændrer EF Core-modellerne, kan du installere EF Core CLI:

```bash
dotnet tool install --global dotnet-ef
```

Opret en ny migration:

```bash
dotnet ef migrations add <MigrationNavn> --project Infrastructure --startup-project Controller
```

Anvend migrationer manuelt:

```bash
dotnet ef database update --project Infrastructure --startup-project Controller
```

På Windows kan du også bruge:

```bat
Scripts\create_migration.bat
Scripts\apply_migrations.bat
```

## Kør tests

Kør alle tests fra projektets rodmappe:

```bash
dotnet test
```

Testene bruger SQLite in-memory-databaser, hvor det er relevant, og kræver normalt
ikke en forbindelse til din lokale PostgreSQL-database.

## Typiske problemer

### API'et siger, at databaseindstillinger mangler

Kontrollér, at filen hedder præcis `Controller/data/.env`, at alle fem `DB__*`-værdier
er udfyldt, og at API'et startes fra projektets rodmappe.

### API'et kan ikke forbinde til PostgreSQL

Kontrollér, at PostgreSQL kører, at porten er korrekt, at databasen `fileodon` findes,
og at brugeren har adgang til databasen.

### Registreringsmails kommer ikke frem

Kontrollér, at Mailpit kører, at `SMTP__*`-værdierne er sat, og åbn
<http://localhost:8025> for at se modtagne mails.

### Swagger kan ikke åbnes

Brug URL'en med `/api/`, mens API'et kører i Development, og kontrollér terminalen
for opstartsfejl. OpenAPI-dokumentet findes på
`/api/openapi/v1.json` i Development.

## Gode vaner

- Gem adgangskoder og andre hemmeligheder i miljøvariabler eller en lokal secrets-provider.
- Commit ikke `.env`-filer, nøgler, logs eller indholdet af `Controller/data`.
- Kør `dotnet build` og `dotnet test`, før du sender ændringer videre.
- Tilføj en EF Core-migration, når du ændrer datamodellen.
