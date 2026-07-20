namespace Bouchonnois.Tests.Unit.Service;

public class ReprendreLaPartieDeChasse : PartieDeChasseServiceTest
{
    [Fact]
    public void QuandLapéroEstEnCours()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé(), Bernard(), Robert())
                .EnPleinApéro()
        );

        PartieDeChasseService.ReprendreLaPartie(partieDeChasse.Id);

        Repository.SavedPartieDeChasse()!
            .ALeStatus(PartieStatus.EnCours)
            .AÉmisLÉvénement(Now, "Reprise de la chasse");
    }

    [Fact]
    public void EchoueCarPartieNexistePas()
    {
        var reprendrePartieQuandPartieExistePas = () => PartieDeChasseService.ReprendreLaPartie(Guid.NewGuid());

        Check.ThatCode(reprendrePartieQuandPartieExistePas).Throws<LaPartieDeChasseNexistePas>();
        Check.That(Repository.SavedPartieDeChasse()).IsNull();
    }

    [Fact]
    public void EchoueSiLaChasseEstEnCours()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé(), Bernard(), Robert())
        );

        var reprendreLaPartieQuandChasseEnCours = () => PartieDeChasseService.ReprendreLaPartie(partieDeChasse.Id);

        Check.ThatCode(reprendreLaPartieQuandChasseEnCours).Throws<LaChasseEstDéjàEnCours>();
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

        var prendreLapéroQuandTerminée = () => PartieDeChasseService.ReprendreLaPartie(partieDeChasse.Id);

        Check.ThatCode(prendreLapéroQuandTerminée).Throws<QuandCestFiniCestFini>();
        Check.That(Repository.SavedPartieDeChasse()).IsNull();
    }
}
