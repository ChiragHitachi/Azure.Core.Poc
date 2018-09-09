
using System;

namespace Azure.Core.Model
{
public class AzureTopicSetting  
 {  
     public AzureTopicSetting(string topicEndPoint, string topicKey)  
     {  
         if (string.IsNullOrEmpty(topicEndPoint))  
             throw new ArgumentNullException("topicEndPoint");  
  
         if (string.IsNullOrEmpty(topicKey))  
             throw new ArgumentNullException("topicKey");  
  
         this.TopicEndPoint = topicEndPoint;  
         this.TopicKey = topicKey;  
     }  
          
     public string TopicEndPoint { get; }  
     public string TopicKey { get; }  
 }
} 

 