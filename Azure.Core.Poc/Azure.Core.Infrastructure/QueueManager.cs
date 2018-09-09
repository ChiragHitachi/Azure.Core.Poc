
using Azure.Core.Model;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Core.Infrastructure
{

    public class QueueManager<T>
  {  
      public QueueManager(AzureQueueSettings settings)  
      {  
          this.settings = settings;  
          Init();  
      }  
  
      public async Task SendAsync(T item, Dictionary<string, object> properties)  
      {  
          var json = JsonConvert.SerializeObject(item);  
          var message = new Message(Encoding.UTF8.GetBytes(json));  
  
          if (properties != null)  
          {  
              foreach (var prop in properties)  
              {  
                  message.UserProperties.Add(prop.Key, prop.Value);  
              }  
          }  
  
          await client.SendAsync(message);  
      }  
  
      private AzureQueueSettings settings;  
      private QueueClient client;  
  
      private void Init()  
      {  
          client = new QueueClient(  
this.settings.ConnectionString, this.settings.QueueName);  
      }  

public void Receive(  
            Func<T, bool> onProcess,  
            Action<Exception> onError,  
            Action onWait)  
        {  
            var options = new MessageHandlerOptions(e =>  
            {  
                onError(e.Exception);  
                return Task.CompletedTask;  
            })  
            {  
                AutoComplete = false,  
                MaxAutoRenewDuration = TimeSpan.FromMinutes(1)  
            };  
   
            client.RegisterMessageHandler(  
                async (message, token) =>  
                {  
                    try  
                    {  
                        // Get message  
                        var data = Encoding.UTF8.GetString(message.Body);  
                        T item = JsonConvert.DeserializeObject<T>(data);  
   
                        // Process message  
                        var result = onProcess(item);  
   
                        if (result)  
                            await client.CompleteAsync(  
message.SystemProperties.LockToken);  
                        else
                            await client.DeadLetterAsync(  
message.SystemProperties.LockToken);  
   
                        // Wait for next message  
                        onWait();  
                    }  
                    catch (Exception ex)  
                    {  
                        await client.DeadLetterAsync(  
message.SystemProperties.LockToken);  
                        onError(ex);  
                    }  
                }, options);  
        }  

  } 
}