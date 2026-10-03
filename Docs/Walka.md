# Walka

## Typy

| Typ | Grupa |
| --- | --- |
| Fizyczne | cios |
| Ogień, zimno, elektryczność | magiczne |
| Trucizna, krwawienie, głód | nieuchronne |

Nieuchronne obrażenia nie przechodzą przez obronę, blok, odporność magiczną ani odporność na żywioł. Wrażliwość nadal je podwaja.

## Zadanie ciosu

Fizyczny atak: baza to atak postaci. Rzut: baza to obrażenia rzutu.

Magiczny atak: baza to `SpellPower` skilla. Domyślnie jest to obrażenia skilla, bez ataku postaci. Potem baza rośnie o `magicAmplify` procent. 10 i 50% daje 15.

Potem los 75–125% tej bazy. Zaokrąglenie do najbliższej całości, przy 0,5 w górę.

Głód i token podpalenia nie używają tego rozrzutu. Podpalenie zadaje ogień: los 2 albo 3, przez 20 tur. Każdy token podpalenia ma te same liczby, argumenty konstruktora są ignorowane.

## Przyjęcie ciosu

1. Unik, tylko przy obrażeniach fizycznych. Szansa to `max(0, unik celu − kontra atakującego)`. Obie wartości to procenty, obcięte do 0–100. Chybienie daje 0 obrażeń, nie zużywa pancerza i nie nakłada tokenów. Magia i obrażenia nieuchronne zawsze trafiają.
2. Wrażliwość. Odporność na ogień, zimno, truciznę i elektryczność jest równa −1, 0 albo 1. −1 podwaja obrażenia tego typu, zanim zadziała cokolwiek dalej. 1 oznacza pełną odporność, ale tylko dla magii. Trucizna przy 1 i tak przechodzi w całości.
3. Nieuchronne kończą się na tej wartości.
4. Fizyczny cios: odejmij obronę. Potem blok, też procent 0–100. Sukces zostawia połowę pozostałych obrażeń. Cios i tak trafia. Obrona nie działa na magię ani na obrażenia, które nie są ciosem.
5. Magia: odporność 1 zeruje obrażenia. Inaczej `magicResistance` ścina procent i wynik idzie w górę do pełnej liczby. 10 obrażeń przy 25% zostaje 8.

Głód przy pustym żołądku to `DamageType.Hunger` i nie jest ciosem, więc pancerz się nie zużywa.
