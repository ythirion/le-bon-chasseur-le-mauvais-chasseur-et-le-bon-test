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

    // Pour les tests de propriétés sur les cas non-passants : exécute `action` et valide
    // qu'elle lève bien `TException`, avec une assertion optionnelle sur l'état sauvegardé.
    protected bool MustFailWith<TException>(Action action, Func<PartieDeChasse?, bool>? assert = null)
        where TException : Exception
    {
        try
        {
            action();
            return false;
        }
        catch (TException)
        {
            return assert?.Invoke(Repository.SavedPartieDeChasse()) ?? true;
        }
    }
}