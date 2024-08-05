using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Model
{
    public class GameOverEventArgs
    {
        public bool isWon { get; set; }
        public int Score { get; set; }

        public GameOverEventArgs(bool iswon, int score)
        {
            this.isWon = iswon;
            this.Score = score;
        }
    }
}
