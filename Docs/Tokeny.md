# Tokeny

Na postaci jest co najwyżej jeden token danego typu. Kolejny tego samego typu woła `Merge` na już nałożonym. Token zna postać, na której leży (`Host`). Odliczanie idzie na początku jej akcji. Obrażenia z tokenów są pełnymi punktami: do najbliższej całości, przy 0,5 w górę. Nie używają rozrzutu 75–125% i nie krytykują.

Identyfikatory do efektów i trafień: `Burning`, `Chill`, `Frozen`, `Poison`, `Bleeding`, `Satiety`, `Hydration`, `Sanity`, `Hunger`, `Thirst`, `Madness`, `LowSatiety`, `StrongHunger`, `Starving`, `LowHydration`, `StrongThirst`, `Dehydrated`.

## Ogień i zimno

Podpalenie i oziębienie znoszą się. Gdy oba są na postaci, oba znikają. Podpalenie znosi też zamrożenie i oba znikają. Znoszenie jest sprawdzane zanim oziębienie przejdzie w zamrożenie.

### Podpalenie

Identyfikator `Burning`. Argumenty nałożenia są ignorowane. Każde podpalenie ma siłę 3 i 20 tur.

Co turę losuje 2 albo 3 ognia. Ogień jest magiczny: odporność na ogień −1 podwaja, 1 zeruje, potem ścina go odporność magiczna, wynik w górę do całości.

Kolejne podpalenie dodaje swoje tury, najwyżej do 20.

### Oziębienie

Identyfikator `Chill`. Nie zadaje obrażeń. Siła z nałożenia zostaje, kolejne oziębienie dokłada tylko tury.

Dopóki wisi na postaci, każda jej akcja kosztuje dwa razy więcej energii. Jeden token wystarcza, kilka nie spowalnia mocniej.

Gdy czas trwania dochodzi do 20 tur, oziębienie samo znika i nakłada zamrożenie na 5 tur.

### Zamrożenie

Identyfikator `Frozen`. Trwa 5 tur i nie zadaje obrażeń. Postać w każdej swojej turze tylko czeka, dopóki token nie zejdzie. Dotyczy gracza i przeciwników.

Kolejne zamrożenie albo oziębienie na zamarzniętej postaci ustawia czas zamrożenia z powrotem na 5 tur.

## Zatrucie

Identyfikator `Poison`. Siła i tury z obu tokenów dodają się. Co turę jest 20% szans, że zada tę siłę jako truciznę. Czas i tak schodzi co turę.

## Krwawienie

Identyfikator `Bleeding`. Trwa, dopóki rana nie zejdzie do zera. Czas z nałożenia nie liczy tur.

Co turę zadaje siłę rany w pełnych punktach. Potem siła spada o logarytm naturalny tej siły powiększonej o 1, podzielony przez 2,5. Rana o sile 5 trwa około 10 tur: ciosy 5, 4, 4, 3, 2, 2, 2, 1, 1, 1. Nowe krwawienie zostawia wyższą siłę. Nałożenie krwawienia sprawia, że `CanRegen` jest fałszywe, dopóki token wisi. Zebrany ułamek czeka. Głód przy pustym żołądku dalej zbija zdrowie.

## Samopoczucie

Siła bazowa zawsze wynosi 1, także gdy nałożenie poda inną. Kolejne nałożenie tego samego tokena tylko wydłuża tury. Działają na gracza. Pozytywne i negatywne są osobnymi typami i mają osobne obrazki, więc mogą wisieć razem.

| Token | Identyfikator | Co turę |
| --- | --- | --- |
| Najedzenie | `Satiety` | +1 sytości |
| Napicie | `Hydration` | +1 nawodnienia |
| Poczytalność | `Sanity` | +1 poczytalności |
| Głód | `Hunger` | −1 sytości |
| Pragnienie | `Thirst` | −1 nawodnienia |
| Obłęd | `Madness` | −1 poczytalności |

## Stan najedzenia i napicia

Próg liczy się jako udział zasobu w jego maksimum. Powyżej połowy regeneracja idzie w całości i token stanu nie wisi.

Od połowy włącznie, a powyżej jednej dziesiątej, regeneracja zwalnia do połowy. Najedzenie nakłada lekki głód (`LowSatiety`), napicie nakłada lekkie pragnienie (`LowHydration`).

Od jednej dziesiątej włącznie, dopóki zasób jest powyżej zera, regeneracja staje. Lekki głód schodzi i wchodzi silny głód (`StrongHunger`). Lekkie pragnienie schodzi i wchodzi silne pragnienie (`StrongThirst`).

Przy zera silny głód schodzi i wchodzi wygłodzenie (`Starving`). Silne pragnienie schodzi i wchodzi odwodnienie (`Dehydrated`).

Token stanu nie liczy tur. Zostaje, dopóki zasób jest w tym progu, i schodzi, gdy poziom wróci wyżej.
