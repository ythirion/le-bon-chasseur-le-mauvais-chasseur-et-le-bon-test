---
theme: default
layout: cover
title: Le bon chasseur, le mauvais chasseur, et le bon test
info: |
  Atelier Devfest Dijon 2026 — Mutation Testing, Test Data Builders,
  Approval Testing, Property-Based Testing, Tests d'architecture.
class: text-center
highlighter: shiki
lineNumbers: true
drawings:
  persist: false
transition: fade
mdc: true
css: unocss
---

<!-- Toute la slide est portée par l'image de couverture (cover.vue) -->

---
layout: section
---

# Qui suis-je ?

<div class="accent-badge mb-6">Yoan Thirion</div>

- 🎯 TODO : rôle / titre (ex. Software Crafter, Coach Agile...)
- 🐙 GitHub : [@ythirion](https://github.com/ythirion)
- 🐦 TODO : X / LinkedIn / Bluesky
- ✍️ TODO : blog / newsletter

<!--
  Placeholder à compléter avec ta bio réelle : parcours, missions actuelles,
  liens vers tes talks / articles précédents.
-->

---
layout: statement
---

# "Never trust a test you haven't seen fail."

Vladimir Khorikov

---
layout: section
---

# Le contexte

Nos vaillants chasseurs du Bouchonnois ont besoin de pouvoir gérer leurs parties de chasse.

Ils ont fait développer un système de gestion par l'entreprise `Toshiba`... et depuis, plus rien n'avance.

- Chaque nouvelle fonctionnalité prend plus de temps que la précédente
- L'entreprise parle d'une soi-disant **dette technique**, sans jamais l'expliquer
- La CI est verte, le coverage est bon... mais personne n'ose plus toucher au code

---
layout: statement
---

# Un signe qui ne trompe pas

Quand une suite de tests verte n'inspire plus confiance à personne, le problème n'est pas dans le code de production.

**Il est dans les tests eux-mêmes.**

---
layout: section
---

# La base de code

Les chasseurs du Bouchonnois et leurs parties de chasse aux galinettes.

- 🧱 `C#` / `.NET 10`
- 🧪 `xUnit` + `NFluent`
- 📊 Coverage : `coverlet`
- 🧬 Mutation Testing : `Stryker.NET`
- ☁️ Analyse continue : `SonarCloud`

<div class="accent-badge mt-8">On code en live à partir d'ici</div>

---

# Un test, en apparence propre

La CI est verte. Voici un test de `PartieDeChasseServiceTests.cs` :

```csharp {all|1-8|10-11|all}
[Fact]
public void EchoueAvecUnChasseurNayantPlusDeBalles()
{
    var id = Guid.NewGuid();
    var repository = new PartieDeChasseRepositoryForTests();
    repository.Add(/* ... Bernard, 0 balle ... */);

    var service = new PartieDeChasseService(repository, () => DateTime.Now);
    var tirerSansBalle = () => service.TirerSurUneGalinette(id, "Bernard");

    Check.ThatCode(tirerSansBalle)
        .Throws<TasPlusDeBallesMonVieuxChasseALaMain>();
}
```

<v-click>

Il vérifie le *type* de l'exception. Pas le message métier, pas l'événement ajouté, pas la sauvegarde.

**Ce test ment.**

</v-click>

---
layout: section
---

# Au programme

<div class="text-lg space-y-3 mt-4">

1. **Le bon test ne ment pas** — Mutation Testing
2. **Le bon test, on le lit** — Test Data Builders, DSL Given/When/Then
3. **Le bon test, on le maintient** — Clean Code appliqué aux tests
4. **Le bon test, parfois, ne s'écrit pas à la main** — Approval Testing
5. **Le bon test couvre ce que tu n'as pas pensé à tester** — Property-Based Testing
6. **Le bon test protège l'architecture** — Tests d'architecture

</div>

---
layout: statement
---

# Merci !

Des questions ?

<div class="accent-badge mt-6">#sharingiscaring</div>
