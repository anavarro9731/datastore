namespace DataStore
{
    using System;
    using CircuitBoard;

    /// <summary>
    /// $SUMMARY$
    /// </summary>
    public class ErrorCodes : ErrorCode
    {
        public static ErrorCode MissingDbPermissions =  Create(Guid.Parse("D2E124E4-CF91-43C5-ADE9-CADB109942E7"),  "User not authorized to perform this action.");
        public static ErrorCode DuplicateDbPermissions =  Create(Guid.Parse("AA70F3B1-8992-4B41-9657-F53740C9F4D7"),  "User has the same DatabasePermission twice instead the scopes should be merged");
        public static ErrorCode IdentityMissingWhenApplyingAuthorisation =  Create(Guid.Parse("D3C9DDA8-BE83-4D76-AE95-9058D011C9B7"),  "Data authorisation enabled but no identity has been provided. "
            + "Please set the .AuthoriseFor(identity) option when calling your DataStore operation or set the SecureFor(identity) option on the DataStoreOptions object");
    }
}