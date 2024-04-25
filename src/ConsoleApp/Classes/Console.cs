using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using ConsoleApp.Constants;
using ConsoleApp.Helpers;
using ConsoleApp.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static System.Console;


namespace ConsoleApp.Classes
{
    internal class Console : IConsole
    {
        private readonly ILogger<Console> _logger;
        private readonly IConfiguration _config;
        private readonly ISbMessages _sbMessages;

        public Console(
            ILogger<Console> logger,
            IConfiguration config,
            ISbMessages sbMessages
        )
        {
            _logger = logger;
            _config = config;
            _sbMessages = sbMessages;
        }

        public async Task<bool> RunAsync()
        {
            using (_logger.BeginScope("RunAsync()"))
            {
                _logger.LogTrace("Method start");

                var qcs = _config.GetValue<string>(ConfigKey.SbQueueConnectionString);
                if (qcs != null)
                {
                  var messages = await _sbMessages.ListMessagesAsync(qcs);

                  if (messages.Any())
                  {
                    DisplayMessages(messages);
                  }
                  else
                  {
                    WriteLine($"No messages found.");
                  }
                }

                WriteLine($"Press Enter to exit....");
                ReadLine();

                _logger.LogTrace("Method end");
            }

            return true;
        }

        private static void DisplayMessages(IEnumerable<ServiceBusReceivedMessage> messages)
        {
          ConsoleTable.PrintLine();
          ConsoleTable.PrintRow("Seq","Id","deliveryCount","state", "Subject", "Body");
          ConsoleTable.PrintLine();
          foreach (var message in messages)
          {
            ConsoleTable.PrintRow(
  message.SequenceNumber.ToString(),
              message.MessageId ?? string.Empty,
              message.DeliveryCount.ToString(),
              message.State.ToString(),
              message.Subject ?? string.Empty,
              message.Body.ToString()
            );
            //WriteLine($"Message: {message.Body}");
          }
          ConsoleTable.PrintLine();

    }
  }
}
