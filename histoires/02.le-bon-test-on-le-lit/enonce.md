# Histoire 2 : Le bon test, on le lit
> "Any fool can write code that a computer can understand. Good programmers write code that humans can understand." — Martin Fowler

## Un test qu'il faut déchiffrer
Si tu as fait l'Histoire 1, tu as tué tes mutants : `PartieDeChasseServiceTests.cs` vérifie maintenant `Events` partout, `Stryker` ne trouve plus grand-chose à se mettre sous la dent, et la CI est verte pour de bonnes raisons. Bravo.

Regarde pourtant ce que ça a donné sur `AvecUnChasseurAyantDesBallesEtAssezDeGalinettesSurLeTerrain`, dans `src/Bouchonnois.Tests/Service/PartieDeChasseServiceTests.cs`, une fois la correction appliquée :

```csharp
public class PartieDeChasseServiceTests
{
    private static readonly DateTime Now = new(2024, 6, 6, 14, 50, 45);
    private static readonly Func<DateTime> TimeProvider = () => Now;

    private static void AssertLastEvent(PartieDeChasse partieDeChasse, string expectedMessage)
    {
        Check.That(partieDeChasse.Events).HasSize(1);
        Check.That(partieDeChasse.Events[0]).IsEqualTo(new Event(Now, expectedMessage));
    }

    public class TirerSurUneGalinette
    {
        [Fact]
        public void AvecUnChasseurAyantDesBallesEtAssezDeGalinettesSurLeTerrain()
        {
            var id = Guid.NewGuid();
            var repository = new PartieDeChasseRepositoryForTests();

            repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
            [
                new("Dédé") { BallesRestantes = 20 },
                new("Bernard") { BallesRestantes = 8 },
                new("Robert") { BallesRestantes = 12 }
            ]));

            var service = new PartieDeChasseService(repository, TimeProvider);

            service.TirerSurUneGalinette(id, "Bernard");

            var savedPartieDeChasse = repository.SavedPartieDeChasse();
            Check.That(savedPartieDeChasse!.Id).IsEqualTo(id);
            Check.That(savedPartieDeChasse.Status).IsEqualTo(PartieStatus.EnCours);
            Check.That(savedPartieDeChasse.Terrain.Nom).IsEqualTo("Pitibon sur Sauldre");
            Check.That(savedPartieDeChasse.Terrain.NbGalinettes).IsEqualTo(2);
            Check.That(savedPartieDeChasse.Chasseurs).HasSize(3);
            Check.That(savedPartieDeChasse.Chasseurs[0].Nom).IsEqualTo("Dédé");
            Check.That(savedPartieDeChasse.Chasseurs[0].BallesRestantes).IsEqualTo(20);
            Check.That(savedPartieDeChasse.Chasseurs[0].NbGalinettes).IsEqualTo(0);
            Check.That(savedPartieDeChasse.Chasseurs[1].Nom).IsEqualTo("Bernard");
            Check.That(savedPartieDeChasse.Chasseurs[1].BallesRestantes).IsEqualTo(7);
            Check.That(savedPartieDeChasse.Chasseurs[1].NbGalinettes).IsEqualTo(1);
            Check.That(savedPartieDeChasse.Chasseurs[2].Nom).IsEqualTo("Robert");
            Check.That(savedPartieDeChasse.Chasseurs[2].BallesRestantes).IsEqualTo(12);
            Check.That(savedPartieDeChasse.Chasseurs[2].NbGalinettes).IsEqualTo(0);

            AssertLastEvent(savedPartieDeChasse, "Bernard tire sur une galinette");
        }
    }
}
```

Ce test ne ment plus. Mais il n'a pas rétréci pour autant - il a même grandi d'une ligne (`AssertLastEvent(...)`), et chaque test d'erreur du fichier en a gagné deux (`.WithMessage(...)` sur l'exception, puis `AssertLastEvent(...)` sur l'événement). Le problème n'a pas disparu, il a juste changé de nature. **Combien de temps te faut-il pour dire, sans exécuter le test, ce qu'il vérifie vraiment ?**

Ouvre `PartieDeChasseServiceTests.cs` en entier. Une seule classe, toujours aussi volumineuse, des dizaines de tests qui se ressemblent tous - même `Arrange` copié-collé, même bloc de 13 assertions à la suite. Le signal (*"1 balle en moins et 1 galinette de plus pour Bernard"*) est noyé dans le bruit (l'`Id`, le `Nom` de chaque chasseur, le `Terrain`...) répété identique d'un test à l'autre. `AssertLastEvent` est un bon réflexe - Histoire 1 t'a fait sortir la première brique d'assertion métier du lot - mais une seule brique isolée au milieu d'un mur de `Check.That` ne suffit pas à rendre le mur lisible.

## Ta mission (partie 1)

Choisis 3 tests au hasard dans le fichier (un cas "heureux", un cas d'erreur, un test de `TerminerLaPartieDeChasse`). Pour chacun, chronomètre-toi : combien de temps pour répondre à *"qu'est-ce que ce test prouve, en une phrase ?"*

Pose-toi ensuite ces questions :
- Qu'est-ce qui, dans l'`Arrange`, est vraiment nécessaire à la compréhension du test ? Qu'est-ce qui est juste là parce qu'il faut bien instancier une `PartieDeChasse` ?
- Dans le bloc d'assertions, combien de lignes vérifient le comportement testé et combien vérifient juste "que le reste n'a pas bougé" ? Où se cache `AssertLastEvent` là-dedans - est-ce qu'on le remarque au premier coup d'œil ?
- Si tu devais expliquer ce fichier à quelqu'un qui rejoint l'équipe demain, combien de temps ça prendrait ?

Note tes réponses, on va y revenir.

## Le concept : rendre le test lisible en 5 secondes
Un test qui ne ment pas ne sert à rien si personne n'ose plus le lire. Le but ici n'est pas d'ajouter des vérifications, mais de faire ressortir ce qui compte pour un être humain qui parcourt le fichier.

Trois outils vont nous y aider :

### Test Data Builders
Prends le temps de découvrir le pattern [`Test Data Builder`](https://xtrem-tdd.netlify.app/Flavours/Testing/test-data-builders). L'idée : remplacer la construction directe d'objets (`new PartieDeChasse(id, new Terrain(...) {...}, [new("Dédé") {...}, ...])`) par une API fluide qui exprime en mots ce qui compte pour le test (`UnePartieDeChasseDuBouchonnois().SurUnTerrainRicheEnGalinettes().Avec(Dédé(), Bernard(), Robert())`), et qui cache tout le reste (l'`Id`, la construction du `Terrain`, ...).

Pour paraphraser Robert C. Martin : *"this eliminates the irrelevant, and amplifies the essentials of the test."*

Ressors-en les avantages : qu'est-ce que ça change pour la maintenance quand `PartieDeChasse` gagne un nouveau champ obligatoire ? Quand deux tests ont besoin d'un terrain légèrement différent ?

### Object Mothers
On peut combiner le `Builder` avec le pattern [`Object Mother`](https://martinfowler.com/bliki/ObjectMother.html) de Martin Fowler : des méthodes statiques nommées (`Dédé()`, `Bernard()`, `Robert()`) qui encapsulent des instances "connues" et réutilisables, plutôt que de ressaisir `("Bernard") { BallesRestantes = 8 }` dans chaque test.

### DSL Given / When / Then
On peut aller plus loin en structurant chaque test autour de 3 étapes explicites (`Given` / `When` / `Then`, à la `gherkin`), pour que la forme du test annonce elle-même ce qui est *contexte*, ce qui est *action*, et ce qui est *vérification*.

C'est un outil puissant, mais qui a un coût - on y reviendra dans le `Reflect`.

### Des assertions qui parlent le métier
`AssertLastEvent` (le fruit de l'Histoire 1) est déjà une bonne intuition : une méthode nommée qui cache 2 `Check.That` derrière une phrase métier. On va généraliser cette intuition à tout le reste du bloc d'assertions - puis pousser le raisonnement un cran plus loin.

Plutôt que d'enchaîner `Check.That(...).IsEqualTo(...)` sur chaque champ, on écrit de simples méthodes d'extension sur `PartieDeChasse` qui encapsulent en interne les `Check.That` nécessaires. Pas besoin de plonger dans l'API d'extensibilité interne de [`NFluent`](https://www.n-fluent.net/) (`ICheck<T>`, `ExecuteCheck`, ...) pour ça : une méthode d'extension classique sur l'objet métier suffit, se lit aussi bien, et reste accessible à qui ne connaît pas les entrailles de la librairie d'assertion.

Attention cependant au nom que tu leur donnes. `ShouldHaveChasseurWith(...)` est déjà plus lisible qu'un bloc de `Check.That`, mais `Should` / `Have` restent du vocabulaire de testeur, pas du vocabulaire métier - c'est un test de `PartieDeChasse`, pas un test de framework. Préfère une phrase qui pourrait sortir de la bouche d'un chasseur du Bouchonnois : `partieDeChasse.ContientLeChasseurAvec("Bernard", ballesRestantes: 7, galinettes: 1)`, `.ContientLesGalinettes(2)`, `.AÉmisLÉvénement(...)`. Chaînées, ces assertions se lisent comme une phrase qui décrit l'état attendu de la partie de chasse - pas comme une checklist technique.

C'est plus qu'une question de style : quand un de ces tests échoue en CI, le nom de la méthode qui a levé l'exception **est** le message d'erreur. `ContientLeChasseurAvec` qui échoue te dit immédiatement, dans le langage du métier, quel changement d'état attendu ne s'est pas produit - pas besoin de retraduire mentalement un `Check.That(x.Y).IsEqualTo(z)` générique pour comprendre ce qui a cassé. C'est exactement au moment où tu en as le moins (un test rouge, en pleine CI, sous pression) que cette charge mentale en moins compte le plus.

> ⚠️ Ces `Builders` et ces assertions vont devenir le socle de tous les tests du fichier. S'ils mentent ou laissent passer un mutant, c'est toute la suite de tests qui en hérite. Applique-leur la même exigence qu'à l'Histoire 1.

## Ta mission (partie 2) : rendre le fichier lisible
En partant du test `AvecUnChasseurAyantDesBallesEtAssezDeGalinettesSurLeTerrain` (ou `Tirer.AvecUnChasseurAyantDesBalles`, qui a le même profil) :

1. **Split** la classe de tests : sors chaque classe imbriquée (`DemarrerUnePartieDeChasse`, `TirerSurUneGalinette`, `Tirer`, ...) dans son propre fichier, avec une classe de base commune qui centralise l'instanciation du `Repository` / `Service` - et qui récupère au passage `Now`, `TimeProvider` et `AssertLastEvent` (il te suffit de les rendre `protected`, tu les as déjà écrits en Histoire 1).
2. **Écris un `PartieDeChasseBuilder`** (et un `ChasseurBuilder` avec ses `Object Mothers` `Dédé()` / `Bernard()` / `Robert()`) pour remplacer la construction manuelle de `PartieDeChasse` dans l'`Arrange`.
3. **Écris des méthodes d'extension** sur `PartieDeChasse` pour remplacer le bloc de 13 `Check.That` par 2-3 lignes qui nomment le comportement vérifié - `AssertLastEvent` devient l'une d'entre elles. Nomme-les avec le vocabulaire du métier (`ContientLeChasseurAvec`, `AÉmisLÉvénement`, ...), pas avec le vocabulaire `Should`/`Have` calqué sur l'anglais des frameworks de test.
4. Vérifie que ces nouveaux outils sont dignes de confiance : introduis un mutant à la main dans `PartieDeChasseService` (ex : commente `chasseurQuiTire.NbGalinettes++;`) et assure-toi que ton assertion le détecte.
5. Propage la même stratégie aux tests voisins (`Tirer`, `PrendreLApéro`, `ReprendreLaPartieDeChasse`, `TerminerLaPartieDeChasse`, ...).

## Pour aller plus loin
- Sépare les tests unitaires (`Unit/`) du test de bout en bout (`ScenarioTests.cs`, à ranger dans `Acceptance/`) : ils ne se lisent pas de la même manière et n'ont pas besoin des mêmes outils.
- Essaie d'écrire un DSL `Given` / `When` / `Then` pour un des tests, puis pose-toi la question du `Reflect` ci-dessous avant de le généraliser à tout le fichier.
- Une fois les `Builders` et les assertions en place, relance `Stryker` (Histoire 1) : le score de mutation ne devrait pas bouger - c'est la preuve que t'as changé la forme des tests sans changer ce qu'ils vérifient.

## Reflect
Compare un test avant et après ce refactoring : qu'est-ce qui saute aux yeux en premier maintenant ? Est-ce que tu peux répondre à *"qu'est-ce que ce test prouve ?"* en 5 secondes ?

Regarde aussi le nom de tes méthodes d'assertion : sonnent-elles comme une phrase que dirait un chasseur du Bouchonnois, ou comme un vocabulaire de testeur (`Should` / `Have`) plaqué sur le métier ? Imagine ce test rouge en pleine CI, sous pression : est-ce que le nom de la méthode qui a échoué te dit, sans réfléchir, quel changement d'état attendu ne s'est pas produit ?

Si tu es allé jusqu'au DSL `Given` / `When` / `Then`, prends aussi le temps de lister ses limites :
- Que devient un test simple, à une seule assertion, une fois passé dans le DSL ? Est-ce toujours plus lisible ?
- Que se passe-t-il quand le comportement à tester ne rentre pas dans le moule `Action<Guid>` (ex : `TerminerLaPartie` qui retourne le nom du vainqueur) ?
- Quand ce test échoue en CI, la stack trace pointe-t-elle vers ton `Given`/`When`/`Then` ou vers le vrai problème ?
- Qui, dans l'équipe, doit apprendre ce DSL avant de pouvoir écrire ou déboguer un test ?

Un DSL maison est un outil de plus à maintenir et à documenter - pas juste du sucre syntaxique gratuit. Il ne se justifie que si les tests qu'il simplifie sont nombreux et suffisamment similaires.

## Solution
Guide étape par étape disponible [ici](solution.md).
