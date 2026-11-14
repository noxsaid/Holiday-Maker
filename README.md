# Holiday Maker

Individuell projektuppgift i kursen MAI26. En konsolapplikation i C# där en resebyråoperatör kan registrera kunder, söka lediga rum och hantera bokningar över telefon. All data ligger i en MySQL-databas.

## Funktioner

- Registrera och välja kund
- Söka lediga rum på ort, datum och antal personer, med valfria filter (pool, restaurang, max avstånd till strand och centrum) och sortering (pris eller omdöme)
- Skapa bokning med bokningsnummer och totalpris (antal nätter × pris per natt)
- Visa en kunds bokningar
- Ändra antal personer i en bokning (måste rymmas i rummet)
- Avboka (bokningen tas bort och rummet blir ledigt igen)

Sökning, filtrering och sortering görs i SQL. Alla frågor från programmet är parametriserade.

## Krav

- .NET 10 SDK
- Docker
- MySQL 8.4 (körs i Docker, se nedan)
- En databasklient, till exempel DataGrip (alternativt MySQL-klienten i containern)
- NuGet-paketet `MySqlConnector` 2.6.2 (hämtas automatiskt av `dotnet run`)

## Skapa databasen

### 1. Starta MySQL i Docker

```bash
docker run --name mysql-kurs -e MYSQL_ROOT_PASSWORD=kurslosenord -p 127.0.0.1:3306:3306 -v mysql-kurs-data:/var/lib/mysql -d mysql:8.4
```

Volymen `mysql-kurs-data` gör att datan finns kvar när containern stoppas eller tas bort. Databasen är bara nåbar från den egna datorn (`127.0.0.1`).

Om containern redan finns startar du den med:

```bash
docker start mysql-kurs
```

### 2. Anslut med en databasklient

Anslut till `localhost`, port `3306`, användare `root` och lösenordet från steg 1.

### 3. Kör SQL-skripten i den här ordningen

I DataGrip öppnar du filen och kör hela den med Ctrl+Shift+F10.

| Ordning | Fil | Innehåll |
|---|---|---|
| 1 | `SQL/01_schema.sql` | Tar bort en gammal databas med samma namn och skapar `holiday_maker` med alla tabeller och begränsningar |
| 2 | `SQL/02_sql_data_tester.sql` | Testdata: 5 boenden i 2 orter, 20 rum, 5 kunder och 25 bokningar |
| 3 | `SQL/03_search.sql` | Valfri demofråga för tillgänglighetssökningen |

Observera: `01_schema.sql` raderar hela databasen, så kör alltid `02_sql_data_tester.sql` efter den.

Kontrollera att allt lästes in:

```sql
SELECT (SELECT COUNT(*) FROM cities) AS orter,
       (SELECT COUNT(*) FROM accommodations) AS boenden,
       (SELECT COUNT(*) FROM rooms) AS rum,
       (SELECT COUNT(*) FROM customers) AS kunder,
       (SELECT COUNT(*) FROM bookings) AS bokningar;
```

Förväntat resultat: 2, 5, 20, 5, 25.

## Köra programmet

Anslutningssträngen finns i `Database.cs` och pekar på `localhost:3306`, databasen `holiday_maker` och användaren `root`. Lösenordet i filen ska vara samma som i `docker run`-kommandot ovan.

```bash
dotnet run
```

## Meny

| Val | Funktion |
|---|---|
| 1 | Registrera kund |
| 2 | Sök lediga rum |
| 3 | Skapa bokning |
| 4 | Visa kunds bokningar |
| 5 | Ändra antal personer |
| 6 | Avboka |
| 7 | Kör automatiska tester av bokningsreglerna |
| 0 | Avsluta |

Vid sökning kan du hoppa över filtren genom att trycka Enter. Avstånd anges i meter.

## Så fungerar tillgänglighet

Ett rum är upptaget om en annan bokning överlappar perioden:

```sql
booking_start < @checkOut AND booking_end > @checkIn
```

Utcheckningsdagen räknas inte som upptagen, så en kund kan checka in samma dag som en annan checkar ut. Kontrollen görs i sökningen och görs om precis innan en bokning sparas. Tillgängligheten kan inte säkras med en `CHECK`-begränsning, eftersom den beror på andra rader i tabellen.

## Projektstruktur

```
Program.cs            Startar programmet och visar huvudmenyn
menu/                 Menyval, uppdelade i klasser (kund, bokning, sökning, tester, inmatning)
*Repository.cs        All SQL: rum, kunder och bokningar (parametriserade frågor)
Database.cs           Anslutningssträng och databasanslutning
SQL/                  Skript för databas, testdata och sökfrågor
diagrams/             E/R-diagram (ursprungligt och slutligt)
```

## Dokumentation

- E/R-diagram, ursprungligt: `diagrams/er_original.png`
- E/R-diagram, slutligt: `diagrams/er_final.png`
- Testanteckningar och individuell reflektion lämnas in separat som PDF på LearnPoint.

## Kända begränsningar

- Anslutningssträngen med lösenord ligger i `Database.cs`. Det är en lokal Docker-databas som bara nås från den egna datorn. En förbättring är att läsa lösenordet från en miljövariabel.
- Tillgängligheten kontrolleras med en SQL-fråga strax före `INSERT`, utan transaktion. Det räcker när en operatör arbetar åt gången, men två samtidiga operatörer skulle kunna dubbelboka.
- Ingen validering av e-postformat. Datum tolkas av `DateTime.TryParse`, som är tillåtande.
- Avbokning tar bort bokningen helt, utan historik.