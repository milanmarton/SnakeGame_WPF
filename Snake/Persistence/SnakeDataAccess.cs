using Snake.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Persistence
{
    public class SnakeDataAccess : ISnakeDataAccess
    {
        public SnakeDataException SnakeDataException
        {
            get => default;
            set
            {
            }
        }

        public ISnakeDataAccess ISnakeDataAccess
        {
            get => default;
            set
            {
            }
        }

        public async Task<GridValue[,]> LoadAsync(string path)
        {
            GridValue[,] Grid;
            try
            {
                using (StreamReader reader = new StreamReader(path)) // fájl megnyitása
                {
                    string line = await reader.ReadLineAsync() ?? string.Empty;
                    int n = int.Parse(line);
                    Grid = new GridValue[n,n];

                    for (int r = 0; r < n; r++)
                    {
                        line = await reader.ReadLineAsync() ?? string.Empty;
                        string[] blocks = line.Split(' ');

                        for (int c = 0; c < n; c++)
                        {
                            if (Int16.Parse(blocks[c]) == 0) { Grid[r, c] = Model.GridValue.Empty; }
                            else if (Int16.Parse(blocks[c]) == 1) { Grid[r, c] = Model.GridValue.Snake; }
                            else if (Int16.Parse(blocks[c]) == 2) { Grid[r, c] = Model.GridValue.Egg; }
                            else if (Int16.Parse(blocks[c]) == -1) { Grid[r, c] = Model.GridValue.Outside; }
                        }
                    }

                    return Grid;
                }
            }
            catch { throw new SnakeDataException(); }
        }


        public async Task SaveAsync(string path, GameState gameState)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    await writer.WriteLineAsync(gameState.Rows.ToString()); // mivel n*n -es a pálya

                    for (int r = 0; r < gameState.Rows; r++)
                    {
                        for (int c = 0; c < gameState.Cols; c++)
                        {
                            await writer.WriteAsync(gameState.Grid[r, c] + " ");
                        }
                        await writer.WriteLineAsync();
                    }
                }
            }
            catch { throw new SnakeDataException(); }
        }

        public int Size(string path)
        {
            int n;
            try
            {
                using (StreamReader reader = new StreamReader(path)) // fájl megnyitása
                {
                    string line = reader.ReadLine() ?? string.Empty;
                    n = int.Parse(line);
                    return n;
                }
            }
            catch { throw new SnakeDataException(); }
        }
    }
}
