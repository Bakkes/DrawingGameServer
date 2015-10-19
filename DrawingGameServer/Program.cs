using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrawingGameServer.DrawingGame;
using log4net.Config;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace DrawingGameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            BasicConfigurator.Configure();
            DrawingGame.DrawingGame game = new DrawingGame.DrawingGame();
            game.Setup();
            game.Serve();
            game.Stop();
        }
    }
}
