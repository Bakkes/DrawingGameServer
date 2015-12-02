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
        private Room Lobby { get { return rooms[-1]; } }

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
            this.rooms.Add(0, new Room { ID = 0, Name = "Main room" });
            this.rooms.Add(1, new Room { ID = 1, Name = "TestRoom" });
            this.rooms.Add(2, new Room { ID = 2, Name = "TestRoom2" });
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
            //Logger.InfoFormat("Received message {0}", value);
            Player currentPlayer = players[session.SessionID];

            Request request = JsonConvert.DeserializeObject<Request>(value);
            Response response;
            Logger.InfoFormat("Got message: {0}", request.Data);
            switch (request.MessageID)
            {
                case 0://ping, add pong later
                    break;
                case 1://list rooms
                    response = new Response
                    {
                        MessageID = 10001,
                        Data = rooms.Select(x => new { ID = x.Value.ID, Name = x.Value.Name, Players = x.Value.Players.Count, MaxPlayers = Room.CAPACITY })
                    };
                    currentPlayer.SendMessage(response);
                    break;
                case 2://join room
                    currentPlayer.LeaveRoom();
                    int resp = joinRoom(currentPlayer, (int)request.DataJson.ID);
                    response = new Response
                    {
                        MessageID = 10002,
                        Data = new { Status = resp }
                    };
                    currentPlayer.SendMessage(response);

                    currentPlayer.CurrentRoom.SendQueueToPlayer(currentPlayer);
                    break;

                case 3: //say
                    if (currentPlayer.CurrentRoom != null)
                    {
                        currentPlayer.CurrentRoom.PlayerSaidText(currentPlayer, request);

                    }
                    break;

                case 4: //figure
                    if (currentPlayer.CurrentRoom != null)
                    {
                        response = new Response
                        {
                            MessageID = 10004,
                            Data = request.DataJson
                        };
                        currentPlayer.CurrentRoom.Broadcast(response, currentPlayer, currentPlayer);
                    }
                    break;
                case 5:
                    currentPlayer.LeaveRoom();
                    break;
                case 6: //undo
                    if (currentPlayer.CurrentRoom != null)
                    {
                        response = new Response()
                        {
                            MessageID = 10006,
                            Data = request.DataJson
                        };
                        currentPlayer.CurrentRoom.Broadcast(response, currentPlayer, currentPlayer);
                    }
                    break;

                case 7: //redo
                    if (currentPlayer.CurrentRoom != null)
                    {
                        response = new Response()
                        {
                            MessageID = 10007,
                        };
                        currentPlayer.CurrentRoom.Broadcast(response, currentPlayer, currentPlayer);
                    }
                    break;
                case 9:
                    currentPlayer.CurrentRoom.Broadcast(new Response()
                    {
                        MessageID = 10009,
                        Data = null
                    }, currentPlayer, currentPlayer);
                    break;
                case 10: //enqueue
                    currentPlayer.CurrentRoom.addPlayerToQueue(currentPlayer);
                    break;
                case 11:
                    currentPlayer.CurrentRoom.SendQueueToPlayer(currentPlayer);
                    break;
                default:
                    //players[session.SessionID].ReceiveMessage(value);
                    break;
            }
        }

        

        int joinRoom(Player currentPlayer, int roomNumber)
        {

            if (!rooms.ContainsKey(roomNumber))
            {
                return 2001; //Room doesn't exist
            }
            Room room = rooms[roomNumber];
            if (room.Players.Count() >= Room.CAPACITY)
            {
                return 2002; //room is full
            }
            currentPlayer.JoinRoom(rooms[roomNumber]);

            Logger.InfoFormat("Added player {0} to room {1}", currentPlayer.ID, roomNumber);
            return 2000;
        }

        private void websocketServer_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason closeReason)
        {
            Logger.InfoFormat("Session from {0}, reason: ", session.Host, closeReason.ToString());
            Player currentPlayer = players[session.SessionID];
            sessions.Remove(session);
            currentPlayer.LeaveRoom();
            players.Remove(session.SessionID);
        }

        private void websocketServer_NewSessionConnected(WebSocketSession session)
        {
            Logger.InfoFormat("New session from {0}", session.Host);
            sessions.Add(session);
            Player p = new Player(session);
            players.Add(p.ID, p);
            //p.JoinRoom(Lobby);
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
