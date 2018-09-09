using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Core.Model
{
   public class Page
    {
        public int PageNumber { get; set; }
        public string PageUrl { get; set; }
        public byte[] PageBytes { get; set; }
    }
}
