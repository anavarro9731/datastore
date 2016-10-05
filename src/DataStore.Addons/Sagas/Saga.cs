namespace DataStore.Infrastructure.Impl.Sagas
{
    using System;
    using System.Threading.Tasks;
    using DataAccess.Interfaces;
    using DataAccess.Interfaces.Addons;
    using DataAccess.Models;

    public class SagaState : Aggregate
    {
        public FlaggedState Flags { get; set; }
    }

    public class Saga
    {
        private readonly SagaState _sagaState;

        protected readonly IDataStore DataStore;
        protected readonly IEventAggregator EventAggregator;

        public Saga(IEventAggregator eventAggregator, IDataStore dataStore, Guid? sagaId = null)
        {
            EventAggregator = eventAggregator;
            DataStore = dataStore;

            if (sagaId != null)
            {
                if (DataStore.Exists(sagaId.Value).Result)
                {
                    _sagaState = DataStore.ReadActiveById<SagaState>(sagaId.Value).Result;
                }
                else
                {
                    _sagaState = DataStore.Create(new SagaState()).Result;
                }
            }
        }

        protected Guid SagaId => _sagaState.id;

        protected async Task AddStatus(Enum additionalStatus)
        {
            if (_sagaState == null) throw new Exception("This saga is not stateful.");
            if (_sagaState.Flags != null)
            {
                _sagaState.Flags.AddState(additionalStatus);
            }
            else
            {
                _sagaState.Flags = new FlaggedState(additionalStatus);
            }

            await DataStore.UpdateUsingValuesFromAnotherInstanceWithTheSameId(_sagaState);
        }

        protected void CompleteSaga()
        {
            EventAggregator.Store(new SagaCompleted());
        }

        protected T GetStatus<T>()
        {
            if (_sagaState == null) throw new Exception("This saga is not stateful.");
            return _sagaState.Flags.AsEnum<T>();
        }

        protected async Task RemoveStatus(Enum statusToRemove)
        {
            if (_sagaState == null) throw new Exception("This saga is not stateful.");
            if (_sagaState.Flags != null)
            {
                _sagaState.Flags.RemoveState(statusToRemove);
            }
            else
            {
                throw new Exception("This saga's state has not been set.");
            }

            await DataStore.UpdateUsingValuesFromAnotherInstanceWithTheSameId(_sagaState);
        }

        protected async Task UpdateStatus(Enum newStatus)
        {
            if (_sagaState == null) throw new Exception("This saga is not stateful.");
            if (_sagaState.Flags != null)
            {
                _sagaState.Flags.ReplaceState(newStatus);
            }
            else
            {
                _sagaState.Flags = new FlaggedState(newStatus);
            }

            await DataStore.UpdateUsingValuesFromAnotherInstanceWithTheSameId(_sagaState);
        }
    }

    public class Saga<TReturnType> : Saga
        where TReturnType : class
    {
        private TReturnType _returnValue;

        public Saga(IEventAggregator eventAggregator, IDataStore dataStore, Guid? sagaId = null)
            : base(eventAggregator, dataStore, sagaId)
        {
        }

        public TReturnType ReturnValue
        {
            get
            {
                if (_returnValue == null)
                {
                    throw new Exception("Saga not completed.");
                }

                return _returnValue;
            }

            private set { _returnValue = value; }
        }

        protected void CompleteSaga<TCompleted>(TCompleted completedEvent) where TCompleted : SagaCompleted<TReturnType>
        {
            EventAggregator.Store(completedEvent);

            ReturnValue = completedEvent.Model;
        }
    }
}