using Moq;
using Snake.Model;
using Snake.Persistence;
using System.Threading.Tasks;

namespace Snake.Tests
{
    [TestClass]
    public class GameStateTests
    {
        private readonly string small = @"Maps\small.txt";
        //private readonly string medium = @"Maps\medium.txt";
        //private readonly string large = @"Maps\large.txt";
        private int rows, cols;
        private SnakeDataAccess _dataAccess = null!;
        private GameState _gameState = null!;
        //private GridValue[,] _gridValue = null!;

        [TestInitialize]
        public void Initialize()
        {
            _dataAccess = new SnakeDataAccess();
            rows = _dataAccess.Size(small);
            cols = rows;
            _gameState = new GameState(_dataAccess);
        }

        [TestMethod]
        public void GameStateInitializationTest()
        {
            // Verify initial conditions of the game state
            Assert.AreEqual(Direction.Right, _gameState.Dir);
            Assert.AreEqual(0, _gameState.Score);
            Assert.IsFalse(_gameState.GameOver);
            // Add more assertions as needed
        }

        [TestMethod]
        public void AddSnakePositionsSnakeCorrectly()
        {
            // Assuming the snake is added in the middle row starting from column 1
            int middleRow = _gameState.Rows / 2;
            for (int i = 1; i <= 5; i++) // Assuming snake length is 5
            {
                Assert.AreEqual(GridValue.Snake, _gameState.Grid[middleRow, i],
                                $"Snake segment should be present at ({middleRow}, {i}).");
            }
        }

        [TestMethod]
        public void AddEggPlacesEggOnGrid()
        {
            var eggCount = _gameState.Grid.Cast<GridValue>().Count(value => value == GridValue.Egg);
            Assert.AreEqual(1, eggCount, "One egg should be placed on the grid.");
        }

        [TestMethod]
        public void MoveUpdatesSnakePosition()
        {
            var initialHeadPosition = _gameState.HeadPosition();
            _gameState.Move();
            var newHeadPosition = _gameState.HeadPosition();
            Assert.AreNotEqual(initialHeadPosition, newHeadPosition,
                               "Snake's head position should change after moving.");
        }

        [TestMethod]
        public void ChangeDirectionUpdatesDirection()  // ha nem szembe levo irany
        {
            var newDirection = Direction.Up;
            _gameState.ChangeDirection(newDirection);
            // Move the snake to apply the direction change
            _gameState.Move();
            Assert.AreEqual(newDirection, _gameState.Dir,
                            "Direction should change to the new direction.");
        }

        [TestMethod]
        public void ChangeDirectionUpdatesDirectionOpposite() // ha szembelevo irany akkor nem valtozik
        {
            var newDirection = Direction.Left;
            _gameState.ChangeDirection(newDirection);
            // Move the snake to apply the direction change
            _gameState.Move();
            Assert.AreNotEqual(newDirection, _gameState.Dir,
                            "Direction should change to the new direction.");
        }

        [TestMethod]
        public void GameOverMoveMethodTest()
        {
            _gameState.ChangeDirection(Direction.Right);

            for (int i = 0; i <= 9; i++)
            {
                _gameState.Move();
            }

            Assert.IsTrue(_gameState.GameOver, "The game should be over after a nine moves on a 15*15 map.");
        }

        [TestMethod]
        public async Task NewGameResetsGameState()
        {
            _gameState.Move();

            await _gameState.NewGame();

            Assert.AreEqual(0, _gameState.Score, "Score should be reset to 0.");
            Assert.IsFalse(_gameState.GameOver, "Game over should be reset to false.");
        }

        [TestMethod]
        public void SnakePositionsReturnsAllPositions()
        {
            var positions = _gameState.SnakePositions();

            Assert.IsNotNull(positions, "Snake positions should not be null.");
            Assert.IsTrue(positions.Any(), "There should be at least one position in snake positions.");
        }

    }
}