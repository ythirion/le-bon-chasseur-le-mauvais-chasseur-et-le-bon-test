---
theme: default
colorSchema: light
layout: cover
title: Le bon chasseur, le mauvais chasseur, et le bon test
info: |
  Atelier Devfest Dijon 2026
class: text-center
highlighter: shiki
lineNumbers: true
drawings:
  persist: false
transition: fade
mdc: true
css: unocss
---

---
layout: section
---

<div class="flex items-center gap-16">

<div class="flex-1">

# Qui suis-je ?

<div class="accent-badge mb-6">Yoan Thirion</div>

- Responsable de la pédagogie - [école Coda Dijon](https://coda.school/)
- Software Crafter, Coach Agile, Juste un Dév
- GitHub : [@ythirion](https://github.com/ythirion)
- LinkedIn : [yoanthirion](https://www.linkedin.com/in/yoanthirion/)

</div>

<img src="/ythirion.webp" class="w-56 h-56 rounded-full object-cover flex-shrink-0" style="border: 4px solid var(--sv-yellow)" />

</div>

---
layout: section
---

# Qui connait le Bouchonnois ?

<img src="/inconnus.webp" class="w-2/3 mx-auto rounded-lg" />

---
codeSlide: true
---

<div class="flex items-center gap-12">

<div class="flex-1">


# Le contexte

> Nos valeureux chasseurs du Bouchonnois ont besoin de pouvoir gérer leurs parties de chasse.

Ils ont fait développer un système de gestion par l'entreprise `Toshiba`... et depuis, plus rien n'avance.

- Chaque nouvelle fonctionnalité prend plus de temps que la précédente
- L'entreprise parle d'une soi-disant `dette technique`, sans jamais l'expliquer

</div>

<img src="/chasseur.webp" class="w-56 flex-shrink-0" />

</div>

---
codeSlide: true
---

<div class="relative h-full flex items-center justify-center">

<img src="/example-mapping.webp" class="max-h-full max-w-full object-contain rounded-lg" />

<a href="https://xtrem-tdd.netlify.app/Flavours/Practices/example-mapping" target="_blank" class="link-preview link-preview-sm absolute top-1 right-1">
  <div class="link-preview-title">Example Mapping</div>
  <div class="link-preview-url">xtrem-tdd.netlify.app/Flavours/Practices/example-mapping</div>
</a>

</div>

---
layout: section
---

# La base de code

Les chasseurs du Bouchonnois et leurs parties de chasse aux galinettes.

- `C#` / `.NET 10`
- `xUnit` + `NFluent`
- Coverage : `coverlet`
- Analyse statique de code : `SonarCloud`

<div class="accent-badge mt-8">On code en live à partir d'ici</div>

---
codeSlide: true
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

---
layout: statement
---

# "Never trust a test you haven't seen fail."

Vladimir Khorikov