# Le bon chasseur, le mauvais chasseur, et le bon test
> "Le mauvais chasseur, il voit un truc qui bouge, il tire. 
> Le bon chasseur, il voit un truc qui bouge, il tire… mais c'est un bon chasseur."

![Couverture de l'atelier "Le bon chasseur, le mauvais chasseur, et le bon test"](img/le-bon-chasseur-le-mauvais-chasseur-et-le-bon-test.webp)
> "Le mauvais test, il assert un truc, il passe au vert. Le bon test, il assert un truc, il passe au vert… mais c'est un bon test."

- Concrètement, qu'est-ce qui sépare les deux ? 
- Pourquoi certaines suites de tests deviennent un harnais qui permet de refactorer en confiance, et d'autres un boulet qu'on désactive au premier sprint chargé ?

## Origine
Atelier créé pour le [Devfest Dijon 2026](https://devfest.developers-group-dijon.fr/) en me basant sur un précédent atelier nommé le [Refactoring du Bouchonnois](https://github.com/ythirion/refactoring-du-bouchonnois/). 

## Le contexte
Nos vaillants chasseurs du Bouchonnois ont besoin de pouvoir gérer leurs parties de chasse.  
Ils ont commencé à faire développer 1 système de gestion par l'entreprise `Toshiba` mais ne sont pas satisfaits.  

L'entreprise leur parle d'une soi-disant `dette technique` qui les ralentit dans le développement de nouvelles fonctionnalités...

[![Les Inconnus](img/inconnus.webp)](https://youtu.be/QuGcoOJKXT8?si=N0e-w8GhgEnrBWv4)

Les chasseurs comptent sur nous pour améliorer la situation.

### Example Mapping
Ils ont fait quelques ateliers avec `Toshiba` et ont réussi à clarifier ce qui est attendu du système.
Pour ce faire, ils ont utilisé le format `Example Mapping` à découvrir [ici](https://xtrem-tdd.netlify.app/Flavours/Practices/example-mapping).

Voici l'`Example Mapping` qui a servi d'alignement pour développer ce système.

![Refactoring du Bouchonnois](example-mapping/example-mapping.webp)

Version PDF disponible [ici](example-mapping/example-mapping.pdf)

## L'atelier
Une base de code en `C#` / `.NET 10` : les chasseurs du Bouchonnois et leurs parties de chasse aux galinettes qu'on va reprendre ensemble.

À chaque histoire, un symptôme, un diagnostic, un remède.

- **Histoire 1 : Le bon test ne ment pas.** Mutation testing : faire la chasse aux assertions qui passent toujours, parce que *"never trust a test you haven't seen fail"*.
- **Histoire 2 : Le bon test, on le lit.**
  Test Data Builders, Object Mothers, custom assertions, DSL Given/When/Then : transformer un test de 30 lignes en spec métier lisible en 5 secondes.
- **Histoire 3 : Le bon test, on le maintient.**
  SRP, DRY, hiérarchie de classes : les principes Clean Code s'appliquent aux tests autant qu'au code de prod.
- **Histoire 4 : Le bon test, parfois, ne s'écrit pas à la main.**
  Approval Testing pour les scénarios à grosses assertions et ses pièges.
- **Histoire 5 : Le bon test couvre ce que tu n'as pas pensé à tester.**
  Property-Based Testing : une propriété, 100 exécutions, des cas tordus que tu n'aurais jamais imaginés.
- **Histoire 6 : Le bon test protège l'architecture.**
  Tests d'architecture : matérialiser les règles d'oignon, d'hexagonal, de Clean Architecture en tests qui échouent au premier merge incorrect.

**Pour qui ?**
Développeur·se·s, tech leads, toute personne qui a déjà soupiré devant un fichier de tests de 900 lignes.
Des exemples concrets transposables à tous les langages.