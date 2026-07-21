# Histoire 3 : Le bon test, parfois, ne s'écrit pas à la main
> "A picture's worth 1000 tests." — Approval Tests

## Un test qu'on n'ose plus mettre à jour
Si tu as fait les Histoires 1 et 2, `PartieDeChasseServiceTests.cs` ne ment plus et se lit en 5 secondes - `Test Data Builders`, `Object Mothers`, assertions métier (`ContientLeChasseurAvec`, `AÉmisLÉvénement`, ...) sont en place. 

Mais regarde ces deux tests, restés tels quels dans `src/Bouchonnois.Tests/Service/PartieDeChasseServiceTests.cs` :

```csharp
public class DemarrerUnePartieDeChasse
{
    [Fact]
    public void AvecPlusieursChasseurs()
    {
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, () => DateTime.Now);
        var chasseurs = new List<(string, int)>
        {
            ("Dédé", 20),
            ("Bernard", 8),
            ("Robert", 12)
        };
        var terrainDeChasse = ("Pitibon sur Sauldre", 3);
        var id = service.Demarrer(terrainDeChasse, chasseurs);

        var savedPartieDeChasse = repository.SavedPartieDeChasse();
        Check.That(savedPartieDeChasse!.Id).IsEqualTo(id);
        Check.That(savedPartieDeChasse.Status).IsEqualTo(PartieStatus.EnCours);
        Check.That(savedPartieDeChasse.Terrain.Nom).IsEqualTo("Pitibon sur Sauldre");
        Check.That(savedPartieDeChasse.Terrain.NbGalinettes).IsEqualTo(3);
        Check.That(savedPartieDeChasse.Chasseurs).HasSize(3);
        Check.That(savedPartieDeChasse.Chasseurs[0].Nom).IsEqualTo("Dédé");
        Check.That(savedPartieDeChasse.Chasseurs[0].BallesRestantes).IsEqualTo(20);
        Check.That(savedPartieDeChasse.Chasseurs[0].NbGalinettes).IsEqualTo(0);
        Check.That(savedPartieDeChasse.Chasseurs[1].Nom).IsEqualTo("Bernard");
        Check.That(savedPartieDeChasse.Chasseurs[1].BallesRestantes).IsEqualTo(8);
        Check.That(savedPartieDeChasse.Chasseurs[1].NbGalinettes).IsEqualTo(0);
        Check.That(savedPartieDeChasse.Chasseurs[2].Nom).IsEqualTo("Robert");
        Check.That(savedPartieDeChasse.Chasseurs[2].BallesRestantes).IsEqualTo(12);
        Check.That(savedPartieDeChasse.Chasseurs[2].NbGalinettes).IsEqualTo(0);
    }
}
```

Et, un peu plus bas dans `src/Bouchonnois.Tests/Service/ScenarioTests.cs`, un test qui rejoue une partie complète - démarrage, tirs, apéros, reprise, jusqu'à la fin de partie - puis vérifie tout ça d'un coup :

```csharp
Check.That(service.ConsulterStatus(id))
    .IsEqualTo(
        @"15:30 - La partie de chasse est terminée, vainqueur : Robert - 3 galinettes
15:00 - Robert tire sur une galinette
14:41 - Bernard tire -> T'as plus de balles mon vieux, chasse à la main
14:41 - Bernard tire
...
09:00 - La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"
    );
```

À première vue, `AvecPlusieursChasseurs` ressemble à n'importe quel autre test "avant Histoire 2" - une checklist de `Check.That`. On pourrait le corriger avec les outils de l'Histoire 2 : `savedPartieDeChasse.ALeStatus(EnCours).ContientLesGalinettes(3).ContientLeChasseurAvec("Dédé", 20, 0).ContientLeChasseurAvec("Bernard", 8, 0).ContientLeChasseurAvec("Robert", 12, 0).AÉmisLÉvénement(...)` fonctionnerait très bien, et ferait déjà passer le test de 13 à 6 lignes. Garde cette option sous le coude, on va y revenir.

Le test de `ScenarioTests`, lui, a l'air minutieux : une seule assertion, mais qui semble vérifier l'intégralité de l'historique produit par 19 actions.

**Essaie, honnêtement, d'écrire à la main la valeur attendue de cette `string` de 19 lignes - horodatages compris - sans lancer le test une seule fois.** Personne ne fait ça. Dans les faits, cette chaîne a été obtenue en exécutant le code une première fois, en lisant le résultat, puis en le recopiant tel quel dans le test. Ce n'est pas un jugement métier sur ce que le système *devrait* produire : c'est une capture de ce qu'il produit *déjà*, promue au rang d'assertion.

## Ta mission (partie 1)
Pose-toi ces questions sur les deux tests ci-dessus :

- Dans `AvecPlusieursChasseurs`, combien de lignes `Check.That` existent ? Si `Chasseur` gagne un nouveau champ obligatoire demain, combien de ces lignes faut-il toucher - alors que le comportement testé (`Demarrer` avec plusieurs chasseurs) n'a pas changé d'un poil ? Et si, une fois passé aux assertions métier de l'Histoire 2, la partie de chasse comptait 20 chasseurs au lieu de 3 - qu'est-ce qui grossirait, `ContientLeChasseurAvec(...)` répété 20 fois, ou un `Approval Test` ?
- Pour la `string` de `ScenarioTests`, d'où vient concrètement chaque horodatage ? Est-ce que tu peux l'expliquer sans relancer le test - ou est-ce que, comme tout le monde, tu ferais confiance au dernier run vert pour la regénérer ?
- Essaie d'imaginer une assertion métier qui validerait les 19 lignes d'historique de `ScenarioTests` d'un coup, comme `ContientLeChasseurAvec` le fait pour un chasseur. À quoi ressemblerait-elle ? Qu'est-ce que ça te dit sur la différence entre "quelques faits ciblés" (Histoire 2) et "une trace complète" (ici) ?

Note tes réponses, on va y revenir.

## Le concept : Approval Testing
Le point commun entre ces deux tests : ce qu'ils vérifient n'est **pas** "2 ou 3 faits ciblés sur un objet" (le terrain d'Histoire 2), mais **l'intégralité d'une structure ou d'une trace**. Écrire cela à la main, champ par champ, ligne par ligne, ne fait que déplacer le problème : on obtient une nouvelle checklist, aussi longue que l'ancienne, tout aussi coûteuse à maintenir à la moindre évolution de forme.

L'[`Approval Testing`](https://github.com/ythirion/approval-testing-kata#2-approval-testing) propose une autre approche : au lieu d'écrire la valeur attendue à la main, on **capture** la sortie réelle du code dans un fichier de référence ("approved" / "verified"), et on laisse un outil comparer, à chaque exécution, la sortie du jour à ce fichier.

- Première exécution : pas de fichier de référence -> le test échoue, on nous montre la sortie produite.
- On la relit, on juge qu'elle est correcte -> on l'**approuve** (on la copie comme référence).
- Exécutions suivantes : le test compare la sortie du jour au fichier approuvé. Identique -> vert. Différent -> rouge, avec un diff sous les yeux : à toi de décider si c'est une régression (tu corriges le code) ou une évolution voulue (tu réapprouves).

C'est un test comme un autre : il peut - et doit - échouer. La seule différence, c'est *qui* écrit la valeur attendue la première fois : toi, en relisant une sortie, plutôt que toi, en la devinant/recalculant à la main.

### Un air de déjà-vu : le `Golden Master`
Cette idée n'est pas nouvelle. Michael Feathers la décrit dans *Working Effectively with Legacy Code* sous le nom de `Golden Master` (ou `Characterization Test`) : face à du code existant, sans spec fiable, dont personne ne sait exactement ce qu'il est censé faire, on ne *devine* pas le comportement attendu - on **capture** le comportement actuel, tel quel, et on s'en sert de filet de sécurité pour refactorer sans tout casser en silence.

Ça devrait te rappeler quelque chose : c'est très exactement la situation de départ de tout cet atelier - un système développé par `Toshiba`, plus personne pour expliquer la `dette technique`, et des tests qui sont la seule trace fiable de ce que le système fait vraiment (cf. le contexte du [`README`](../../README.md)). `Verify` et l'`Approval Testing` sont l'outillage moderne de cette même idée : que ce soit pour caractériser du code existant avant de le toucher, ou pour remplacer une assertion écrite à la main sur une trace complète, le principe est identique - la sortie réelle du programme fait foi, on l'approuve, on la protège.

[`Verify`](https://github.com/VerifyTests/Verify) est la librairie qui va jouer ce rôle sur notre codebase `C#` :

```bash
dotnet add package Verify.xUnit
```

Un détail important : les données non déterministes (`Guid`, `DateTime`, ...) sont automatiquement remplacées par des placeholders dans le fichier approuvé - c'est le `scrubbing`. Pratique la plupart du temps ; à désactiver explicitement (`.DontScrubDateTimes()`) quand l'horodatage fait justement partie de ce qu'on veut vérifier.

## Ta mission (partie 2) : approuver ce qui mérite de l'être
1. Ajoute la dépendance `Verify.xUnit` au projet de tests.
2. Transforme `AvecPlusieursChasseurs` en `Approval Test` :
   - La méthode de test retourne un `Task` et se termine par `return Verify(...)`
   - Premier lancement : le fichier `[NomClasse].[NomTest].verified.txt` n'existe pas encore, le test échoue et produit un `.received.txt` - relis-le, puis copie-le en `.verified.txt` pour l'approuver (ou laisse ton IDE/outil de diff le faire pour toi).
3. `Repository.SavedPartieDeChasse()` contient un `Guid` et un `DateTime` : regarde ce que le `scrubbing` par défaut en fait dans le fichier approuvé, puis désactive-le pour l'horodatage (`.DontScrubDateTimes()`) puisqu'on veut le garder sous contrôle.
4. Casse volontairement le test (modifie le fichier `.verified.txt` à la main) pour t'assurer qu'il détecte bien un écart - `never trust a test you haven't seen fail`, Approval Test ou pas.
5. Ajoute les fichiers `*.received.txt` au `.gitignore` (c'est la sortie produite à chaque run raté, jamais à committer).
6. Applique la même transformation à `ConsulterStatus.QuandLaPartieVientDeDémarrer` et `QuandLaPartieEstTerminée`, puis à `ScenarioTests.DéroulerUnePartie` - c'est là que le gain est le plus spectaculaire : une chaîne de 19 lignes recopiée à la main devient un `return Verify(...)` d'une ligne.
7. Applique la [règle du boy scout](https://deviq.com/principles/boy-scout-rule) au test `ScenarioTests` une fois qu'il est vert et fiable : extraction de champs (`_time`, `_service`), suppression des `string` en dur (chasseurs, terrain) via un petit `Builder` dédié à la commande `Demarrer`, extraction d'une méthode pour factoriser le motif répété "avance le temps, exécute une action, ignore l'exception éventuelle".

## Pour aller plus loin
- Repère d'autres tests du fichier qui ressemblent aux deux étudiés ici (grosse structure ou grosse trace vérifiée d'un bloc) - sont-ils, eux aussi, de bons candidats à l'Approval Testing ?
- Relance une analyse de santé du code (`Codescene`/`SonarCloud`) avant/après ce refactoring : qu'est-ce que ça donne sur les fichiers de test que tu viens de transformer ?

## Reflect
- Qu'est-ce qui empêche quelqu'un d'approuver une sortie sans la relire vraiment (le fameux *"rubber stamping"*) ? Qu'est-ce que ça coûterait si ça arrivait sur `ScenarioTests.DéroulerUnePartie` ?
- Le `scrubbing` des `DateTime`/`Guid` masque-t-il un vrai problème de déterminisme dans le code, ou est-ce juste un outil qui accepte que certaines valeurs ne soient jamais identiques d'un run à l'autre ?
- Reprends `AvecPlusieursChasseurs` : est-ce que l'Approval Testing avait vraiment plus de sens ici que les assertions métier de l'Histoire 2 (`ContientLeChasseurAvec`, ...) ? Où tracerais-tu la limite entre "quelques faits ciblés -> assertion métier nommée" et "toute une structure/trace -> Approval Test" ?
- On vient d'utiliser cette technique sur du code qu'on comprend déjà (Histoires 1 et 2 sont passées par là). Imagine maintenant qu'on découvre `PartieDeChasseService` pour la première fois, sans aucun test : à quoi servirait un `Golden Master` *avant* de toucher au code, plutôt qu'*après* comme on vient de le faire ?

## Solution
Guide étape par étape disponible [ici](solution.md).
