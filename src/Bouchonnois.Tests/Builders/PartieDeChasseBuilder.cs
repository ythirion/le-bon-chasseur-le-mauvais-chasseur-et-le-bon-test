namespace Bouchonnois.Tests.Builders;

public class PartieDeChasseBuilder
{
    private int _nbGalinettes = 3;
    private PartieStatus _status = PartieStatus.EnCours;
    private ChasseurBuilder[] _chasseurs = [];
    private List<Event>? _historique;

    public static PartieDeChasseBuilder UnePartieDeChasseDuBouchonnois() => new();

    public PartieDeChasseBuilder SurUnTerrainRicheEnGalinettes(int nbGalinettes = 3)
    {
        _nbGalinettes = nbGalinettes;
        return this;
    }

    public PartieDeChasseBuilder SurUnTerrainSansGalinettes()
    {
        _nbGalinettes = 0;
        return this;
    }

    public PartieDeChasseBuilder Avec(params ChasseurBuilder[] chasseurs)
    {
        _chasseurs = chasseurs;
        return this;
    }

    public PartieDeChasseBuilder EnPleinApéro()
    {
        _status = PartieStatus.Apéro;
        return this;
    }

    public PartieDeChasseBuilder Terminée()
    {
        _status = PartieStatus.Terminée;
        return this;
    }

    public PartieDeChasseBuilder AvecCommeHistorique(params Event[] events)
    {
        _historique = events.ToList();
        return this;
    }

    public PartieDeChasse Build()
    {
        var terrain = new Terrain("Pitibon sur Sauldre") {NbGalinettes = _nbGalinettes};
        var chasseurs = _chasseurs.Select(c => c.Build()).ToList();

        return _historique is not null
            ? new PartieDeChasse(Guid.NewGuid(), terrain, chasseurs, _historique)
            : new PartieDeChasse(Guid.NewGuid(), terrain, chasseurs, _status);
    }
}