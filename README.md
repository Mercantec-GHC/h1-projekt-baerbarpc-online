LapSwap: Din Online Markedsplads for Brugte Laptops
LapSwap er en C2C (Consumer-to-Consumer) webplatform, udviklet til at facilitere nemt og sikkert køb og salg af brugte bærbare computere. Projektet er skabt for at give brugere en centraliseret og troværdig markedsplads, hvor de kan oprette annoncer for deres brugte laptops eller finde deres næste computer til en fair pris.

Vores mål er at gøre processen så gennemsigtig som muligt. Sælgere kan nemt oprette detaljerede annoncer med specifikationer, billeder og en beskrivelse af computerens stand. Købere kan browse, søge og filtrere i et bredt udvalg af laptops, og kontakte sælgere direkte for at arrangere en handel.

Teknologivalg (Tech Stack)
Vores valg af teknologier er baseret på at skabe en robust og skalerbar applikation med moderne værktøjer.

Backend & Frontend: Hele projektet er udviklet i Visual Studio 2022, hvilket giver et stærkt og integreret udviklingsmiljø (IDE) til at bygge applikationen.
Database: Vi har valgt en PostgreSQL-database, som vi hoster hos Neon.Tech. PostgreSQL er et kraftfuldt open-source relationelt databasesystem, kendt for sin pålidelighed og store funktionssæt. Neon.Tech giver os mulighed for at køre en serverless og skalerbar database, hvilket minimerer vedligeholdelse og sikrer stabil drift.
Systemkrav og Opsætning
For at køre og videreudvikle på denne applikation er følgende nødvendigt:

Software
Visual Studio 2022: Applikationen er skrevet og testet i denne version.
Database: En PostgreSQL-database.
Forbindelsen til databasen skal konfigureres korrekt i appsettings.json-filen.
Databasen skal indeholde de tre specificerede tabeller: users, listings og listing_images. Du kan se den præcise tabelstruktur nedenfor.
Hardware
Der er ingen specifikke, krævende hardwarekrav for at køre applikationen lokalt, udover hvad der er nødvendigt for at køre Visual Studio 2022 uden problemer. En standard udviklingscomputer vil være fuldt tilstrækkelig.

Databasestruktur
Vores database består af tre centrale tabeller, der er forbundet for at håndtere brugere, annoncer og billeder.

users

id: SERIAL (Unik identifikator for brugeren)
name: VARCHAR(255)
email: VARCHAR(255)
password: VARCHAR(255) (Husk at hashe passwords!)
phone: VARCHAR(255)
address: VARCHAR(255)
city: VARCHAR(255)
zip_code: VARCHAR(255)
listings

id: SERIAL (Unik identifikator for annoncen)
user_id: INTEGER (Fremmednøgle, der refererer til users.id)
title: TEXT
description: TEXT
brand: VARCHAR(255)
model: VARCHAR(255)
gpu: VARCHAR(255)
cpu: VARCHAR(255)
ram: INTEGER
storage: INTEGER
os: VARCHAR(255)
screen_size: VARCHAR(255)
condition: VARCHAR(255)
price: NUMERIC(10, 2)
phone: VARCHAR(255)
location: VARCHAR(255)
created_utc: TIMESTAMP WITH TIME ZONE
listing_images

id: SERIAL (Unik identifikator for billedet)
listing_id: INTEGER (Fremmednøgle, der refererer til listings.id)
image_path: VARCHAR(255) (Stien til den gemte billedfil)
