using Snake.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Persistence
{
    public interface ISnakeDataAccess
    {
        Task<GridValue[,]> LoadAsync(string path);
        Task SaveAsync(string path, GameState gameState);

        int Size(string path);
    }
}
