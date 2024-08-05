using Snake.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Model
{
    public class GameChangedEventArgs
    {
        public int score { get; set; }
        public GameState state { get; set; }

        public GameChangedEventArgs(int score, GameState state)
        {
            this.score = score;
            this.state = state;
        }   
    }
}
