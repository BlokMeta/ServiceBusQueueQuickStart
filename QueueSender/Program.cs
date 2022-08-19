using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace QueueSender
{
    class Program
    {

        // connection string to your Service Bus namespace  ||TR: Service Bus ad alanını için bağlantı dizesi
        static string connectionString = "***************(endpoint)";

        // name of your Service Bus queue || Service Bus kuyruk adı
        static string queueName = "**********";

        // the client that owns the connection and can be used to create senders and receivers  ||TR: göndericiler ve alıcılar oluşturmak bağlantıya sahip olan ve kullanılabilen istemci
        static ServiceBusClient? client;

        // the sender used to publish messages to the queue || TR: mesajları kuyruğa yayınlamak için kullanılan gönderen(gönderici)
        static ServiceBusSender? sender;

        // number of messages to be sent to the queue  ||TR:kuyruğa gönderilecek mesaj sayısı
        private const int numOfMessages = 5;

        static async Task Main()
        {
            // The Service Bus client types are safe to cache and use as a singleton for the lifetime   || TR: Service Bus istemci türlerinin önbelleğe alınması ve uygulamanın ömrü boyunca Singleton(Tek bir objenin yaratılmasına izin veren yazılım tasarımı) olarak kullanılması güvenlidir; bu, iletiler düzenli olarak yayınlanırken veya okunurken en iyi uygulamadır.
            // of the application, which is best practice when messages are being published or read
            // regularly.
            //
            // Create the clients that we'll use for sending and processing messages. || TR: Mesaj göndermek ve işlemek için kullanacağımız istemcileri oluşturur.
            client = new ServiceBusClient(connectionString);
            sender = client.CreateSender(queueName);

            // create a batch || TR: Batch oluşturur.
            using (ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync())
            {
                for (int i = 1; i <= numOfMessages; i++)
                {
                    // try adding a message to the batch || TR: gruba bir mesaj eklemeyi deneyin
                    if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
                    {
                        // if it is too large for the batch || TR: Batch için çok fazla mesaj gönderilmeye çalışıldıysa hata fırlat.
                        throw new Exception($"The message {i} is too large to fit in the batch.");
                    }
                }

                try
                {
                    // Use the producer client to send the batch of messages to the Service Bus queue  || TR: Toplu iletileri Service Bus kuyruğuna göndermek için üretici istemciyi kullanın
                    await sender.SendMessagesAsync(messageBatch);
                    Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
                }

                finally
                {
                    // Calling DisposeAsync on client types is required to ensure that network || TR: Ağın olduğundan emin olmak için istemci türlerinde DisposeAsync'in çağrılması gerekir.
                    // resources and other unmanaged objects are properly cleaned up. || TR: Toplu iletileri Service Bus kuyruğuna göndermek için üretici istemciyi kullanın
                    await sender.DisposeAsync();
                    await client.DisposeAsync();
                }
            }

            Console.WriteLine("Press any key to end the application");
            Console.ReadKey();
        }
    }
}
