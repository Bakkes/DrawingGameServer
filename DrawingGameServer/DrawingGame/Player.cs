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

        public void SendJson(String json)
        {
            session.Send(json);
        }

        public void SendMessage(object message)
        {
            this.SendJson(JsonConvert.SerializeObject(message));
        }


    }
}
