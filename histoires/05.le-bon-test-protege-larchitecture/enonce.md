# Histoire 5 : Le bon test protège l'architecture
> "Architecture represents the significant design decisions that shape a system... the important stuff. Whatever that is." — Ralph Johnson

## Un `using` qui n'a rien à faire là
Ouvre `src/Bouchonnois/Domain/PartieDeChasse.cs`. Première ligne du fichier :

```csharp
using Bouchonnois.Service;

namespace Bouchonnois.Domain
{
    public class PartieDeChasse
    {
        ...
    }
}
```

Une classe du `Domain` - le cœur métier, chasseurs, terrain, statut, historique - qui importe le namespace `Service`. Relis le reste du fichier : ce `using` n'est utilisé nulle part. Aucune propriété, aucun champ, aucune signature ne référence quoi que ce soit venant de `Service`.

Les Histoires 1 à 4 ont rendu la suite de tests fiable, lisible, et large. Tout est vert. Le build compile sans warning. Et pourtant, rien dans tout ça n'a de raison de remarquer cette ligne : un `using` inutile ne casse aucun test, ne fait échouer aucune assertion, ne baisse pas le `coverage`. Il est juste... là, invisible, jusqu'à ce qu'un œil humain tombe dessus par hasard - ou jamais.

## Ta mission (partie 1)
- Pourquoi ce `using` a-t-il pu survivre sans que personne ne s'en aperçoive ? Qu'est-ce que ça te dit sur ce que couvrent vraiment les tests des Histoires précédentes ?
- Le `Domain` est censé être le cœur du système - les règles métier, indépendantes de toute technique. `Service` orchestre des cas d'utilisation. `Repository` (une interface, pour l'instant) gère la persistance. Dans quel sens ces dépendances devraient-elles aller ? Que se passerait-il si `Domain` dépendait vraiment de `Service` ?
- Est-ce qu'un test `example-based` (Histoire 1-3) ou un test de propriété (Histoire 4) pourrait attraper ce genre de problème ? Sur quoi portent-ils, au juste - le comportement, ou la structure ?

Note tes réponses, on va y revenir.

## Le concept : Architecture (Unit) Tests
Un test classique vérifie un **comportement** : *"si j'appelle `Tirer` avec ces données, j'obtiens ce résultat"*. Un test de propriété (Histoire 4) généralise ce même comportement à *toutes* les entrées valides. Mais aucun des deux ne dit rien sur la **structure** du code : qui a le droit de dépendre de qui.

Un `Architecture (Unit) Test` fait exactement ça : il exprime une règle de conception - une frontière entre couches, un sens de dépendance, une convention de nommage - et la vérifie automatiquement, à chaque run, exactement comme n'importe quel autre test.

```text
forall (classe du Domain)
elle ne doit dépendre d'aucune classe de Service, ni de Repository
```

Tu viens d'écrire une propriété sur le *comportement* à l'Histoire 4. Une règle d'architecture est une propriété sur la *structure* : elle doit rester vraie pour toutes les classes qui existent aujourd'hui - et pour toutes celles qui seront écrites demain, longtemps après que tu aies quitté le projet.

> Prends le temps de lire [Architecture Unit Tests](https://xtrem-tdd.netlify.app/Flavours/Architecture/archunit) pour une explication plus complète du concept.

### Pourquoi une frontière Domain / Service / Repository ?
C'est le principe de l'architecture en `Onion` (ou `Clean Architecture`, ou `Hexagonal` - plusieurs noms pour la même idée) : le `Domain` forme le centre, indépendant de tout détail technique. Les couches qui l'entourent (cas d'utilisation, persistance, framework web, ...) peuvent en dépendre - jamais l'inverse. C'est la **règle de dépendance** : les couches externes dépendent des couches internes, jamais l'inverse.

> Prends le temps de lire [Clean Architecture](https://xtrem-tdd.netlify.app/Flavours/Architecture/clean-architecture) - notamment les schémas en cercles concentriques (`Onion`) et le sens des flèches de dépendance.

Sur le Bouchonnois :
- `Domain` : `Chasseur`, `Terrain`, `PartieDeChasse`, `Event`, les `Exceptions` métier - le cœur, ne doit dépendre de rien d'autre dans le projet.
- `Service` : `PartieDeChasseService` - orchestre les cas d'utilisation (`Demarrer`, `Tirer`, ...), peut dépendre du `Domain`.
- `Repository` : pour l'instant, une seule interface (`IPartieDeChasseRepository`) - un **port**, au sens `Ports & Adapters`. Est-ce vraiment à sa place dans une couche à part, séparée du `Domain` qu'elle sert ?

[`ArchUnitNET`](https://github.com/TNG/ArchUnitNET) est la librairie qui va jouer ce rôle sur notre codebase `C#` (via `TngTech.ArchUnitNET.xUnit`).

## Ta mission (partie 2) : écrire la première règle d'architecture
1. Ajoute la dépendance `TngTech.ArchUnitNET.xUnit` au projet de tests.
2. Écris une classe utilitaire qui charge l'assembly de production (`Bouchonnois.dll`) dans une `Architecture` `ArchUnitNET`, et une extension `Check()` pour lancer une règle.
3. Écris la règle `DomainModelRules` : le `Domain` ne doit dépendre ni de `Service`, ni de `Repository`. Lance-la.
4. Regarde le résultat. Est-ce celui auquel tu t'attendais, vu le `using` repéré en partie 1 ? Si non - pourquoi ? Qu'est-ce qu'`ArchUnitNET` inspecte réellement pour décider qu'une classe "dépend" d'une autre : le texte source, ou autre chose ?
5. Prouve que ta règle sait vraiment détecter un problème (même technique que le mutant de l'Histoire 4) : ajoute *temporairement* un vrai champ de type `PartieDeChasseService` dans `PartieDeChasse`, relance la règle, lis le message d'échec, puis annule ton changement.
6. Applique la règle du `Boy Scout Rule` (Histoire 3) : supprime le `using Bouchonnois.Service;` mort de `PartieDeChasse.cs` - pas parce que la règle d'architecture l'exige, mais parce que du code mort reste du code mort.

## Ta mission (partie 3) : le port du repository
`IPartieDeChasseRepository` vit aujourd'hui dans `Bouchonnois.Repository`, séparé du `Domain`, et `Service` en dépend directement pour persister une `PartieDeChasse`.

En `Ports & Adapters`, un port (l'interface) appartient à la couche qui en a besoin pour exprimer ses règles - ici, le `Domain` lui-même. Seule une implémentation concrète (base de données, fichier, ...) - un *adapter* - appartient à une couche externe.

1. Déplace `IPartieDeChasseRepository` dans `Bouchonnois.Domain`.
2. Ajoute une règle `ApplicationServicesRules` : `Service` ne doit dépendre d'aucune classe de `Repository` (l'éventuelle infrastructure concrète, à venir).
3. Essaie d'ajouter une règle `InfrastructureRules` (*"tout ce qui réside dans `Repository` doit implémenter `IPartieDeChasseRepository`"*) et lance-la. Que se passe-t-il, maintenant que plus aucune classe ne réside dans ce namespace ? Est-ce que le résultat te semble légitime ?

## Ta mission (partie 4) : des règles d'équipe
Les règles d'architecture ne se limitent pas aux couches - elles peuvent aussi documenter des conventions de nommage. Ajoute une classe `Guidelines` avec :
- Toutes les interfaces du projet doivent commencer par `I` (déjà vrai aujourd'hui).
- Une méthode `GetXxx` ne doit pas retourner `void`.
- Une méthode `IsXxx`/`HasXxx` doit retourner un `bool`.
- Une méthode `SetXxx` doit retourner `void`.

Lance-les. Est-ce que toutes se comportent pareil ? Que se passe-t-il pour celles qui portent sur des méthodes qui n'existent tout simplement pas encore dans le projet - même piège qu'en partie 3, ou différent ? Si le `Given` vide te bloque à nouveau, cherche comment l'assumer explicitement plutôt que de renoncer à la règle - est-ce toujours la bonne décision, ici, comparé à `InfrastructureRules` ?

## Pour aller plus loin
- Repère d'autres frontières qui mériteraient une règle explicite (ex : les classes d'`Exceptions` ne devraient dépendre de rien d'autre que du strict nécessaire).
- Relance une analyse `SonarCloud`/`Codescene` : les dépendances entre packages/namespaces sont-elles visibles dans ces outils ? Est-ce la même information qu'une règle `ArchUnitNET`, ou autre chose ?

## Reflect
- Reviens à la citation d'ouverture du [`README`](../../README.md) : *"un signe qui ne trompe pas : quand une suite de tests verte n'inspire plus confiance à personne, le problème n'est pas dans le code de production. Il est dans les tests eux-mêmes."* Une suite 100% verte (Histoires 1 à 4) peut-elle cohabiter avec une architecture qui part en vrille ? Qu'est-ce que ça change à ta définition d'un "bon test" ?
- `Never trust a test you haven't seen fail` (Histoire 1) : as-tu vraiment vu `DomainModelRules` échouer pour une bonne raison, avant de lui faire confiance ?
- Une règle dont l'ensemble de départ (le `Given`) est vide peut donner une fausse impression de sécurité - exactement le problème des mauvais exemples (Histoire 1) ou des générateurs trop étroits (Histoire 4). Qu'est-ce que le comportement d'`ArchUnitNET` sur `InfrastructureRules` (partie 3) t'apprend sur la façon dont la librairie elle-même essaie d'éviter ce piège ?

## Solution
Guide étape par étape disponible [ici](solution.md).
