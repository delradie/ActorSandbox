using Azure.Messaging.ServiceBus;

using BusActor.Interfaces;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;

using System.Diagnostics;
using System.Fabric;

namespace BusActor
{
    internal class BusInterfaceService : ActorService
    {
        private String Ident
        {
            get
            {
                return FabricRuntime.GetNodeContext().NodeName + "|" + this.Partition.PartitionInfo.Id;
            }
        }

        private String _connectionString;
        private String _queueName;

        private ServiceBusClient _busClient;
        private ServiceBusProcessor _sessionlessClient;

        protected async override Task RunAsync(CancellationToken cancellationToken)
        {
            await base.RunAsync(cancellationToken);

            this._busClient = new ServiceBusClient(this._connectionString);

            this._sessionlessClient = this._busClient.CreateProcessor(this._queueName, new ServiceBusProcessorOptions()
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                AutoCompleteMessages = true,
                MaxConcurrentCalls = 100
            });

            this._sessionlessClient.ProcessMessageAsync += this._sessionlessClient_ProcessMessageAsync;
            this._sessionlessClient.ProcessErrorAsync += this._sessionlessClient_ProcessErrorAsync;

            await this._sessionlessClient.StartProcessingAsync();

            Debug.Print("Started listening on " + Ident);
        }

        private Task _sessionlessClient_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private async Task _sessionlessClient_ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            ServiceBusReceivedMessage Message = arg.Message;
            String Content = Message.Body.ToString();

            ActorId Target = ActorId.CreateRandom();

            IBusActor proxy = ActorProxy.Create<IBusActor>(Target);

            await proxy.ProcessMessageAsync(Ident, Content, CancellationToken.None);
        }

        public BusInterfaceService(StatefulServiceContext context, ActorTypeInformation actorTypeInfo,
            String connectionString, String queueName,
            Func<ActorService, ActorId, ActorBase> actorFactory = null,
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, IActorStateProvider stateProvider = null,
            ActorServiceSettings settings = null)
       : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {
            Console.WriteLine("FOOO");
            this._connectionString = connectionString;
            this._queueName = queueName;
        }
    }
}
