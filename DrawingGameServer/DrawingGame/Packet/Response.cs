using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawingGameServer.DrawingGame.Packet
{
    public class Response
    {
        public int MessageID = -1;

        public object Data;
    }
}
