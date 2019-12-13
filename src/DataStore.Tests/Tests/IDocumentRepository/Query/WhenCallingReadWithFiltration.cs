namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Threading.Tasks;
    using CircuitBoard.Permissions;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadWithFiltration
    {
        private Guid volvo1Id, volvo2Id;

        private IEnumerable<Car> carsFromDatabase;

        private ITestHarness testHarness;

        private IIdentityWithPermissions user;

        private Permission permission1 = new Permission(Guid.NewGuid(), "Perm 1");
        private Permission permission2 = new Permission(Guid.NewGuid(), "Perm 2");

        [Fact]
        public async void ItShouldReturnOnlyVolvosWhichTheUserHasPermissionTo()
        {
            await Setup();

            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(2, this.carsFromDatabase.Count());
            Assert.Equal(1, this.carsFromDatabase.Count(x => x.id == this.volvo1Id));
            Assert.Equal(1, this.carsFromDatabase.Count(x => x.id == this.volvo2Id));
        }

        private async Task Setup()
        {
            // Given
            var scopeHierarchy = ScopeHierarchy.Create().WithScopeLevel<Company>(x => Guid.Empty).WithScopeLevel<CompanyDivision>(x => x.CompanyId)
                                               .WithScopeLevel<CompanyOffice>(x => x.CompanyDivisionId);

            var dataStoreOptions = DataStoreOptions.Create().WithSecurity(scopeHierarchy);

            this.testHarness = TestHarness.Create(nameof(WhenCallingReadWithFiltration), dataStoreOptions);

            var companyA = new Company("CompanyA", Guid.NewGuid());
            this.testHarness.AddToDatabase(companyA);

            var companyADivision1 = new CompanyDivision("CompanyA_D1", Guid.NewGuid(), companyA.id);
            this.testHarness.AddToDatabase(companyADivision1);

            var companyADivision1Office1 = new CompanyOffice("CompanyA_D1_O1", Guid.NewGuid(), companyADivision1.id);
            this.testHarness.AddToDatabase(companyADivision1Office1);

            var companyADivision1Office2 = new CompanyOffice("CompanyA_D1_O2", Guid.NewGuid(), companyADivision1.id);
            this.testHarness.AddToDatabase(companyADivision1Office2);

            var companyADivision2 = new CompanyDivision("CompanyA_D2", Guid.NewGuid(), companyA.id);
            this.testHarness.AddToDatabase(companyADivision2);

            var companyADivision2Office1 = new CompanyOffice("CompanyA_D2_O1", Guid.NewGuid(), companyADivision2.id);
            this.testHarness.AddToDatabase(companyADivision2Office1);

            var companyADivision2Office2 = new CompanyOffice("CompanyA_D2_O2", Guid.NewGuid(), companyADivision2.id);
            this.testHarness.AddToDatabase(companyADivision2Office2);

            var companyB = new Company("companyB", Guid.NewGuid());
            this.testHarness.AddToDatabase(companyB);

            var companyBDivision1 = new CompanyDivision("companyB_D1", Guid.NewGuid(), companyB.id);
            this.testHarness.AddToDatabase(companyBDivision1);

            var companyBDivision1Office1 = new CompanyOffice("companyB_D1_O1", Guid.NewGuid(), companyBDivision1.id);
            this.testHarness.AddToDatabase(companyBDivision1Office1);

            var companyBDivision1Office2 = new CompanyOffice("companyB_D1_O2", Guid.NewGuid(), companyBDivision1.id);
            this.testHarness.AddToDatabase(companyBDivision1Office2);

            var companyBDivision2 = new CompanyDivision("companyB_D2", Guid.NewGuid(), companyB.id);
            this.testHarness.AddToDatabase(companyBDivision2);

            var companyBDivision2Office1 = new CompanyOffice("companyB_D2_O1", Guid.NewGuid(), companyBDivision2.id);
            this.testHarness.AddToDatabase(companyBDivision2Office1);

            var companyBDivision2Office2 = new CompanyOffice("companyB_D2_O2", Guid.NewGuid(), companyBDivision2.id);
            this.testHarness.AddToDatabase(companyBDivision2Office2);

            var userId = Guid.NewGuid();
            this.user = new User(userId, "dr.who");

            var pi1s1 = companyADivision1;
            var pi1s2 = companyBDivision2Office1;
            var permission1Instance = new PermissionInstance(
                this.permission1,
                new List<ScopeReference>
                {
                    new ScopeReference(pi1s1.id, pi1s1.GetType().FullName, scopeObjectDebugId: pi1s1.Name),
                    new ScopeReference(pi1s2.id, pi1s2.GetType().FullName, scopeObjectDebugId:pi1s2.Name)
                });

            var pi2s1 = companyBDivision1Office2;
            var permission2Instance = new PermissionInstance(
                this.permission2,
                new List<ScopeReference>
                {
                    new ScopeReference(pi2s1.id, pi2s1.GetType().FullName, scopeObjectDebugId:pi2s1.Name)
                });

            user.AddPermission(permission1Instance);
            user.AddPermission(permission2Instance);

            this.volvo1Id = Guid.NewGuid();
            var volvo1 = new Car
            {
                id = this.volvo1Id,
                Make = "Volvo",
                FriendlyId = "Volvo 1",
                OfficeId = companyADivision1Office2.id
            };

            this.volvo2Id = Guid.NewGuid();
            var volvo2 = new Car
            {
                id = this.volvo2Id,
                Active = false,
                Make = "Volvo",
                FriendlyId = "Volvo 2",
                OfficeId = companyBDivision2Office1.id
            };

            var volvo3Id = Guid.NewGuid();
            var volvo3 = new Car
            {
                id = volvo3Id,
                Make = "Volvo",
                FriendlyId = "Volvo 3"
            };

            var volvo4Id = Guid.NewGuid();
            var volvo4 = new Car
            {
                id = volvo4Id,
                Make = "Volvo",
                FriendlyId = "Volvo 4",
                OfficeId = companyADivision2Office1.id
            };
            this.testHarness.AddToDatabase(volvo1);
            this.testHarness.AddToDatabase(volvo2);
            this.testHarness.AddToDatabase(volvo3);
            this.testHarness.AddToDatabase(volvo4);


            // When
            this.carsFromDatabase = await this.testHarness.DataStore.FilterByPermission(this.permission1, user).Read<Car>(car => car.Make == "Volvo");

        }
    }
}