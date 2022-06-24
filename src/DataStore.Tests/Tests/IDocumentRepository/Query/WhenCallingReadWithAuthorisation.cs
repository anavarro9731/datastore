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

        private ProjectTask task1, task2; 

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
            var permission2Instance = new DatabasePermission(
                SecurableOperations.READ,
                new List<AggregateReference>
                {
                    new AggregateReference(pi2s1.id, nullScopeReferenceTypes ? null :pi2s1.GetType().FullName , pi2s1.Name)
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
        public async void ItShouldFailWhenRequestingAProjectTaskRequiringTheScopeHierachyAndProjectsHaveNotBeenAddedToTheHierarchy()
        {
            Setup(doNotAddProjectsToScopeHierarchy:true);

            //* the key point about testing Project vs Cars is that it tests a scope hierarchy which has sibling branches
            await Assert.ThrowsAsync<SecurityException>(
                async () =>
                    {
                    // When
                        await this.testHarness.DataStore.ReadById<ProjectTask>(this.task1.id, o => o.AuthoriseFor(this.user));
                    });
        }
        
        [Fact]
        public async void ItShouldSucceedWhenRequestingOnlyProjectTasksMatchingTheUsersScope()
        {
            Setup();

            // When
            var projectTaskFromDb = await this.testHarness.DataStore.ReadById<ProjectTask>(this.task1.id, 
                                    o => o.AuthoriseFor(this.user));

            Assert.NotNull(projectTaskFromDb);
        }

        [Fact]
        public async void ItShouldFailWhenAllProjectTasksAreRequested()
        {
            Setup();

            
            await Assert.ThrowsAsync<SecurityException>(
                async () =>
                    {
                    // When
                    var results = await this.testHarness.DataStore.Read<ProjectTask>(setOptions: o => o.AuthoriseFor(this.user));
                    
                    });
        }
        
        [Fact]
        public async void ItShouldFailWhenProjectTasksAreRequestedWithTheRightPermissionScopesButWithTheWrongPermissionName()
        {
            Setup();
            //* remove READ
            this.user.DatabasePermissions.RemoveAll(p => p.PermissionName == SecurableOperations.READ.PermissionName);

            //* the key point about testing Project vs Cars is that it tests a scope hierarchy which has sibling branches
            await Assert.ThrowsAsync<SecurityException>(
                async () =>
                    {
                    // When
                    var projectTaskFromDb = await this.testHarness.DataStore.ReadById<Project>(this.task1.id,
                                            o => o.AuthoriseFor(this.user)
                                            );
                    
                    });
        }

        [Fact]
        public async void ItShouldSucceedWhenRequestingOnlyVolvosMatchingTheUsersPermissionsAreDeleted()
        {
            Setup();

            // When
            var carFromDatabase = await this.testHarness.DataStore.DeleteById<Car>(this.volvo2Id, o => o.AuthoriseFor(this.user));

            Assert.NotNull(carFromDatabase);
        }
        

        private void Setup(bool nullScopeReferenceTypes = false, bool doNotAddProjectsToScopeHierarchy = false)
        {
            // Given
            var scopeHierarchy = ScopeHierarchy.Create().WithScopeLevel<Company>(x => Guid.Empty).WithScopeLevel<CompanyDivision>(x => x.CompanyId)
                                               .WithScopeLevel<CompanyOffice>(x => x.CompanyDivisionId);
            if (!doNotAddProjectsToScopeHierarchy)
            {
                scopeHierarchy.WithScopeLevel<Project>(x => x.CompanyDivisionId);
            }

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

            var project1 = new Project() 
            {
                id = Guid.NewGuid(), Name = "Project 1", CompanyDivisionId = companyADivision1.id
            };
            this.testHarness.AddItemDirectlyToUnderlyingDb(project1);

            
            var project2 = new Project()
            {
                id = Guid.NewGuid(), Name = "Project 2", CompanyDivisionId = companyADivision2.id
            };
            
            this.testHarness.AddItemDirectlyToUnderlyingDb(project2);

            var userId = Guid.NewGuid();
            this.user = new User(userId, "dr.who");

            var pi1s1 = companyADivision1;
            var pi1s2 = this.companyBDivision2Office1;
            var pi1s3 = this.companyBDivision1Office2;

            var permission1Instance = new DatabasePermission(
                SecurableOperations.READ,
                new List<AggregateReference>
                {
                    new AggregateReference(pi1s1.id, nullScopeReferenceTypes ? null : pi1s1.GetType().FullName, pi1s1.Name),
                    new AggregateReference(pi1s2.id, nullScopeReferenceTypes ? null : pi1s2.GetType().FullName, pi1s2.Name),
                    new AggregateReference(pi1s3.id, nullScopeReferenceTypes ? null : pi1s3.GetType().FullName, pi1s3.Name)
                });

            var pi2s1 = this.companyBDivision2Office1;
            var permission2Instance = new DatabasePermission(
                SecurableOperations.DELETE,
                new List<AggregateReference>
                {
                    new AggregateReference(pi2s1.id, nullScopeReferenceTypes ? null :  pi2s1.GetType().FullName, pi2s1.Name),
                });

            this.user.DatabasePermissions.Add(permission1Instance);
            this.user.DatabasePermissions.Add(permission2Instance);

            this.volvo1Id = Guid.NewGuid(); //* should MATCH
            var volvo1 = new Car
            {
                id = this.volvo1Id, Make = "Volvo", FriendlyId = "Volvo 1", OfficeId = companyADivision1Office2.id  
            };

            this.volvo2Id = Guid.NewGuid(); //* should MATCH
            var volvo2 = new Car
            {
                id = this.volvo2Id,
                Active = false,
                Make = "Volvo",
                FriendlyId = "Volvo 2",
                OfficeId = this.companyBDivision2Office1.id
            };

            var volvo3Id = Guid.NewGuid(); //* should NOT MATCH
            var volvo3 = new Car
            {
                id = volvo3Id, Make = "Volvo", FriendlyId = "Volvo 3"
            };

            var volvo4Id = Guid.NewGuid(); //* should NOT MATCH
            var volvo4 = new Car
            {
                id = volvo4Id, Make = "Volvo", FriendlyId = "Volvo 4", OfficeId = companyADivision2Office1.id
            };

            this.task1 = new ProjectTask
            {
                id = Guid.NewGuid(), Name = "Task 1", ProjectId = project1.id  //* should MATCH 
            };
            this.task2 = new ProjectTask
            {
                id = Guid.NewGuid(), Name = "Task 2", ProjectId = project2.id //* should NOT MATCH
            };
            
            this.testHarness.AddItemDirectlyToUnderlyingDb(task1);
            this.testHarness.AddItemDirectlyToUnderlyingDb(task2);
            
            this.testHarness.AddItemDirectlyToUnderlyingDb(volvo1);
            this.testHarness.AddItemDirectlyToUnderlyingDb(volvo2);
            this.testHarness.AddItemDirectlyToUnderlyingDb(volvo3);
            this.testHarness.AddItemDirectlyToUnderlyingDb(volvo4);
            

        }
    }
}
