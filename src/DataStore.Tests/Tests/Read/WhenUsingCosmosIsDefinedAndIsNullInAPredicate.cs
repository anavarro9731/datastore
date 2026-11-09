namespace DataStore.Tests.Tests.Read
{
    #region

    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PartitionKeys;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Microsoft.Azure.Cosmos.Linq;
    using Xunit;

    #endregion

    public class WhenUsingCosmosIsDefinedAndIsNullInAPredicate
    {
        private ITestHarness testHarness;

        [Fact]
        public async Task IsDefinedOnAStringReturnsOnlyItemsWithAValue()
        {
            await Setup();
            var result = await this.testHarness.DataStore.WithoutEventReplay.Read<Car>(c => c.Make.IsDefined());
            Assert.Equal(new[] { "Volvo" }, result.Select(c => c.Make).ToArray());
        }

        [Fact]
        public async Task IsNullOnAStringReturnsOnlyItemsWithoutAValue()
        {
            await Setup();
            var result = await this.testHarness.DataStore.WithoutEventReplay.Read<Car>(c => c.Make.IsNull());
            Assert.Single(result);
            Assert.Null(result.Single().Make);
        }

        [Fact]
        public async Task IsDefinedOnANullableValueType()
        {
            await Setup();
            var result = await this.testHarness.DataStore.WithoutEventReplay.Read<Car>(c => c.OfficeId.IsDefined());
            Assert.Single(result);
        }

        // Known caveat: in memory a non-nullable value type can never be "missing" (there is no JSON document, only a CLR object),
        // so it is always treated as filled, even when it holds its default value (0, false, Guid.Empty, DateTime.MinValue, enum zero).
        // Cosmos would treat documents saved before the property existed as not defined. Use a nullable property to simulate that.
        [Fact]
        public async Task IsDefinedOnNonNullableValueTypesIsAlwaysTrueEvenForDefaultValues()
        {
            await SetupGadgets();
            var store = this.testHarness.DataStore.WithoutEventReplay;

            Assert.Equal(2, (await store.Read<Gadget>(g => g.Count.IsDefined())).Count());
            Assert.Equal(2, (await store.Read<Gadget>(g => g.Flag.IsDefined())).Count());
            Assert.Equal(2, (await store.Read<Gadget>(g => g.Reference.IsDefined())).Count());
            Assert.Equal(2, (await store.Read<Gadget>(g => g.When.IsDefined())).Count());
            Assert.Equal(2, (await store.Read<Gadget>(g => g.Kind.IsDefined())).Count());
        }

        [Fact]
        public async Task IsNullOnNonNullableValueTypesIsAlwaysFalse()
        {
            await SetupGadgets();
            Assert.Empty(await this.testHarness.DataStore.WithoutEventReplay.Read<Gadget>(g => g.Count.IsNull()));
            Assert.Empty(await this.testHarness.DataStore.WithoutEventReplay.Read<Gadget>(g => g.Flag.IsNull()));
            Assert.Empty(await this.testHarness.DataStore.WithoutEventReplay.Read<Gadget>(g => g.Reference.IsNull()));
            Assert.Empty(await this.testHarness.DataStore.WithoutEventReplay.Read<Gadget>(g => g.When.IsNull()));
            Assert.Empty(await this.testHarness.DataStore.WithoutEventReplay.Read<Gadget>(g => g.Kind.IsNull()));
        }

        [Fact]
        public async Task IsDefinedCanBeCombinedWithOtherConditionsAndOrdering()
        {
            await Setup();
            var result = await this.testHarness.DataStore.WithoutEventReplay.Read<Car>(c => c.Active && (c.Make.IsDefined() || c.OfficeId.IsDefined()), o => o.OrderBy(c => c.id));
            Assert.Single(result);
        }

        [Fact]
        public async Task IsDefinedAndIsNullWorkInCount()
        {
            await Setup();
            Assert.Equal(1, await this.testHarness.DataStore.WithoutEventReplay.Count<Car>(c => c.Make.IsDefined()));
            Assert.Equal(1, await this.testHarness.DataStore.WithoutEventReplay.Count<Car>(c => c.Make.IsNull()));
        }

        [Fact]
        public async Task IsDefinedAndIsNullOnAPropertyOfANullParentDoNotThrow()
        {
            await Setup();
            this.testHarness.AddItemDirectlyToUnderlyingDb(new Garage { id = Guid.NewGuid(), Location = null });
            this.testHarness.AddItemDirectlyToUnderlyingDb(new Garage { id = Guid.NewGuid(), Location = new Garage.Address { City = "Oslo" } });
            this.testHarness.AddItemDirectlyToUnderlyingDb(new Garage { id = Guid.NewGuid(), Location = new Garage.Address { City = null } });

            var defined = await this.testHarness.DataStore.WithoutEventReplay.Read<Garage>(g => g.Location.City.IsDefined());
            var isNull = await this.testHarness.DataStore.WithoutEventReplay.Read<Garage>(g => g.Location.City.IsNull());

            Assert.Equal("Oslo", defined.Single().Location.City);
            Assert.Single(isNull); // a missing parent is not a null value
            Assert.NotNull(isNull.Single().Location);
        }

        public enum GadgetKind
        {
            None = 0,

            Fancy = 1
        }

        [PartitionKey__Type_Id]
        public class Gadget : Aggregate
        {
            public int Count { get; set; }

            public bool Flag { get; set; }

            public Guid Reference { get; set; }

            public DateTime When { get; set; }

            public GadgetKind Kind { get; set; }

            public string Name { get; set; }
        }

        [PartitionKey__Type_Id]
        public class Garage : Aggregate
        {
            public Address Location { get; set; }

            public class Address
            {
                public string City { get; set; }
            }
        }

        private async Task SetupGadgets()
        {
            this.testHarness = TestHarness.Create(nameof(WhenUsingCosmosIsDefinedAndIsNullInAPredicate));

            this.testHarness.AddItemDirectlyToUnderlyingDb(new Gadget { id = Guid.NewGuid(), Name = "default" });
            this.testHarness.AddItemDirectlyToUnderlyingDb(new Gadget
            {
                id = Guid.NewGuid(), Name = "non-default", Count = 5, Flag = true, Reference = Guid.NewGuid(), When = new DateTime(2024, 1, 1), Kind = GadgetKind.Fancy
            });
        }

        private async Task Setup()
        {
            this.testHarness = TestHarness.Create(nameof(WhenUsingCosmosIsDefinedAndIsNullInAPredicate));

            this.testHarness.AddItemDirectlyToUnderlyingDb(new Car { id = Guid.NewGuid(), Make = "Volvo", OfficeId = Guid.NewGuid() });
            this.testHarness.AddItemDirectlyToUnderlyingDb(new Car { id = Guid.NewGuid(), Make = null });
        }
    }
}
