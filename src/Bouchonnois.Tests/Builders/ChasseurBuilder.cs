namespace Bouchonnois.Tests.Builders;

public class ChasseurBuilder
{
    private readonly string _nom;
    private int _ballesRestantes;
    private int _nbGalinettes;

    private ChasseurBuilder(string nom, int ballesRestantes)
    {
        _nom = nom;
        _ballesRestantes = ballesRestantes;
    }

    // Object Mothers
    public static ChasseurBuilder Dédé() => new("Dédé", ballesRestantes: 20);
    public static ChasseurBuilder Bernard() => new("Bernard", ballesRestantes: 8);
    public static ChasseurBuilder Robert() => new("Robert", ballesRestantes: 12);

    // Pour les tests de propriétés : un chasseur avec un nom et un nombre de balles quelconques
    public static ChasseurBuilder UnChasseurArmé(string nom, int ballesRestantes) => new(nom, ballesRestantes);

    public ChasseurBuilder SansBalles()
    {
        _ballesRestantes = 0;
        return this;
    }

    public ChasseurBuilder AyantDéjàCapturé(int nbGalinettes)
    {
        _nbGalinettes = nbGalinettes;
        return this;
    }

    public Chasseur Build() => new(_nom) {BallesRestantes = _ballesRestantes, NbGalinettes = _nbGalinettes};
}