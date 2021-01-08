## v15

### Breaking Changes
- The MillisecondsFromEpochTime on Aggregates was mistakenly saving seconds in many cases. This has been fixed but will mean existing data will be incorrect

### Bug Fixes
- Fix bug in deletebyid/deletewhere where items created in this session were not considered

### Features
- Allowing for container and database name in CosmosDb to be different
- Added .Exists() extension to IDocumentRepository
- Fixed a missing await in HardDelete function that could have meant items were not deleted 

### Other
- Reverted to Newtonsoft Json.Net
