using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Core.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Azure.Core.Functions
{
    public static class StartWorkflowFunction
    {
        [FunctionName("StartWorkflowFunction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req,
             [OrchestrationClient]DurableOrchestrationClient starter,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            RequestPayload data = await req.Content.ReadAsAsync<RequestPayload>();

            log.Info($"About to start orchestration for {data}");

            var orchestrationId = await starter.StartNewAsync("ProcessRequestOrchestrator", data);

            return starter.CreateCheckStatusResponse(req, orchestrationId);

        }
    }
}
