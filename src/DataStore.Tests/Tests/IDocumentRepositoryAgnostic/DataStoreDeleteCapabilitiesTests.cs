namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic
{
    using System;
    using System.Linq;
    using Constants;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    [Collection(TestCollections.RunSerially)]
    public class DataStoreDeleteCapabilitiesTests
    {
        public class WhenCallingDeleteHardWhereWithoutCommitting
        {
            [Fact]
            public async void WhenCallingDeleteHardWhereWithoutCommitting_ItShouldOnlyMakeChangesInSession()
            {
                // Given
                var testHarness =
                    TestHarnessFunctions.GetTestHarness(
                        nameof(WhenCallingDeleteHardWhereWithoutCommitting_ItShouldOnlyMakeChangesInSession));

                var carId = Guid.NewGuid();
                await testHarness.AddToDatabase(new Car
                {
                    id = carId,
                    Make = "Volvo"
                });

                //When
                await testHarness.DataStore.DeleteHardWhere<Car>(car => car.id == carId);

                //Then
                Assert.Null(testHarness.Operations.SingleOrDefault(e => e is HardDeleteOperation<Car>));
                Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
                Assert.NotEmpty(testHarness.QueryDatabase<Car>().Result);
                Assert.Empty(testHarness.DataStore.Read<Car>(car => car).Result);
            }
        }



        [Fact]
        public async void WhenCallingDeleteSoftByIdWithoutCommitting_ItShouldOnlyMakeChangesInSession()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenCallingDeleteSoftByIdWithoutCommitting_ItShouldOnlyMakeChangesInSession));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.DeleteSoftById<Car>(carId);

            //Then
            Assert.Null(testHarness.Operations.SingleOrDefault(e => e is SoftDeleteOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedSoftDeleteOperation<Car>));
            Assert.NotEmpty(testHarness.QueryDatabase<Car>().Result);
            Assert.Empty(testHarness.DataStore.ReadActive<Car>(car => car).Result);
            Assert.NotEmpty(testHarness.DataStore.Read<Car>(car => car).Result);
        }

        [Fact]
        public async void WhenCallingDeleteSoftWhereWithoutCommitting_ItShouldOnlyMakeTheChangesInSession()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenCallingDeleteSoftWhereWithoutCommitting_ItShouldOnlyMakeTheChangesInSession));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.DeleteSoftWhere<Car>(car => car.id == carId);

            //Then
            Assert.Null(testHarness.Operations.SingleOrDefault(e => e is SoftDeleteOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedSoftDeleteOperation<Car>));
            Assert.NotEmpty(testHarness.QueryDatabase<Car>().Result);
            Assert.Empty(testHarness.DataStore.ReadActive<Car>(car => car).Result);
            Assert.NotEmpty(testHarness.DataStore.Read<Car>(car => car).Result);
        }

        [Fact]
        public async void WhenChangingTheItemsReturnFromDeleteHardById_ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenChangingTheItemsReturnFromDeleteHardById_ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned
                    ));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            var result = await testHarness.DataStore.DeleteHardById<Car>(carId);

            //When
            result.id = Guid.NewGuid(); //change in memory before commit
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is HardDeleteOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
            Assert.Empty(testHarness.QueryDatabase<Car>().Result);
            Assert.Empty(testHarness.DataStore.Read<Car>(car => car).Result);
        }

        [Fact]
        public async void WhenChangingTheItemsReturnFromDeleteHardWhere_ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenChangingTheItemsReturnFromDeleteHardWhere_ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned
                    ));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            var result = await testHarness.DataStore.DeleteHardWhere<Car>(car => car.id == carId);

            //When
            result.Single().id = Guid.NewGuid(); //change in memory before commit
            await testHarness.DataStore.CommitChanges();

            //Then

            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is HardDeleteOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
            Assert.Empty(testHarness.QueryDatabase<Car>().Result);
            Assert.Empty(testHarness.DataStore.Read<Car>(car => car).Result);
        }

        [Fact]
        public async void WhenChangingTheItemsReturnFromDeleteSoftById_ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenChangingTheItemsReturnFromDeleteSoftById_ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned
                    ));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });


            var result = await testHarness.DataStore.DeleteSoftById<Car>(carId);
            //When
            result.id = Guid.NewGuid(); //change in memory before commit
            await testHarness.DataStore.CommitChanges();

            //Then

            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is SoftDeleteOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedSoftDeleteOperation<Car>));
            Assert.False(testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Active);
            Assert.Empty(testHarness.DataStore.ReadActive<Car>(car => car).Result);
            Assert.NotEmpty(testHarness.DataStore.Read<Car>(car => car).Result);
        }

        [Fact]
        public async void
            WhenChangingTheItemsReturnFromDeleteSoftDeleteWhere_ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenCallingCommitAfterDeleteSoftWhere_ItShouldPersistTheChangesToTheDatabase));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });


            var result = await testHarness.DataStore.DeleteSoftWhere<Car>(car => car.id == carId);

            //When
            result.Single().id = Guid.NewGuid(); //change in memory before commit
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is SoftDeleteOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedSoftDeleteOperation<Car>));
            Assert.False(testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Active);
            Assert.Empty(testHarness.DataStore.ReadActive<Car>(car => car).Result);
            Assert.NotEmpty(testHarness.DataStore.Read<Car>(car => car).Result);
        }
    }
}