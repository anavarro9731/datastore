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

##  v16

### Many aspects of the Security layer have changed.
If you enable security in DataStoreOptions. It is now on by default for ALL Aggregates, whether they have ScopeReferences or not.
You can bypass this either with the BypassSecurity attribute or by calling the options => options.ByPassSecurity() option when calling a Store method.
ScopeHierarchy is now optional when using security, and you no longer have to provide a mapping function to find the foreign keys of parent entities.
You can also have multiple parent entities. List<Guid> Guid Guid? and List<Guid?> are all supported with the ScopeObjectReference attribute.
Finally the DatabasePermissionInstance class on IIdentityWithDatbasePermissions has been shortened to DatabasePermission and the former DatabasePermission
class has been replaced with SecurableOperations which are now just string.

### Features
- Projections support. The underlying Azure provider has has this for a long time, but you can now pass a mapping function similar to .Select(orig => mapped)
- The CosmosClientOptions are now exposed when creating the CosmosSettings object 