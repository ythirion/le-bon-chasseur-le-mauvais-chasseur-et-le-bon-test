namespace Bouchonnois.Tests.Assertions;

internal static class PartieDeChasseAssertions
{
    internal static PartieDeChasse ALeStatus(this PartieDeChasse partieDeChasse, PartieStatus expected)
    {
        Check.That(partieDeChasse.Status).IsEqualTo(expected);
        return partieDeChasse;
    }

    internal static PartieDeChasse ContientLesGalinettes(this PartieDeChasse partieDeChasse, int nbGalinettes)
    {
        Check.That(partieDeChasse.Terrain.NbGalinettes).IsEqualTo(nbGalinettes);
        return partieDeChasse;
    }

    internal static PartieDeChasse ContientLeChasseurAvec(
        this PartieDeChasse partieDeChasse,
        string nom,
        int ballesRestantes,
        int galinettes)
    {
        var chasseur = partieDeChasse.Chasseurs.Single(c => c.Nom == nom);
        Check.That(chasseur.BallesRestantes).IsEqualTo(ballesRestantes);
        Check.That(chasseur.NbGalinettes).IsEqualTo(galinettes);
        return partieDeChasse;
    }

    internal static PartieDeChasse AÉmisLÉvénement(
        this PartieDeChasse partieDeChasse,
        DateTime expectedTime,
        string expectedMessage)
    {
        Check.That(partieDeChasse.Events).HasSize(1);
        Check.That(partieDeChasse.Events[0]).IsEqualTo(new Event(expectedTime, expectedMessage));
        return partieDeChasse;
    }
}