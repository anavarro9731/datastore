namespace DataStore.Interfaces.Options.ClientSide
{
    using DataStore.Interfaces.Options.LibrarySide;

    public class CreateOptionsClientSideBase
    {
        protected CreateOptionsClientSideBase()
        {
            LibrarySide = new CreateOptionsLibrarySide();
        }

        protected CreateOptionsLibrarySide LibrarySide { get; }

        public static implicit operator CreateOptionsLibrarySide(CreateOptionsClientSideBase options)
        {
            return options.LibrarySide;
        }

    }
}