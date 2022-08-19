# ServiceBusQueueQuickStart

Sign in to the Azure portal

In the left navigation pane of the portal, select + Create a resource, select Integration, and then select Service Bus.

In the Basics tag of the Create namespace page, follow these steps:

For Subscription, choose an Azure subscription in which to create the namespace.

For Resource group, choose an existing resource group in which the namespace will live, or create a new one.

Enter a name for the namespace. The namespace name should adhere to the following naming conventions:

The name must be unique across Azure. The system immediately checks to see if the name is available.
The name length is at least 6 and at most 50 characters.
The name can contain only letters, numbers, hyphens “-“.
The name must start with a letter and end with a letter or number.
The name doesn't end with “-sb“ or “-mgmt“.
For Location, choose the region in which your namespace should be hosted.

For Pricing tier, select the pricing tier (Basic, Standard, or Premium) for the namespace. For this quickstart, select Standard.

If you selected the Premium pricing tier, specify the number of messaging units. The premium tier provides resource isolation at the CPU and memory level so that each workload runs in isolation. This resource container is called a messaging unit. A premium namespace has at least one messaging unit. You can select 1, 2, 4, 8 or 16 messaging units for each Service Bus Premium namespace. For more information, see Service Bus Premium Messaging.

Select Review + create. The system now creates your namespace and enables it. You might have to wait several minutes as the system provisions resources for your account.
On the Create page, review settings, and select Create.

Select Go to resource on the deployment page.

You see the home page for your service bus namespace.

Get the connection string
Creating a new namespace automatically generates an initial Shared Access Signature (SAS) policy with primary and secondary keys, and primary and secondary connection strings that each grant full control over all aspects of the namespace. See Service Bus authentication and authorization for information about how to create rules with more constrained rights for regular senders and receivers.

To copy the primary connection string for your namespace, follow these steps:

On the Service Bus Namespace page, select Shared access policies on the left menu.

On the Shared access policies page, select RootManageSharedAccessKey.

In the Policy: RootManageSharedAccessKey window, select the copy button next to Primary Connection String, to copy the connection string to your clipboard for later use. Paste this value into Notepad or some other temporary location.

Screenshot shows an S A S policy called RootManageSharedAccessKey, which includes keys and connection strings.

You can use this page to copy primary key, secondary key, primary connection string, and secondary connection string.

Create a queue in the Azure portal
On the Service Bus Namespace page, select Queues in the left navigational menu.

On the Queues page, select + Queue on the toolbar.

Enter a name for the queue, and leave the other values with their defaults.

Now, select Create.

Image showing creation of a queue in the portal

Send messages to the queue
This section shows you how to create a .NET Core console application to send messages to a Service Bus queue.

 Note

This quick start provides step-by-step instructions to implement a simple scenario of sending a batch of messages to a Service Bus queue and then receiving them. For more samples on other and advanced scenarios, see Service Bus .NET samples on GitHub.

Create a console application
Start Visual Studio 2019.

Select Create a new project.

On the Create a new project dialog box, do the following steps: If you don't see this dialog box, select File on the menu, select New, and then select Project.

Select C# for the programming language.

Select Console for the type of the application.

Select Console Application from the results list.

Then, select Next.

Image showing the Create a new project dialog box with C# and Console selected

Enter QueueSender for the project name, ServiceBusQueueQuickStart for the solution name, and then select Next.

Image showing the solution and project names in the Configure your new project dialog box 

On the Additional information page, select Create to create the solution and the project.

Add the Service Bus NuGet package
Select Tools > NuGet Package Manager > Package Manager Console from the menu.

Run the following command to install the Azure.Messaging.ServiceBus NuGet package:

cmd

Install-Package Azure.Messaging.ServiceBus
Add code to send messages to the queue
In Program.cs, add the following using statements at the top of the namespace definition, before the class declaration.


using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
Within the Program class, declare the following properties, just before the Main method.

Replace <NAMESPACE CONNECTION STRING> with the primary connection string to your Service Bus namespace. And, replace <QUEUE NAME> with the name of your queue.



// connection string to your Service Bus namespace
static string connectionString = "<NAMESPACE CONNECTION STRING>";

// name of your Service Bus queue
static string queueName = "<QUEUE NAME>";

// the client that owns the connection and can be used to create senders and receivers
static ServiceBusClient client;

// the sender used to publish messages to the queue
static ServiceBusSender sender;

// number of messages to be sent to the queue
private const int numOfMessages = 3;

Replace code in the Main method with the following code. See code comments for details about the code. Here are the important steps from the code.

Creates a ServiceBusClient object using the primary connection string to the namespace.

Invokes the CreateSender method on the ServiceBusClient object to create a ServiceBusSender object for the specific Service Bus queue.

Creates a ServiceBusMessageBatch object by using the ServiceBusSender.CreateMessageBatchAsync method.

Add messages to the batch using the ServiceBusMessageBatch.TryAddMessage.

Sends the batch of messages to the Service Bus queue using the ServiceBusSender.SendMessagesAsync method.


static async Task Main()
{
    // The Service Bus client types are safe to cache and use as a singleton for the lifetime
    // of the application, which is best practice when messages are being published or read
    // regularly.
    //
    // Create the clients that we'll use for sending and processing messages.
    client = new ServiceBusClient(connectionString);
    sender = client.CreateSender(queueName);

    // create a batch 
    using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

    for (int i = 1; i <= numOfMessages; i++)
    {
        // try adding a message to the batch
        if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
        {
            // if it is too large for the batch
            throw new Exception($"The message {i} is too large to fit in the batch.");
        }
    }

    try
    {
        // Use the producer client to send the batch of messages to the Service Bus queue
        await sender.SendMessagesAsync(messageBatch);
        Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
    }
    finally
    {
        // Calling DisposeAsync on client types is required to ensure that network
        // resources and other unmanaged objects are properly cleaned up.
        await sender.DisposeAsync();
        await client.DisposeAsync();
    }

    Console.WriteLine("Press any key to end the application");
    Console.ReadKey();
}    

***************Here's what your Program.cs file should look like:**********************


using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace QueueSender
{
    class Program
    {
        // connection string to your Service Bus namespace
        static string connectionString = "<NAMESPACE CONNECTION STRING>";

        // name of your Service Bus queue
        static string queueName = "<QUEUE NAME>";

        // the client that owns the connection and can be used to create senders and receivers
        static ServiceBusClient client;

        // the sender used to publish messages to the queue
        static ServiceBusSender sender;

        // number of messages to be sent to the queue
        private const int numOfMessages = 3;

        static async Task Main()
        {
            // The Service Bus client types are safe to cache and use as a singleton for the lifetime
            // of the application, which is best practice when messages are being published or read
            // regularly.
            //
            // Create the clients that we'll use for sending and processing messages.
            client = new ServiceBusClient(connectionString);
            sender = client.CreateSender(queueName);

            // create a batch 
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            for (int i = 1; i <= numOfMessages; i++)
            {
                // try adding a message to the batch
                if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
                {
                    // if it is too large for the batch
                    throw new Exception($"The message {i} is too large to fit in the batch.");
                }
            }

            try
            {
                // Use the producer client to send the batch of messages to the Service Bus queue
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            Console.WriteLine("Press any key to end the application");
            Console.ReadKey();
        }
    }
}   
Replace <NAMESPACE CONNECTION STRING> with the primary connection string to your Service Bus namespace. And, replace <QUEUE NAME> with the name of your queue.

Build the project, and ensure that there are no errors.

Run the program and wait for the confirmation message.


A batch of 3 messages has been published to the queue
In the Azure portal, follow these steps:

Navigate to your Service Bus namespace.

On the Overview page, select the queue in the bottom-middle pane.

Image showing the Service Bus Namespace page in the Azure portal with the queue selected.

Notice the values in the Essentials section.

Image showing the number of messages received and the size of the queue

Notice the following values:

The Active message count value for the queue is now 3. Each time you run this sender app without retrieving the messages, this value increases by 3.
The current size of the queue increments each time the app adds messages to the queue.
In the Messages chart in the bottom Metrics section, you can see that there are three incoming messages for the queue.
Receive messages from the queue
In this section, you'll create a .NET Core console application that receives messages from the queue.

 Note
This quick start provides step-by-step instructions to implement a simple scenario of sending a batch of messages to a Service Bus queue and then receiving them. For more samples on other and advanced scenarios, see Service Bus .NET samples on GitHub.

Create a project for the receiver
In the Solution Explorer window, right-click the ServiceBusQueueQuickStart solution, point to Add, and select New Project.
Select Console application, and select Next.
Enter QueueReceiver for the Project name, and select Create.
In the Solution Explorer window, right-click QueueReceiver, and select Set as a Startup Project.
Add the Service Bus NuGet package
Select Tools > NuGet Package Manager > Package Manager Console from the menu.

In the Package Manager Console window, confirm that QueueReceiver is selected for the Default project. If not, use the drop-down list to select QueueReceiver.

Screenshot showing QueueReceiver project selected in the Package Manager Console

Run the following command to install the Azure.Messaging.ServiceBus NuGet package:

cmd

Install-Package Azure.Messaging.ServiceBus
Add the code to receive messages from the queue
In this section, you'll add code to retrieve messages from the queue.

In Program.cs, add the following using statements at the top of the namespace definition, before the class declaration.

using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
Within the Program class, declare the following properties, just before the Main method.

Replace <NAMESPACE CONNECTION STRING> with the primary connection string to your Service Bus namespace. And, replace <QUEUE NAME> with the name of your queue.


// connection string to your Service Bus namespace
static string connectionString = "<NAMESPACE CONNECTION STRING>";

// name of your Service Bus queue
static string queueName = "<QUEUE NAME>";

// the client that owns the connection and can be used to create senders and receivers
static ServiceBusClient client;

// the processor that reads and processes messages from the queue
static ServiceBusProcessor processor;
Add the following methods to the Program class to handle received messages and any errors.


// handle received messages
static async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"Received: {body}");

    // complete the message. message is deleted from the queue. 
    await args.CompleteMessageAsync(args.Message);
}

// handle any errors when receiving messages
static Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}
Replace code in the Main method with the following code. See code comments for details about the code. Here are the important steps from the code.

Creates a ServiceBusClient object using the primary connection string to the namespace.

Invokes the CreateProcessor method on the ServiceBusClient object to create a ServiceBusProcessor object for the specified Service Bus queue.

Specifies handlers for the ProcessMessageAsync and ProcessErrorAsync events of the ServiceBusProcessor object.

Starts processing messages by invoking the StartProcessingAsync on the ServiceBusProcessor object.

When user presses a key to end the processing, invokes the StopProcessingAsync on the ServiceBusProcessor object.

For more information, see code comments.


        static async Task Main()
        {
            // The Service Bus client types are safe to cache and use as a singleton for the lifetime
            // of the application, which is best practice when messages are being published or read
            // regularly.
            //

            // Create the client object that will be used to create sender and receiver objects
            client = new ServiceBusClient(connectionString);

            // create a processor that we can use to process the messages
            processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

            try
            {
                // add handler to process messages
                processor.ProcessMessageAsync += MessageHandler;

                // add handler to process any errors
                processor.ProcessErrorAsync += ErrorHandler;

                // start processing 
                await processor.StartProcessingAsync();

                Console.WriteLine("Wait for a minute and then press any key to end the processing");
                Console.ReadKey();

                // stop processing 
                Console.WriteLine("\nStopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }
        }
        
        
        
****************Here's what your Program.cs should look like:*************************


using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace QueueReceiver
{
    class Program
    {
        // connection string to your Service Bus namespace
        static string connectionString = "<NAMESPACE CONNECTION STRING>";

        // name of your Service Bus queue
        static string queueName = "<QUEUE NAME>";


        // the client that owns the connection and can be used to create senders and receivers
        static ServiceBusClient client;

        // the processor that reads and processes messages from the queue
        static ServiceBusProcessor processor;

        // handle received messages
        static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Received: {body}");

            // complete the message. messages is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        static async Task Main()
        {
            // The Service Bus client types are safe to cache and use as a singleton for the lifetime
            // of the application, which is best practice when messages are being published or read
            // regularly.
            //

            // Create the client object that will be used to create sender and receiver objects
            client = new ServiceBusClient(connectionString);

            // create a processor that we can use to process the messages
            processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

            try
            {
                // add handler to process messages
                processor.ProcessMessageAsync += MessageHandler;

                // add handler to process any errors
                processor.ProcessErrorAsync += ErrorHandler;

                // start processing 
                await processor.StartProcessingAsync();

                Console.WriteLine("Wait for a minute and then press any key to end the processing");
                Console.ReadKey();

                // stop processing 
                Console.WriteLine("\nStopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }
        }
    }
}
Replace <NAMESPACE CONNECTION STRING> with the primary connection string to your Service Bus namespace. And, replace <QUEUE NAME> with the name of your queue.

Build the project, and ensure that there are no errors.

Run the receiver application. You should see the received messages. Press any key to stop the receiver and the application.


Wait for a minute and then press any key to end the processing
Received: Message 1
Received: Message 2
Received: Message 3

Stopping the receiver...
Stopped receiving messages
Check the portal again. Wait for a few minutes and refresh the page if you don't see 0 for Active messages.

The Active message count and Current size values are now 0.

In the Messages chart in the bottom Metrics section, you can see that there are three incoming messages and three outgoing messages for the queue.

Active messages and size after receive

