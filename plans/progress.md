# Rev_n_Roll Szakdolgozat - Haladási Napló

**Projekt:** Rev_n_Roll – Közösségi platform autós események szervezésére  
**Szerző:** [Név]  
**Neptun kód:** [NEPTUNKOD]  
**Tanszék:** Rendszer- és Számítástudományi Tanszék  
**Szak:** Programtervező informatikus BSc

---

## Teljes Tartalomjegyzék

### Előlap és Nyilatkozatok
- [ ] Címlap
- [ ] Témakiírás
- [ ] Hallgatói nyilatkozat
- [ ] Témavezetői nyilatkozat
- [ ] Köszönetnyilvánítás

### Összefoglalók
- [ ] Tartalmi összefoglaló (magyar)
- [ ] Abstract (angol)
- [ ] Kulcsszavak

### Tartalomjegyzék és Jegyzékek
- [ ] Tartalomjegyzék
- [ ] Jelölésjegyzék
- [ ] Ábrajegyzék
- [ ] Táblázatjegyzék

---

## Érdemi Rész (40-60 oldal)

### 1. Bevezetés (4-5 oldal)
- [ ] 1.1. Motiváció és problémafelvetés
- [ ] 1.2. A dolgozat célja és célkitűzései
- [ ] 1.3. A megoldás rövid bemutatása
- [ ] 1.4. A dolgozat felépítése

### 2. Technológiai Áttekintés (6-8 oldal)
- [ ] 2.1. Backend technológiák
  - [ ] 2.1.1. .NET 9 és ASP.NET Core
  - [ ] 2.1.2. Entity Framework Core
  - [ ] 2.1.3. C# nyelvi jellemzők
- [ ] 2.2. Frontend technológiák
  - [ ] 2.2.1. Angular 19 keretrendszer
  - [ ] 2.2.2. TypeScript
  - [ ] 2.2.3. Material Design
- [ ] 2.3. Adatbázis technológia
  - [ ] 2.3.1. PostgreSQL
  - [ ] 2.3.2. Relációs adatmodell
- [ ] 2.4. Egyéb technológiák
  - [ ] 2.4.1. JWT autentikáció
  - [ ] 2.4.2. FluentValidation
  - [ ] 2.4.3. Serilog naplózás
  - [ ] 2.4.4. AWS CloudWatch

### 3. Rendszerterv és Architektúra (8-10 oldal)
- [ ] 3.1. Követelményanalízis
  - [ ] 3.1.1. Funkcionális követelmények
  - [ ] 3.1.2. Nem-funkcionális követelmények
- [ ] 3.2. Rendszerarchitektúra
  - [ ] 3.2.1. Háromrétegű architektúra
  - [ ] 3.2.2. RESTful API tervezés
  - [ ] 3.2.3. Kliens-szerver kommunikáció
- [ ] 3.3. Tervezési minták
  - [ ] 3.3.1. Repository pattern
  - [ ] 3.3.2. Dependency Injection
  - [ ] 3.3.3. Service layer pattern
- [ ] 3.4. Use Case diagramok
- [ ] 3.5. Komponens diagram

### 4. Adatbázis Tervezés (6-8 oldal)
- [ ] 4.1. Adatmodell tervezése
  - [ ] 4.1.1. Entitások azonosítása
  - [ ] 4.1.2. Kapcsolatok definiálása
- [ ] 4.2. Adatbázis séma
  - [ ] 4.2.1. User entitás
  - [ ] 4.2.2. Car entitás
  - [ ] 4.2.3. Crew entitás és UserCrew kapcsolótábla
  - [ ] 4.2.4. Meet entitás
  - [ ] 4.2.5. Race entitás
  - [ ] 4.2.6. Many-to-many kapcsolatok
- [ ] 4.3. Entity Framework Core konfiguráció
  - [ ] 4.3.1. DbContext implementáció
  - [ ] 4.3.2. Fluent API konfiguráció
  - [ ] 4.3.3. Indexek és megszorítások
- [ ] 4.4. Migrációk kezelése
- [ ] 4.5. ER diagram

### 5. Backend Implementáció (10-12 oldal)
- [ ] 5.1. Projekt struktúra
  - [ ] 5.1.1. ThesisBackend.Domain - Domain modellek
  - [ ] 5.1.2. ThesisBackend.Data - Adatelérési réteg
  - [ ] 5.1.3. ThesisBackend.Services - Üzleti logika
  - [ ] 5.1.4. ThesisBackend - API réteg
- [ ] 5.2. Autentikáció és autorizáció
  - [ ] 5.2.1. JWT token generálás (TokenGenerator.cs)
  - [ ] 5.2.2. Jelszó hashelés (PasswordHasher.cs)
  - [ ] 5.2.3. AuthService implementáció
  - [ ] 5.2.4. AuthController végpontok
- [ ] 5.3. Validáció
  - [ ] 5.3.1. FluentValidation integráció
  - [ ] 5.3.2. Request validátorok
- [ ] 5.4. API végpontok implementációja
  - [ ] 5.4.1. UserController
  - [ ] 5.4.2. CarController
  - [ ] 5.4.3. CrewController
  - [ ] 5.4.4. MeetController
  - [ ] 5.4.5. RaceController
- [ ] 5.5. Service réteg
  - [ ] 5.5.1. UserService
  - [ ] 5.5.2. CarService
  - [ ] 5.5.3. CrewService
  - [ ] 5.5.4. MeetService
  - [ ] 5.5.5. RaceService
- [ ] 5.6. Hibakezelés és naplózás
  - [ ] 5.6.1. Globális hibakezelés
  - [ ] 5.6.2. Serilog konfiguráció
  - [ ] 5.6.3. CloudWatch integráció
- [ ] 5.7. CORS és middleware konfiguráció

### 6. Frontend Implementáció (8-10 oldal)
- [ ] 6.1. Angular projekt struktúra
  - [ ] 6.1.1. Komponensek szervezése
  - [ ] 6.1.2. Szolgáltatások (Services)
  - [ ] 6.1.3. Modellek
  - [ ] 6.1.4. Guards és routing
- [ ] 6.2. Autentikáció a kliensen
  - [ ] 6.2.1. AuthService (auth.service.ts)
  - [ ] 6.2.2. AuthGuard implementáció
  - [ ] 6.2.3. Token kezelés
- [ ] 6.3. Főbb komponensek
  - [ ] 6.3.1. Login és Register komponensek
  - [ ] 6.3.2. Profile komponens
  - [ ] 6.3.3. Meets komponens
  - [ ] 6.3.4. Races komponens
  - [ ] 6.3.5. Crews komponens
- [ ] 6.4. HTTP szolgáltatások
  - [ ] 6.4.1. UserService
  - [ ] 6.4.2. CarService
  - [ ] 6.4.3. MeetService
  - [ ] 6.4.4. RaceService
  - [ ] 6.4.5. CrewService
- [ ] 6.5. Material Design integráció
  - [ ] 6.5.1. Dialog komponensek
  - [ ] 6.5.2. Form elemek
  - [ ] 6.5.3. Navigáció
- [ ] 6.6. Routing és navigáció

### 7. Tesztelés és Minőségbiztosítás (6-8 oldal)
- [ ] 7.1. Tesztelési stratégia
- [ ] 7.2. Backend tesztek
  - [ ] 7.2.1. Egységtesztek (Unit Tests)
    - [ ] ThesisBackend.Services.Tests
  - [ ] 7.2.2. Integrációs tesztek
    - [ ] ThesisBackend.Api.Tests
    - [ ] AuthControllerTests
    - [ ] CarControllerTests
    - [ ] CrewControllerTests
    - [ ] MeetControllerTests
    - [ ] RaceControllerTests
    - [ ] UserControllerTests
- [ ] 7.3. xUnit keretrendszer használata
- [ ] 7.4. Test coverage és eredmények
- [ ] 7.5. Kódminőség

### 8. CI/CD és DevOps (4-5 oldal)
- [ ] 8.1. GitHub Actions workflow
- [ ] 8.2. Automatizált build folyamat
- [ ] 8.3. Automatizált tesztelés
- [ ] 8.4. Deployment stratégia
- [ ] 8.5. Környezeti változók kezelése

### 9. Összefoglalás és Továbbfejlesztési Lehetőségek (3-4 oldal)
- [ ] 9.1. Elért eredmények
- [ ] 9.2. Tapasztalatok
- [ ] 9.3. Továbbfejlesztési lehetőségek
  - [ ] 9.3.1. Valós idejű értesítések (SignalR)
  - [ ] 9.3.2. Képfeltöltés és tárolás
  - [ ] 9.3.3. Email értesítések
  - [ ] 9.3.4. Térkép integráció fejlesztése
  - [ ] 9.3.5. Mobil alkalmazás
- [ ] 9.4. Záró gondolatok

---

## Mellékletek
- [ ] Irodalomjegyzék
- [ ] Mellékletek
  - [ ] A. Mappaszerkezet
  - [ ] B. API dokumentáció
  - [ ] C. Adatbázis séma diagram
  - [ ] D. Képernyőképek az alkalmazásról

---

## Haladási Állapot

### Aktuális fázis: ✅ BEFEJEZVE - Szakdolgozat teljes!

**Befejezett részek:**
- Projekt struktúra elemzése ✓
- Adatmodell megértése ✓
- Tartalomjegyzék elkészítése ✓
- 1. Bevezetés fejezet ✓
- 2. Technológiai áttekintés ✓
- 3. Rendszerterv és architektúra ✓
- 4. Adatbázis-tervezés ✓
- 5. Backend implementáció ✓
- 6. Frontend implementáció ✓
- 7. Tesztelés és minőségbiztosítás ✓
- 8. CI/CD és DevOps ✓
- 9. Összegzés és továbbfejlesztési lehetőségek ✓
- Mellékletek (irodalomjegyzék, mappaszerkezet, rövidítések) ✓

**Folyamatban:**
- Nincs - minden fejezet kész!

**Következő lépések:**
1. Szakdolgozat átnézése és finomítása
2. Ábrák és diagramok hozzáadása (opcionális)
3. Formázás a hivatalos Word sablonba
4. Leadás előtti végső ellenőrzés

---

## Technikai Megjegyzések

### Hivatkozási Konvenciók
- Backend fájlok: `ThesisBackend/Controllers/AuthController.cs`
- Frontend fájlok: `rev-n-roll/src/app/services/auth.service.ts`
- Tesztek: `ThesisBackend/ThesisBackend.Api.Tests/Authentication/AuthControllerTests.cs`

### Főbb Entitások
1. **User** - Felhasználók (ThesisBackend.Domain/Models/User.cs)
2. **Car** - Autók (ThesisBackend.Domain/Models/Car.cs)
3. **Crew** - Csapatok (ThesisBackend.Domain/Models/Crew.cs)
4. **Meet** - Találkozók (ThesisBackend.Domain/Models/Meet.cs)
5. **Race** - Versenyek (ThesisBackend.Domain/Models/Race.cs)
6. **UserCrew** - Kapcsolótábla (ThesisBackend.Domain/Models/UserCrew.cs)

### Főbb Controllerek
1. **AuthController** - Autentikáció (regisztráció, bejelentkezés)
2. **UserController** - Felhasználó kezelés
3. **CarController** - Autó kezelés
4. **CrewController** - Csapat kezelés
5. **MeetController** - Találkozó kezelés
6. **RaceController** - Verseny kezelés

### Adatbázis Kapcsolatok
- User 1:N Car
- User N:M Crew (UserCrew kapcsolótáblán keresztül)
- User N:M Meet (UserMeet kapcsolótáblán keresztül)
- User N:M Race (UserRace kapcsolótáblán keresztül)
- Crew 1:N Meet
- User 1:N CreatedMeets
- User 1:N CreatedRaces

---

## Időbélyegek

- **Projekt kezdete:** 2026-03-03
- **Tartalomjegyzék elkészítése:** 2026-03-03
- **Bevezetés írása kezdete:** 2026-03-03

---

## Megjegyzések a Folytatáshoz

Ha megszakad a munka, a következő információk alapján lehet folytatni:
1. Ellenőrizd a "Haladási Állapot" részt
2. Nézd meg az aktuális fázist
3. Folytasd a "Következő lépések" alapján
4. Minden fejezetnél hivatkozz a megfelelő fájlokra a projekt struktúrában
5. Ne módosítsd a kódot, csak hivatkozz rá!
