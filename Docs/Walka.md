# Walka

## Typy

| Typ | Grupa |
| --- | --- |
| Fizyczne | cios |
| Ogień, zimno, elektryczność | magiczne |
| Trucizna, krwawienie, głód | nieuchronne |

Nieuchronne obrażenia nie przechodzą przez obronę, blok ani odporność magiczną. Ich własna odporność działa jak przy ogniu: −1 podwaja, 1 zeruje.

## Zadanie ciosu

Fizyczny atak: baza to atak postaci. Rzut broni: baza to obrażenia rzutu. Rzut amunicji: obrażenia pocisku plus obrażenia tej amunicji z założonej broni i z amuletu. Po trafieniu, także gdy redukcja zbiła obrażenia do zera, broń nakłada swoje efekty przy trafieniu. Przeciwnik nakłada efekty z pola onHits. Unik ich nie nakłada. Magia ich nie uruchamia. Strzał nakłada najpierw efekty wyrzutni, potem amunicji.

Magiczny atak: baza to `SpellPower` skilla. Domyślnie jest to obrażenia skilla, bez ataku postaci. Potem baza rośnie o `magicAmplify` procent. 10 i 50% daje 15.

Potem los 75–125% tej bazy. Zaokrąglenie do najbliższej całości, przy 0,5 w górę. Rozmiar tabliczki mapuje ten los, sprzed odporności, na 75–125% rozmiaru. Dolny kraniec przedziału ma 75%, górny 125%. Trafienie krytyczne ustawia rozmiar na 200%.

Szansa na trafienie krytyczne to procent 0–100. Po udanym losie, a przed odpornością, podwaja obrażenia ciosu wręcz i pocisku. Chybienie unikiem nie losuje krytyka. Głód, podpalenie, oziębienie, zatrucie i krwawienie go nie mają.

Głód i tokeny obrażeń nie używają tego rozrzutu. Podpalenie zadaje ogień: los 2 albo 3, przez 20 tur. Każdy token podpalenia ma te same liczby, argumenty konstruktora są ignorowane. Kolejne podpalenie dodaje tury, ale płonięcie trwa najwyżej 20. Oziębienie nie zadaje obrażeń, a kolejne dokłada tylko tury. Dopóki wisi na postaci, każda jej akcja kosztuje dwa razy więcej energii. Podpalenie i oziębienie znoszą się: gdy oba są na postaci, oba znikają. Podpalenie znosi też zamrożenie i oba znikają. Gdy oziębienie trwa co najmniej 20 tur, samo znika i nakłada zamrożenie na 5 tur. Zamrożona postać co turę dostaje 2 zimna i w każdej swojej turze tylko czeka, aż zamrożenie zejdzie. Kolejne oziębienie albo zamrożenie na zamarzniętej postaci ustawia zamrożenie z powrotem na 5 tur. Zatrucie dodaje siłę i tury. Co turę jest 20% szans, że zada tę siłę. Krwawienie co turę zadaje obrażenia równe sile rany, w pełnych punktach. Potem siła spada o logarytm naturalny tej siły powiększonej o 1, podzielony przez 2,5. Rana o sile 5 trwa około 10 tur, a przy zerze token znika. Nowe krwawienie zostawia wyższą siłę. Dopóki token wisi, `CanRegen` jest fałszywe i zdrowie nie wraca. Najedzenie, napicie i poczytalność oraz głód, pragnienie i obłęd mają siłę 1 i kolejne nałożenie tylko wydłuża tury. Pierwsze trzy podnoszą potrzebę, drugie trzy ją zbijają. Każdy z nich ma własny obrazek. Zimno ścina odporność magiczna. Trucizna i krwawienie są nieuchronne.

## Przyjęcie ciosu

1. Unik, tylko przy obrażeniach fizycznych. Szansa to `max(0, unik celu − kontra atakującego)`. Obie wartości to procenty, obcięte do 0–100. Chybienie daje 0 obrażeń, nie zużywa pancerza i nie nakłada tokenów. Magia i obrażenia nieuchronne zawsze trafiają.
2. Odporność na typ. Ogień, zimno, trucizna, elektryczność, krwawienie i głód mają −1, 0 albo 1. −1 podwaja obrażenia tego typu. 1 je zeruje.
3. Nieuchronne, których odporność nie wyzerowała, kończą się na tej wartości.
4. Fizyczny cios: odejmij obronę. Potem blok, też procent 0–100. Sukces zostawia połowę pozostałych obrażeń. Cios i tak trafia. Obrona nie działa na magię ani na obrażenia, które nie są ciosem.
5. Magia, której odporność na typ nie wyzerowała: `magicResistance` ścina procent i wynik idzie w górę do pełnej liczby. 10 obrażeń przy 25% zostaje 8.

Trucizna, krwawienie i głód nie zużywają pancerza. Głód przy pustym żołądku schodzi do licznika regeneracji zdrowia. Pełny ujemny punkt idzie jako `DamageType.Hunger`. Pragnienie przy pustym napiciu schodzi do licznika regeneracji many i zabiera manę.

## Poziom

Gracz zaczyna na poziomie 1, z zerem doświadczenia i zerem punktów umiejętności. Pokonany przeciwnik daje swoją ustaloną ilość doświadczenia, bazowo 15. Kolejny poziom kosztuje 10. Nadmiar zostaje i może dać kilka awansów naraz. Pułap to poziom 30. Dalej doświadczenie nie wchodzi.

Awans daje 1 punkt umiejętności. Punktów na razie nie ma gdzie wydać. Sam start na poziomie 1 punktu nie daje.

Klasę wybiera się przed wejściem do lochu. Szermierz, Kusznik i Mag zaczynają z 20 zdrowia, 20 many, 5 siły i 5 wiedzy. Awans dokłada zdrowie i manę do maksimum i do bieżących pul: szermierz +7 i +3, kusznik +5 i +5, mag +3 i +7. Siła i wiedza rosną na 5., 10., 15., 20., 25. i 30. poziomie. Szermierz dostaje wtedy +1,5 siły i +0,5 wiedzy, kusznik +1 i +1, mag +0,5 i +1,5. Wynik zaokrągla się od połówki w górę. Na 30. poziomie, po sześciu takich wypłatach, wychodzi 14/8, 11/11 i 8/14. Stary zapis bez klasy liczy się jako kusznik.
