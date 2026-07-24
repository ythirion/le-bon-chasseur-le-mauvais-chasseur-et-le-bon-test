# Histoire 4 : Le bon test couvre ce que tu n'as pas pensé à tester
> "Don't write tests. Generate them." — John Hughes (créateur de QuickCheck)

## Un exemple, c'est un point. La règle, c'est une droite.
Reprends `DemarrerUnePartieDeChasse.AvecPlusieursChasseurs` : `Dédé (20 balles), Bernard (8 balles), Robert (12 balles)`, sur un terrain de 3 galinettes. Ou `TirerSurUneGalinette.AvecUnChasseurAyantDesBallesEtAssezDeGalinettesSurLeTerrain` : `Bernard`, `8 balles`, tire, il lui en reste `7`.

Ces tests sont bons - Histoire 1 les a rendus fiables, Histoire 2 les a rendus lisibles. Mais regarde ce qu'ils vérifient vraiment : *"si Bernard a 8 balles et qu'il tire, il lui en reste 7"*. Est-ce que c'est ça, la règle métier ? Ou est-ce qu'`8` et `7` ne sont que des valeurs choisies au hasard pour illustrer une règle plus générale - *"peu importe le nombre de balles qu'a un chasseur, tant qu'il en a au moins une, tirer lui en retire exactement une"* ?

Un test avec des exemples fixes ne peut te dire que : *"ça marche pour ces valeurs-là"*. Il ne dit rien sur les 7, les 100, les 4 999 999 balles que personne n'a pensé à tester. Et c'est là que se cachent les bugs qu'aucune review, aucun exemple choisi à la main, n'aurait révélés.

## Ta mission (partie 1)
Reprends 2-3 tests "heureux" du fichier (`DemarrerUnePartieDeChasse`, `TirerSurUneGalinette`, `Tirer`, ...) et pour chacun :

- Identifie les valeurs concrètes utilisées (`8`, `"Bernard"`, `3`, ...). Pour chacune, demande-toi : *est-ce que cette valeur précise compte pour le test, ou est-ce qu'elle représente "n'importe quelle valeur valide" ?*
- Formule la règle générale que le test illustre, en une phrase commençant par *"quel que soit..."* ou *"pour n'importe quel..."*.
- Note ce qui rendrait une valeur *invalide* pour ce test (ex : `0 balle` change le comportement attendu - ce n'est plus le même cas).

Garde ces phrases sous le coude, on va les transformer en code.

## Le concept : Property-Based Testing
Un test classique (`example-based`) fixe une entrée et vérifie une sortie précise. Un test de propriété (`Property-Based Testing`, ou `PBT`) fait l'inverse : il fixe une **règle** censée être vraie pour *toute* entrée valide, puis laisse un framework **générer** des dizaines d'entrées aléatoires pour essayer de la mettre en défaut.

```text
forall (x, y, ...) satisfaisant une précondition,
la propriété (x, y, ...) est vérifiée
```

Un seul test de propriété, ce sont potentiellement des centaines de tests `example-based` exécutés à chaque run - avec, en prime, des valeurs que tu n'aurais jamais pensé à essayer toi-même.

> Prends le temps de lire [Property-Based Testing](https://xtrem-tdd.netlify.app/Flavours/Testing/pbt/) pour une explication plus complète du concept.

### Quel genre de règle chercher ?
Toutes les propriétés ne se ressemblent pas. Quelques familles récurrentes :

- **Invariant** : une caractéristique reste vraie après l'opération - *"après un tir, le nombre de balles a diminué d'exactement 1"*.
- **There and back again** (aller-retour) : une opération et son inverse se neutralisent - *"sérialiser puis désérialiser redonne l'objet de départ"*.
- **Idempotence** : répéter l'opération ne change rien de plus - `f(f(x)) == f(x)`.
- **Commutativité** : l'ordre des opérations n'a pas d'importance - `f(x, y) == f(y, x)`.
- **Oracle / refactoring** : l'ancienne implémentation et la nouvelle donnent le même résultat, pour n'importe quelle entrée.

Sur le Bouchonnois, `Tirer` retire une balle -> c'est un **invariant**. `Demarrer` réussit dès qu'il y a des galinettes et des chasseurs armés -> c'est une propriété de **succès sous précondition**.

### Générateurs et shrinking
Le framework a besoin de savoir comment fabriquer des entrées valides : c'est le rôle des **générateurs**. Tu peux composer les générateurs fournis par la librairie (`int`, `string`, listes, ...) pour construire les tiens (*"un terrain avec au moins 1 galinette"*, *"un groupe de 1 à 50 chasseurs, tous avec au moins 1 balle"*).

Quand une propriété échoue, la librairie ne s'arrête pas à la première entrée trouvée : elle **réduit** (`shrinking`) automatiquement cette entrée vers le plus petit cas qui la fait encore échouer. Une liste de 37 chasseurs qui fait échouer le test devient, après shrinking, 1 seul chasseur avec 1 balle - beaucoup plus simple à déboguer.

### Deux pièges à éviter
- **Dupliquer la logique de production dans la propriété.** Si ta propriété recalcule le même algorithme que le code testé, tu ne vérifies plus rien - tu compares le code à sa propre copie.
- **Filtrer à la main plutôt que génerer juste.** `if (donnée invalide) return;` dans le corps de la propriété gaspille des exécutions et fausse la couverture. Génère directement des données valides (précondition dans le générateur) plutôt que de les rejeter après coup.

[`FsCheck`](https://fscheck.github.io/FsCheck/Properties.html) est la librairie qui va jouer ce rôle sur notre codebase `C#` (via `FsCheck.Xunit`).

## Ta mission (partie 2) : écrire tes premières propriétés
1. Ajoute la dépendance `FsCheck.Xunit` au projet de tests.
2. Reprends `Demarrer` : identifie une propriété de succès sous précondition (*"sur un terrain avec des galinettes et un groupe de chasseurs tous armés, la partie démarre toujours avec succès"*) et écris les générateurs nécessaires (terrain, chasseur, groupe de chasseurs).
3. Écris la `[Property]` correspondante avec `Prop.ForAll(...)`. Lance-la : combien de cas ont été générés ? Regarde les valeurs produites - est-ce que certaines t'auraient surpris si tu les avais écrites à la main ?
4. Reprends `Tirer` : écris la propriété *"peu importe le nombre de balles initial (au moins 1), tirer en retire exactement une"*.
5. Casse volontairement le code de production (ex : `chasseurQuiTire.BallesRestantes -= 2;`) et vérifie que la propriété échoue - regarde le message de `shrinking` : à quel point l'entrée qui casse le test est-elle simple ?

## Pour aller plus loin
- Identifie d'autres propriétés à partir de l'`Example Mapping` de l'atelier : `PrendreLapéro`/`ReprendreLaPartie` (aller-retour de statut ?), `TerminerLaPartie` (le vainqueur a toujours le plus de galinettes, quel que soit le nombre de chasseurs ?).
- Les tests `example-based` existants deviennent-ils inutiles une fois la propriété écrite ? Qu'est-ce que chacun des deux apporte que l'autre n'apporte pas ?

## Reflect
- Le `Property-Based Testing` remplace-t-il les tests `example-based`, ou les complète-t-il ? Repense à `DemarrerUnePartieDeChasse.AvecPlusieursChasseurs` (Histoire 3) : que t'apporte-t-il que la propriété n'apporte pas ?
- Une propriété qui a l'air de tout vérifier peut mentir aussi vite qu'un test classique (Histoire 1) : que se passe-t-il si ton générateur ne produit jamais les cas limites qui comptent (ex : jamais `0`, jamais de doublon de nom) ?
- Le `TimeProvider` figé (Histoire 1) devient-il plus ou moins important quand les entrées elles-mêmes deviennent aléatoires ?

## Solution
Guide étape par étape disponible [ici](solution.md).
