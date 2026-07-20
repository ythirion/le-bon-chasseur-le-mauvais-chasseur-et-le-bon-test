namespace Bouchonnois.Tests.Unit.Service;

public abstract class PartieDeChasseServiceTest
{
    protected static readonly DateTime Now = new(2024, 6, 6, 14, 50, 45, DateTimeKind.Utc);
    private static readonly Func<DateTime> TimeProvider = () => Now;

    protected readonly PartieDeChasseRepositoryForTests Repository;
    protected readonly PartieDeChasseService PartieDeChasseService;

    protected PartieDeChasseServiceTest()
    {
        Repository = new PartieDeChasseRepositoryForTests();
        PartieDeChasseService = new PartieDeChasseService(Repository, TimeProvider);
    }

    protected PartieDeChasse AvecUnePartieDeChasseExistante(PartieDeChasseBuilder builder)
    {
        var partieDeChasse = builder.Build();
        Repository.Add(partieDeChasse);

        return partieDeChasse;
    }
}