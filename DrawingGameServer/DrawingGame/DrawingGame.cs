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

namespace DrawingGameServer.DrawingGame
{
    public class DrawingGame
    {
        private static readonly log4net.ILog Logger = LogManager.GetLogger(typeof(DrawingGame));

        private bool active = false;
        private WebSocketServer websocketServer;
        private Encoding encoding;

        private List<WebSocketSession> sessions;

        public DrawingGame()
        {
            this.encoding = new UTF8Encoding();
        }

        public void Setup()
        {
            RootConfig rootConfig = new RootConfig { DisablePerformanceDataCollector = true };
            this.sessions = new List<WebSocketSession>();
            this.websocketServer = new WebSocketServer();
            this.websocketServer.NewSessionConnected += new SessionHandler<WebSocketSession>(websocketServer_NewSessionConnected);
            this.websocketServer.SessionClosed += new SessionHandler<WebSocketSession, SuperSocket.SocketBase.CloseReason>(websocketServer_SessionClosed);

            ServerConfig config = new ServerConfig() {
                Name = "DrawingGame server",
                Port = 4444,
                Ip = "Any",
                MaxConnectionNumber = 100
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
        }

        private void websocketServer_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason closeReason)
        {
            Logger.InfoFormat("Session from {0}, reason: ", session.Host, closeReason.ToString());
            sessions.Remove(session);
        }

        private void websocketServer_NewSessionConnected(WebSocketSession session)
        {
            Logger.InfoFormat("New session from {0}", session.Host);
            sessions.Add(session);
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
