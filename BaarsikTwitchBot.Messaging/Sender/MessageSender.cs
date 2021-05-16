using System;
using BaarsikTwitchBot.Core.Messages;
using BaarsikTwitchBot.Messaging.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace BaarsikTwitchBot.Messaging.Sender
{
    public class MessageSender : IMessageSender
    {
        private readonly string _hostname;
        private readonly string _password;
        private readonly string _queueName;
        private readonly string _username;
        private IConnection _connection;

        public MessageSender(IOptions<RabbitMqConfiguration> config)
        {
            _hostname = config.Value.Hostname;
            _password = config.Value.Password;
            _queueName = config.Value.QueueName;
            _username = config.Value.UserName;
            
            CreateConnection();
        }

        public void SendMessage(BaseMessage message)
        {
            if (!ConnectionExists()) return;
            using var channel = _connection.CreateModel();

            channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false,
                arguments: null);

            channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null,
                body: message.ToByteArray());
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password
                };
                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                //TODO add logging
                throw ex;
            }
        }

        private bool ConnectionExists()
        {
            if (_connection != null)
            {
                return true;
            }

            CreateConnection();

            return _connection != null;
        }
    }
}