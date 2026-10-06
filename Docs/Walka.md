# Walka

## Typy

| Typ | Grupa |
| --- | --- |
| Fizyczne | cios |
| Ogień, zimno, elektryczność | magiczne |
| Trucizna, krwawienie, głód | nieuchronne |

Nieuchronne obrażenia nie przechodzą przez obronę, blok ani odporność magiczną. Ich własna odporność działa jak przy ogniu: −1 podwaja, 1 zeruje.

## Zadanie ciosu

Fizyczny atak: baza to atak postaci. Rzut broni: baza to obrażenia rzutu. Rzut amunicji: obrażenia pocisku plus obrażenia tej amunicji z założonej broni i z amuletu.

Magiczny atak: baza to `SpellPower` skilla. Domyślnie jest to obrażenia skilla, bez ataku postaci. Potem baza rośnie o `magicAmplify` procent. 10 i 50% daje 15.

Potem los 75–125% tej bazy. Zaokrąglenie do najbliższej całości, przy 0,5 w górę. Rozmiar tabliczki mapuje ten los, sprzed odporności, na 75–125% rozmiaru. Dolny kraniec przedziału ma 75%, górny 125%. Trafienie krytyczne ustawia rozmiar na 200%.

Szansa na trafienie krytyczne to procent 0–100. Po udanym losie, a przed odpornością, podwaja obrażenia ciosu wręcz i pocisku. Chybienie unikiem nie losuje krytyka. Głód i podpalenie go nie mają.

Głód i token podpalenia nie używają tego rozrzutu. Podpalenie zadaje ogień: los 2 albo 3, przez 20 tur. Każdy token podpalenia ma te same liczby, argumenty konstruktora są ignorowane.

## Przyjęcie ciosu

1. Unik, tylko przy obrażeniach fizycznych. Szansa to `max(0, unik celu − kontra atakującego)`. Obie wartości to procenty, obcięte do 0–100. Chybienie daje 0 obrażeń, nie zużywa pancerza i nie nakłada tokenów. Magia i obrażenia nieuchronne zawsze trafiają.
2. Odporność na typ. Ogień, zimno, trucizna, elektryczność, krwawienie i głód mają −1, 0 albo 1. −1 podwaja obrażenia tego typu. 1 je zeruje.
3. Nieuchronne, których odporność nie wyzerowała, kończą się na tej wartości.
4. Fizyczny cios: odejmij obronę. Potem blok, też procent 0–100. Sukces zostawia połowę pozostałych obrażeń. Cios i tak trafia. Obrona nie działa na magię ani na obrażenia, które nie są ciosem.
5. Magia, której odporność na typ nie wyzerowała: `magicResistance` ścina procent i wynik idzie w górę do pełnej liczby. 10 obrażeń przy 25% zostaje 8.

Trucizna, krwawienie i głód nie zużywają pancerza. Głód przy pustym żołądku to `DamageType.Hunger`.
