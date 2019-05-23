using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class MyInformation
    {
        public string myId { get; set; }
        public string myChannel { get; set; }
        public string myChannelIdx { get; set; }
        public string mybeforeChannel { get; set; }

        public MyInformation(string id, string channel, string channelidx, string beforechannel)
        {
            myId = id;                          
            myChannel = channel;                
            myChannelIdx = channelidx;          
            mybeforeChannel = beforechannel;    
        }
    }
}
