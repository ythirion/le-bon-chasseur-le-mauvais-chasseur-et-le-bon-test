using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Syntax.Elements.Types;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Bouchonnois.Tests.Architecture
{
    public static class ArchUnitExtensions
    {
        public static readonly ArchUnitNET.Domain.Architecture Architecture =
            new ArchLoader()
                .LoadAssemblies(typeof(PartieDeChasseService).Assembly)
                .Build();

        public static GivenTypesConjunction TypesInAssembly() =>
            Types().That().Are(Architecture.Types);

        public static void Check(this IArchRule rule) => rule.Check(Architecture);
    }

    public class ArchitectureRules
    {
        private static GivenTypesConjunctionWithDescription ApplicationServices() =>
            ArchUnitExtensions.TypesInAssembly().And()
                .ResideInNamespaceMatching(".*Service.*")
                .As("Application Services");

        private static GivenTypesConjunctionWithDescription DomainModel() =>
            ArchUnitExtensions.TypesInAssembly().And()
                .ResideInNamespaceMatching(".*Domain.*")
                .As("Domain Model");

        private static GivenTypesConjunctionWithDescription Infrastructure() =>
            ArchUnitExtensions.TypesInAssembly().And()
                .ResideInNamespaceMatching(".*Repository.*")
                .As("Infrastructure");

        [Fact]
        public void DomainModelRules() =>
            DomainModel().Should()
                .NotDependOnAny(ApplicationServices()).AndShould()
                .NotDependOnAny(Infrastructure())
                .Check();

        [Fact]
        public void ApplicationServicesRules() =>
            ApplicationServices().Should()
                .NotDependOnAny(Infrastructure())
                .Check();
    }
}