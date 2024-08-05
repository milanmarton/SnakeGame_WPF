using Snake.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Enumeration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Snake.Persistence
{
    public class GameState
    {
        private readonly string small = @"Maps\small.txt";
        private readonly string medium = @"Maps\medium.txt";
        private readonly string large = @"Maps\large.txt";
        public enum MapSize { Small, Medium, Large }

        public MapSize _mapSize { get; set; } = MapSize.Small;
        private ISnakeDataAccess _dataAccess;
        public int Rows { get; private set; }
        public int Cols { get; private set; }
        public GridValue[,] Grid { get; private set; }
        public Direction Dir { get; private set; }
        public int Score { get; set; }
        public bool GameOver { get; private set; }
        private LinkedList<Position> snakePositions = new LinkedList<Position>(); // linkedlist elejéről és végéről is tudunk törölni
        private Random random = new Random();
        private readonly int INITIAL_SNAKELENGTH = 5;  // kagyó alaphossz
        private LinkedList<Direction> dirChanges = new LinkedList<Direction>(); // előre lehessen mozogni 2-t

        public event EventHandler<GameOverEventArgs> GameOverEvent;
        public event EventHandler<GameChangedEventArgs> GameChangedEvent;

        public int enumToNumber(int x, int y)
        {
            switch (Grid[y,x])
            {
                case GridValue.Empty: return 0;
                case GridValue.Egg: return 1;
                case GridValue.Snake: return 2;
                    default: return -1;
            }
        }

        public GameState(ISnakeDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
            Rows = dataAccess.Size(small);
            Cols = Rows;
            _mapSize = MapSize.Small;
            Grid = new GridValue[Rows, Cols];
            Dir = Direction.Right;

            AddSnake();
            AddEgg();
            //GameChangedEvent?.Invoke(this, new GameChangedEventArgs(Score, new GameState(_dataAccess)));
        }

        private void AddSnake()
        {
            int m = Rows / 2; // középső sor

            for (int c = 1; c <= INITIAL_SNAKELENGTH; c++) // középső sorba a második oszloptól a hatodikig Snake-é tesszük a gridet
            {
                Grid[m, c] = GridValue.Snake;  // ezzel
                snakePositions.AddFirst(new Position(m, c));  // a snakePositions-ben is eltároljuk ezeket a pozíciókat
            }
        }

        private IEnumerable<Position> EmptyPositions()  // megnézzük hol üres a pálya (nem tartózkodik rajta snake)
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);
                    }
                }
            }
        }

        private void AddEgg()
        {
            List<Position> empty = new List<Position>(EmptyPositions());  // listát csinálunk az üres pályahelyekből, így biztosan olyan helyre kerül a tojás, ahol nincsen snake meg más se
            if (empty.Count == 0)  // ha a játékos megnyeri a játékot, akkor nem marad üres hely
            {
                GameOverEvent?.Invoke(this, new GameOverEventArgs(true, Score)); // nincs több hely tojásnak => nyert a játékos
                return;
            }

            Position pos = empty[random.Next(empty.Count)];  // kiválasztunk egy random helyet
            Grid[pos.Row, pos.Col] = GridValue.Egg;  // tojássá tesszük a random helyet
        }

        public Position HeadPosition()  // a kígyó fejének pozíciója
        {
            return snakePositions.First.Value;
        }
        public Position TailPosition()  // a kígyó farkának pozíciója
        {
            return snakePositions.Last.Value;
        }

        public IEnumerable<Position> SnakePositions()  // a kígyó összes pozíciója
        {
            return snakePositions;
        }

        private void AddHead(Position pos)  // új fej adása a kíygyónak (előrehaladás)
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.Snake;
        }
        private void RemoveTail()  // kagyó farkának eltüntetése (hossz csökkentése)
        {
            Position tail = snakePositions.Last.Value;
            Grid[tail.Row, tail.Col] = GridValue.Empty;
            snakePositions.RemoveLast();
        }

        public void ChangeDirection(Direction dir)  // irányváltoztatás
        {
            if (CanChangeDirection(dir))
            {
                dirChanges.AddLast(dir);
            }
        }

        private Direction GetLastDirection()
        {
            if (dirChanges.Count == 0) { return Dir; }
            return dirChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDir)
        {
            if (dirChanges.Count == 2) { return false; }

            Direction lastDir = GetLastDirection();
            return newDir != lastDir && newDir != lastDir.Opposite();
        }

        private bool OutsideGrid(Position pos)  // a pozíció pályán kívüli-e? (fal)
        {
            return pos.Row < 0 || pos.Col < 0 || pos.Row >= Rows || pos.Col >= Cols;
        }

        private GridValue WillHit(Position newHeadPos)  // az új pozíciója a kígyó fejének mibe ütközne?
        {
            if (OutsideGrid(newHeadPos)) // pályán kívül / fal
            {
                return GridValue.Outside;
            }
            if (newHeadPos == TailPosition()) // ha a kígyó feje a jelenlegi farka helyére menne, azt engedélyezzük (a farka hamarabb mozdul mint a feje tegyük fel)
            {
                return GridValue.Empty;
            }

            return Grid[newHeadPos.Row, newHeadPos.Col];
        }

        public void Move()
        {
            // Debug.WriteLine("lala");
            if (dirChanges.Count > 0)
            {
                Dir = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }

            Position newHeadPos = HeadPosition().Transition(Dir);
            GridValue hit = WillHit(newHeadPos);

            if (hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;
                GameOverEvent?.Invoke(this, new GameOverEventArgs(false, Score)); // vesztett a játékos
            }
            else if (hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPos);
            }
            else if (hit == GridValue.Egg)
            {
                AddHead(newHeadPos);
                Score++;
                AddEgg();
            }
            GameChangedEvent?.Invoke(this, new GameChangedEventArgs(Score, this));
        }

        public async Task NewGame()
        {
            _dataAccess = new SnakeDataAccess();
            Dir = Direction.Right;
            snakePositions = new LinkedList<Position>();
            random = new Random();
            dirChanges = new LinkedList<Direction>();
            Score = 0;
            GameOver = false;
            int n;
            switch (_mapSize)
            {
                case MapSize.Small:
                    n = _dataAccess.Size(small);
                    Grid = new GridValue[n, n];
                    Rows = n;
                    Cols = n;
                    Grid = await _dataAccess.LoadAsync(small);
                    break;
                case MapSize.Medium:
                    n = _dataAccess.Size(medium);
                    Grid = new GridValue[n, n];
                    Rows = n;
                    Cols = n;
                    Grid = await _dataAccess.LoadAsync(medium);
                    break;
                case MapSize.Large:
                    n = _dataAccess.Size(large);
                    Grid = new GridValue[n, n];
                    Rows = n;
                    Cols = n;
                    Grid = await _dataAccess.LoadAsync(large);
                    break;
            }
            AddSnake();
            AddEgg();
            GameChangedEvent?.Invoke(this, new GameChangedEventArgs(Score, this));
        }

        public void setMapSize(MapSize m)
        {
            _mapSize = m;
        }

        public GameOverEventArgs GameOverEventArgs
        {
            get => default;
            set
            {
            }
        }

        public GameChangedEventArgs GameChangedEventArgs
        {
            get => default;
            set
            {
            }
        }

        public Position Position
        {
            get => default;
            set
            {
            }
        }

        public Direction Direction
        {
            get => default;
            set
            {
            }
        }

        /*public async Task SaveGameAsync(String path)
        {
            if (_dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            await _dataAccess.SaveAsync(path, this);
        }*/
    }
}
