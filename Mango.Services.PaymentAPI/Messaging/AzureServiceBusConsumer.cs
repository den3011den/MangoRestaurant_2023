using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Confluent.Kafka;
using Microsoft.AspNetCore.Hosting;
using static Confluent.Kafka.ConfigPropertyNames;
using Mango.MessageBus;
using Mango.Services.PaymentAPI.Messages;
using PaymentProcessor;

namespace Mango.Services.PaymentAPI.Messaging
{
    public class AzureServiceBusConsumer :IAzureServiceBusConsumer
    {
        //private readonly string serviceBusConnectionString;
        //private readonly string subscriptionPayment;        
        //private readonly string orderPaymentProcessTopic;
        //private readonly string orderUpdatePaymentResultTopic;
        private readonly IProcessPayment _processPayment;


        private readonly string yandexKafkaHost;
        private readonly string oprderPaymentProcessTopic;
        private readonly string orderPaymentProcessTopicConsumerUser;
        private readonly string producerUser;
        private readonly string orderUpdatePaymentResultTopic;
        private readonly string allUsersPassword = "Admin123*";

        private readonly IMessageBus _messageBus;

        private readonly IConfiguration _configuration;

        private string CA_FILE = (Environment.OSVersion.Platform == PlatformID.Unix ||
                  Environment.OSVersion.Platform == PlatformID.MacOSX)
                  ? Environment.GetEnvironmentVariable("HOME")
                  : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + "\\.kafka\\YandexCA.crt";


        private ConsumerConfig consumerConfig;
        private IConsumer<string, string> client;

        //    ConsumerConfig consumerConfig;

        //   IConsumer<string, string> client;

        public AzureServiceBusConsumer(IConfiguration configuration, IMessageBus messageBus, IProcessPayment processPayment)
        {
             _configuration = configuration;
            _messageBus = messageBus;
            _processPayment = processPayment;

            //serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            //subscriptionPayment = _configuration.GetValue<string>("OrderPaymentProcessSubscription");             
            //orderUpdatePaymentResultTopic = _configuration.GetValue<string>("OrderUpdatePaymentResultTopic");
            //orderPaymentProcessTopic = _configuration.GetValue<string>("OrderPaymentProcessTopic");

            //HOST = serviceBusConnectionString;
            //TOPIC = orderPaymentProcessTopic;

            yandexKafkaHost = _configuration.GetValue<string>("YandexKafkaHost");
            oprderPaymentProcessTopic = _configuration.GetValue<string>("OrderPaymentProcessTopic");
            orderPaymentProcessTopicConsumerUser = _configuration.GetValue<string>("OrderPaymentProcessTopicConsumerUser");
            producerUser = _configuration.GetValue<string>("ProducerUser");
            orderUpdatePaymentResultTopic = _configuration.GetValue<string>("OrderUpdatePaymentResultTopic");


            consumerConfig = new ConsumerConfig(
                   new Dictionary<string, string>{
                    {"bootstrap.servers", yandexKafkaHost},
                    {"security.protocol", "SASL_SSL"},
                    {"ssl.ca.location", CA_FILE},
                    {"sasl.mechanisms", "SCRAM-SHA-512"},
                    {"sasl.username", orderPaymentProcessTopicConsumerUser},
                    {"sasl.password", allUsersPassword},
                    {"group.id", "demo"}
                    });

            //client = new ConsumerBuilder<string, string>(consumerConfig).Build();
            //client.Subscribe(TOPIC);

        }


        public async Task Start()
        {

            while (true)
            {
                try
                {

                    var hhh = 3;

                    client = new ConsumerBuilder<string, string>(consumerConfig).Build();
                    client.Subscribe(oprderPaymentProcessTopic);
                    var cr = client.Consume(new TimeSpan(0, 0, 0, 5));
                    //Console.WriteLine($"{cr.Message.Key}:{cr.Message.Value}");

                    if (cr != null) 
                    {
                        Console.WriteLine("PaymentAPI: I have read this: ");
                        Console.WriteLine(cr.Value.ToString());

                        await ProcessPayments(cr.Value.ToString());
                    }

                    //await Stop();
                }
                catch (Exception eex)
                {
                    Console.WriteLine("ERROR: " + eex.Message);
                }
                finally
                {
                                      
                }
            }
        }

        public async Task Stop()
        {
            if (client!=null)
            { 
                client.Close();
                client.Dispose();
            }
        }

        private async Task ProcessPayments(string message)
        {
            //var message = args.Message;
            //var body = Encoding.UTF8.GetString(message.get);
            var body = message;

            PaymentRequestMessage paymentRequestMessage = JsonConvert.DeserializeObject<PaymentRequestMessage>(body);

            var result = _processPayment.PaymentProcessor();

            UpdatePaymentResultMessage updatePaymentResultMessage = new()
            {
                Status = result,
                OrderId = paymentRequestMessage.OrderId
            };


            try
            {
                await _messageBus.PublishMessage(updatePaymentResultMessage, orderUpdatePaymentResultTopic, producerUser, allUsersPassword);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

    }
}
