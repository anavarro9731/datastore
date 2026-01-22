


## 16.42

Add the ability to serialise derived types when Typenamehandling is set to Auto.
Handle changes to Typenames on existing data successfully through the use of SerialisedNames attribute.

###  v16.38.0-alpha

### Breaking Changes

#### Many aspects of the Security layer have changed.
If you enable security in DataStoreOptions. It is now on by default for ALL Aggregates, whether they have ScopeReferences or not.
You can bypass this either with the BypassSecurity attribute or by calling the options => options.ByPassSecurity() option when calling a Store method.
ScopeHierarchy is now optional when using security, and you no longer have to provide a mapping function to find the foreign keys of parent entities.
You can also have multiple parent entities. List<Guid> Guid Guid? and List<Guid?> are all supported with the ScopeObjectReference attribute.
Finally the DatabasePermissionInstance class on IIdentityWithDatbasePermissions has been shortened to DatabasePermission and the former DatabasePermission
class has been replaced with SecurableOperations which are now just string.

### Features
- Projections support.
  The underlying Azure provider has has this for a long time, but you can now pass a mapping function similar to .Select(orig => mapped) to the Read functions on DataStore.WithoutEventReplay to get a different structure back.
- The CosmosClientOptions are now exposed when creating the CosmosSettings object
- 
#### Partition Key Support
There are now 5 partition key options with 2 overall modes (hierarchical [uses the new Cosmos Preview feature] and synthetic)
* Type_Id
* Type_Tenant_Id
* Type_TimePeriod_Id
* Type_Tenant_TimePeriod
* Shared (legacy single partition)

You can use a mix and match of these by placing them attributes on the aggregate classes.
When you goto query an aggregate and you are using any of the keys with Tenant or TimePeriod properties
you will need to use query options to provide them options => options.ProvidePartitionKeyValues()
or if reading by Id, there is an overload that takes an Aggregate's "long id" which when used
will set these options for you automatically. The longId is a base64 encoded string that contains
all partition key information. You can retrieve from any aggregate
(except those using Shared key where it is unnecessary) through the Aggregate.GetLongId() method.

## v15

### Breaking Changes
- Bugfix: The MillisecondsFromEpochTime on Aggregates was mistakenly saving seconds in many cases. This has been fixed but will mean existing data will be incorrect
- DatabasePermission Ids have been removed. The name is used as the Id.
### Bug Fixes
- Fix bug in deletebyid/deletewhere where items created in this session were not considered
### Features
- Allowing for container and database name in CosmosDb to be different
- Added .Exists() extension to IDocumentRepository
- Fixed a missing await in HardDelete function that could have meant items were not deleted
### Other
- Reverted to Newtonsoft Json.Net


