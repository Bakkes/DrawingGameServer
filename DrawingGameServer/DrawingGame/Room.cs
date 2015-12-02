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
        public String CurrentWord { get; set; }
        public bool Active
        {
            get
            {
                return Players.Count() >= 2 && DrawQueue.Any();
            }
        }
        public static readonly int CAPACITY = 16; //max players per room

        private List<Player> drawQueue;

        private List<Player> DrawQueue
        {
            get { return drawQueue; }
            set { drawQueue = value; }
        }

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


        public Player CurrentDrawingPlayer
        {
            get
            {
                return DrawQueue[0];
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
            DrawQueue = new List<Player>();
        }

        public void addPlayerToQueue(Player p)
        {
            if (DrawQueue.Contains(p))
            {
                return;
            }

            DrawQueue.Add(p);

            BroadcastQueue();

            BroadcastText(p.Name + " joined the drawing queue, number " + DrawQueue.IndexOf(p));

            if(Players.Count() < 2)
            {
                BroadcastText("Cannot start game yet, need atleast 2 players");
            }
            else if(CurrentWord == null)
            {
                NextPerson();
            }
        }

        public void NextPerson()
        {
            if(CurrentWord == null)
            {
                BroadcastText("Starting a new game.");
            }
            else
            {
                DrawQueue.RemoveAt(0);
            }

            if (DrawQueue.Any())
            {
                Broadcast(new Response()
                {
                    MessageID = 10009,
                    Data = null
                }, CurrentDrawingPlayer);
                BroadcastText(CurrentDrawingPlayer.Name.Substring(0, 6) + " is up!");
                CurrentWord = RandomWordGenerator.instance.GetRandomWord();
                CurrentDrawingPlayer.SendText("The word you have to draw is " + CurrentWord + ", good luck!");
            }
            else
            {
                CurrentWord = null;
            }
            BroadcastQueue();
        }

        public void PlayerSaidText(Player currentPlayer, Request request)
        {
            Response response = new Response
            {
                MessageID = 10003,
                Data = request.DataJson
            };
            response.Data.author = currentPlayer.Name;

            Broadcast(response, currentPlayer, currentPlayer);

            if (CurrentWord != null)
            {
                string text = request.DataJson.text;
                if (text.ToLower().Equals(CurrentWord.ToLower()))
                {
                    BroadcastText(currentPlayer + " guessed the word, the word was: " + CurrentWord + "!");
                    NextPerson();
                }
            }

        }

        public void AddPlayer(Player p)
        {
            this.players.Add(p);
            if (!DrawQueue.Any() && players.Count() > 1)
            {
                NextPerson();
            }
            p.SendText(String.Format("You joined room {0}", name));
            BroadcastText(p.Name + " joined to room!");
        }

        public void SendQueueToPlayer(Player player)
        {
            player.SendMessage(new Response()
            {
                MessageID = 10011,
                Data = new
                {
                    MyTurn = DrawQueue.Contains(player) ? DrawQueue.IndexOf(player) : -1,
                    Queue = DrawQueue.Select((x, i) => new { Turn = i, ID = x.ID, Name = x.Name.Substring(0, 6) })
                }
            });
        }

        public void RemovePlayer(Player p)
        {
            this.players.Remove(p);
            if(DrawQueue.Contains(p))
            {
                if (CurrentDrawingPlayer.Equals(p) && DrawQueue.Count() != 1)
                {
                    NextPerson();
                }
                else
                {
                    DrawQueue.Remove(p); //resend queuelist
                    BroadcastQueue();
                }

            }
        }

        void BroadcastQueue()
        {
            foreach (Player player in Players)
            {
                SendQueueToPlayer(player);
            }
        }



        void BroadcastText(string text)
        {
            foreach (Player player in Players)
            {
                player.SendText(text);
            }
        }

        public void Broadcast(Response data, Player source, params Player[] ignore)
        {
            if (data.MessageID == 10004 || data.MessageID == 10009) //drawing, only currently drawing player can do this
            {
                if (DrawQueue.Any() && CurrentDrawingPlayer != source)
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
