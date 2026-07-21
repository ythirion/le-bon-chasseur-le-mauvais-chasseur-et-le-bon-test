namespace Bouchonnois.Tests.Acceptance;

public class ScenarioTests
{
    private DateTime _time = new(2024, 4, 25, 9, 0, 0, DateTimeKind.Utc);
    private readonly PartieDeChasseRepositoryForTests _repository = new();
    private readonly PartieDeChasseService _service;

    public ScenarioTests() => _service = new PartieDeChasseService(_repository, () => _time);

    [Fact]
    public Task DéroulerUnePartie()
    {
        var command = DémarrerUnePartieDeChasse()
            .Avec((Chasseurs.Dédé, 20), (Chasseurs.Bernard, 8), (Chasseurs.Robert, 12))
            .SurUnTerrainRicheEnGalinettes();

        var id = _service.Demarrer(command.Terrain, command.Chasseurs);

        After(TimeSpan.FromMinutes(10), () => _service.Tirer(id, Chasseurs.Dédé));
        After(TimeSpan.FromMinutes(30), () => _service.TirerSurUneGalinette(id, Chasseurs.Robert));
        After(TimeSpan.FromMinutes(20), () => _service.PrendreLapéro(id));
        After(TimeSpan.FromHours(1), () => _service.ReprendreLaPartie(id));
        After(TimeSpan.FromMinutes(2), () => _service.Tirer(id, Chasseurs.Bernard));
        After(TimeSpan.FromMinutes(1), () => _service.Tirer(id, Chasseurs.Bernard));
        After(TimeSpan.FromMinutes(1), () => _service.TirerSurUneGalinette(id, Chasseurs.Dédé));
        After(TimeSpan.FromMinutes(26), () => _service.TirerSurUneGalinette(id, Chasseurs.Robert));
        After(TimeSpan.FromMinutes(10), () => _service.PrendreLapéro(id));
        After(TimeSpan.FromMinutes(170), () => _service.ReprendreLaPartie(id));
        After(TimeSpan.FromMinutes(11), () => _service.Tirer(id, Chasseurs.Bernard));
        After(TimeSpan.FromSeconds(1), () => _service.Tirer(id, Chasseurs.Bernard));
        After(TimeSpan.FromSeconds(1), () => _service.Tirer(id, Chasseurs.Bernard));
        After(TimeSpan.FromSeconds(1), () => _service.Tirer(id, Chasseurs.Bernard));
        After(TimeSpan.FromSeconds(1), () => _service.Tirer(id, Chasseurs.Bernard));
        After(TimeSpan.FromSeconds(1), () => _service.Tirer(id, Chasseurs.Bernard));
        After(TimeSpan.FromSeconds(1), () => _service.Tirer(id, Chasseurs.Bernard));
        After(TimeSpan.FromMinutes(19), () => _service.TirerSurUneGalinette(id, Chasseurs.Robert));
        After(TimeSpan.FromMinutes(30), () => _service.TerminerLaPartie(id));

        return Verify(_service.ConsulterStatus(id));
    }

    private void After(TimeSpan time, Action act)
    {
        _time = _time.Add(time);
        try
        {
            act();
        }
        catch
        {
            // le scénario contient volontairement un tir sans balle -> exception attendue, ignorée ici
        }
    }
}