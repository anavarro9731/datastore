namespace DataStore.Messages
{
    public class CRUDCommand<TCommand> : Command
    {
        public CRUDCommand(TCommand model)
        {
            this.Model = model;
        }

        public TCommand Model { get; set; }
    }
}