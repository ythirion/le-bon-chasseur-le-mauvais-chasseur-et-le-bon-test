# Histoire 5 - Le bon test protège l'architecture
Durant cette étape :
- Écrire des règles d'architecture avec [`ArchUnitNET`](https://github.com/TNG/ArchUnitNET)
- Comprendre ce qu'une règle d'architecture inspecte réellement (le compilé, pas le texte source)
- Déplacer un port (`Ports & Adapters`) du `Repository` vers le `Domain`
- Repérer un piège propre aux règles d'architecture : un `Given` vide

## Ajouter la dépendance
```bash
dotnet add package TngTech.ArchUnitNET.xUnit
```

> 🔵 Le nom du package NuGet est `TngTech.ArchUnitNET.xUnit` (préfixé par l'organisation `TngTech`) - pas `ArchUnitNET.xUnit` tout court, qui n'existe pas.

## L'outillage : charger l'assembly de production
```csharp
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Syntax.Elements.Types;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Bouchonnois.Tests.Architecture
{
    public static class ArchUnitExtensions
    {
        public static readonly ArchUnitNET.Domain.Architecture Architecture =
            new ArchLoader()
                .LoadAssemblies(typeof(PartieDeChasseService).Assembly)
                .Build();

        public static GivenTypesConjunction TypesInAssembly() =>
            Types().That().Are(Architecture.Types);

        public static void Check(this IArchRule rule) => rule.Check(Architecture);
    }
}
```

`ArchLoader` charge l'assembly compilée `Bouchonnois.dll` (via un type qu'elle contient, `PartieDeChasseService` - accessible sans `using` explicite grâce au `global using Bouchonnois.Service;` du fichier `Usings.cs`) et construit un graphe `Architecture` - la liste des types et de leurs dépendances réelles, telles qu'elles existent dans le binaire.

> 🔵 `Architecture` est `public`, pas `private` : la classe `Guidelines` (partie 4) en a besoin pour scoper sa propre règle sur les interfaces au même assembly.

## Première règle : le Domain ne dépend de rien d'autre
```csharp
public class ArchitectureRules
{
    private static GivenTypesConjunctionWithDescription ApplicationServices() =>
        ArchUnitExtensions.TypesInAssembly().And()
            .ResideInNamespaceMatching(".*Service.*")
            .As("Application Services");

    private static GivenTypesConjunctionWithDescription DomainModel() =>
        ArchUnitExtensions.TypesInAssembly().And()
            .ResideInNamespaceMatching(".*Domain.*")
            .As("Domain Model");

    private static GivenTypesConjunctionWithDescription Infrastructure() =>
        ArchUnitExtensions.TypesInAssembly().And()
            .ResideInNamespaceMatching(".*Repository.*")
            .As("Infrastructure");

    [Fact]
    public void DomainModelRules() =>
        DomainModel().Should()
            .NotDependOnAny(ApplicationServices()).AndShould()
            .NotDependOnAny(Infrastructure())
            .Check();
}
```

> 🔵 `ResideInNamespaceMatching(".*Domain.*")` plutôt que `ResideInNamespace("Bouchonnois.Domain")` : la version `Matching` prend un pattern (`regex`), et couvre aussi les sous-namespaces (`Bouchonnois.Domain.Exceptions`). La version sans suffixe attend une correspondance exacte et complète du namespace - trop stricte ici.

## Surprise : le test passe au vert
On lance `DomainModelRules` en gardant le `using Bouchonnois.Service;` mort dans `PartieDeChasse.cs` (repéré dans l'énoncé). Résultat : **vert**.

Ce n'est pas un bug de la règle. `ArchUnitNET` inspecte l'assembly **compilée** - les types réellement référencés dans les signatures, les champs, les appels. Un `using` (ou un `import`) est un artefact du **code source** : s'il n'est utilisé nulle part, il ne produit strictement aucune instruction dans le binaire compilé. Rien à détecter, parce qu'il n'y a rien, littéralement, dans le code qui tourne.

C'est une différence importante avec un `linter` de style ("`using` inutilisé" - un warning purement source), et une bonne nouvelle : ta règle d'architecture ne va pas hurler sur du bruit cosmétique, seulement sur de vrais couplages.

## On prouve que la règle sait vraiment détecter un problème
Même réflexe que le mutant de l'Histoire 4 : on casse le code *volontairement*, pour vérifier que le test sait réagir.

```csharp
// Ajout temporaire dans PartieDeChasse.cs
public class PartieDeChasse
{
    public Terrain? Terrain { get; set; }
    private PartieDeChasseService? _service; // <- vrai couplage, cette fois
    ...
}
```

On relance :

```text
FailedArchRuleException : "Domain Model should not depend on any Application Services and should not depend on any Infrastructure" failed:
	Bouchonnois.Domain.PartieDeChasse does depend on "Bouchonnois.Service.PartieDeChasseService"
```

Rouge, avec un message qui pointe exactement la classe et la dépendance en cause - aussi lisible qu'un `Falsifiable` de `FsCheck`. On retire le champ temporaire, on revient au vert.

## Boy Scout Rule : nettoyer le using mort
La règle d'architecture ne l'exigeait pas (elle ne le voyait même pas) - mais un `using` mort reste du code mort (Histoire 3). On le supprime :

```diff
- using Bouchonnois.Service;
-
  namespace Bouchonnois.Domain
  {
      public class PartieDeChasse
```

## Le port du repository : Ports & Adapters
`IPartieDeChasseRepository` vit dans `Bouchonnois.Repository`, séparé du `Domain`. Or c'est un **port** : une interface qui exprime un besoin du métier ("je dois pouvoir sauvegarder et retrouver une `PartieDeChasse`"), pas un détail technique. Seule une implémentation concrète (une base de données, un fichier, ...) - un **adapter** - a sa place dans une couche externe.

On déplace le fichier :
```csharp
// Bouchonnois/Domain/IPartieDeChasseRepository.cs (déplacé depuis Repository/)
namespace Bouchonnois.Domain;

public interface IPartieDeChasseRepository
{
    void Save(PartieDeChasse partieDeChasse);
    PartieDeChasse GetById(Guid partieDeChasseId);
}
```

`Service/PartieDeChasseService.cs` avait `using Bouchonnois.Repository;` en plus de `using Bouchonnois.Domain;` - devenu inutile, on le supprime aussi.

Le namespace `Bouchonnois.Repository` ne contient plus aucune classe de production - il attend une future implémentation concrète (adapter SQL, fichier, ...).

### Une nouvelle règle : Service ne dépend pas de l'infrastructure
```csharp
[Fact]
public void ApplicationServicesRules() =>
    ApplicationServices().Should()
        .NotDependOnAny(Infrastructure())
        .Check();
```

Ce test passe - légitimement, pas par hasard. `ApplicationServices()` (le `Given`, non vide : `PartieDeChasseService` existe bien) est réellement vérifié, même si `Infrastructure()` (la cible de `NotDependOnAny`) est vide pour l'instant. Le test protège dès aujourd'hui contre l'ajout futur, par erreur, d'une dépendance directe de `Service` vers un adapter concret plutôt que vers le port du `Domain`.

### Le piège : un Given vide
On essaie une dernière règle, à l'identique de l'ancien atelier :
```csharp
[Fact]
public void InfrastructureRules() =>
    Infrastructure().Should()
        .ImplementInterface(typeof(IPartieDeChasseRepository))
        .Check();
```

Résultat, en la lançant :
```text
FailedArchRuleException : "Infrastructure should implement interface "Bouchonnois.Domain.IPartieDeChasseRepository"" failed:
	The rule requires positive evaluation, not just absence of violations. Use WithoutRequiringPositiveResults() or improve your rule's predicates.
```

Ni vert, ni rouge classique : `ArchUnitNET` **refuse d'évaluer** la règle. Ici, c'est le côté `Given` (`Infrastructure()`, le sujet de la règle) qui est vide - plus aucune classe ne réside dans `Repository` depuis le déplacement du port. Une règle sur un ensemble vide serait vraie *par défaut*, sans avoir vérifié quoi que ce soit - exactement le genre de "vrai qui ne prouve rien" qu'on a appris à sentir venir depuis l'Histoire 1. La librairie choisit de planter plutôt que de laisser passer ce faux vert - et propose `WithoutRequiringPositiveResults()` pour l'assumer explicitement, si un jour c'est vraiment voulu.

On **n'ajoute pas** cette règle pour l'instant : `Infrastructure` est vide, il n'y a rien à protéger. Le jour où un vrai adapter (SQL, fichier, ...) arrive dans `Repository`, la règle redevient pertinente - elle attend, documentée ici, prête à être réactivée.

## Des règles d'équipe
Les règles d'architecture ne se limitent pas aux couches - elles documentent aussi des conventions de nommage, à l'identique de l'ancien atelier :

```csharp
using ArchUnitNET.Fluent.Syntax.Elements.Members.MethodMembers;
using ArchUnitNET.Fluent.Syntax.Elements.Types.Interfaces;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Bouchonnois.Tests.Architecture;

public class Guidelines
{
    private static GivenInterfacesConjunction InterfaceTypes() =>
        Interfaces().That().Are(ArchUnitExtensions.Architecture.Types);

    private static GivenMethodMembersThat Methods() => MethodMembers().That().AreNoConstructors().And();

    [Fact]
    public void NoGetMethodShouldReturnVoid() =>
        Methods()
            .HaveNameMatching("Get[A-Z].*").Should()
            .NotHaveReturnType(typeof(void))
            .Check();

    [Fact]
    public void IserAndHaserShouldReturnBooleans() =>
        Methods()
            .HaveNameMatching("Is[A-Z].*").Or()
            .HaveNameMatching("Has[A-Z].*").Should()
            .HaveReturnType(typeof(bool))
            .WithoutRequiringPositiveResults()
            .Check();

    [Fact]
    public void SettersShouldNotReturnSomething() =>
        Methods()
            .HaveNameMatching("Set[A-Z].*").Should()
            .HaveReturnType(typeof(void))
            .WithoutRequiringPositiveResults()
            .Check();

    [Fact]
    public void InterfacesShouldStartWithI() =>
        InterfaceTypes().Should()
            .HaveNameMatching("^I[A-Z].*")
            .Because("C# convention...")
            .Check();
}
```

> 🔵 `.HaveNameMatching(pattern)` remplace le `.HaveName(pattern, useRegularExpressions: true)` de l'ancien atelier - la version `ArchUnitNET` utilisée ici sépare une correspondance exacte (`HaveName`) d'une correspondance par pattern (`HaveNameMatching`), au lieu d'un booléen optionnel sur une seule méthode.

`NoGetMethodShouldReturnVoid` a un `Given` non vide dès aujourd'hui - `IPartieDeChasseRepository.GetById` existe - et se comporte comme une règle classique. `InterfacesShouldStartWithI` aussi (`IPartieDeChasseRepository`).

### Encore le piège du Given vide - mais traité différemment cette fois
Aucune méthode du projet ne s'appelle `Is...`/`Has...` ou `Set...` aujourd'hui. En lançant `IserAndHaserShouldReturnBooleans` sans `WithoutRequiringPositiveResults()`, même échec qu'`InfrastructureRules` plus haut :

```text
FailedArchRuleException : "Method members that are no constructors and have name matching "Is[A-Z].*" or have name matching "Has[A-Z].*" should have return type "System.Boolean"" failed:
	The rule requires positive evaluation, not just absence of violations. Use WithoutRequiringPositiveResults() or improve your rule's predicates.
```

Même cause qu'avant (`Given` vide) - mais cette fois, on ne retire pas la règle, on l'assume explicitement avec `.WithoutRequiringPositiveResults()`. La différence avec `InfrastructureRules` :

- `InfrastructureRules` dépend d'une couche qui n'existe pas encore (aucun adapter concret dans `Repository`) - la règle n'a tout simplement rien à protéger tant que cette couche n'existe pas. On la laisse de côté jusqu'à ce jour.
- `IserAndHaserShouldReturnBooleans` et `SettersShouldNotReturnSomething` sont des conventions de nommage intemporelles, comme `InterfacesShouldStartWithI` - elles ne dépendent d'aucune couche particulière et peuvent légitimement rester "vides" pendant des mois, jusqu'au jour où quelqu'un ajoute une méthode `IsValide()` ou `SetNom(...)`. Le `Given` vide n'est pas un signe que la règle est inutile ; ici, on l'assume.

## Reflect
- Une suite 100% verte (Histoires 1 à 4) peut cohabiter avec une architecture qui part en vrille en toute discrétion : aucun de ces tests ne regarde qui dépend de qui. Les règles d'architecture comblent un angle mort que ni l'`example-based`, ni le `Property-Based Testing` ne couvrent.
- `Never trust a test you haven't seen fail` (Histoire 1) s'applique texto ici : le mutant temporaire sur `PartieDeChasse` a servi exactement à ça - voir `DomainModelRules` échouer pour une bonne raison, avant de lui faire confiance.
- Le crash sur un `Given` vide (`InfrastructureRules`, puis `IserAndHaserShouldReturnBooleans`) est le même risque que les mauvais exemples de l'Histoire 1 ou les générateurs trop étroits de l'Histoire 4 - un test qui ne peut pas vraiment échouer ne prouve rien. Ce qui change ici : ce n'est pas nous qui l'avons repéré à la relecture, c'est la librairie elle-même qui a refusé de laisser passer ce faux vert - à charge pour nous de décider, au cas par cas, si on retire la règle (`InfrastructureRules`) ou si on assume le vide (`IserAndHaserShouldReturnBooleans`, `SettersShouldNotReturnSomething`).

## Le résultat dans le code
Cette étape s'applique à :
- `src/Bouchonnois.Tests/Bouchonnois.Tests.csproj` : `TngTech.ArchUnitNET.xUnit`
- `src/Bouchonnois.Tests/Architecture/ArchitectureRules.cs` : `ArchUnitExtensions`, `DomainModelRules`, `ApplicationServicesRules`
- `src/Bouchonnois.Tests/Architecture/Guidelines.cs` : `NoGetMethodShouldReturnVoid`, `IserAndHaserShouldReturnBooleans`, `SettersShouldNotReturnSomething`, `InterfacesShouldStartWithI`
- `src/Bouchonnois/Domain/PartieDeChasse.cs` : suppression du `using Bouchonnois.Service;` mort
- `src/Bouchonnois/Domain/IPartieDeChasseRepository.cs` : déplacé depuis `Repository/`
- `src/Bouchonnois/Service/PartieDeChasseService.cs` : suppression du `using Bouchonnois.Repository;` devenu inutile
