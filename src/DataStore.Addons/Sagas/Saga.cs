namespace DataStore.Infrastructure.Objects
{
    using System;
    using System.Threading.Tasks;
    using DataAccess.Interfaces;
    using DataAccess.Interfaces.Addons;
    using DataAccess.Models;
    using Messages;

    public class SagaState : Aggregate
    {
        public FlaggedState Flags { get; set; }
    }

    public class Saga
    {
        protected readonly IEventAggregator EventAggregator;

        protected readonly IDataStore DataStore;

        private readonly SagaState sagaState;

        public Saga(IEventAggregator eventAggregator, IDataStore dataStore, Guid? sagaId = null)
        {
            this.EventAggregator = eventAggregator;
            this.DataStore = dataStore;

            if (sagaId != null)
            {
                if (this.DataStore.Exists(sagaId.Value).Result)
                {
                    this.sagaState = this.DataStore.ReadActiveById<SagaState>(sagaId.Value).Result;
                }
                else
                {
                    this.sagaState = this.DataStore.Create(new SagaState()).Result;
                }
            }
        }

        protected Guid SagaId => this.sagaState.id;

        protected async Task AddStatus(Enum additionalStatus)
        {
            if (this.sagaState == null) throw new Exception("This saga is not stateful.");
            if (this.sagaState.Flags != null)
            {
                this.sagaState.Flags.AddState(additionalStatus);
            }
            else
            {
                this.sagaState.Flags = new FlaggedState(additionalStatus);
            }

            await this.DataStore.UpdateUsingValuesFromAnotherInstanceWithTheSameId(this.sagaState);
        }

        protected void CompleteSaga()
        {
            this.EventAggregator.Store(new SagaCompleted());
        }

        protected T GetStatus<T>()
        {
            if (this.sagaState == null) throw new Exception("This saga is not stateful.");
            return this.sagaState.Flags.AsEnum<T>();
        }

        protected async Task RemoveStatus(Enum statusToRemove)
        {
            if (this.sagaState == null) throw new Exception("This saga is not stateful.");
            if (this.sagaState.Flags != null)
            {
                this.sagaState.Flags.RemoveState(statusToRemove);
            }
            else
            {
                throw new Exception("This saga's state has not been set.");
            }

            await this.DataStore.UpdateUsingValuesFromAnotherInstanceWithTheSameId(this.sagaState);
        }

        protected async Task UpdateStatus(Enum newStatus)
        {
            if (this.sagaState == null) throw new Exception("This saga is not stateful.");
            if (this.sagaState.Flags != null)
            {
                this.sagaState.Flags.ReplaceState(newStatus);
            }
            else
            {
                this.sagaState.Flags = new FlaggedState(newStatus);
            }

            await this.DataStore.UpdateUsingValuesFromAnotherInstanceWithTheSameId(this.sagaState);
        }
    }

    public class Saga<TReturnType> : Saga
        where TReturnType : class
    {
        private TReturnType returnValue;

        public Saga(IEventAggregator eventAggregator, IDataStore dataStore, Guid? sagaId = null)
            : base(eventAggregator, dataStore, sagaId)
        {
        }

        public TReturnType ReturnValue
        {
            get
            {
                if (this.returnValue == null)
                {
                    throw new Exception("Saga not completed.");
                }

                return this.returnValue;
            }

            private set
            {
                this.returnValue = value;
            }
        }

        protected void CompleteSaga<TCompleted>(TCompleted completedEvent) where TCompleted : SagaCompleted<TReturnType>
        {
            this.EventAggregator.Store(completedEvent);

            this.ReturnValue = completedEvent.Model;
        }
    }
}