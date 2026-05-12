using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using ProjectSolution.Services.EmailAPI.Data;
using ProjectSolution.Services.EmailAPI.Models;
using ProjectSolution.Services.EmailAPI.Models.Dto;
using System.Text;

namespace ProjectSolution.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer : BackgroundService
    {
        private readonly string _serviceBusConnectionString;
        private readonly string _emailCartQueue;
        private readonly IConfiguration _configuration;

        private readonly ServiceBusClient _client;
        private ServiceBusProcessor _emailCartProcessor;
        private readonly ILogger<AzureServiceBusConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public AzureServiceBusConsumer(IConfiguration configuration, ILogger<AzureServiceBusConsumer> logger, IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString")!;
            _emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue")!;

            _client = new ServiceBusClient(_serviceBusConnectionString);
            _emailCartProcessor = _client.CreateProcessor(_emailCartQueue, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1
            });
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _emailCartProcessor.ProcessMessageAsync += args => ProcessEmailCartMessage(args);
            _emailCartProcessor.ProcessErrorAsync += ErrorHandler;

            await _emailCartProcessor.StartProcessingAsync(stoppingToken);
            _logger.LogInformation("AzureServiceBusConsumer started processing messages.");

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Expected when the service is stopping
                _logger.LogInformation("AzureServiceBusConsumer is stopping.");
            }
            finally
            {
                await _emailCartProcessor.StopProcessingAsync(stoppingToken);
                await _emailCartProcessor.DisposeAsync();
                await _client.DisposeAsync();
                _logger.LogInformation("AzureServiceBusConsumer has stopped processing messages.");
            }
        }

        private async Task ProcessEmailCartMessage(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            CartDto objMessage = JsonConvert.DeserializeObject<CartDto>(body)!;

            try
            {
                // Simulate email sending logic here
                using (var scope = _scopeFactory.CreateScope())
                {
                    // You can resolve your email service from the scope here and send the email
                    // var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    // await emailService.SendCartEmailAsync(objMessage);

                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var emailLog = new EmailLogger
                    {
                        Email = objMessage.CartHeaderDto?.Email,
                        Message = $"Email sent for cart with ID: {objMessage.CartHeaderDto?.CartHeaderId} and user: {objMessage.CartHeaderDto?.UserId}: {objMessage.CartDetailsListDto}",
                        EmailSent = DateTime.UtcNow
                    };
                    dbContext.EmailLoggers.Add(emailLog);
                    await dbContext.SaveChangesAsync();
                }
                _logger.LogInformation("Processing email cart message for user: {UserId}", objMessage.CartHeaderDto?.UserId);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email cart message for user: {UserId}", objMessage.CartHeaderDto?.UserId);
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Error processing message from Azure Service Bus. Entity Path: {EntityPath}, Error Source: {ErrorSource}", args.EntityPath, args.ErrorSource);
            return Task.CompletedTask;
        }
    }
}
