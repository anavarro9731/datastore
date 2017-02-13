namespace PalmTree.Infrastructure.Models.Messages
{
    public class CrudCommand<TCommand> : Command
    {
        public CrudCommand(TCommand model)
        {
            this.Model = model;
        }

        public TCommand Model { get; set; }
    }
}