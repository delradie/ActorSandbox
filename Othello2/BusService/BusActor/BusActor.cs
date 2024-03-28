using BusActor.Interfaces;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

using System.Diagnostics;
using System.Fabric;

namespace BusActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class BusActor : Actor, IBusActor
    {
        private readonly String _ident;

        /// <summary>
        /// Initializes a new instance of BusActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public BusActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            _ident = actorId.ToString();
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Bus Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            return this.StateManager.TryAddStateAsync("count", 0);
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <returns></returns>
        Task IBusActor.ProcessMessageAsync(String ident, String message, CancellationToken cancellationToken)
        {
            ActorEventSource.Current.ActorMessage(this, $"START {message} - {ident} : {FabricRuntime.GetNodeContext().NodeName}|{this.ServiceUri}|{this._ident}");

            Thread.Sleep(5000);

            ActorEventSource.Current.ActorMessage(this, $"END {message} - {ident} : {FabricRuntime.GetNodeContext().NodeName}|{this.ServiceUri}|{this._ident}");
            return Task.CompletedTask;
        }
    }
}
