# Histoire 4 - Le bon test couvre ce que tu n'as pas pensé à tester
Durant cette étape :
- Identifier des propriétés à partir de l'`Example Mapping`
- Écrire des générateurs et des tests de propriétés avec [`FsCheck`](https://fscheck.github.io/FsCheck/Properties.html)
- Vérifier qu'une propriété détecte bien un vrai bug, et lire un message de `shrinking`

## Ajouter la dépendance
```bash
dotnet add package FsCheck.Xunit
```

Dans `Usings.cs` :

```csharp
global using FsCheck;
global using FsCheck.Fluent;
global using FsCheck.Xunit;
```

> 🔵 `FsCheck.Fluent` contient l'API pensée pour du C# (`Gen`, `Arb`, `Prop` comme classes statiques, générateurs qui retournent des `List<T>` natifs). L'autre namespace, `FsCheck.FSharp`, est pensé pour du F# - à éviter ici, il ramène des types comme `FSharpList<T>` inutiles dans un projet 100% C#.

## Propriété sur `Demarrer` : succès sous précondition
On reprend la règle identifiée dans l'énoncé :

```text
forall (terrain avec au moins 1 galinette, groupe de 1 à 50 chasseurs ayant chacun au moins 1 balle)
La partie de chasse démarre avec succès
```

### Les générateurs
```csharp
private static Gen<string> NomGenerator()
    => ArbMap.Default.GeneratorFor<string>().Where(nom => !string.IsNullOrWhiteSpace(nom));

private static Arbitrary<(string nom, int nbGalinettes)> TerrainRicheEnGalinettesGenerator()
    => (from nom in NomGenerator()
        from nbGalinettes in Gen.Choose(1, 1000)
        select (nom, nbGalinettes)).ToArbitrary();

private static Gen<(string nom, int nbBalles)> ChasseurArméGenerator()
    => from nom in NomGenerator()
       from nbBalles in Gen.Choose(1, 1000)
       select (nom, nbBalles);

private static Arbitrary<List<(string nom, int nbBalles)>> GroupeDeChasseursArmésGenerator()
    => (from nbChasseurs in Gen.Choose(1, 50)
        from chasseurs in Gen.ListOf(ChasseurArméGenerator(), nbChasseurs)
        select chasseurs).ToArbitrary();
```

🔵 `Gen.Choose(1, 1000)` plutôt que `Gen.Choose(1, int.MaxValue)` : on borne volontairement à des valeurs réalistes. Un terrain à `int.MaxValue` galinettes ne teste rien de plus intéressant, et évite de traîner des valeurs dégénérées jusque dans une future histoire (ex : un classement qui somme des galinettes).

### La propriété
```csharp
public class DemarrerUnePartieDeChasse : PartieDeChasseServiceTest
{
    [Property]
    public Property Sur1TerrainAvecGalinettesEtDesChasseursArmésLaPartieDémarre()
        => Prop.ForAll(
            TerrainRicheEnGalinettesGenerator(),
            GroupeDeChasseursArmésGenerator(),
            (terrain, chasseurs) =>
            {
                var id = PartieDeChasseService.Demarrer(terrain, chasseurs);
                return Repository.SavedPartieDeChasse()!.Id == id;
            });

    ...
}
```

`Repository` et `PartieDeChasseService` viennent de `PartieDeChasseServiceTest` (Histoire 2) - une propriété est un test comme un autre, elle profite des mêmes fondations.

Ce test vaut, à lui seul, des dizaines d'exécutions de `Demarrer` avec des terrains et des groupes de chasseurs différents à chaque run.

🔵 Cette propriété est **complémentaire** à `DemarrerUnePartieDeChasse.AvecPlusieursChasseurs` (Histoire 3), pas un remplacement. `AvecPlusieursChasseurs` capture *un* état précis (3 chasseurs nommés, leurs balles exactes, le contenu complet de la `PartieDeChasse`) - utile pour documenter un cas concret et détecter toute régression de forme. La propriété, elle, valide une règle indépendante du nombre de chasseurs ou de leurs noms : *"tant que la précondition est respectée, ça démarre"*. L'une documente un exemple, l'autre balaie l'espace des possibles.

## Propriété sur `Tirer` : un invariant
```text
forall (nom de chasseur, nombre de balles initial >= 1)
Après Tirer, il reste exactement une balle de moins
```

```csharp
public class Tirer : PartieDeChasseServiceTest
{
    [Property]
    public Property TirerRetireExactementUneBalle(NonEmptyString nom, PositiveInt ballesInitiales)
    {
        var id = Guid.NewGuid();
        Repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") { NbGalinettes = 3 },
            [new Chasseur(nom.Get) { BallesRestantes = ballesInitiales.Get }]));

        PartieDeChasseService.Tirer(id, nom.Get);

        var chasseur = Repository.SavedPartieDeChasse()!.Chasseurs.Single();
        return Prop.ToProperty(chasseur.BallesRestantes == ballesInitiales.Get - 1);
    }
}
```

🔵 Pas de `PartieDeChasseBuilder`/`ChasseurBuilder` ici. Ces `Builders` (Histoire 2) encapsulent des `Object Mothers` nommées (`Dédé()`, `Bernard()`, `Robert()`) faites pour des exemples mémorables - pas pour des valeurs générées à la volée. Pour une propriété, on construit l'état directement à partir des valeurs produites par le générateur : c'est plus court, et ça évite de plier un `Builder` fait pour autre chose.

`NonEmptyString` et `PositiveInt` sont des types fournis par `FsCheck` : ils encapsulent respectivement "une `string` non vide" et "un `int` strictement positif", et s'utilisent directement comme paramètres de la méthode `[Property]` - pas besoin d'écrire de générateur pour eux.

## On vérifie que la propriété détecte vraiment un bug
On introduit un mutant dans `Tirer`, à la main :

```csharp
// Avant
chasseurQuiTire.BallesRestantes--;
// Après (mutant)
chasseurQuiTire.BallesRestantes -= 2;
```

On relance :

```text
Falsifiable, after 1 test (1 shrink).
Original:
(NonEmptyString "\027", PositiveInt 1) (At least one control character has been escaped as a char code, e.g. \023)
Shrunk:
(NonEmptyString "a", PositiveInt 1)
```

La première entrée générée contenait un caractère de contrôle bizarre (`FsCheck` génère des `string` volontairement "hostiles" pour maximiser les chances de casser quelque chose) - illisible pour déboguer. Le `shrinking` réduit ensuite automatiquement vers le cas le plus simple qui échoue encore : un nom d'une seule lettre, un chasseur avec une seule balle. Impossible de se tromper sur la cause : `BallesRestantes` ne descend pas de 1.

On remet `BallesRestantes--;` et on vérifie que tout redevient vert.

## Autres propriétés
D'autres règles se prêtent au même traitement, à partir de l'`Example Mapping` :
- `PrendreLapéro` puis `ReprendreLaPartie` : un aller-retour qui doit ramener le `Status` à `EnCours`, quel que soit l'état des chasseurs.
- `TerminerLaPartie` : quel que soit le nombre de chasseurs et leur nombre de galinettes, le vainqueur annoncé a toujours capturé au moins autant de galinettes que n'importe quel autre chasseur du groupe.

## Reflect
- Une propriété avec un générateur trop étroit (ex : des noms toujours différents, jamais de doublon) peut donner une fausse confiance, exactement comme un mauvais exemple (Histoire 1) : elle ne teste que ce que le générateur sait produire.
- Le `TimeProvider` figé (Histoire 1) reste indispensable : même avec des entrées aléatoires, le temps, lui, doit rester déterministe - sinon un run rouge devient impossible à rejouer à l'identique pour le déboguer.
- `AvecPlusieursChasseurs` (Histoire 3) et la propriété de cette histoire cohabitent très bien : demande-toi, pour chaque nouveau test que tu écris, si tu documentes un cas précis (exemple) ou une règle générale (propriété) - les deux se justifient, rarement pour la même raison.

## Le résultat dans le code
Cette étape est appliquée dans `src/Bouchonnois.Tests/` :
- `Usings.cs` : `global using FsCheck;`, `global using FsCheck.Fluent;`, `global using FsCheck.Xunit;`
- `Unit/Service/DemarrerUnePartieDeChasse.cs` : `Sur1TerrainAvecGalinettesEtDesChasseursArmésLaPartieDémarre`
- `Unit/Service/Tirer.cs` : `TirerRetireExactementUneBalle`
