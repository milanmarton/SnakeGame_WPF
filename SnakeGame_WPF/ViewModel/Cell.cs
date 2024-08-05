using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame_WPF.ViewModel
{
    public class Cell : ViewModelBase
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int t;
        public int Type
        {
            get { return t; }
            set
            {
                if (t != value)
                {
                    t = value;
                    OnPropertyChanged();
                }
            }
        }

        public Cell(int X, int Y, int Value)
        {
            this.X = X;
            this.Y = Y;
            this.Type = Value;
        }
    }
}
