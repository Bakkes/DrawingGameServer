using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawingGameServer.DrawingGame.Packet
{
    public class Request
    {
        public int MessageID = -1;

        public string Data;

        public dynamic DataJson
        {
            get
            {
                return JsonConvert.DeserializeObject<dynamic>(Data);
            }
        }
    }
}
