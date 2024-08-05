using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Snake.Model;
using Snake.Persistence;
using static System.Formats.Asn1.AsnWriter;
using System.Diagnostics;

namespace SnakeGame_WPF.ViewModel
{
    public class SnakeGameViewModel : ViewModelBase
    {
        private GameState _model;

        public int BoardRows { get; set; }
        public int BoardColumns { get; set; }

        public DelegateCommand NewGameCommand { get; private set; }
        public DelegateCommand ExitCommand { get; private set; }
        public ObservableCollection<Cell> Map { get; set; }
        public int GameScore
        {
            get { return _model.Score; }
            set
            {
                if (_model.Score != value)
                {
                    _model.Score = value;
                    OnPropertyChanged(nameof(GameScore));
                }
            }
        }


        public event EventHandler? NewGame;
        public event EventHandler? ExitGame;
        public Boolean IsMapSmall
        {
            get { return _model._mapSize == GameState.MapSize.Small; }
            set
            {
                if (_model._mapSize == GameState.MapSize.Small)
                    return;

                _model._mapSize = GameState.MapSize.Small;
                OnPropertyChanged(nameof(IsMapSmall));
                OnPropertyChanged(nameof(IsMapMedium));
                OnPropertyChanged(nameof(IsMapLarge));
            }
        }

        public Boolean IsMapMedium
        {
            get { return _model._mapSize == GameState.MapSize.Medium; }
            set
            {
                if (_model._mapSize == GameState.MapSize.Medium)
                    return;

                _model._mapSize = GameState.MapSize.Medium;
                OnPropertyChanged(nameof(IsMapSmall));
                OnPropertyChanged(nameof(IsMapMedium));
                OnPropertyChanged(nameof(IsMapLarge));
            }
        }

        public Boolean IsMapLarge
        {
            get { return _model._mapSize == GameState.MapSize.Large; }
            set
            {
                if (_model._mapSize == GameState.MapSize.Large)
                    return;

                _model._mapSize = GameState.MapSize.Large;
                OnPropertyChanged(nameof(IsMapSmall));
                OnPropertyChanged(nameof(IsMapMedium));
                OnPropertyChanged(nameof(IsMapLarge));
            }
        }


        public SnakeGameViewModel(GameState model)
        {
            _model = model;
            _model.GameChangedEvent += new EventHandler<GameChangedEventArgs>(Model_GameChanged);

            NewGameCommand = new DelegateCommand(param => OnNewGame());
            ExitCommand = new DelegateCommand(param => OnExitGame());

            Map = new ObservableCollection<Cell>();
            Init(0);
        }

        private void OnNewGame()
        {
            NewGame?.Invoke(this, EventArgs.Empty);
            Init(0);
        }

        private void Init(int xyz)
        {
            BoardRows = _model.Rows;
            BoardColumns = _model.Cols;
            Map.Clear();
            for (int r = 0; r < _model.Rows; r++)
            {
                for (int c = 0; c < _model.Cols-1; c++)
                {
                    Map.Add(new Cell(r,c, xyz));
                }
                    Map.Add(new Cell(r, _model.Cols - 1, -1));
            }
            OnPropertyChanged(nameof(Map));
            OnPropertyChanged(nameof(BoardRows));
            OnPropertyChanged(nameof(BoardColumns));

        }
        private void Model_GameChanged(object? sender, GameChangedEventArgs e)
        {
            foreach (Cell cell in Map)
                {
                cell.Type = e.state.enumToNumber(cell.Y, cell.X);
                // Debug.WriteLine("celltype" + cell.Type + " x:" + cell.X + " y:" + cell.Y );
            }
            OnPropertyChanged(nameof(Map));
            OnPropertyChanged(nameof(GameScore));
        }


        /* private static Brush GridColor(GridValue gridValue)
        {
            switch (gridValue)
            {
                case GridValue.Snake:
                    return Brushes.Green;
                case GridValue.Egg:
                    return Brushes.Red;
                case GridValue.Empty:
                    return Brushes.LightGray;
                case GridValue.Outside:
                    return Brushes.Black;
                default:
                    return Brushes.White;
            }
        }*/



        private void OnExitGame()
        {
            ExitGame?.Invoke(this, EventArgs.Empty);
        }
    }
}
