using Bouchonnois.Domain;
using Bouchonnois.Domain.Exceptions;
using Bouchonnois.Service;
using Bouchonnois.Tests.Doubles;

namespace Bouchonnois.Tests.Service
{
    public class PartieDeChasseServiceTests
    {
        private static readonly DateTime Now = new(2024, 6, 6, 14, 50, 45, DateTimeKind.Utc);
        private static readonly Func<DateTime> TimeProvider = () => Now;

        private static void AssertLastEvent(PartieDeChasse partieDeChasse, string expectedMessage)
        {
            Check.That(partieDeChasse.Events).HasSize(1);
            Check.That(partieDeChasse.Events[0]).IsEqualTo(new Event(Now, expectedMessage));
        }

        public class DemarrerUnePartieDeChasse
        {
            [Fact]
            public void AvecPlusieursChasseurs()
            {
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);
                var chasseurs = new List<(string, int)>
                {
                    ("Dédé", 20),
                    ("Bernard", 8),
                    ("Robert", 12)
                };
                var terrainDeChasse = ("Pitibon sur Sauldre", 3);
                var id = service.Demarrer(
                    terrainDeChasse,
                    chasseurs
                );

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

                AssertLastEvent(savedPartieDeChasse,
                    "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)");
            }

            [Fact]
            public void EchoueSansChasseurs()
            {
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);
                var chasseurs = new List<(string, int)>();
                var terrainDeChasse = ("Pitibon sur Sauldre", 3);

                Action demarrerPartieSansChasseurs = () => service.Demarrer(terrainDeChasse, chasseurs);

                Check.ThatCode(demarrerPartieSansChasseurs).Throws<ImpossibleDeDémarrerUnePartieSansChasseur>();
                Check.That(repository.SavedPartieDeChasse()).IsNull();
            }

            [Fact]
            public void EchoueAvecUnTerrainSansGalinettes()
            {
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);
                var chasseurs = new List<(string, int)>();
                var terrainDeChasse = ("Pitibon sur Sauldre", 0);

                Action demarrerPartieSansChasseurs = () => service.Demarrer(terrainDeChasse, chasseurs);

                Check.ThatCode(demarrerPartieSansChasseurs).Throws<ImpossibleDeDémarrerUnePartieSansGalinettes>();
                Check.That(repository.SavedPartieDeChasse()).IsNull();
            }

            [Fact]
            public void EchoueSiChasseurSansBalle()
            {
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);
                var chasseurs = new List<(string, int)>
                {
                    ("Dédé", 20),
                    ("Bernard", 0),
                };
                var terrainDeChasse = ("Pitibon sur Sauldre", 3);

                Action demarrerPartieAvecChasseurSansBalle = () => service.Demarrer(terrainDeChasse, chasseurs);

                Check.ThatCode(demarrerPartieAvecChasseurSansBalle)
                    .Throws<ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle>();
                Check.That(repository.SavedPartieDeChasse()).IsNull();
            }
        }

        public class TirerSurUneGalinette
        {
            [Fact]
            public void AvecUnChasseurAyantDesBallesEtAssezDeGalinettesSurLeTerrain()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12}
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

            [Fact]
            public void EchoueCarPartieNexistePas()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);
                var tirerQuandPartieExistePas = () => service.TirerSurUneGalinette(id, "Bernard");

                Check.ThatCode(tirerQuandPartieExistePas).Throws<LaPartieDeChasseNexistePas>();
                Check.That(repository.SavedPartieDeChasse()).IsNull();
            }

            [Fact]
            public void EchoueAvecUnChasseurNayantPlusDeBalles()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 0},
                    new("Robert") {BallesRestantes = 12}
                ]));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var tirerSansBalle = () => service.TirerSurUneGalinette(id, "Bernard");

                Check.ThatCode(tirerSansBalle).Throws<TasPlusDeBallesMonVieuxChasseALaMain>();
                AssertLastEvent(repository.SavedPartieDeChasse()!,
                    "Bernard veut tirer sur une galinette -> T'as plus de balles mon vieux, chasse à la main");
            }

            [Fact]
            public void EchoueCarPasDeGalinetteSurLeTerrain()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 0},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12}
                ]));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var tirerAlorsQuePasDeGalinettes = () => service.TirerSurUneGalinette(id, "Bernard");

                Check.ThatCode(tirerAlorsQuePasDeGalinettes).Throws<TasTropPicoléMonVieuxTasRienTouché>();
                Check.That(repository.SavedPartieDeChasse()).IsNull();
            }

            [Fact]
            public void EchoueCarLeChasseurNestPasDansLaPartie()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12}
                ]));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var chasseurInconnuVeutTirer = () => service.TirerSurUneGalinette(id, "Chasseur inconnu");

                Check.ThatCode(chasseurInconnuVeutTirer)
                    .Throws<ChasseurInconnu>()
                    .WithMessage("Chasseur inconnu Chasseur inconnu");
                Check.That(repository.SavedPartieDeChasse()).IsNull();
            }

            [Fact]
            public void EchoueSiLesChasseursSontEnApero()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12}
                ], PartieStatus.Apéro));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var tirerEnPleinApéro = () => service.TirerSurUneGalinette(id, "Chasseur inconnu");

                Check.ThatCode(tirerEnPleinApéro).Throws<OnTirePasPendantLapéroCestSacré>();
                AssertLastEvent(repository.SavedPartieDeChasse()!,
                    "Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!");
            }

            [Fact]
            public void EchoueSiLaPartieDeChasseEstTerminée()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12}
                ], PartieStatus.Terminée));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var tirerQuandTerminée = () => service.TirerSurUneGalinette(id, "Chasseur inconnu");

                Check.ThatCode(tirerQuandTerminée).Throws<OnTirePasQuandLaPartieEstTerminée>();
                AssertLastEvent(repository.SavedPartieDeChasse()!,
                    "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée");
            }
        }

        public class Tirer
        {
            [Fact]
            public void AvecUnChasseurAyantDesBalles()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12}
                ]));

                var service = new PartieDeChasseService(repository, TimeProvider);

                service.Tirer(id, "Bernard");

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
                Check.That(savedPartieDeChasse.Chasseurs[1].BallesRestantes).IsEqualTo(7);
                Check.That(savedPartieDeChasse.Chasseurs[1].NbGalinettes).IsEqualTo(0);
                Check.That(savedPartieDeChasse.Chasseurs[2].Nom).IsEqualTo("Robert");
                Check.That(savedPartieDeChasse.Chasseurs[2].BallesRestantes).IsEqualTo(12);
                Check.That(savedPartieDeChasse.Chasseurs[2].NbGalinettes).IsEqualTo(0);

                AssertLastEvent(savedPartieDeChasse, "Bernard tire");
            }

            [Fact]
            public void EchoueCarPartieNexistePas()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);
                var tirerQuandPartieExistePas = () => service.Tirer(id, "Bernard");

                Check.ThatCode(tirerQuandPartieExistePas).Throws<LaPartieDeChasseNexistePas>();
                Check.That(repository.SavedPartieDeChasse()).IsNull();
            }

            [Fact]
            public void EchoueAvecUnChasseurNayantPlusDeBalles()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 0},
                    new("Robert") {BallesRestantes = 12}
                ]));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var tirerSansBalle = () => service.Tirer(id, "Bernard");

                Check.ThatCode(tirerSansBalle).Throws<TasPlusDeBallesMonVieuxChasseALaMain>();
                AssertLastEvent(repository.SavedPartieDeChasse()!,
                    "Bernard tire -> T'as plus de balles mon vieux, chasse à la main");
            }

            [Fact]
            public void EchoueCarLeChasseurNestPasDansLaPartie()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12}
                ]));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var chasseurInconnuVeutTirer = () => service.Tirer(id, "Chasseur inconnu");

                Check.ThatCode(chasseurInconnuVeutTirer)
                    .Throws<ChasseurInconnu>()
                    .WithMessage("Chasseur inconnu Chasseur inconnu");
                Check.That(repository.SavedPartieDeChasse()).IsNull();
            }

            [Fact]
            public void EchoueSiLesChasseursSontEnApero()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12}
                ], PartieStatus.Apéro));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var tirerEnPleinApéro = () => service.Tirer(id, "Chasseur inconnu");

                Check.ThatCode(tirerEnPleinApéro).Throws<OnTirePasPendantLapéroCestSacré>();
                AssertLastEvent(repository.SavedPartieDeChasse()!,
                    "Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!");
            }

            [Fact]
            public void EchoueSiLaPartieDeChasseEstTerminée()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12}
                ], PartieStatus.Terminée));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var tirerQuandTerminée = () => service.Tirer(id, "Chasseur inconnu");

                Check.ThatCode(tirerQuandTerminée).Throws<OnTirePasQuandLaPartieEstTerminée>();
                AssertLastEvent(repository.SavedPartieDeChasse()!,
                    "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée");
            }
        }

        public class PrendreLApéro
        {
            [Fact]
            public void QuandLaPartieEstEnCours()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12}
                ]));

                var service = new PartieDeChasseService(repository, TimeProvider);
                service.PrendreLapéro(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                Check.That(savedPartieDeChasse!.Id).IsEqualTo(id);
                Check.That(savedPartieDeChasse.Status).IsEqualTo(PartieStatus.Apéro);
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

                AssertLastEvent(savedPartieDeChasse, "Petit apéro");
            }

            [Fact]
            public void EchoueCarPartieNexistePas()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);
                var apéroQuandPartieExistePas = () => service.PrendreLapéro(id);

                Check.ThatCode(apéroQuandPartieExistePas).Throws<LaPartieDeChasseNexistePas>();
                Check.That(repository.SavedPartieDeChasse()).IsNull();
            }

            [Fact]
            public void EchoueSiLesChasseursSontDéjaEnApero()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12}
                ], PartieStatus.Apéro));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var prendreLApéroQuandOnPrendDéjàLapéro = () => service.PrendreLapéro(id);

                Check.ThatCode(prendreLApéroQuandOnPrendDéjàLapéro).Throws<OnEstDéjàEnTrainDePrendreLapéro>();
                Check.That(repository.SavedPartieDeChasse()).IsNull();
            }

            [Fact]
            public void EchoueSiLaPartieDeChasseEstTerminée()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12}
                ], PartieStatus.Terminée));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var prendreLapéroQuandTerminée = () => service.PrendreLapéro(id);

                Check.ThatCode(prendreLapéroQuandTerminée).Throws<OnPrendPasLapéroQuandLaPartieEstTerminée>();
                Check.That(repository.SavedPartieDeChasse()).IsNull();
            }
        }

        public class ReprendreLaPartieDeChasse
        {
            [Fact]
            public void QuandLapéroEstEnCours()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12}
                ], PartieStatus.Apéro));

                var service = new PartieDeChasseService(repository, TimeProvider);
                service.ReprendreLaPartie(id);

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

                AssertLastEvent(savedPartieDeChasse, "Reprise de la chasse");
            }

            [Fact]
            public void EchoueCarPartieNexistePas()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);
                var reprendrePartieQuandPartieExistePas = () => service.ReprendreLaPartie(id);

                Check.ThatCode(reprendrePartieQuandPartieExistePas).Throws<LaPartieDeChasseNexistePas>();
                Check.That(repository.SavedPartieDeChasse()).IsNull();
            }

            [Fact]
            public void EchoueSiLaChasseEstEnCours()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12}
                ]));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var reprendreLaPartieQuandChasseEnCours = () => service.ReprendreLaPartie(id);

                Check.ThatCode(reprendreLaPartieQuandChasseEnCours).Throws<LaChasseEstDéjàEnCours>();

                Check.That(repository.SavedPartieDeChasse()).IsNull();
            }

            [Fact]
            public void EchoueSiLaPartieDeChasseEstTerminée()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12}
                ], PartieStatus.Terminée));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var prendreLapéroQuandTerminée = () => service.ReprendreLaPartie(id);

                Check.ThatCode(prendreLapéroQuandTerminée).Throws<QuandCestFiniCestFini>();

                Check.That(repository.SavedPartieDeChasse()).IsNull();
            }
        }

        public class TerminerLaPartieDeChasse
        {
            [Fact]
            public void QuandLaPartieEstEnCoursEt1SeulChasseurGagne()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12, NbGalinettes = 2}
                ]));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var meilleurChasseur = service.TerminerLaPartie(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                Check.That(savedPartieDeChasse!.Id).IsEqualTo(id);
                Check.That(savedPartieDeChasse.Status).IsEqualTo(PartieStatus.Terminée);
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
                Check.That(savedPartieDeChasse.Chasseurs[2].NbGalinettes).IsEqualTo(2);

                Check.That(meilleurChasseur).IsEqualTo("Robert");
                AssertLastEvent(savedPartieDeChasse,
                    "La partie de chasse est terminée, vainqueur : Robert - 2 galinettes");
            }

            [Fact]
            public void QuandLaPartieEstEnCoursEt1SeulChasseurDansLaPartie()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Robert") {BallesRestantes = 12, NbGalinettes = 2}
                ]));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var meilleurChasseur = service.TerminerLaPartie(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                Check.That(savedPartieDeChasse!.Id).IsEqualTo(id);
                Check.That(savedPartieDeChasse.Status).IsEqualTo(PartieStatus.Terminée);
                Check.That(savedPartieDeChasse.Terrain.Nom).IsEqualTo("Pitibon sur Sauldre");
                Check.That(savedPartieDeChasse.Terrain.NbGalinettes).IsEqualTo(3);
                Check.That(savedPartieDeChasse.Chasseurs).HasSize(1);
                Check.That(savedPartieDeChasse.Chasseurs[0].Nom).IsEqualTo("Robert");
                Check.That(savedPartieDeChasse.Chasseurs[0].BallesRestantes).IsEqualTo(12);
                Check.That(savedPartieDeChasse.Chasseurs[0].NbGalinettes).IsEqualTo(2);

                Check.That(meilleurChasseur).IsEqualTo("Robert");
                AssertLastEvent(savedPartieDeChasse,
                    "La partie de chasse est terminée, vainqueur : Robert - 2 galinettes");
            }

            [Fact]
            public void QuandLaPartieEstEnCoursEt2ChasseursExAequo()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20, NbGalinettes = 2},
                    new("Bernard") {BallesRestantes = 8, NbGalinettes = 2},
                    new("Robert") {BallesRestantes = 12}
                ]));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var meilleurChasseur = service.TerminerLaPartie(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                Check.That(savedPartieDeChasse!.Id).IsEqualTo(id);
                Check.That(savedPartieDeChasse.Status).IsEqualTo(PartieStatus.Terminée);
                Check.That(savedPartieDeChasse.Terrain.Nom).IsEqualTo("Pitibon sur Sauldre");
                Check.That(savedPartieDeChasse.Terrain.NbGalinettes).IsEqualTo(3);
                Check.That(savedPartieDeChasse.Chasseurs).HasSize(3);
                Check.That(savedPartieDeChasse.Chasseurs[0].Nom).IsEqualTo("Dédé");
                Check.That(savedPartieDeChasse.Chasseurs[0].BallesRestantes).IsEqualTo(20);
                Check.That(savedPartieDeChasse.Chasseurs[0].NbGalinettes).IsEqualTo(2);
                Check.That(savedPartieDeChasse.Chasseurs[1].Nom).IsEqualTo("Bernard");
                Check.That(savedPartieDeChasse.Chasseurs[1].BallesRestantes).IsEqualTo(8);
                Check.That(savedPartieDeChasse.Chasseurs[1].NbGalinettes).IsEqualTo(2);
                Check.That(savedPartieDeChasse.Chasseurs[2].Nom).IsEqualTo("Robert");
                Check.That(savedPartieDeChasse.Chasseurs[2].BallesRestantes).IsEqualTo(12);
                Check.That(savedPartieDeChasse.Chasseurs[2].NbGalinettes).IsEqualTo(0);

                Check.That(meilleurChasseur).IsEqualTo("Dédé, Bernard");
                AssertLastEvent(savedPartieDeChasse,
                    "La partie de chasse est terminée, vainqueur : Dédé - 2 galinettes, Bernard - 2 galinettes");
            }

            [Fact]
            public void QuandLaPartieEstEnCoursEtToutLeMondeBrocouille()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(
                    id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                    [
                        new("Dédé") {BallesRestantes = 20},
                        new("Bernard") {BallesRestantes = 8},
                        new("Robert") {BallesRestantes = 12}
                    ]
                    , []
                ));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var meilleurChasseur = service.TerminerLaPartie(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                Check.That(savedPartieDeChasse!.Id).IsEqualTo(id);
                Check.That(savedPartieDeChasse.Status).IsEqualTo(PartieStatus.Terminée);
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

                Check.That(meilleurChasseur).IsEqualTo("Brocouille");
                AssertLastEvent(savedPartieDeChasse, "La partie de chasse est terminée, vainqueur : Brocouille");
            }

            [Fact]
            public void QuandLesChasseursSontALaperoEtTousExAequo()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20, NbGalinettes = 3},
                    new("Bernard") {BallesRestantes = 8, NbGalinettes = 3},
                    new("Robert") {BallesRestantes = 12, NbGalinettes = 3}
                ], PartieStatus.Apéro));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var meilleurChasseur = service.TerminerLaPartie(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                Check.That(savedPartieDeChasse!.Id).IsEqualTo(id);
                Check.That(savedPartieDeChasse.Status).IsEqualTo(PartieStatus.Terminée);
                Check.That(savedPartieDeChasse.Terrain.Nom).IsEqualTo("Pitibon sur Sauldre");
                Check.That(savedPartieDeChasse.Terrain.NbGalinettes).IsEqualTo(3);
                Check.That(savedPartieDeChasse.Chasseurs).HasSize(3);
                Check.That(savedPartieDeChasse.Chasseurs[0].Nom).IsEqualTo("Dédé");
                Check.That(savedPartieDeChasse.Chasseurs[0].BallesRestantes).IsEqualTo(20);
                Check.That(savedPartieDeChasse.Chasseurs[0].NbGalinettes).IsEqualTo(3);
                Check.That(savedPartieDeChasse.Chasseurs[1].Nom).IsEqualTo("Bernard");
                Check.That(savedPartieDeChasse.Chasseurs[1].BallesRestantes).IsEqualTo(8);
                Check.That(savedPartieDeChasse.Chasseurs[1].NbGalinettes).IsEqualTo(3);
                Check.That(savedPartieDeChasse.Chasseurs[2].Nom).IsEqualTo("Robert");
                Check.That(savedPartieDeChasse.Chasseurs[2].BallesRestantes).IsEqualTo(12);
                Check.That(savedPartieDeChasse.Chasseurs[2].NbGalinettes).IsEqualTo(3);

                Check.That(meilleurChasseur).IsEqualTo("Dédé, Bernard, Robert");
                AssertLastEvent(savedPartieDeChasse,
                    "La partie de chasse est terminée, vainqueur : Dédé - 3 galinettes, Bernard - 3 galinettes, Robert - 3 galinettes");
            }

            [Fact]
            public void EchoueSiLaPartieDeChasseEstDéjàTerminée()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12}
                ], PartieStatus.Terminée));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var prendreLapéroQuandTerminée = () => service.TerminerLaPartie(id);

                Check.ThatCode(prendreLapéroQuandTerminée).Throws<QuandCestFiniCestFini>();

                Check.That(repository.SavedPartieDeChasse()).IsNull();
            }
        }

        public class ConsulterStatus
        {
            [Fact]
            public void QuandLaPartieVientDeDémarrer()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);

                repository.Add(new(id, new("Pitibon sur Sauldre")
                {
                    NbGalinettes = 3
                }, [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12, NbGalinettes = 2}
                ], [
                    new(new(2024, 4, 25, 9, 0, 12, DateTimeKind.Utc),
                        "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)")
                ]));

                var status = service.ConsulterStatus(id);

                Check.That(status)
                    .IsEqualTo(
                        "09:00 - La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"
                    );
            }

            [Fact]
            public void QuandLaPartieEstTerminée()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);

                repository.Add(new(id, new("Pitibon sur Sauldre") {NbGalinettes = 3},
                [
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12, NbGalinettes = 2}
                ], [
                    new(new(2024, 4, 25, 9, 0, 12, DateTimeKind.Utc),
                        "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"),

                    new(new(2024, 4, 25, 9, 10, 0, DateTimeKind.Utc), "Dédé tire"),
                    new(new(2024, 4, 25, 9, 40, 0, DateTimeKind.Utc), "Robert tire sur une galinette"),
                    new(new(2024, 4, 25, 10, 0, 0, DateTimeKind.Utc), "Petit apéro"),
                    new(new(2024, 4, 25, 11, 0, 0, DateTimeKind.Utc), "Reprise de la chasse"),
                    new(new(2024, 4, 25, 11, 2, 0, DateTimeKind.Utc), "Bernard tire"),
                    new(new(2024, 4, 25, 11, 3, 0, DateTimeKind.Utc), "Bernard tire"),
                    new(new(2024, 4, 25, 11, 4, 0, DateTimeKind.Utc), "Dédé tire sur une galinette"),
                    new(new(2024, 4, 25, 11, 30, 0, DateTimeKind.Utc), "Robert tire sur une galinette"),
                    new(new(2024, 4, 25, 11, 40, 0, DateTimeKind.Utc), "Petit apéro"),
                    new(new(2024, 4, 25, 14, 30, 0, DateTimeKind.Utc), "Reprise de la chasse"),
                    new(new(2024, 4, 25, 14, 41, 0, DateTimeKind.Utc), "Bernard tire"),
                    new(new(2024, 4, 25, 14, 41, 1, DateTimeKind.Utc), "Bernard tire"),
                    new(new(2024, 4, 25, 14, 41, 2, DateTimeKind.Utc), "Bernard tire"),
                    new(new(2024, 4, 25, 14, 41, 3, DateTimeKind.Utc), "Bernard tire"),
                    new(new(2024, 4, 25, 14, 41, 4, DateTimeKind.Utc), "Bernard tire"),
                    new(new(2024, 4, 25, 14, 41, 5, DateTimeKind.Utc), "Bernard tire"),
                    new(new(2024, 4, 25, 14, 41, 6, DateTimeKind.Utc), "Bernard tire"),
                    new(new(2024, 4, 25, 14, 41, 7, DateTimeKind.Utc),
                        "Bernard tire -> T'as plus de balles mon vieux, chasse à la main"),

                    new(new(2024, 4, 25, 15, 0, 0, DateTimeKind.Utc), "Robert tire sur une galinette"),
                    new(new(2024, 4, 25, 15, 30, 0, DateTimeKind.Utc),
                        "La partie de chasse est terminée, vainqueur :  Robert - 3 galinettes")
                ]));

                var status = service.ConsulterStatus(id);

                Check.That(status)
                    .IsEqualTo(
                        @"15:30 - La partie de chasse est terminée, vainqueur :  Robert - 3 galinettes
15:00 - Robert tire sur une galinette
14:41 - Bernard tire -> T'as plus de balles mon vieux, chasse à la main
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:30 - Reprise de la chasse
11:40 - Petit apéro
11:30 - Robert tire sur une galinette
11:04 - Dédé tire sur une galinette
11:03 - Bernard tire
11:02 - Bernard tire
11:00 - Reprise de la chasse
10:00 - Petit apéro
09:40 - Robert tire sur une galinette
09:10 - Dédé tire
09:00 - La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"
                    );
            }

            [Fact]
            public void EchoueCarPartieNexistePas()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);
                var reprendrePartieQuandPartieExistePas = () => service.ConsulterStatus(id);

                Check.ThatCode(reprendrePartieQuandPartieExistePas).Throws<LaPartieDeChasseNexistePas>();
                Check.That(repository.SavedPartieDeChasse()).IsNull();
            }
        }
    }
}