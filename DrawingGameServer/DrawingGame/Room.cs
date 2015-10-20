﻿using DrawingGameServer.DrawingGame.Packet;
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

        public Player currentDrawingPlayer { get; set; }
        

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
                currentDrawingPlayer = p;
            }
        }

        public void RemovePlayer(Player p)
        {
            this.players.Remove(p);
            if (currentDrawingPlayer == p)
            {
                if (players.Any())
                {
                    currentDrawingPlayer = players.First();
                }
                else
                {
                    currentDrawingPlayer = null; //close room
                }
            }
        }

        public void Broadcast(Response data, Player source, params Player[] ignore)
        {
            if (data.MessageID == 10004) //drawing, only currently drawing player can do this
            {
                if (currentDrawingPlayer != source)
                {
                    return;
                }
            }
            foreach (Player p in players.Where(x => !ignore.Contains(x)))
            {
                p.SendMessage(data);
            }
        }
    }
}
