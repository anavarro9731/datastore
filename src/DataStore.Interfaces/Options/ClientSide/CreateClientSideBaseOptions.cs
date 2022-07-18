namespace DataStore.Interfaces.Options.ClientSide
{
    using DataStore.Interfaces.Options.LibrarySide;

    public class CreateClientSideBaseOptions
    {
        protected CreateClientSideBaseOptions()
        {
            LibrarySide = new CreateOptionsLibrarySide();
        }

        protected CreateOptionsLibrarySide LibrarySide { get; }

        public static implicit operator CreateOptionsLibrarySide(CreateClientSideBaseOptions options)
        {
            return options.LibrarySide;
        }

    }
}