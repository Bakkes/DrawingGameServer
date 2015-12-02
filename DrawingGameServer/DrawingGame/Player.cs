using DrawingGameServer.DrawingGame.Packet;
using log4net;
using Newtonsoft.Json;
using SuperSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawingGameServer.DrawingGame
{
    public delegate void MessageReceived(string message);
    public class Player
    {
        private static readonly log4net.ILog Logger = LogManager.GetLogger(typeof(DrawingGame));

        public String Name { get { return ID; } }

        public MessageReceived OnMessageReceived;
        public String ID { get { return session.SessionID; } }
        public WebSocketSession Session { get { return session; } }

        private WebSocketSession session;
        public Room CurrentRoom { get; set; }

        public Player(WebSocketSession session)
        {
            this.session = session;
        }

        public void ReceiveMessage(string message)
        {
            if (OnMessageReceived != null)
            {
                OnMessageReceived(message);
            }
        }

        public void JoinRoom(Room room)
        {
            if (CurrentRoom != null)
            {
                LeaveRoom();
            }

            CurrentRoom = room;
            CurrentRoom.AddPlayer(this);
        }

        public void LeaveRoom()
        {
            if (CurrentRoom == null)
            {
                return;
            }
            CurrentRoom.RemovePlayer(this);
            CurrentRoom = null;
        }

        public long UnixTimeNow()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }

        public void SendText(String text)
        {
            SendMessage(new Response()
            {
                MessageID = 10003,
                Data = new { author = "GAME", timestamp = UnixTimeNow(), text = text }

            });
        }

        public void SendJson(String json)
        {
            session.Send(json);
        }

        public void SendMessage(object message)
        {
            string jsonMsg = JsonConvert.SerializeObject(message);
            Logger.Info(jsonMsg);
            this.SendJson(jsonMsg);
        }


    }
}
