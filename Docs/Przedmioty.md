# Przedmioty

## Losowanie ekwipunku

Modyfikatory wpisane na przedmiocie bazowym przechodzą bez mnożnika tieru.

Do tego dochodzi do 3 wylosowanych statystyk. Gdy przedmiot nie ma żadnego wpisanego modyfikatora, pierwsza wylosowana wpada zawsze. Każda kolejna z szansą 20%. Siła, wiedza, koszt ruchu, koszt ataku i zasięg widzenia nie mnożą się przez tier. Reszta wylosowanych statystyk mnoży się przez tier.

Zakres 0–0 w tabeli losowania oznacza, że statystyka istnieje, ale się nie losuje. Tak są wpisane spalanie najedzenia, napicia i poczytalności oraz odporność magiczna i odporności na ogień, zimno, truciznę i elektryczność. Da się je wpisać ręcznie na przedmiocie bazowym.

Poza tabelą, też tylko ręcznie: unik, kontra uniku, wzmocnienie magii, obrażenia strzał, bełtów i rzutek oraz maksymalne najedzenie, nawodnienie i poczytalność. Te trzy maksima przedmiot i tak nie podnosi.

Ulepszenie ekwipunku losuje się osobno, do poziomu 3, każdy krok z szansą 20%.

## Zwój

Koszt użycia to jedna tura (`GAME_SPEED`).

Poziom zaklęcia na zwoju startuje od 1. Każdy kolejny stopień wypada z szansą 20%, do 5. Poziom 2 ma więc 20%, poziom 3 ma 4%. Opis pokazuje ten poziom, na przykład „Kula ognia, poziom 3”, i z taką siłą leci zaklęcie. Poziom zapisuje się razem z przedmiotem.

## Runa

Użycie kosztuje 1 ładunek. Poziom zaklęcia losuje się jak na zwoju: od 1, każdy kolejny stopień z szansą 20%, do 5. Z tym poziomem leci zaklęcie.

Osobny poziom runy losuje się tą samą szansą, ale tylko do 3.

Poziom 1 zostaje przy limicie ładunku z definicji i regeneracji 0,1 na turę. Każdy kolejny poziom dodaje 1 do limitu. Regeneracja rośnie o 20% bazy na poziom powyżej pierwszego: poziom 2 daje 0,12, poziom 3 daje 0,14.

## Jedzenie

Pełne zdrowie przepala najedzenie o połowę wolniej. Pełna mana przepala napicie o połowę wolniej. Pusty żołądek zadaje głód opisany w `Docs/Walka.md`.
