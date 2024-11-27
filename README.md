# Program pro sledování průjezdu aut bránami

Program běží jako služba v OS Windows. Služba funguje na principu čtení názvu souboru, jakmile přijde nový soubor, přečte název souboru a určí:
1. SPZ
1. Datum a čas
1. Z které brány auto přijelo

Následně vytvoří SQL řetězec a pošle záznam na základě Connection String.

Služba je dlouhodobě otestovaná na Windows 11.
