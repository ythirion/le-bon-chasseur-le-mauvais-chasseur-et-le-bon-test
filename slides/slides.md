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
layout: section
---

# Leur philosophie

<img src="/quote-chasseurs.webp" class="w-3/4 mx-auto rounded-lg" />

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

<div class="flex items-center gap-12">

<div class="flex-1">

# Outside-in Code Review
- [ ] Technologies utilisées
- [ ] Compiler / exécuter le code : analyser les potentiels `Warning`
- [ ] Analyser la structure de la solution afin de comprendre l'architecture en place
- [ ] Regarder les dépendances afin de comprendre les interactions potentielles du système
- [ ] Calculer le `code coverage`
- [ ] Analyser le rapport d'analyse static de code
- [ ] Identifier s'il y a des [`hotspots`](https://understandlegacycode.com/blog/focus-refactoring-with-hotspots-analysis/) et où ils sont localisés

</div>
    <a href="https://canva.link/4b9mxwe0oxw67js" target="_blank">
        <img src="/outside-in-discovery.webp" class="w-56 flex-shrink-0" />
    </a>
</div>

<!-- Libyear, Analyse comportementale de code, skill claude associée, C4 model, ... -->

---
layout: section
---

# Technologies utilisées

- `C#` / `.NET 10`
- `xUnit` + `NFluent`
- Coverage : `coverlet`
- Analyse statique de code : `SonarCloud`

---
layout: section
---

# Compiler

![Build .NET 10 app](/build.webp)

<div class="accent-badge mt-8">Aucun warning</div>

---
layout: section
---

# Architecture / Dépendance

<img src="/dependencies.webp" class="w-4/5 mx-auto" />

---
layout: section
---

# Calculer le code coverage

<img src="/coverage.webp" class="w-4/5 mx-auto" />

---
layout: section
---

# Analyse static de code

<div class="flex items-center gap-4">
    <img src="/sonar.webp" class="w-1/2 object-contain" />
    <img src="/sonar-cc.webp" class="w-1/2 object-contain" />
</div>

---
layout: section
---

# Analyse comportementale de code

<img src="/hotspot.webp" class="w-2/3 mx-auto" />

---
codeSlide: true
---

# Où se cache la complexité...

```csharp {*}{maxHeight:'380px'}
namespace Bouchonnois.Service
{
    public class PartieDeChasseService
    {
        ...
        
        public void TirerSurUneGalinette(Guid id, string chasseur)
        {
            var partieDeChasse = _repository.GetById(id);

            if (partieDeChasse == null)
            {
                throw new LaPartieDeChasseNexistePas();
            }

            if (partieDeChasse.Terrain.NbGalinettes != 0)
            {
                if (partieDeChasse.Status != PartieStatus.Apéro)
                {
                    if (partieDeChasse.Status != PartieStatus.Terminée)
                    {
                        if (partieDeChasse.Chasseurs.Exists(c => c.Nom == chasseur))
                        {
                            var chasseurQuiTire = partieDeChasse.Chasseurs.First(c => c.Nom == chasseur);

                            if (chasseurQuiTire.BallesRestantes == 0)
                            {
                                partieDeChasse.Events.Add(new Event(_timeProvider(),
                                    $"{chasseur} veut tirer sur une galinette -> T'as plus de balles mon vieux, chasse à la main"));
                                _repository.Save(partieDeChasse);

                                throw new TasPlusDeBallesMonVieuxChasseALaMain();
                            }

                            chasseurQuiTire.BallesRestantes--;
                            chasseurQuiTire.NbGalinettes++;
                            partieDeChasse.Terrain.NbGalinettes--;
                            partieDeChasse.Events.Add(new Event(_timeProvider(), $"{chasseur} tire sur une galinette"));
                        }
                        else
                        {
                            throw new ChasseurInconnu(chasseur);
                        }
                    }
                    else
                    {
                        partieDeChasse.Events.Add(new Event(_timeProvider(),
                            $"{chasseur} veut tirer -> On tire pas quand la partie est terminée"));
                        _repository.Save(partieDeChasse);

                        throw new OnTirePasQuandLaPartieEstTerminée();
                    }
                }
                else
                {
                    partieDeChasse.Events.Add(new Event(_timeProvider(),
                        $"{chasseur} veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!"));
                    _repository.Save(partieDeChasse);
                    throw new OnTirePasPendantLapéroCestSacré();
                }
            }
            else
            {
                throw new TasTropPicoléMonVieuxTasRienTouché();
            }

            _repository.Save(partieDeChasse);
        }
        ...
    }
}
```

<style>
:deep(pre) {
  font-size: 0.7em;
}
</style>

<v-click>

Il vérifie le *type* de l'exception. Pas le message métier, pas l'événement ajouté, pas la sauvegarde.

**Ce test ment.**

</v-click>

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

