namespace DataStore.Models
{
    using FluentValidation;
    using Interfaces.LowLevel;

    public class EntityValidator<T> : AbstractValidator<T>
        where T : Entity
    {
        public EntityValidator()
        {
            RuleFor(x => x.id).NotEmpty();
        }
    }
}