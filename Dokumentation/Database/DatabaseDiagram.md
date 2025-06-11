Databasestruktur Vores database består af tre centrale tabeller, der er forbundet for at håndtere brugere, annoncer og billeder.

users

id: SERIAL (Unik identifikator for brugeren) name: VARCHAR(255) email: VARCHAR(255) password: VARCHAR(255) (Husk at hashe passwords!) phone: VARCHAR(255) address: VARCHAR(255) city: VARCHAR(255) zip_code: VARCHAR(255) listings

id: SERIAL (Unik identifikator for annoncen) user_id: INTEGER (Fremmednøgle, der refererer til users.id) title: TEXT description: TEXT brand: VARCHAR(255) model: VARCHAR(255) gpu: VARCHAR(255) cpu: VARCHAR(255) ram: INTEGER storage: INTEGER os: VARCHAR(255) screen_size: VARCHAR(255) condition: VARCHAR(255) price: NUMERIC(10, 2) phone: VARCHAR(255) location: VARCHAR(255) created_utc: TIMESTAMP WITH TIME ZONE listing_images

id: SERIAL (Unik identifikator for billedet) listing_id: INTEGER (Fremmednøgle, der refererer til listings.id) image_path: VARCHAR(255) (Stien til den gemte billedfil)



erDiagram
    users {
        int id PK "Unikt ID"
        string name "Fulde navn"
        string email UK "Unik email"
        string password "Hashed adgangskode"
        string phone "Telefon (valgfri)"
        string address "Adresse (valgfri)"
        string city "By (valgfri)"
        string zip_code "Postnummer (valgfri)"
    }

    listings {
        int id PK "Unikt ID"
        int user_id FK "Reference til users(id)"
        string brand "Mærke"
        string model "Model"
        string cpu "CPU-model"
        string gpu "GPU-model"
        int ram "RAM i GB"
        int storage "Lagerplads i GB"
        string os "Styresystem"
        decimal price "Pris"
        string screen_size "Skærmstørrelse"
        string condition "Stand"
        string title "Annoncens overskrift"
        string description "Annoncens beskrivelse"
        string phone "Sælgers kontakt-tlf"
        string location "Sælgers lokation"
        datetime created_utc "Tidsstempel for oprettelse"
    }

    listing_images {
        int id PK "Unikt ID"
        int listing_id FK "Reference til listings(id)"
        string image_path "Sti til billedfil"
    }

    users ||--o{ listings : "opretter"
    listings ||--o{ listing_images : "har"


 
