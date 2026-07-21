using ArchUnitNET.Fluent.Syntax.Elements.Members.MethodMembers;
using ArchUnitNET.Fluent.Syntax.Elements.Types.Interfaces;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Bouchonnois.Tests.Architecture
{
    public class Guidelines
    {
        private static GivenInterfacesConjunction InterfaceTypes() =>
            Interfaces().That().Are(ArchUnitExtensions.Architecture.Types);

        private static GivenMethodMembersThat Methods() => MethodMembers().That().AreNoConstructors().And();

        [Fact]
        public void NoGetMethodShouldReturnVoid() =>
            Methods()
                .HaveNameMatching("Get[A-Z].*").Should()
                .NotHaveReturnType(typeof(void))
                .Check();

        [Fact]
        public void IserAndHaserShouldReturnBooleans() =>
            Methods()
                .HaveNameMatching("Is[A-Z].*").Or()
                .HaveNameMatching("Has[A-Z].*").Should()
                .HaveReturnType(typeof(bool))
                .WithoutRequiringPositiveResults()
                .Check();

        [Fact]
        public void SettersShouldNotReturnSomething() =>
            Methods()
                .HaveNameMatching("Set[A-Z].*").Should()
                .HaveReturnType(typeof(void))
                .WithoutRequiringPositiveResults()
                .Check();

        [Fact]
        public void InterfacesShouldStartWithI() =>
            InterfaceTypes().Should()
                .HaveNameMatching("^I[A-Z].*")
                .Because("C# convention...")
                .Check();
    }
}