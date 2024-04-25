using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace ConsoleApp.Interfaces
{
  internal interface ISbMessages : IAsyncDisposable
  {
    Task<IReadOnlyList<ServiceBusReceivedMessage>> ListMessagesAsync(string qcs);
  }
}
