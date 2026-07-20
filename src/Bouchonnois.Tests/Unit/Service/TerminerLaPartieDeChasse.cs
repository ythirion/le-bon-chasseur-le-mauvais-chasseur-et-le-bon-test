namespace Bouchonnois.Tests.Unit.Service;

public class TerminerLaPartieDeChasse : PartieDeChasseServiceTest
{
    [Fact]
    public void QuandLaPartieEstEnCoursEt1SeulChasseurGagne()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé(), Bernard(), Robert().AyantDéjàCapturé(2))
        );

        var meilleurChasseur = PartieDeChasseService.TerminerLaPartie(partieDeChasse.Id);

        Check.That(meilleurChasseur).IsEqualTo("Robert");
        Repository.SavedPartieDeChasse()!
            .ALeStatus(PartieStatus.Terminée)
            .AÉmisLÉvénement(Now, "La partie de chasse est terminée, vainqueur : Robert - 2 galinettes");
    }

    [Fact]
    public void QuandLaPartieEstEnCoursEt1SeulChasseurDansLaPartie()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Robert().AyantDéjàCapturé(2))
        );

        var meilleurChasseur = PartieDeChasseService.TerminerLaPartie(partieDeChasse.Id);

        Check.That(meilleurChasseur).IsEqualTo("Robert");
        Repository.SavedPartieDeChasse()!
            .ALeStatus(PartieStatus.Terminée)
            .AÉmisLÉvénement(Now, "La partie de chasse est terminée, vainqueur : Robert - 2 galinettes");
    }

    [Fact]
    public void QuandLaPartieEstEnCoursEt2ChasseursExAequo()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé().AyantDéjàCapturé(2), Bernard().AyantDéjàCapturé(2), Robert())
        );

        var meilleurChasseur = PartieDeChasseService.TerminerLaPartie(partieDeChasse.Id);

        Check.That(meilleurChasseur).IsEqualTo("Dédé, Bernard");
        Repository.SavedPartieDeChasse()!
            .ALeStatus(PartieStatus.Terminée)
            .AÉmisLÉvénement(Now,
                "La partie de chasse est terminée, vainqueur : Dédé - 2 galinettes, Bernard - 2 galinettes");
    }

    [Fact]
    public void QuandLaPartieEstEnCoursEtToutLeMondeBrocouille()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé(), Bernard(), Robert())
        );

        var meilleurChasseur = PartieDeChasseService.TerminerLaPartie(partieDeChasse.Id);

        Check.That(meilleurChasseur).IsEqualTo("Brocouille");
        Repository.SavedPartieDeChasse()!
            .ALeStatus(PartieStatus.Terminée)
            .AÉmisLÉvénement(Now, "La partie de chasse est terminée, vainqueur : Brocouille");
    }

    [Fact]
    public void QuandLesChasseursSontALaperoEtTousExAequo()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(
                    Dédé().AyantDéjàCapturé(3),
                    Bernard().AyantDéjàCapturé(3),
                    Robert().AyantDéjàCapturé(3))
                .EnPleinApéro()
        );

        var meilleurChasseur = PartieDeChasseService.TerminerLaPartie(partieDeChasse.Id);

        Check.That(meilleurChasseur).IsEqualTo("Dédé, Bernard, Robert");
        Repository.SavedPartieDeChasse()!
            .ALeStatus(PartieStatus.Terminée)
            .AÉmisLÉvénement(Now,
                "La partie de chasse est terminée, vainqueur : Dédé - 3 galinettes, Bernard - 3 galinettes, Robert - 3 galinettes");
    }

    [Fact]
    public void EchoueSiLaPartieDeChasseEstDéjàTerminée()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé(), Bernard(), Robert())
                .Terminée()
        );

        var prendreLapéroQuandTerminée = () => PartieDeChasseService.TerminerLaPartie(partieDeChasse.Id);

        Check.ThatCode(prendreLapéroQuandTerminée).Throws<QuandCestFiniCestFini>();
        Check.That(Repository.SavedPartieDeChasse()).IsNull();
    }
}
