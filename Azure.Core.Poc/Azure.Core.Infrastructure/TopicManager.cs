using Azure.Core.Model;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Core.Infrastructure{
    public class TopicManager<T>
    {
        AzureTopicSetting topicSetting;
        public TopicManager(AzureTopicSetting setting){
            this.topicSetting = setting;    
        }

         public async Task<bool> Send(string subject, T data){

             var credentials = new TopicCredentials(this.topicSetting.TopicKey);
             var client = new EventGridClient(credentials);
            var eventGridEvent = new EventGridEvent
            {
                Subject = subject,
                EventType = "func-event",
                EventTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                Data = data,
                DataVersion = "1.0.0"
            };

             var events = new List<EventGridEvent>();
             events.Add(eventGridEvent);
             await client.PublishEventsWithHttpMessagesAsync(this.topicSetting.TopicEndPoint, events);
             return true;
             
         }
    }

   
}