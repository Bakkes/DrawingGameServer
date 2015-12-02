using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawingGameServer.DrawingGame
{
    class RandomWordGenerator
    {
        private static RandomWordGenerator _instance;

        public static RandomWordGenerator instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new RandomWordGenerator();
                }
                return _instance;
            }
        }
        string[] lines;
        private RandomWordGenerator ()
        {
            lines = File.ReadAllLines("wordsEn.txt");
        }
        static int index = 0;
        static string[] words = { "cat", "dog", "mrb", "lol" };
        public string GetRandomWord()
        {
            
            var r = new Random();
            var randomLineNumber = r.Next(0, lines.Length - 1);
            var line = lines[randomLineNumber];
            return line;//return words[index++ % words.Length];
        }

    }
}
