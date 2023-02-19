using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Repository;
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
using Mango.Services.OrderAPI.Messages;

namespace Mango.Services.OrderAPI.Messaging
{
    public class AzureServiceBusConsumer :IAzureServiceBusConsumer
    {

        private readonly string yandexKafkaHost;
        private readonly string checkoutMessageTopic;
        private readonly string checkoutMessageTopicConsumerUser;
        private readonly string orderPaymentProcessTopic;
        private readonly string orderPaymentProcessTopicConsumerUser;
        private readonly string producerUser;        
        private readonly string orderUpdatePaymentResultTopic;
        private readonly string orderUpdatePaymentResultTopicConsumerUser;
        private readonly string allUsersPassword = "Admin123*";

        private readonly OrderRepository _orderRepository;
        private readonly IMessageBus _messageBus;

        private readonly IConfiguration _configuration;

        private string CA_FILE = (Environment.OSVersion.Platform == PlatformID.Unix ||
                  Environment.OSVersion.Platform == PlatformID.MacOSX)
                  ? Environment.GetEnvironmentVariable("HOME")
                  : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + "\\.kafka\\YandexCA.crt";

        private ConsumerConfig consumerConfig;
        private ConsumerConfig consumerConfig_ForPayment;
        private IConsumer<string, string> client;
        private IConsumer<string, string> client_ForPayment;

        //    ConsumerConfig consumerConfig;

        //   IConsumer<string, string> client;

        public AzureServiceBusConsumer(OrderRepository orderRepository, IConfiguration configuration, IMessageBus messageBus)
        {
            _orderRepository = orderRepository;
            _configuration = configuration;
            _messageBus = messageBus;

            yandexKafkaHost = _configuration.GetValue<string>("YandexKafkaHost");
            checkoutMessageTopic = _configuration.GetValue<string>("CheckoutMessageTopic");
            checkoutMessageTopicConsumerUser = _configuration.GetValue<string>("CheckoutMessageTopicConsumerUser");
            orderPaymentProcessTopic = _configuration.GetValue<string>("OrderPaymentProcessTopic");
            orderPaymentProcessTopicConsumerUser = _configuration.GetValue<string>("OrderPaymentProcessTopicConsumerUser");
            producerUser = _configuration.GetValue<string>("ProducerUser");            
            orderUpdatePaymentResultTopic = _configuration.GetValue<string>("OrderUpdatePaymentResultTopic");
            orderUpdatePaymentResultTopicConsumerUser = _configuration.GetValue<string>("OrderUpdatePaymentResultTopicConsumerUser");



            consumerConfig = new ConsumerConfig(
                   new Dictionary<string, string>{
                    {"bootstrap.servers", yandexKafkaHost},
                    {"security.protocol", "SASL_SSL"},
                    {"ssl.ca.location", CA_FILE},
                    {"sasl.mechanisms", "SCRAM-SHA-512"},
                    {"sasl.username", checkoutMessageTopicConsumerUser},
                    {"sasl.password", allUsersPassword},
                    {"group.id", "demo"}
                    });

            consumerConfig_ForPayment = new ConsumerConfig(
                   new Dictionary<string, string>{
                    {"bootstrap.servers", yandexKafkaHost},
                    {"security.protocol", "SASL_SSL"},
                    {"ssl.ca.location", CA_FILE},
                    {"sasl.mechanisms", "SCRAM-SHA-512"},
                    {"sasl.username", orderUpdatePaymentResultTopicConsumerUser},
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

                    //await Task.Delay(3000);

                    var hhh = 3;

                    client = new ConsumerBuilder<string, string>(consumerConfig).Build();
                    client.Subscribe(checkoutMessageTopic);
                    //var cr = client.Consume(new TimeSpan(0, 0, 0, 5));
                    var cr = client.Consume(TimeSpan.FromSeconds(5));                    
                    //Console.WriteLine($"{cr.Message.Key}:{cr.Message.Value}");

                    if (cr != null) 
                    {
                        Console.WriteLine("OrderAPI: I have read this: ");
                        Console.WriteLine(cr.Value.ToString());

                        await OnCheckOutMessageReceived(cr.Value.ToString());
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


                try
                {

                    var kkk = 3;

                    client_ForPayment = new ConsumerBuilder<string, string>(consumerConfig_ForPayment).Build();
                    client_ForPayment.Subscribe(orderUpdatePaymentResultTopic);
                    //var cr_ForPayment = client_ForPayment.Consume(new TimeSpan(0, 0, 0, 5));
                    var cr_ForPayment = client_ForPayment.Consume(TimeSpan.FromSeconds(5));
                    //Console.WriteLine($"{cr.Message.Key}:{cr.Message.Value}");

                    if (cr_ForPayment != null)
                    {
                        Console.WriteLine("OrderAPI: I have read this: ");
                        Console.WriteLine(cr_ForPayment.Value.ToString());

                        await OnOrderPaymentUpdateReceived(cr_ForPayment.Value.ToString());
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
            if (client_ForPayment != null)
            {
                client_ForPayment.Close();
                client_ForPayment.Dispose();
            }
        }

        private async Task OnCheckOutMessageReceived(string message)
        {
            //var message = args.Message;
            //var body = Encoding.UTF8.GetString(message.get);
            var body = message;

            CheckoutHeaderDto checkoutHeaderDto = JsonConvert.DeserializeObject<CheckoutHeaderDto>(body);

            OrderHeader orderHeader = new()
            {
                UserId = checkoutHeaderDto.UserId,
                FirstName = checkoutHeaderDto.FirstName,
                LastName = checkoutHeaderDto.LastName,
                OrderDetails = new List<OrderDetails>(),
                CardNumber = checkoutHeaderDto.CardNumber,
                CouponCode = checkoutHeaderDto.CouponCode,
                CVV = checkoutHeaderDto.CVV,
                DiscountTotal = checkoutHeaderDto.DiscountTotal,
                Email = checkoutHeaderDto.Email,
                ExpiryMonthYear = checkoutHeaderDto.ExpiryMonthYear,
                OrderTime = DateTime.Now,
                OrderTotal = checkoutHeaderDto.OrderTotal,
                PaymentStatus = false,
                Phone = checkoutHeaderDto.Phone,
                PickupDateTime = checkoutHeaderDto.PickupDateTime
            };

            foreach(var detailList in checkoutHeaderDto.CartDetails)
            {
                OrderDetails orderDetails = new()
                {
                    ProductId = detailList.ProductId,
                    ProductName = detailList.Product.Name,
                    Price= detailList.Product.Price,
                    Count = detailList.Count
                };
                orderHeader.CartTotalItems += detailList.Count;
                orderHeader.OrderDetails.Add(orderDetails);
            }

            await _orderRepository.AddOrder(orderHeader);

            PaymentRequestMessage paymentRequestMessage = new()
            {
                Name = orderHeader.FirstName + " " + orderHeader.LastName,
                CardNumber = orderHeader.CardNumber,
                CVV = orderHeader.CVV,
                ExpiryMonthYear = orderHeader.ExpiryMonthYear,
                OrderId = orderHeader.OrderHeaderId,
                OrderTotal = orderHeader.OrderTotal
            };

            try
            {
                await _messageBus.PublishMessage(paymentRequestMessage, orderPaymentProcessTopic, producerUser, allUsersPassword);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        private async Task OnOrderPaymentUpdateReceived(string message)
        {
            var body = message;

            UpdatePaymentResultMessage paymentResultMessage = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(body);

            await _orderRepository.UpdateOrderPaymentStatus(paymentResultMessage.OrderId, paymentResultMessage.Status);
        }

    }
}
