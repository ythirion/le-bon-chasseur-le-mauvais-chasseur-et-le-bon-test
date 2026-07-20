namespace Bouchonnois.Tests.Unit.Service;

public class Tirer : PartieDeChasseServiceTest
{
    [Fact]
    public void AvecUnChasseurAyantDesBalles()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé(), Bernard(), Robert())
        );

        PartieDeChasseService.Tirer(partieDeChasse.Id, "Bernard");

        Repository.SavedPartieDeChasse()!
            .ContientLeChasseurAvec("Bernard", ballesRestantes: 7, galinettes: 0)
            .ContientLesGalinettes(3)
            .AÉmisLÉvénement(Now, "Bernard tire");
    }

    [Fact]
    public void EchoueCarPartieNexistePas()
    {
        var tirerQuandPartieExistePas = () => PartieDeChasseService.Tirer(Guid.NewGuid(), "Bernard");

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

        var tirerSansBalle = () => PartieDeChasseService.Tirer(partieDeChasse.Id, "Bernard");

        Check.ThatCode(tirerSansBalle).Throws<TasPlusDeBallesMonVieuxChasseALaMain>();
        Repository.SavedPartieDeChasse()!
            .AÉmisLÉvénement(Now, "Bernard tire -> T'as plus de balles mon vieux, chasse à la main");
    }

    [Fact]
    public void EchoueCarLeChasseurNestPasDansLaPartie()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé(), Bernard(), Robert())
        );

        var chasseurInconnuVeutTirer = () => PartieDeChasseService.Tirer(partieDeChasse.Id, "Chasseur inconnu");

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

        var tirerEnPleinApéro = () => PartieDeChasseService.Tirer(partieDeChasse.Id, "Chasseur inconnu");

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

        var tirerQuandTerminée = () => PartieDeChasseService.Tirer(partieDeChasse.Id, "Chasseur inconnu");

        Check.ThatCode(tirerQuandTerminée).Throws<OnTirePasQuandLaPartieEstTerminée>();
        Repository.SavedPartieDeChasse()!
            .AÉmisLÉvénement(Now, "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée");
    }
}
