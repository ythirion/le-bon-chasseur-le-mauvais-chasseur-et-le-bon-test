namespace Bouchonnois.Tests.Unit.Service;

public class ConsulterStatus : PartieDeChasseServiceTest
{
    [Fact]
    public void QuandLaPartieVientDeDémarrer()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé(), Bernard(), Robert())
                .AvecCommeHistorique(
                    new Event(new DateTime(2024, 4, 25, 9, 0, 12, DateTimeKind.Utc),
                        "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)")
                )
        );

        var status = PartieDeChasseService.ConsulterStatus(partieDeChasse.Id);

        Check.That(status)
            .IsEqualTo(
                "09:00 - La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"
            );
    }

    [Fact]
    public void QuandLaPartieEstTerminée()
    {
        var partieDeChasse = AvecUnePartieDeChasseExistante(
            UnePartieDeChasseDuBouchonnois()
                .SurUnTerrainRicheEnGalinettes()
                .Avec(Dédé(), Bernard(), Robert())
                .AvecCommeHistorique(
                    new Event(new DateTime(2024, 4, 25, 9, 0, 12, DateTimeKind.Utc),
                        "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"),
                    new Event(new DateTime(2024, 4, 25, 9, 10, 0, DateTimeKind.Utc), "Dédé tire"),
                    new Event(new DateTime(2024, 4, 25, 9, 40, 0, DateTimeKind.Utc), "Robert tire sur une galinette"),
                    new Event(new DateTime(2024, 4, 25, 10, 0, 0, DateTimeKind.Utc), "Petit apéro"),
                    new Event(new DateTime(2024, 4, 25, 11, 0, 0, DateTimeKind.Utc), "Reprise de la chasse"),
                    new Event(new DateTime(2024, 4, 25, 11, 2, 0, DateTimeKind.Utc), "Bernard tire"),
                    new Event(new DateTime(2024, 4, 25, 11, 3, 0, DateTimeKind.Utc), "Bernard tire"),
                    new Event(new DateTime(2024, 4, 25, 11, 4, 0, DateTimeKind.Utc), "Dédé tire sur une galinette"),
                    new Event(new DateTime(2024, 4, 25, 11, 30, 0, DateTimeKind.Utc), "Robert tire sur une galinette"),
                    new Event(new DateTime(2024, 4, 25, 11, 40, 0, DateTimeKind.Utc), "Petit apéro"),
                    new Event(new DateTime(2024, 4, 25, 14, 30, 0, DateTimeKind.Utc), "Reprise de la chasse"),
                    new Event(new DateTime(2024, 4, 25, 14, 41, 0, DateTimeKind.Utc), "Bernard tire"),
                    new Event(new DateTime(2024, 4, 25, 14, 41, 1, DateTimeKind.Utc), "Bernard tire"),
                    new Event(new DateTime(2024, 4, 25, 14, 41, 2, DateTimeKind.Utc), "Bernard tire"),
                    new Event(new DateTime(2024, 4, 25, 14, 41, 3, DateTimeKind.Utc), "Bernard tire"),
                    new Event(new DateTime(2024, 4, 25, 14, 41, 4, DateTimeKind.Utc), "Bernard tire"),
                    new Event(new DateTime(2024, 4, 25, 14, 41, 5, DateTimeKind.Utc), "Bernard tire"),
                    new Event(new DateTime(2024, 4, 25, 14, 41, 6, DateTimeKind.Utc), "Bernard tire"),
                    new Event(new DateTime(2024, 4, 25, 14, 41, 7, DateTimeKind.Utc),
                        "Bernard tire -> T'as plus de balles mon vieux, chasse à la main"),
                    new Event(new DateTime(2024, 4, 25, 15, 0, 0, DateTimeKind.Utc), "Robert tire sur une galinette"),
                    new Event(new DateTime(2024, 4, 25, 15, 30, 0, DateTimeKind.Utc),
                        "La partie de chasse est terminée, vainqueur :  Robert - 3 galinettes")
                )
        );

        var status = PartieDeChasseService.ConsulterStatus(partieDeChasse.Id);

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
        var reprendrePartieQuandPartieExistePas = () => PartieDeChasseService.ConsulterStatus(Guid.NewGuid());

        Check.ThatCode(reprendrePartieQuandPartieExistePas).Throws<LaPartieDeChasseNexistePas>();
        Check.That(Repository.SavedPartieDeChasse()).IsNull();
    }
}
