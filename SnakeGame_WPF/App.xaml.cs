using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Snake.Model;
using Snake.Persistence;
using SnakeGame_WPF.View;
using SnakeGame_WPF.ViewModel;

namespace SnakeGame_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private GameState _model = null!;
        private SnakeGameViewModel _viewModel = null!;
        private MainWindow _view = null!;
        private DispatcherTimer _timer = null!;

        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        private void App_Startup(object? sender, StartupEventArgs e)
        {
            // modell létrehozása
            _model = new GameState(new SnakeDataAccess());
            _model.GameOverEvent += new EventHandler<GameOverEventArgs>(Model_GameOver);
            _model.NewGame();

            // nézemodell létrehozása
            _viewModel = new SnakeGameViewModel(_model);
            _viewModel.NewGame += new EventHandler(ViewModel_NewGame);
            _viewModel.ExitGame += new EventHandler(ViewModel_ExitGame);

            // nézet létrehozása
            _view = new MainWindow();
            _view.KeyDown += new System.Windows.Input.KeyEventHandler(MainWindowKeydownHandler);
            _view.DataContext = _viewModel;
            _view.Closing += new System.ComponentModel.CancelEventHandler(View_Closing); // eseménykezelés a bezáráshoz
            _view.Show();

            // időzítő létrehozása
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(120);
            _timer.Tick += new EventHandler(Timer_Tick);
            // _timer.Start();
        }

        public void MainWindowKeydownHandler(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    _model.ChangeDirection(Direction.Up);
                    break;
                case Key.A:
                    _model.ChangeDirection(Direction.Left);
                    break;
                case Key.S:
                    _model.ChangeDirection(Direction.Down);
                    break;
                case Key.D:
                    _model.ChangeDirection(Direction.Right);
                    break;
                case Key.Space:
                    if (_timer.IsEnabled) { _timer.Stop(); }
                    else { _timer.Start(); }
                    break;
            }
        }

        private void ViewModel_NewGame(object? sender, EventArgs e)
        {
            _model.NewGame();
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _model.Move();
        }

        private void ViewModel_ExitGame(object? sender, System.EventArgs e)
        {
            _view.Close(); // ablak bezárása
        }

        private void View_Closing(object? sender, CancelEventArgs e)
        {
            Boolean restartTimer = _timer.IsEnabled;

            _timer.Stop();

            if (MessageBox.Show("Biztos, hogy ki akar lépni?", "Snake", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true; // töröljük a bezárást

                if (restartTimer) // ha szükséges, elindítjuk az időzítőt
                    _timer.Start();
            }
        }

        private void Model_GameOver(object? sender, GameOverEventArgs e)
        {
            _timer.Stop();

            if (e.isWon) // győzelemtől függő üzenet megjelenítése
            {
                MessageBox.Show("Gratulálok, győztél!" + Environment.NewLine +
                                "Összesen " + e.Score + " pontot értél el.",
                                "Kígyó játék",
                                MessageBoxButton.OK,
                                MessageBoxImage.Asterisk);
            }
            else
            {
                MessageBox.Show("Sajnálom, vesztettél!" + Environment.NewLine +
                                "Összesen " + e.Score + " pontot értél el.",
                                "Kígyó játék",
                                MessageBoxButton.OK,
                                MessageBoxImage.Asterisk);
            }
        }
    }

}
