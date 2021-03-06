namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.LowLevel.Permissions;
    using global::DataStore.Models.Messages;
    using global::DataStore.Options;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadWithAuthorisation
    {
        private IEnumerable<Car> carsFromDatabase;

        private CompanyOffice companyBDivision1Office2;

        private CompanyOffice companyBDivision2Office1;

        private ITestHarness testHarness;

        private IIdentityWithDatabasePermissions user;

        private Guid volvo1Id, volvo2Id;

        [Fact]
        public async void ItShouldFailWhenAllVolvosAreDeleted()
        {
            Setup();

            await Assert.ThrowsAsync<SecurityException>(
                async () =>
                    {
                    // When
                    await this.testHarness.DataStore.DeleteWhere<Car>(car => car.Make == "Volvo", o => o.AuthoriseFor(this.user));
                    });
        }

        [Fact]
        public async void ItShouldFailWhenAllVolvosAreRequested()
        {
            Setup();

            await Assert.ThrowsAsync<SecurityException>(
                async () =>
                    {
                    // When
                    await this.testHarness.DataStore.Read<Car>(car => car.Make == "Volvo", o => o.AuthoriseFor(this.user));
                    });
        }

        [Fact]
        public async void ItShouldFailWhenUserHasOverlappingPermissions()
        {
            var nullScopeReferenceTypes = false;
            Setup(nullScopeReferenceTypes);
            
            var pi2s1 = this.companyBDivision1Office2;
            var permission2Instance = new DatabasePermissionInstance(
                DatabasePermissions.READ,
                new List<DatabaseScopeReference>
                {
                    new DatabaseScopeReference(pi2s1.id, nullScopeReferenceTypes ? null :pi2s1.GetType().FullName , pi2s1.Name)
                });

            this.user.DatabasePermissions.Add(permission2Instance);

            await Assert.ThrowsAsync<SecurityException>(
                async () =>
                    {
                    // When
                    await this.testHarness.DataStore.Read<Car>(car => car.Make == "Volvo", o => o.AuthoriseFor(this.user));
                    });
        }

        [Fact]
        public async void ItShouldSucceedWhenRequestingOnlyVolvosMatchingTheUsersCarFromDatabasePermissionsAreRequested()
        {
            Setup();

            // When
            this.carsFromDatabase = await this.testHarness.DataStore.Read<Car>(
                                        car => car.id == this.volvo1Id || car.id == this.volvo2Id,
                                        o => o.AuthoriseFor(this.user));

            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(2, this.carsFromDatabase.Count());
            Assert.Equal(1, this.carsFromDatabase.Count(x => x.id == this.volvo1Id));
            Assert.Equal(1, this.carsFromDatabase.Count(x => x.id == this.volvo2Id));
        }
        
        [Fact]
        public async void ItShouldSucceedWhenRequestingOnlyVolvosMatchingTheUsersCarFromDatabasePermissionsAreRequestedAndPermissionsAreCreatedWithoutScopeReferenceTypeNamesDefined()
        {
            Setup(true);

            // When
            this.carsFromDatabase = await this.testHarness.DataStore.Read<Car>(
                                        car => car.id == this.volvo1Id || car.id == this.volvo2Id,
                                        o => o.AuthoriseFor(this.user));

            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(2, this.carsFromDatabase.Count());
            Assert.Equal(1, this.carsFromDatabase.Count(x => x.id == this.volvo1Id));
            Assert.Equal(1, this.carsFromDatabase.Count(x => x.id == this.volvo2Id));
        }

        [Fact]
        public async void ItShouldSucceedWhenRequestingOnlyVolvosMatchingTheUsersPermissionsAreDeleted()
        {
            Setup();

            // When
            var carFromDatabase = await this.testHarness.DataStore.DeleteById<Car>(this.volvo2Id, o => o.AuthoriseFor(this.user));

            Assert.NotNull(carFromDatabase);
        }

        private void Setup(bool nullScopeReferenceTypes = false)
        {
            // Given
            var scopeHierarchy = ScopeHierarchy.Create().WithScopeLevel<Company>(x => Guid.Empty)
                                               .WithScopeLevel<CompanyDivision>(x => x.CompanyId)
                                               .WithScopeLevel<CompanyOffice>(x => x.CompanyDivisionId);

            var dataStoreOptions = DataStoreOptions.Create().WithSecurity(scopeHierarchy);

            this.testHarness = TestHarness.Create(nameof(WhenCallingReadWithAuthorisation), dataStoreOptions);

            var companyA = new Company("CompanyA", Guid.NewGuid());
            this.testHarness.AddItemDirectlyToUnderlyingDb(companyA);

            var companyADivision1 = new CompanyDivision("CompanyA_D1", Guid.NewGuid(), companyA.id);
            this.testHarness.AddItemDirectlyToUnderlyingDb(companyADivision1);

            var companyADivision1Office1 = new CompanyOffice("CompanyA_D1_O1", Guid.NewGuid(), companyADivision1.id);
            this.testHarness.AddItemDirectlyToUnderlyingDb(companyADivision1Office1);

            var companyADivision1Office2 = new CompanyOffice("CompanyA_D1_O2", Guid.NewGuid(), companyADivision1.id);
            this.testHarness.AddItemDirectlyToUnderlyingDb(companyADivision1Office2);

            var companyADivision2 = new CompanyDivision("CompanyA_D2", Guid.NewGuid(), companyA.id);
            this.testHarness.AddItemDirectlyToUnderlyingDb(companyADivision2);

            var companyADivision2Office1 = new CompanyOffice("CompanyA_D2_O1", Guid.NewGuid(), companyADivision2.id);
            this.testHarness.AddItemDirectlyToUnderlyingDb(companyADivision2Office1);

            var companyADivision2Office2 = new CompanyOffice("CompanyA_D2_O2", Guid.NewGuid(), companyADivision2.id);
            this.testHarness.AddItemDirectlyToUnderlyingDb(companyADivision2Office2);

            var companyB = new Company("companyB", Guid.NewGuid());
            this.testHarness.AddItemDirectlyToUnderlyingDb(companyB);

            var companyBDivision1 = new CompanyDivision("companyB_D1", Guid.NewGuid(), companyB.id);
            this.testHarness.AddItemDirectlyToUnderlyingDb(companyBDivision1);

            var companyBDivision1Office1 = new CompanyOffice("companyB_D1_O1", Guid.NewGuid(), companyBDivision1.id);
            this.testHarness.AddItemDirectlyToUnderlyingDb(companyBDivision1Office1);

            this.companyBDivision1Office2 = new CompanyOffice("companyB_D1_O2", Guid.NewGuid(), companyBDivision1.id);
            this.testHarness.AddItemDirectlyToUnderlyingDb(this.companyBDivision1Office2);

            var companyBDivision2 = new CompanyDivision("companyB_D2", Guid.NewGuid(), companyB.id);
            this.testHarness.AddItemDirectlyToUnderlyingDb(companyBDivision2);

            this.companyBDivision2Office1 = new CompanyOffice("companyB_D2_O1", Guid.NewGuid(), companyBDivision2.id);
            this.testHarness.AddItemDirectlyToUnderlyingDb(this.companyBDivision2Office1);

            var companyBDivision2Office2 = new CompanyOffice("companyB_D2_O2", Guid.NewGuid(), companyBDivision2.id);
            this.testHarness.AddItemDirectlyToUnderlyingDb(companyBDivision2Office2);

            var userId = Guid.NewGuid();
            this.user = new User(userId, "dr.who");

            var pi1s1 = companyADivision1;
            var pi1s2 = this.companyBDivision2Office1;
            var pi1s3 = this.companyBDivision1Office2;

            var permission1Instance = new DatabasePermissionInstance(
                DatabasePermissions.READ,
                new List<DatabaseScopeReference>
                {
                    new DatabaseScopeReference(pi1s1.id, nullScopeReferenceTypes ? null : pi1s1.GetType().FullName, pi1s1.Name),
                    new DatabaseScopeReference(pi1s2.id, nullScopeReferenceTypes ? null : pi1s2.GetType().FullName, pi1s2.Name),
                    new DatabaseScopeReference(pi1s3.id, nullScopeReferenceTypes ? null : pi1s3.GetType().FullName, pi1s3.Name)
                });

            var pi2s1 = this.companyBDivision2Office1;
            var permission2Instance = new DatabasePermissionInstance(
                DatabasePermissions.DELETE,
                new List<DatabaseScopeReference>
                {
                    new DatabaseScopeReference(pi2s1.id, nullScopeReferenceTypes ? null :  pi2s1.GetType().FullName, pi2s1.Name)
                });

            this.user.DatabasePermissions.Add(permission1Instance);
            this.user.DatabasePermissions.Add(permission2Instance);

            this.volvo1Id = Guid.NewGuid();
            var volvo1 = new Car
            {
                id = this.volvo1Id, Make = "Volvo", FriendlyId = "Volvo 1", OfficeId = companyADivision1Office2.id
            };

            this.volvo2Id = Guid.NewGuid();
            var volvo2 = new Car
            {
                id = this.volvo2Id,
                Active = false,
                Make = "Volvo",
                FriendlyId = "Volvo 2",
                OfficeId = this.companyBDivision2Office1.id
            };

            var volvo3Id = Guid.NewGuid();
            var volvo3 = new Car
            {
                id = volvo3Id, Make = "Volvo", FriendlyId = "Volvo 3"
            };

            var volvo4Id = Guid.NewGuid();
            var volvo4 = new Car
            {
                id = volvo4Id, Make = "Volvo", FriendlyId = "Volvo 4", OfficeId = companyADivision2Office1.id
            };
            this.testHarness.AddItemDirectlyToUnderlyingDb(volvo1);
            this.testHarness.AddItemDirectlyToUnderlyingDb(volvo2);
            this.testHarness.AddItemDirectlyToUnderlyingDb(volvo3);
            this.testHarness.AddItemDirectlyToUnderlyingDb(volvo4);
        }
    }
}
