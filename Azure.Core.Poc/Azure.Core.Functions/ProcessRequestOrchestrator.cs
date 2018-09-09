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
    public static class ProcessRequestOrchestrator
    {
        [FunctionName("ProcessRequestOrchestrator")]
        public static async Task<object> ProcessRequest([OrchestrationTrigger] DurableOrchestrationContext ctx, TraceWriter log)
        {
            var requestPayload = ctx.GetInput<RequestPayload>();

            if (!ctx.IsReplaying)
                log.Info("About to call Process Document Orchestrator");

            var documents = await ctx.CallSubOrchestratorAsync<Document[]>("ProcessDocumentOrchestrator", requestPayload);

            //if (!ctx.IsReplaying)
            //    log.Info("About to call extract thumbanil activity");

            //var thumbnailLocation = await ctx.CallActivityAsync<string>("A_ExtractThumbnail", transcodedLocation);

            //if (!ctx.IsReplaying)
            //    log.Info("About to call prepend intro activity");

            //var withIntroLocation = await ctx.CallActivityAsync<string>("A_PrependIntro", transcodedLocation);

            return new
            {
                Documets = documents
            };
        }
    }
}
