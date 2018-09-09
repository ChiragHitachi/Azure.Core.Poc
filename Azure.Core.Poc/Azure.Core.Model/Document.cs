using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Core.Model
{
    public class Document
    {
        public string BlobUrl { get; set; }
        public Guid BlobId { get; set; }
        public FileFormat FileFormat { get; set; }
        public List<Document> ChildDocument { get; set; }
        public List<Page> Pages { get; set; }
    }
}
