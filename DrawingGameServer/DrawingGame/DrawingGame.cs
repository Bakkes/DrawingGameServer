using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using log4net;
using SuperSocket.SocketBase.Logging;
using SuperSocket.WebSocket;
using DrawingGameServer.DrawingGame.Packet;
using Newtonsoft.Json;

namespace DrawingGameServer.DrawingGame
{
    public class DrawingGame
    {
        private static readonly log4net.ILog Logger = LogManager.GetLogger(typeof(DrawingGame));

        private bool active = false;
        private WebSocketServer websocketServer;
        private Encoding encoding;

        private List<WebSocketSession> sessions;
        private Dictionary<String, Player> players;
        private Dictionary<int, Room> rooms = new Dictionary<int, Room>();

        public DrawingGame()
        {
            this.encoding = new UTF8Encoding();
        }

        public void Setup()
        {
            RootConfig rootConfig = new RootConfig { DisablePerformanceDataCollector = true };
            this.sessions = new List<WebSocketSession>();
            this.players = new Dictionary<String, Player>();
            this.rooms = new Dictionary<int, Room>();

            this.rooms.Add(0, new Room { ID = 0, Name = "TestRoom" });

            this.websocketServer = new WebSocketServer();
            this.websocketServer.NewSessionConnected += new SessionHandler<WebSocketSession>(websocketServer_NewSessionConnected);
            this.websocketServer.SessionClosed += new SessionHandler<WebSocketSession, SuperSocket.SocketBase.CloseReason>(websocketServer_SessionClosed);

            ServerConfig config = new ServerConfig() {
                Name = "DrawingGame server",
                Port = 4444,
                Ip = "Any",
                MaxConnectionNumber = 1000
            };

            active = true;
            Logger.InfoFormat("Setting up {0} on {1}:{2}. Maximum number of connections: {3}", config.Name, config.Ip, config.Port, config.MaxConnectionNumber);

            bool success = this.websocketServer.Setup(rootConfig, config, null, null, new ConsoleLogFactory(), null, null);
            if (success)
            {
                Logger.InfoFormat("Success!");
            }
            else
            {
                Logger.InfoFormat("Setting up server failed");
                return;
            }

            this.websocketServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(websocketServer_NewMessageReceived);
        }

        private void websocketServer_NewMessageReceived(WebSocketSession session, string value)
        {
            Logger.InfoFormat("Received message {0}", value);
            Player currentPlayer = players[session.SessionID];

            Request request = JsonConvert.DeserializeObject<Request>(value);
            Logger.InfoFormat("Got message: {0}", request.Data);
            switch (request.MessageID)
            {
                case 0://ping, add pong later
                    break;
                case 1://list rooms
                    break;
                case 2://join room
                    int roomNumber = request.DataJson.ID;
                    if(!rooms.ContainsKey(roomNumber)) 
                    {
                        break;
                    }
                    rooms[roomNumber].AddPlayer(currentPlayer);
                    Logger.InfoFormat("Added player {0} to room {1}", currentPlayer.ID, roomNumber);
                    currentPlayer.CurrentRoom = rooms[roomNumber];
                    break;

                case 3:
                    break;

                case 4:
                    if (currentPlayer.CurrentRoom != null)
                    {
                        Response response = new Response
                        {
                            MessageID = 10004,
                            Data = request.DataJson
                        };
                        request.MessageID = 10004;
                        currentPlayer.CurrentRoom.Broadcast(response, currentPlayer);
                    }
                    break;
                case 5:
                    currentPlayer.CurrentRoom.RemovePlayer(currentPlayer);
                    currentPlayer.CurrentRoom = null;
                    break;


                default:
                    //players[session.SessionID].ReceiveMessage(value);
                    break;
            }
            
        }

        private void websocketServer_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason closeReason)
        {
            Logger.InfoFormat("Session from {0}, reason: ", session.Host, closeReason.ToString());
            Player currentPlayer = players[session.SessionID];
            sessions.Remove(session);
            currentPlayer.Disconnect();
            players.Remove(session.SessionID);
        }

        private void websocketServer_NewSessionConnected(WebSocketSession session)
        {
            Logger.InfoFormat("New session from {0}", session.Host);
            sessions.Add(session);
            Player p = new Player(session);
            players.Add(p.ID, p);
        }

        public void Serve()
        {
            websocketServer.Start();
            Logger.Info("Started serving...");
            while (active)
            {
                while (Console.ReadKey().KeyChar != 'q')
                {
                    Console.WriteLine();
                    continue;
                }
                active = false;
            }
        }

        public void Stop()
        {
            active = false;
            websocketServer.Stop();
        }
    }
}
