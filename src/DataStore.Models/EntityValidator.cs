namespace DataStore.Models
{
    using FluentValidation;

    public class EntityValidator<T> : AbstractValidator<T>
        where T : Entity
    {
        public EntityValidator()
        {
            this.RuleFor(x => x.Id).NotEmpty();
        }
    }
}