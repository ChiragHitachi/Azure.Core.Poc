using Azure.Core.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Core.Functions
{
   public static class ProcessDocumentActivities
    {
        [FunctionName("DocumentDetailsActivity")]
        public static async Task<Document> GetDocumentDetails([ActivityTrigger]Guid blobId, TraceWriter log)
        {
            log.Info($"BlobId {blobId}");

            //Simulating getting blob details
            await Task.Delay(1000);

            return new Document() { BlobId = blobId,
                BlobUrl = "http://www.someserver.com/" + blobId.ToString(),
                FileFormat = FileFormat.ZIP };
        }

        [FunctionName("DocumentConversionActivity")]
        public static async Task<Document> GetConvertedDocument([ActivityTrigger]Document document, TraceWriter log)
        {
            log.Info($"Document {document}");

            //Simulating getting blob details
            await Task.Delay(1000);

            document.ChildDocument = new List<Document>() { new Document() { BlobId = Guid.NewGuid(),
                BlobUrl = "http://www.someserver.com/Document_1.pdf",
                FileFormat = FileFormat.PDF },
                new Document() { BlobId = Guid.NewGuid(),
                BlobUrl = "http://www.someserver.com/Document_2.pdf",
                FileFormat = FileFormat.PDF }
            };

            return document;
        }

        [FunctionName("SplitDocumentPagesActivity")]
        public static async Task<Document> GetDocumentPages([ActivityTrigger]Document document, TraceWriter log)
        {
            log.Info($"Document {document}");

            //Simulating getting blob details
            await Task.Delay(1000);

            List<Task<Document>> splitDocumentPagesTasks = new List<Task<Document>>();

            foreach (Document childDocument in document.ChildDocument)
            {
                var splitDocumentPagesTask = ProcessDocumentActivities.GetPages(childDocument);
                splitDocumentPagesTasks.Add(splitDocumentPagesTask);
            }

            var splitDocumentPagesResult = await Task.WhenAll(splitDocumentPagesTasks);

            document.ChildDocument = splitDocumentPagesResult.ToList();

            return document;
        }

        private static async Task<Document> GetPages(Document document)
        {
            //simulating document page split.
            await Task.Delay(100);
            document.Pages = new List<Page>();

            document.Pages.Add(new Page() { PageNumber = 1, PageUrl = "http://www.document.com/"+ document.BlobId + "_page1.pdf" });
            document.Pages.Add(new Page() { PageNumber = 2, PageUrl = "http://www.document.com/" + document.BlobId + "_page2.pdf" });
            document.Pages.Add(new Page() { PageNumber = 3, PageUrl = "http://www.document.com/" + document.BlobId + "_page3.pdf" });
            document.Pages.Add(new Page() { PageNumber = 4, PageUrl = "http://www.document.com/" + document.BlobId + "_page4.pdf" });

            return document;
        }
    }
}
