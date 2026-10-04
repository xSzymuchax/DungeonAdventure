# Skille

Każdy skill może odczytać wiedzę rzucającego przez `KnowledgeOf`. Sam odczyt nic nie dodaje. Sposób użycia wiedzy jest w definicji konkretnego skilla.

## Cios

Fizyczny skill bierze atak postaci, potem rozrzut z `Docs/Walka.md`.

Magiczny skill bierze `SpellPower`, potem wzmocnienie magii, potem ten sam rozrzut. Domyślne `SpellPower` to same obrażenia skilla.

## Kula ognia

Obrażenia skilla to `5 × poziom`. Poziom na skillu mieści się w 1–5. Poziom 1 zadaje 5, każdy kolejny dokłada 5.

`SpellPower` kuli to te obrażenia plus wiedza gracza, 1 za punkt. Atak postaci do tego nie wchodzi. Trafienie jest od ognia, więc zawsze dochodzi, a potem działa wzmocnienie magii i rozrzut. Po trafieniu nakłada token podpalenia.

Ten sam asset skilla jest na zwoju, runie i kosturze. Na czas rzutu podkładają swój wylosowany poziom zaklęcia. Po rzucie skill wraca do poziomu z assetu.

Koszt many jest na skilla w edytorze. Kula ognia ma tam 10. Każdy poziom zaklęcia dokłada 50% tej bazy: poziom 1 płaci 10, poziom 2 płaci 15, poziom 3 płaci 20. Zwój i runa płacą pełny koszt podpiętego zaklęcia. Kostur płaci 80% tego kosztu.
