# Program pro sledování průjezdu aut bránami - Weppler
![alt text](weppler_group.png "Title")

Program běží jako služba v OS Windows. Služba funguje na principu čtení názvu souboru, jakmile přijde nový soubor, přečte název souboru a určí:
1. SPZ
1. Datum a čas
1. Z které brány auto přijelo

Následně vytvoří SQL řetězec a pošel záznam na základě Connection String.

Služba je otestovaná na Windows 11, kde běží několik týdnů za sebou bez problému.
