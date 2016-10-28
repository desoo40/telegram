using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportfortCrawler
{
    public class Event
    {
        public string Type { get; set; }
        public string Data { get; set; }
        public List<string> MembersBe { get; set; }
        public List<string> MembersMayBe { get; set; }
        public List<string> MembersNotBe { get; set; }
    }
}
