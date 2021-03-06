﻿namespace DataStore.Interfaces.Options
{
    using DataStore.Interfaces.LowLevel.Permissions;

    public abstract class ReadOptionsClientSide
    {
        public static implicit operator ReadOptionsLibrarySide(ReadOptionsClientSide options) => options.LibrarySide;

        protected ReadOptionsClientSide(ReadOptionsLibrarySide librarySide)
        {
            LibrarySide = librarySide;
        }

        protected ReadOptionsLibrarySide LibrarySide { get; }

        //* visible members

        public abstract void AuthoriseFor(IIdentityWithDatabasePermissions identity);
    }

    public class ReadOptionsLibrarySide
    {
        public IIdentityWithDatabasePermissions Identity { get; set; }
    }
}