namespace Bouchonnois.Tests.Unit.Service;

public class PrendreLApéro : PartieDeChasseServiceTest
{
    [Fact]
    public void QuandLaPartieEstEnCours()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé(), Bernard(), Robert())
        );

        PartieDeChasseService.PrendreLapéro(partieDeChasse.Id);

        Repository.SavedPartieDeChasse()!
            .ALeStatus(PartieStatus.Apéro)
            .AÉmisLÉvénement(Now, "Petit apéro");
    }

    [Fact]
    public void EchoueCarPartieNexistePas()
    {
        var apéroQuandPartieExistePas = () => PartieDeChasseService.PrendreLapéro(Guid.NewGuid());

        Check.ThatCode(apéroQuandPartieExistePas).Throws<LaPartieDeChasseNexistePas>();
        Check.That(Repository.SavedPartieDeChasse()).IsNull();
    }

    [Fact]
    public void EchoueSiLesChasseursSontDéjaEnApero()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé(), Bernard(), Robert())
                .EnPleinApéro()
        );

        var prendreLApéroQuandOnPrendDéjàLapéro = () => PartieDeChasseService.PrendreLapéro(partieDeChasse.Id);

        Check.ThatCode(prendreLApéroQuandOnPrendDéjàLapéro).Throws<OnEstDéjàEnTrainDePrendreLapéro>();
        Check.That(Repository.SavedPartieDeChasse()).IsNull();
    }

    [Fact]
    public void EchoueSiLaPartieDeChasseEstTerminée()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé(), Bernard(), Robert())
                .Terminée()
        );

        var prendreLapéroQuandTerminée = () => PartieDeChasseService.PrendreLapéro(partieDeChasse.Id);

        Check.ThatCode(prendreLapéroQuandTerminée).Throws<OnPrendPasLapéroQuandLaPartieEstTerminée>();
        Check.That(Repository.SavedPartieDeChasse()).IsNull();
    }
}
