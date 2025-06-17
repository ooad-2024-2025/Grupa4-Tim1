# 🐎 Jahački Klub Željezničar 🔵

Sistem za upravljanje jahačkim klubom koji omogućava rezervaciju treninga, upravljanje članstvom i rezervaciju trail-ova.

## O Projektu

Sistem jahačkog kluba omogućava korisnicima da rezervišu različite trailove, apliciraju za članstvo u klubu i prate svoje članarine. Korisnici se mogu prijavljivati na treninge, a algoritam na osnovu prethodnih termina treninga, trenera i konja koje su jahali predlaže nove termine. Implementirana je kontrola pristupa gdje članovi i administrator imaju različite ovlasti i funkcionalnosti.

## Tim

**Nastavna grupa:** RI  
**Članovi tima:**
- Naila Delalić
- Emrah Žunić
- Vedad Gaštan
- Ajdin Hajdarević

## Deployment
[Live deployed app](http://tim1grupa4-001-site1.ntempurl.com/)

`Username:` 11248683

`Password:` 60-dayfreetrial

## Database Access (SQL Server)

Connection string:
```json
"Data Source=SQL6031.site4now.net;Initial Catalog=db_aba42b_ooad2025;User Id=db_aba42b_ooad2025_admin;Password=tim1grupa4"
```

## Funkcionalnosti

### Osnovne Usluge Sistema
- **Rezervacija trailova** - Prijava putem e-maila sa odabirom dostupnih termina
- **Učlanjivanje u klub** - Registracija novih članova sa besplatnim prvim mjesecom
- **Organizacija treninga** - Kreiranje treninga sa odgovarajućom težinom

### Upravljanje Podacima (CRUD)
- **Upravljanje članarinom** - Produžavanje i pregled statusa članarine
- **Prijava i upravljanje konjima** - Dodavanje, ažuriranje i brisanje konja
- **Promjena nivoa članova** - Podešavanje nivoa jahača od strane trenera
- **Dodavanje i upravljanje trenerima** - Administracija trenera u sistemu
- **Dodavanje trailova** - Kreiranje novih jahačkih ruta

### Napredne Funkcionalnosti
- **Prijedlog treninga** - Algoritamsko preporučivanje treninga na osnovu historije
- **Asinhrona potvrda uplate** - Automatska e-mail potvrda članarine

##  Korisnici Sistema

### Tipovi Korisnika
- **Admin** - Potpuna kontrola nad sistemom (konji, treneri, trailovi)
- **Trener** - Organizacija treninga i upravljanje nivoima članova
- **Član** - Rezervacija treninga, upravljanje članarinom
- **Obični korisnik** - Rezervacija trailova, apliciranje za članstvo

### Test Korisnici (uneseni u bazu)
Za testiranje aplikacije možete koristiti sljedeće korisničke račune:

| Tip korisnika | Email | Lozinka |
|---------------|-------|---------|
| Guest | `guest@guest.com` | `guest123` |
| Član | `clan@clan.com` | `clan123` |
| Trener | `trener@trener.com` | `trener123` |
| Admin | `admin@admin.com` | `admin123` |




**Univerzitet u Sarajevu - Elektrotehnički Fakultet**  
*Objektno Orijentisana Analiza i Dizajn*
