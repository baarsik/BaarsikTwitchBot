using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BaarsikTwitchBot.Core.Messages;
using BaarsikTwitchBot.Messaging.Configuration;
using BaarsikTwitchBot.UI.UiMessageHandlers.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BaarsikTwitchBot.UI.HostedServices
{
    public class MessageReceiver : BackgroundService
    {
        private readonly RabbitMqConfiguration _configuration;
        private readonly IHubContext<MessageHub> _hubContext;

        private IModel _channel;
        private IConnection _connection;

        public MessageReceiver(IOptions<RabbitMqConfiguration> config, IHubContext<MessageHub> hubContext)
        {
            _hubContext = hubContext;
            _configuration = config.Value;

            InitializeRabbitMqListener();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());

                var settings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};
                var message = JsonConvert.DeserializeObject<BaseMessage>(content, settings);

                await HandleMessage(message);

                _channel.BasicAck(ea.DeliveryTag, false);
            };
            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerCancelled;

            _channel.BasicConsume(_configuration.QueueName, false, consumer);

            return Task.CompletedTask;
        }

        private void InitializeRabbitMqListener()
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration.Hostname,
                UserName = _configuration.UserName,
                Password = _configuration.Password
            };

            _connection = factory.CreateConnection();
            //  _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _configuration.QueueName, durable: false, exclusive: false, autoDelete: false,
                arguments: null);
        }

        private async Task HandleMessage(BaseMessage message)
        {
            await _hubContext.Clients.All.SendAsync(GetMessageTypeName(message), message.ToString());
        }

        private void OnConsumerCancelled(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
        }

        private string GetMessageTypeName(BaseMessage message)
        {
            return message switch
            {
                SongPlayCurrentSongTimeSpanUpdatedMessage songPlayCurrentSongTimeSpanUpdatedMessage => nameof(
                    SongPlayCurrentSongTimeSpanUpdatedMessage),
                SongPlayFinishedMessage songPlayFinishedMessage => nameof(SongPlayCurrentSongTimeSpanUpdatedMessage),
                SongPlayPausedMessage songPlayPausedMessage => nameof(SongPlayCurrentSongTimeSpanUpdatedMessage),
                SongPlayRequestAddedMessage songPlayRequestAddedMessage => nameof(
                    SongPlayCurrentSongTimeSpanUpdatedMessage),
                SongPlayStartedMessage songPlayStartedMessage => nameof(SongPlayCurrentSongTimeSpanUpdatedMessage),
                SongPlayVolumeChange songPlayVolumeChange => nameof(SongPlayCurrentSongTimeSpanUpdatedMessage),
                _ => throw new ArgumentOutOfRangeException(nameof(message))
            };
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}