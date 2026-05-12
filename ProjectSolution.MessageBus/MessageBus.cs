
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System.Text;

namespace ProjectSolution.MessageBus
{
    public class MessageBus : IMessageBus
    {
        private readonly string _connectionString = "your_connection_string_here";
        public async Task PublishMessage(object message, string queue_topic_name)
        {
            await using var client = new ServiceBusClient(_connectionString);
            ServiceBusSender sender = client.CreateSender(queue_topic_name);

            string? messageBody = JsonConvert.SerializeObject(message);
            byte[]? messageBinaryData = Encoding.UTF8.GetBytes(messageBody);

            ServiceBusMessage busMessage = new ServiceBusMessage(messageBinaryData)
            {
                CorrelationId = Guid.NewGuid().ToString(),
                MessageId = Guid.NewGuid().ToString(),
                //Subject = "message_name",
                ApplicationProperties =
                {
                    { "property_name", "property_value" },
                }
            };

            await sender.SendMessageAsync(busMessage);
            await client.DisposeAsync();
        }
    }
}

/*
 First: What is using?

Normal using is for automatic cleanup of resources.

using var stream = new FileStream(...);

At the end of the scope:

stream.Dispose();
Problem: What if cleanup is async?

Some resources (like network connections, sockets, Service Bus clients) need async cleanup.

That’s where await using comes in.

Behind the scenes

The class Azure Service Bus client implements:

IAsyncDisposable

Instead of:

Dispose()

it has:

ValueTask DisposeAsync();
What await using actually does

This:

await using var client = new ServiceBusClient(connectionString);

is roughly equivalent to:

var client = new ServiceBusClient(connectionString);

try
{
    // use client
}
finally
{
    if (client != null)
        await client.DisposeAsync();
}
Key Difference
Keyword	        Interface Used	    Cleanup Type
using	        IDisposable	        Sync
await using	    IAsyncDisposable	Async
 */