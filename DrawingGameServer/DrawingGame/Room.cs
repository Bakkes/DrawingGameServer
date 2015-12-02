using DrawingGameServer.DrawingGame.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawingGameServer.DrawingGame
{
    public class Room
    {
        public static readonly int CAPACITY = 16; //max players per room

        private int id;

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        private String name;

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        private Player currentDrawingPlayer;

        public Player CurrentDrawingPlayer
        {
            get
            {
                return currentDrawingPlayer;
            }
            set
            {
                
                if(value == null)
                {
                    return; //must always be someone, close room?
                }
                currentDrawingPlayer = value;
                Response resp = new Response()
                {
                    MessageID = 10008,
                    Data = new { Turn = true }
                };
                currentDrawingPlayer.SendMessage(resp);

                
                Broadcast(new Response()
                {
                    MessageID = 10008,
                    Data = new { Turn = false }
                }, currentDrawingPlayer, currentDrawingPlayer);
                Broadcast(new Response() { MessageID = 10009 }, currentDrawingPlayer);
            }
        }
        

        private List<Player> players;

        public List<Player> Players
        {
            get { return players; }
        }

        public Room()
        {
            players = new List<Player>();
        }

        public void AddPlayer(Player p)
        {
            this.players.Add(p);
            if (players.Count == 1)
            {
                CurrentDrawingPlayer = p;
            } else
            {
                p.SendMessage(new Response()
                {
                    MessageID = 10008,
                    Data = new { Turn = false }
                });
            }
        }

        public void RemovePlayer(Player p)
        {
            this.players.Remove(p);
            if (CurrentDrawingPlayer == p)
            {
                if (players.Any())
                {
                    CurrentDrawingPlayer = players.First();
                }
                else
                {
                    CurrentDrawingPlayer = null; //close room
                }
            }
        }

        public void Broadcast(Response data, Player source, params Player[] ignore)
        {
            if (data.MessageID == 10004) //drawing, only currently drawing player can do this
            {
                if (CurrentDrawingPlayer != source)
                {
                    return;
                }
            }
            Console.WriteLine(players.Where(x => !ignore.Contains(x)).Count());
            foreach (Player p in players.Where(x => !ignore.Contains(x)))
            {
                p.SendMessage(data);
            }
        }
    }
}
