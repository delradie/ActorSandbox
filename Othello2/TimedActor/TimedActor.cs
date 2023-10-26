using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using TimedActor.Interfaces;

namespace TimedActor
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
    internal class TimedActor : Actor, ITimedActor, IRemindable
    {
        /// <summary>
        /// Initializes a new instance of TimedActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public TimedActor(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
        }


        private async Task UpdateCount()
        {
            Int32 CurrentCount = await this.StateManager.GetStateAsync<int>("count");

            CurrentCount++;

            await this.StateManager.AddOrUpdateStateAsync("count", CurrentCount, (key, value) => CurrentCount > value ? CurrentCount : value);

            Thread.Sleep(30000);
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            switch (reminderName)
            {
                case "UpdateCount":
                    await UpdateCount();
                    break;
            }
        }

        public async Task RegisterReminder()
        {
            try
            {
                var previousRegistration = GetReminder("UpdateCount");
                await UnregisterReminderAsync(previousRegistration);
            }
            catch (ReminderNotFoundException) { }

            var reminderRegistration = await RegisterReminderAsync("UpdateCount", null, TimeSpan.FromMinutes(0), TimeSpan.FromSeconds(20));
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
            await this.StateManager.TryAddStateAsync("count", 0);

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization            
        }

    }
}
