namespace Bouchonnois.Tests.Unit.Service;

public class DemarrerUnePartieDeChasse : PartieDeChasseServiceTest
{
    [Fact]
    public void AvecPlusieursChasseurs()
    {
        var chasseurs = new List<(string, int)>
        {
            ("Dédé", 20),
            ("Bernard", 8),
            ("Robert", 12)
        };
        var terrainDeChasse = ("Pitibon sur Sauldre", 3);

        var id = PartieDeChasseService.Demarrer(terrainDeChasse, chasseurs);

        var savedPartieDeChasse = Repository.SavedPartieDeChasse()!;
        Check.That(savedPartieDeChasse.Id).IsEqualTo(id);
        savedPartieDeChasse
            .ALeStatus(PartieStatus.EnCours)
            .ContientLesGalinettes(3)
            .ContientLeChasseurAvec("Dédé", ballesRestantes: 20, galinettes: 0)
            .ContientLeChasseurAvec("Bernard", ballesRestantes: 8, galinettes: 0)
            .ContientLeChasseurAvec("Robert", ballesRestantes: 12, galinettes: 0)
            .AÉmisLÉvénement(Now,
                "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)");
    }

    [Fact]
    public void EchoueSansChasseurs()
    {
        var chasseurs = new List<(string, int)>();
        var terrainDeChasse = ("Pitibon sur Sauldre", 3);

        Action demarrerPartieSansChasseurs = () => PartieDeChasseService.Demarrer(terrainDeChasse, chasseurs);

        Check.ThatCode(demarrerPartieSansChasseurs).Throws<ImpossibleDeDémarrerUnePartieSansChasseur>();
        Check.That(Repository.SavedPartieDeChasse()).IsNull();
    }

    [Fact]
    public void EchoueAvecUnTerrainSansGalinettes()
    {
        var chasseurs = new List<(string, int)>();
        var terrainDeChasse = ("Pitibon sur Sauldre", 0);

        Action demarrerPartieSansChasseurs = () => PartieDeChasseService.Demarrer(terrainDeChasse, chasseurs);

        Check.ThatCode(demarrerPartieSansChasseurs).Throws<ImpossibleDeDémarrerUnePartieSansGalinettes>();
        Check.That(Repository.SavedPartieDeChasse()).IsNull();
    }

    [Fact]
    public void EchoueSiChasseurSansBalle()
    {
        var chasseurs = new List<(string, int)>
        {
            ("Dédé", 20),
            ("Bernard", 0)
        };
        var terrainDeChasse = ("Pitibon sur Sauldre", 3);

        Action demarrerPartieAvecChasseurSansBalle =
            () => PartieDeChasseService.Demarrer(terrainDeChasse, chasseurs);

        Check.ThatCode(demarrerPartieAvecChasseurSansBalle)
            .Throws<ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle>();
        Check.That(Repository.SavedPartieDeChasse()).IsNull();
    }
}
