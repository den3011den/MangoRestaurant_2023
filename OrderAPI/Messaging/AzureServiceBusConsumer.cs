﻿using Mango.Services.OrderAPI.Models;
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

namespace Mango.Services.OrderAPI.Messaging
{
    public class AzureServiceBusConsumer :IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;
        private readonly string subscriptionCheckOut;
        private readonly string checkoutMessageTopic;
        private readonly OrderRepository _orderRepository;

       private readonly IConfiguration _configuration;

        private string CA_FILE = (Environment.OSVersion.Platform == PlatformID.Unix ||
                  Environment.OSVersion.Platform == PlatformID.MacOSX)
                  ? Environment.GetEnvironmentVariable("HOME")
                  : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + "\\.kafka\\YandexCA.crt";

        private readonly string HOST;
        private readonly string TOPIC;

    //    ConsumerConfig consumerConfig;

     //   IConsumer<string, string> client;

        public AzureServiceBusConsumer(OrderRepository orderRepository, IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _configuration = configuration;

            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            subscriptionCheckOut = _configuration.GetValue<string>("SubscriptionCheckOut");
            checkoutMessageTopic = _configuration.GetValue<string>("CheckoutMessageTopic");

            HOST = serviceBusConnectionString;
            TOPIC = checkoutMessageTopic;



        }


        public async Task Start()
        {


            var consumerConfig = new ConsumerConfig(
               new Dictionary<string, string>{
                    {"bootstrap.servers", HOST},
                    {"security.protocol", "SASL_SSL"},
                    {"ssl.ca.location", CA_FILE},
                    {"sasl.mechanisms", "SCRAM-SHA-512"},
                    {"sasl.username", subscriptionCheckOut},
                    {"sasl.password", "Admin123*"},
                    {"group.id", "demo"}
               }
           );

            var client = new ConsumerBuilder<string, string>(consumerConfig).Build();
            client.Subscribe(TOPIC);

          //  while (true)
          //  {
                try
                {

                    var hhh = 3;
                    var cr = client.Consume();
                    //Console.WriteLine($"{cr.Message.Key}:{cr.Message.Value}");
                    

                    await OnCheckOutMessageReceived(cr.Value.ToString());
                }
                catch (Exception eex)
                {
                    Console.WriteLine("ERROR: " + eex.Message);
                }
                finally
                {
                                      
                }
            //}
        }

        public async Task Stop()
        {
            //Close();
            //Dispose();
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
        }

    }
}
