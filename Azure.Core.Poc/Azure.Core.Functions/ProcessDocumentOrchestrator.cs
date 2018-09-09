using System;
using System.Collections.Generic;
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
    public static class ProcessDocumentOrchestrator
    {
        [FunctionName("ProcessDocumentOrchestrator")]
        public static async Task<Document[]> ProcessRequest([OrchestrationTrigger] DurableOrchestrationContext ctx, TraceWriter log)
        {
            var requestPayload = ctx.GetInput<RequestPayload>();

            if (!ctx.IsReplaying)
                log.Info("About to call Document Details Activity");

            List<Task<Document>> documentDetailsTasks = new List<Task<Document>>();

            foreach (Guid blobId in requestPayload.Blobs)
            {
                var documentDetailsTask = ctx.CallActivityAsync<Document>("DocumentDetailsActivity", blobId);
                documentDetailsTasks.Add(documentDetailsTask);
            }

            var documentDetailsResult = await Task.WhenAll(documentDetailsTasks);

            if (!ctx.IsReplaying)
                log.Info("About to call Document Conversion Activity");

            List<Task<Document>> documentConversionTasks = new List<Task<Document>>();

            foreach (Document document in documentDetailsResult)
            {
                var documentConversionTask = ctx.CallActivityAsync<Document>("DocumentConversionActivity", document);
                documentConversionTasks.Add(documentConversionTask);
            }

            var documentConversionsResult = await Task.WhenAll(documentConversionTasks);


            if (!ctx.IsReplaying)
                log.Info("About to call Split Document Pages Activity");

            List<Task<Document>> splitDocumentPagesTasks = new List<Task<Document>>();

            foreach (Document document in documentConversionsResult)
            {
                var splitDocumentPagesTask = ctx.CallActivityAsync<Document>("SplitDocumentPagesActivity", document);
                splitDocumentPagesTasks.Add(splitDocumentPagesTask);
            }

            var splitDocumentPagesResult = await Task.WhenAll(splitDocumentPagesTasks);

            return splitDocumentPagesResult;
        }
    }
}
