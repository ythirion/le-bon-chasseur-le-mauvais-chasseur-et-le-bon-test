namespace Bouchonnois.Tests.Unit.Service;

public class DemarrerUnePartieDeChasse : PartieDeChasseServiceTest
{
    [Fact]
    public Task AvecPlusieursChasseurs()
    {
        var command = DémarrerUnePartieDeChasse()
            .Avec((Chasseurs.Dédé, 20), (Chasseurs.Bernard, 8), (Chasseurs.Robert, 12))
            .SurUnTerrainRicheEnGalinettes(3);

        PartieDeChasseService.Demarrer(command.Terrain, command.Chasseurs);

        return Verify(Repository.SavedPartieDeChasse())
            .DontScrubDateTimes();
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
