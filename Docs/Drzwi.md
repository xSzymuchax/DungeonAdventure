# Drzwi, skrzynie i kłódki

## Kłódka

Kłódka należy do drzwi albo do skrzyni. Nie jest osobnym polem. Rodzaje to brak, stalowa, słaba, wredna, tymczasowa i złota. Założona kłódka blokuje przejście przez drzwi i otwarcie skrzyni. Po zdjęciu znika razem z kostką.

| Rodzaj | Jak schodzi |
| --- | --- |
| Stalowa | Stalowy klucz z sąsiedniego pola. Klucz się zużywa, kłódka znika. |
| Słaba | Wytrych z sąsiedniego pola, potem jeden przycisk zagadki. Wytrych się zużywa, kłódka znika. |
| Wredna | Wytrych tak samo. Przycisk tylko odciąga zatrzask. Zejście z pola zakłada go z powrotem i zamyka drzwi. |
| Tymczasowa | Sama schodzi po 20 turach świata. |
| Złota | Złoty klucz z sąsiedniego pola. Klucz się zużywa, kłódka znika. Skrzynia zostaje zamknięta do osobnego kliknięcia. |

Klucz i wytrych kosztują jedną turę (`GAME_SPEED`). Odległość to Czebyszew 1, także po skosie. Klik w drzwi ze stalową kłódką sam podchodzi i zużywa stalowy klucz z torby. Klik w skrzynię ze złotą kłódką robi to samo ze złotym kluczem. Wytrych celuje się z ekwipunku. Zagadka nie kosztuje drugiej tury. Klucz działa tylko na swoją kłódkę: stalowy na stalową, złoty na złotą.

Tymczasowa kłódka i płonięcie drewna liczą tę samą turę świata, przy odnowieniu energii gracza.

Opis dopisuje „Wisi na nich …” albo „Wisi na niej …” oraz nazwę: stalowa, słaba, wredna, tymczasowa, złota. Stalowa jest szaroniebieska, słaba żółta, wredna fioletowa, tymczasowa pomarańczowa, złota złota.

## Drzwi

Materiał: przy losie poniżej 0,1 stal, poniżej 0,2 szkło, inaczej drewno. Drewno jest brązowe, szkło jasnoniebieskie, stal szara.

Zamknięte drewno i zamknięta stal zasłaniają wzrok. Zamknięte szkło go przepuszcza. Otwarte drzwi zawsze go przepuszczają.

Drzwi bez założonej kłódki przepuszczają krok. Postać na polu je otwiera. Puste pole bez przedmiotu na ziemi je zamyka. Zamknięte drzwi zatrzymują pocisk na swoim polu, otwarte go przepuszczają. Rzucony przedmiot, gdy drzwi nie mają kłódki, ląduje na progu, otwiera je i leżąc nie daje im się zamknąć. Podniesienie przedmiotu pozwala im się zamknąć. Kłódka zatrzymuje rzut na polu przed drzwiami i ich nie otwiera. Założona kłódka zatrzymuje też krok gracza, ruch wroga, akcję ruchu i szukanie ścieżki. Linię lotu przecina ściana albo pierwsze zamknięte drzwi.

Ognisty pocisk podpala drewniane drzwi na linii lotu. Już płonące nie zaczynają licznika od nowa. Płoną 3 tury, robią się pomarańczowe, opis dopisuje „Płoną.”, a potem znikają. Szkło i stal ogień ignorują. Rzut przedmiotu drewna nie podpala.

## Skrzynia

Skrzynia zajmuje pole od postawienia aż do zniszczenia, także po otwarciu. Nie da się na nią wejść. Wzroku nie zasłania. Front z kłódką jest obrócony w kamerę.

Materiał losuje się jak u drzwi. Kłódka: poniżej 0,60 brak, poniżej 0,95 słaba, inaczej złota. Zwykła skrzynia ma 1 przedmiot. Złota ma 2 albo 3. Zawartość losuje się przy wejściu na piętro.

Skrzynia bez kłódki losuje jedzenie, zwój, runę albo zasób. Wagi tych kategorii zostają. Skrzynia ze słabą kłódką losuje ekwipunek. Złota losuje z całego katalogu, z wagami kategorii bez zmiany.

Klik dochodzi na sąsiednie pole i nie wchodzi na skrzynię. Złoty klucz przy złotej kłódce zużywa turę i zdejmuje kłódkę. Osobny klik otwiera wieko, też za turę. Słaba kłódka schodzi wytrychem, a skrzynia i tak czeka na klik otwarcia. Klik w otwartą przenosi do torby tyle, ile się mieści. Reszta zostaje w skrzyni. Gdy nic nie wejdzie, tura się nie zużywa. Pusta otwarta skrzynia zostaje. Podnoszenie przedmiotu z ziemi zawartości skrzyni nie rusza.

Po otwarciu nad krawędzią stoi złoty sześcian na każdy przedmiot w środku. Każde zabranie chowa jeden.

Szklana skrzynia wypisuje zawartość także zamknięta. Drewno i stal wypisują ją po otwarciu.

Drewniana płonie jak drewniane drzwi: 3 tury od linii ognistego pocisku, potem skrzynia i to, co w niej zostało, znikają, a pole się zwalnia. Przedmioty już zabrane zostają w torbie. Szkło i stal ogień ignorują.

## Piętro lasu

Kolejność: pokoje, zamknięcie części pokoi, korytarze, wredna kłódka, stalowe klucze, zejście, skrzynie.

Zamknięcie bierze 1 albo 2 pokoje poza pokojem startu, takie, które mają próg. Zostaje jeden próg. Reszta wraca do ściany, a gdy pokój ściany nie ma, do podłogi. Te drzwi są stalowe. Kłódka jest w połowie stalowa i w połowie słaba.

Zwykły próg dostaje drzwi bez kłódki, z wylosowanym materiałem.

Wredna kłódka jest najwyżej jedna. Omija drzwi zamkniętych pokoi. Wchodzi tylko tam, gdzie lewa i prawa albo górna i dolna strona progu łączą się ósemką pól z pominięciem samego progu. Materiał się losuje. Gdy takiego progu nie ma, wrednej kłódki nie ma. Tymczasowa na lesie nie wypada.

Stalowy klucz leży po jednym na stalową kłódkę pokoju. Pole jest osiągalne ze startu, poza zamkniętymi pokojami i poza progiem. Najpierw sąsiad na wprost, potem najbliższe pole w metryce Manhattan. Identyfikator `steel_key`, nazwa „Stalowy klucz”. W katalogu łupu go nie ma.

Skrzyń jest od 0 do 3. Stoją na osiągalnej podłodze, poza startem, poza polami stalowych kluczy i poza zejściem. Złoty klucz leży po jednym na złotą skrzynię, tymi samymi zasadami, i omija też zejście oraz pola skrzyń i już położonych kluczy. Identyfikator `gold_key`, nazwa „Złoty klucz”. W katalogu łupu go nie ma.

Wytrych jest zasobem katalogu o wadze 10. Wypada jak inny zasób. Przepisu na niego nie ma. Piętro nie kładzie go przy kłódkach.

## Mapa debug

Rząd drzwi na y = 2, co drugie pole od x = 2: drewno, szkło, stal, stal ze stalową kłódką, stal ze słabą, stal z wredną, stal z tymczasową. Stalowy klucz leży na (2, 3).

Rząd skrzyń na y = 7, od x = 2: drewno, szkło, stal, drewno ze słabą, stal ze złotą na 2 albo 3 przedmioty. Złoty klucz leży na (10, 8).

## Zapis

Powrót na zapisane piętro odtwarza typy pól i przedmioty z ziemi, więc leżący klucz wraca. Zamówienie kłódki nie wraca: próg staje się zwykłymi drzwiami bez kłódki, a materiał losuje się od nowa. Skrzynia i jej zawartość nie wracają.
