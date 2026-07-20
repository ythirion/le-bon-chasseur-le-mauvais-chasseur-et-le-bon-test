namespace Bouchonnois.Tests.Unit.Service;

public class TirerSurUneGalinette : PartieDeChasseServiceTest
{
    [Fact]
    public void AvecUnChasseurAyantDesBallesEtAssezDeGalinettesSurLeTerrain()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé(), Bernard(), Robert())
        );

        PartieDeChasseService.TirerSurUneGalinette(partieDeChasse.Id, "Bernard");

        Repository.SavedPartieDeChasse()!
            .ContientLeChasseurAvec("Bernard", ballesRestantes: 7, galinettes: 1)
            .ContientLesGalinettes(2)
            .AÉmisLÉvénement(Now, "Bernard tire sur une galinette");
    }

    [Fact]
    public void EchoueCarPartieNexistePas()
    {
        var tirerQuandPartieExistePas = () => PartieDeChasseService.TirerSurUneGalinette(Guid.NewGuid(), "Bernard");

        Check.ThatCode(tirerQuandPartieExistePas).Throws<LaPartieDeChasseNexistePas>();
        Check.That(Repository.SavedPartieDeChasse()).IsNull();
    }

    [Fact]
    public void EchoueAvecUnChasseurNayantPlusDeBalles()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé(), Bernard().SansBalles(), Robert())
        );

        var tirerSansBalle = () => PartieDeChasseService.TirerSurUneGalinette(partieDeChasse.Id, "Bernard");

        Check.ThatCode(tirerSansBalle).Throws<TasPlusDeBallesMonVieuxChasseALaMain>();
        Repository.SavedPartieDeChasse()!
            .AÉmisLÉvénement(Now,
                "Bernard veut tirer sur une galinette -> T'as plus de balles mon vieux, chasse à la main");
    }

    [Fact]
    public void EchoueCarPasDeGalinetteSurLeTerrain()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainSansGalinettes()
                .Avec(Dédé(), Bernard(), Robert())
        );

        var tirerAlorsQuePasDeGalinettes =
            () => PartieDeChasseService.TirerSurUneGalinette(partieDeChasse.Id, "Bernard");

        Check.ThatCode(tirerAlorsQuePasDeGalinettes).Throws<TasTropPicoléMonVieuxTasRienTouché>();
        Check.That(Repository.SavedPartieDeChasse()).IsNull();
    }

    [Fact]
    public void EchoueCarLeChasseurNestPasDansLaPartie()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé(), Bernard(), Robert())
        );

        var chasseurInconnuVeutTirer =
            () => PartieDeChasseService.TirerSurUneGalinette(partieDeChasse.Id, "Chasseur inconnu");

        Check.ThatCode(chasseurInconnuVeutTirer)
            .Throws<ChasseurInconnu>()
            .WithMessage("Chasseur inconnu Chasseur inconnu");
        Check.That(Repository.SavedPartieDeChasse()).IsNull();
    }

    [Fact]
    public void EchoueSiLesChasseursSontEnApero()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé(), Bernard(), Robert())
                .EnPleinApéro()
        );

        var tirerEnPleinApéro =
            () => PartieDeChasseService.TirerSurUneGalinette(partieDeChasse.Id, "Chasseur inconnu");

        Check.ThatCode(tirerEnPleinApéro).Throws<OnTirePasPendantLapéroCestSacré>();
        Repository.SavedPartieDeChasse()!
            .AÉmisLÉvénement(Now, "Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!");
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

        var tirerQuandTerminée =
            () => PartieDeChasseService.TirerSurUneGalinette(partieDeChasse.Id, "Chasseur inconnu");

        Check.ThatCode(tirerQuandTerminée).Throws<OnTirePasQuandLaPartieEstTerminée>();
        Repository.SavedPartieDeChasse()!
            .AÉmisLÉvénement(Now, "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée");
    }
}
