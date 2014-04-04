Diplomski Projekt
================
Opis programskog dijela diplomskog rada.

### Zadatak: ###
Zadatak diplomskog rada je implementirati genetski algoritam čija je svrha kratkoročno predviđanje potrošnje električne energije. Ideja je napraviti 24 prediktora potrošnje, po jedan za svaki sat u danu.

## Klase ##
U daljenjem tekstu slijedi opis svakog dijela genetskog programiranja.

### GP ###
Klasa `GP` je zadužena za konfiguriranje operatora i parametara, kao i za poziv svih važnijih dijelova algoritma, kao što su klase Algoritam ili provjeravanje uvjeta završavanja algoritma. Također je zadužena za zapisivanje u log datoteku.

GP se može pokretati na više načina:

- Crossvalidation ili obični GP
- jednom ili batch

### Algoritam ###
Klasa `Algoritam` je glavna klasa koja je zadužena za iteriranje algoritma. Zadužena je za obavljanje svih operacija nad populacijom, kao što su selekcija, križanje i mutacija.

Trenutno implementirani algoritmi:

- Steady State Tournament - eliminacijski turnirski algoritam

### Evaluacija ###
Klasa `Evaluacija` određuje na koji način se određuje greška jedinke. Klasa sadrži podatke koji mogu biti podaci za učenje, provjeru ili evaluaciju.

Trenutno implementirane evaluacije:

- MSE (Mean Squre Error)
- MAE (Mean Absolute Error)
- MAPE (Mean Absolute Percentage Error)

### Podaci ###
Klasa `Podaci` se sastoji od podataka koji su podijeljeni na podatke za učenje, provjeru i evaluaciju. Podaci se učitaju na početku programa, no tijekom izvršavanja programa se može postaviti nad kojim podacima se radi evaluacija. Podaci za jedan GP su podaci u jednom određenom satu u danu.

### Križanje ###
Klasa `Križanje` je apstraktna klasa koju se može implementirati bilo kojim operatorom križanja.

Trenutno implementirana križanja:

- Simple Crossover - križanje koje u roditeljima pronalazi ekvivalentne čvorove (čvorove sa jednakim brojem djece) te ih zamijeni

### Mutacija ###
Klasa `Mutacija` je apstraktna klasa koju se može implementirati bilo kojim operatorom križanja. Svaka mutacija sadrži vjerojatnost mutacije koja je u rasponu [0, 1] te se može uključiti opcija mutacije konstante koja doda konstanti neku vrijednost po Gaussovoj raspodijeli N(0, &mu;). Ako je uključena opcija mutacije konstante, onda se onda događa u 50% slučajeva mutacije konstanti.	

Trenutno implementirane mutacije:

- Simple Mutation - mutacija koja svaki čvor mijenja za slučajni ekvivalentni čvor (čvor s jednakim brojem djece)

### Uvjeti zaustavljanja ###
Uvjeti zaustavljanja zaustavljaju algoritam kada su ispunjeni.

Trenutno implementirani uvijeti zaustavljanja:

- Maksimalan broj generacija

### Populacija ###
Klasa populacija se sastoji od skupine jedinki. Jedinka će biti evaluirana (na skupu za učenje) odmah nakon stvaranja.

### Jedinka ###
Klasa `Jedinka` je klasa koja reprezentira jednu jedinku. Reprezentacija svake jedinke je stablo. Stablo se sastoji od čvorova. Jedinka je određena i najvećom i najmanjom mogućom dubinom stabla.
Jedinku se može kreirati, generirati, izračunati njena greška, kopirati, serijalizirati i deserijalizirati.

**Kod generiranja jedinke**, na dubinama koje su između najmanje i najveće,  generirati će se završni čvor i funkcijski čvor u omjeru 1:1. Također, konstante koje se dodaju kao završni čvorovi će poprimiti vrijednost tek kada se jedinka evaluira. Vrijednost će biti po Gaussovoj razdiobi N(0, 10).

### Čvor ###
Klasa `Čvor` služi za reprezentaciju jednog čvora iz stabla. Svaki čvor može biti funkcijski ili završni. Završni čvor može biti konstanta ili varijabla. 

Primjeri čvorova se generiraju na početku algoritma te se kasnije samo kopiraju i dodaju u stablo. Unaprijed su generirani svi funkcijski čvorovi koji su zadani u konfiguracijskoj datoteci, sve varijable (broj varijabli ovisi o izgledu skupa za učenje), te konstanti onoliko koliko je varijabli. Razlog ovom zadnjem je to da kada se generira završni čvor, sa 0.5 vjerojatnosti se odabere konstanta ili varijabla.

