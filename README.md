# 🐎Jahački Klub Željezničar🔵

Sistem za upravljanje jahačkim klubom koji omogućava rezervaciju treninga, upravljanje članstvom i organizaciju jahačkih aktivnosti.

## 📋 O Projektu

Sistem jahačkog kluba omogućava korisnicima da rezervišu različite trailove, apliciraju za članstvo u klubu i prate svoje članarine. Korisnici se mogu prijavljivati na treninge, a algoritam na osnovu prethodnih termina treninga, trenera i konja koje su jahali predlaže nove termine. Implementirana je kontrola pristupa gdje članovi i administrator imaju različite ovlasti i funkcionalnosti.

## 👥 Tim

**Nastavna grupa:** RI  
**Članovi tima:**
- Naila Delalić
- Emrah Žunić
- Vedad Gaštan
- Ajdin Hajdarević

## 🚀 Funkcionalnosti

### Osnovne Usluge Sistema
- **Rezervacija trailova** - Prijava putem e-maila sa odabirom dostupnih termina
- **Učlanjivanje u klub** - Registracija novih članova sa besplatnom prvom sedmicom
- **Organizacija treninga** - Kreiranje treninga sa odgovarajućim nivoom složenosti

### Upravljanje Podacima (CRUD)
- **Upravljanje članarinom** - Produžavanje i pregled statusa članarine
- **Prijava i upravljanje konjima** - Dodavanje, ažuriranje i brisanje konja
- **Promjena nivoa članova** - Podešavanje nivoa jahača od strane trenera
- **Dodavanje i upravljanje trenerima** - Administracija trenera u sistemu
- **Dodavanje trailova** - Kreiranje novih jahačkih ruta

### Napredne Funkcionalnosti
- **Prijedlog treninga** - Algoritamsko preporučivanje treninga na osnovu istorije
- **Asinhrona potvrda uplate** - Automatska e-mail potvrda članarine

## 👤 Korisnici Sistema

### Tipovi Korisnika
- **Admin** - Potpuna kontrola nad sistemom (konji, treneri, trailovi)
- **Trener** - Organizacija treninga i upravljanje nivoima članova
- **Član** - Rezervacija treninga, upravljanje članarinom
- **Obični korisnik** - Rezervacija trailova, apliciranje za članstvo

### Test Korisnici
Za testiranje aplikacije možete koristiti sljedeće korisničke račune:

| Tip korisnika | Email | Lozinka |
|---------------|-------|---------|
| Guest | `guest@guest.com` | `guest123` |
| Član | `clan@clan.com` | `clan123` |
| Trener | `trener@trener.com` | `trener123` |
| Admin | `admin@admin.com` | `admin123` |

## 🛡️ Nefunkcionalni Zahtjevi

- **Dostupnost 24/7** - Kontinuiran pristup sistemu
- **Mobilna kompatibilnost** - Potpuna funkcionalnost na iPhone i Android uređajima
- **Sigurnost podataka** - Siguran prenos i čuvanje korisničkih informacija

## 🏗️ Arhitektura

Sistem je razvijen koristeći objektno orijentisanu analizu i dizajn sa fokusom na:
- Kontrolu pristupa na osnovu tipova korisnika
- Algoritamsko preporučivanje treninga
- Asinhronu obradu e-mail notifikacija
- Responsivni dizajn za mobilne uređaje

## 📧 Kontakt

Za pitanja vezana za projekat, kontaktirajte članove tima preko GitHub-a.

---

**Univerzitet u Sarajevu - Elektrotehnički Fakultet**  
*Objektno Orijentisana Analiza i Dizajn*
