using Confluent.Kafka;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mango.MessageBus
{
    // на самом деле Yandex Cloud - Managed Service for Apache Kafka
    public class AzureServiceBusMessageBus : IMessageBus
    {
                   
        private string HOST = "rc1a-s2ka732h0etig7b0.mdb.yandexcloud.net:9091";
        private string CA_FILE = @"C:\Users\bds\.kafka\YandexCA.crt";
        //var ttt = System.Environment.SpecialFolder..GetEnvironmentVariable("HOME") + "YandexCA.crt";
        //
        public async Task PublishMessage(BaseMessage message, string topicName, string publishUserName, string publishUserPass)
        {
            var producerConfig = new ProducerConfig(
                    new Dictionary<string, string>{
                        {"bootstrap.servers", HOST},
                        {"security.protocol", "SASL_SSL"},
                        {"ssl.ca.location", CA_FILE},
                        {"sasl.mechanisms", "SCRAM-SHA-512"},
                        {"sasl.username", publishUserName},
                        {"sasl.password", publishUserPass}
                        });

            var jsonMessage = JsonConvert.SerializeObject(message);
            var finalMessage = jsonMessage; //Encoding.UTF8.GetBytes(jsonMessage);

            var producer = new ProducerBuilder<string, string>(producerConfig).Build();
            
            await producer.ProduceAsync(topicName, new Message<string, string> { Key = Guid.NewGuid().ToString(), Value = finalMessage }
            //,
            //    (deliveryReport) =>
            //        {
            //            if (deliveryReport.Error.Code != ErrorCode.NoError)
            //            {
            //                Console.WriteLine($"Failed to deliver message: {deliveryReport.Error.Reason}");
            //            }
            //            else
            //            {
            //                Console.WriteLine($"Produced message to: {deliveryReport.TopicPartitionOffset}");
            //        }
            //        }
                    
                    );        
            producer.Flush(TimeSpan.FromSeconds(10));
            //producer.Dispose();

        }
    }
}
