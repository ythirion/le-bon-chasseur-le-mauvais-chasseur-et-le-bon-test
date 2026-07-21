using FsCheck.Fluent;
using FsCheck.Xunit;

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

    private static FsCheck.Gen<string> NomGenerator()
        => ArbMap.Default.GeneratorFor<string>().Where(nom => !string.IsNullOrWhiteSpace(nom));

    private static FsCheck.Arbitrary<(string nom, int nbGalinettes)> TerrainRicheEnGalinettesGenerator()
        => (from nom in NomGenerator()
            from nbGalinettes in Gen.Choose(1, 1000)
            select (nom, nbGalinettes)).ToArbitrary();

    private static FsCheck.Gen<(string nom, int nbBalles)> ChasseurArméGenerator()
        => from nom in NomGenerator()
            from nbBalles in Gen.Choose(1, 1000)
            select (nom, nbBalles);

    private static FsCheck.Arbitrary<List<(string nom, int nbBalles)>> GroupeDeChasseursArmésGenerator()
        => (from nbChasseurs in Gen.Choose(1, 50)
            from chasseurs in Gen.ListOf(ChasseurArméGenerator(), nbChasseurs)
            select chasseurs).ToArbitrary();

    private static FsCheck.Arbitrary<(string nom, int nbGalinettes)> TerrainSansGalinettesGenerator()
        => (from nom in NomGenerator()
            // On généralise au-delà de 0 : n'importe quelle valeur qui n'est pas "au moins 1" doit échouer
            from nbGalinettes in Gen.Choose(-1000, 0)
            select (nom, nbGalinettes)).ToArbitrary();

    private static FsCheck.Gen<List<(string nom, int nbBalles)>> GroupeDeChasseursArmésOuVideGenerator()
        => Gen.ListOf(ChasseurArméGenerator());

    private static FsCheck.Gen<(string nom, int nbBalles)> ChasseurSansBalleGenerator()
        => from nom in NomGenerator()
           select (nom, 0);

    private static FsCheck.Arbitrary<List<(string nom, int nbBalles)>> GroupeAvecAuMoinsUnChasseurSansBalleGenerator()
        => (from chasseursArmés in GroupeDeChasseursArmésOuVideGenerator()
            from chasseurSansBalle in ChasseurSansBalleGenerator()
            select chasseursArmés.Append(chasseurSansBalle).ToList()).ToArbitrary();

    [Property]
    public FsCheck.Property Sur1TerrainAvecGalinettesEtDesChasseursArmésLaPartieDémarre()
        => Prop.ForAll(
            TerrainRicheEnGalinettesGenerator(),
            GroupeDeChasseursArmésGenerator(),
            (terrain, chasseurs) =>
            {
                var id = PartieDeChasseService.Demarrer(terrain, chasseurs);
                return Repository.SavedPartieDeChasse()!.Id == id;
            });

    private bool EchoueAvec<TException>(
        (string nom, int nbGalinettes) terrain,
        List<(string nom, int nbBalles)> chasseurs,
        Func<PartieDeChasse?, bool>? assert = null) where TException : Exception
        => MustFailWith<TException>(() => PartieDeChasseService.Demarrer(terrain, chasseurs), assert);

    [Property]
    public FsCheck.Property SansChasseursSurNImporteQuelTerrainRicheEnGalinettes()
        => Prop.ForAll(
            TerrainRicheEnGalinettesGenerator(),
            terrain =>
                EchoueAvec<ImpossibleDeDémarrerUnePartieSansChasseur>(
                    terrain,
                    [],
                    savedPartieDeChasse => savedPartieDeChasse == null));

    [Property]
    public FsCheck.Property SurNImporteQuelTerrainSansGalinettesLaPartieNeDémarrePas()
        => Prop.ForAll(
            TerrainSansGalinettesGenerator(),
            GroupeDeChasseursArmésOuVideGenerator().ToArbitrary(),
            (terrain, chasseurs) =>
                EchoueAvec<ImpossibleDeDémarrerUnePartieSansGalinettes>(
                    terrain,
                    chasseurs,
                    savedPartieDeChasse => savedPartieDeChasse == null));

    [Property]
    public FsCheck.Property SiAuMoinsUnChasseurSansBalleLaPartieNeDémarrePas()
        => Prop.ForAll(
            TerrainRicheEnGalinettesGenerator(),
            GroupeAvecAuMoinsUnChasseurSansBalleGenerator(),
            (terrain, chasseurs) =>
                EchoueAvec<ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle>(
                    terrain,
                    chasseurs,
                    savedPartieDeChasse => savedPartieDeChasse == null));
}