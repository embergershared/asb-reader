using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using ConsoleApp.Constants;
using ConsoleApp.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConsoleApp.Classes
{
  public partial class SbMessages : ISbMessages
  {
    private readonly ILogger<SbMessages> _logger;

    public SbMessages(
      ILogger<SbMessages> logger
    )
    {
      _logger = logger;
      logger.LogDebug("SbMessages created");
    }

    #region Interface implementation
    // As we use ConnectionString, we don't need to use Azure.Identity client to authenticate
    async Task<IReadOnlyList<ServiceBusReceivedMessage>> ISbMessages.ListMessagesAsync(string queueConnectionString)
    {
      _logger.LogInformation("SbMessages.ListMessagesAsync() called with ConnectionString: {ConnectionString}", queueConnectionString);

      var returnedValue = new List<ServiceBusReceivedMessage>();

      // Create a Service Bus client based on the connection string
      var sbClient = new ServiceBusClient(queueConnectionString);
      
      // Extract queue name from connection string
      var queueName = ExtractQueueName(queueConnectionString);
      
      // Create a Messages receiver for the queue
      var receiver = sbClient.CreateReceiver(queueName);

      // Receive the messages for the queue with a timeout of 5 seconds (if nothing comes, there's no messages)
      _logger.LogDebug("Receiving messages from queue: {QueueName}, by batch of: {maxMessages}", queueName, Const.MaxMessagesToRetrieve);
      var messages = await receiver.ReceiveMessagesAsync(Const.MaxMessagesToRetrieve, TimeSpan.FromMilliseconds(Const.MaxQueryTimeInSeconds*1000));

      // Check for null
      if (messages == null)
      {
        _logger.LogInformation("SbMessages.ListMessagesAsync() failed to receive messages from queue: {queueName}", queueName);
        return returnedValue;
      }


      // If no messages, return an empty list
      if (!messages.Any())
      {
        _logger.LogInformation("SbMessages.ListMessagesAsync() didn't find any messages to return in {seconds}", Const.MaxQueryTimeInSeconds);
        return returnedValue;
      }

      _logger.LogDebug("Received: {Count} messages from queue: {queueName}", messages.Count, queueName);
      returnedValue.AddRange(messages);

      // As we have messages, abandon them to keep them in the queue
      foreach (var message in returnedValue)
      {
        await receiver.AbandonMessageAsync(message);
      }

      // Return the messages we found
      _logger.LogInformation("SbMessages.ListMessagesAsync() returning {messagesCount} messages", returnedValue.Count);
      return returnedValue;
    }
    #endregion

    #region Public Methods
    public ValueTask DisposeAsync()
    {
      _logger.LogDebug("SbMessages.DisposeAsync() called");

      GC.SuppressFinalize(this);

      return new ValueTask(Task.CompletedTask);
    }
    #endregion

    #region Private Methods
    private static string ExtractQueueName(string connectionString)
    {
      var match = QueueNameRegEx().Match(connectionString);
      if (match.Success)
      {
        return match.Groups[1].Value;
      }
      throw new InvalidOperationException("Queue name not found in the connection string.");
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"EntityPath=([^;]+)")]
    private static partial System.Text.RegularExpressions.Regex QueueNameRegEx();
    #endregion
  }
}
