namespace Pentagon
{
    internal class GameBoard // ігрове поле у логічному представленні
    {
        private readonly int[,] board;
        public int Size { get; private set; } = 12;
        private const int EmptyCell = 0;    // порожня клітинка
        private const int ObstacleCell = -1; // перешкода


        public GameBoard()
        {
            board = new int[Size, Size];
            InitializeBoard();
        }

        public GameBoard(GameBoard board1)  // конструктор копіювання 
        {
            board = new int[board1.Size, board1.Size];
            for(int i = 0; i < board1.Size; i++)
            {
                for (int j = 0; j < board1.Size; j++)
                    board[i, j] = board1[i, j];                                 
            }
        }

        private void InitializeBoard() // ініціалізація поля
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    board[row, col] = EmptyCell;
                }
            }
        }

        public int this[int row, int col]  // індексатор
        {
            get => board[row, col];
            set => board[row, col] = value;
        }      
       

        public bool IsWithinBounds(int row, int col) // метод для перевірки того, чи знаходиться клітинка у межах поля
        {
            return (row >= 0 && row < Size && col >= 0 && col < Size);
        }

        public bool IsEmpty(int row, int col) // перевірка на те, чи є клітинка порожньою
        {
            return board[row, col] == EmptyCell;
        }

        public void Clear() => InitializeBoard(); // очищення поля

        public bool CanPlaceFigure(Figure figure, int baseRow, int baseCol) //перевірка на те, чи можна встановити фігуру
        {
            foreach (var point in figure.GetPoints())
            {
                int row = baseRow + point.Y;
                int col = baseCol + point.X;

                if (!IsWithinBounds(row, col)) //чи у межах
                    return false;

                if (!IsEmpty(row, col)) // чи не зайнята клітинка
                    return false;

                // Перевірка на дотик до інших фігур 
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dy == 0 && dx == 0) continue;

                        int nRow = row + dy;
                        int nCol = col + dx;

                        if (IsWithinBounds(nRow, nCol) && board[nRow, nCol] >= 1)
                            return false;
                    }
                }
            }
            return true;
        }

        public bool CanPlaceFigure(List<IntPoint> blocks, int offsetRow, int offsetCol)   // також перевірка можливості встановлення фігури на поле
        {
            foreach (var block in blocks)
            {
                int row = offsetRow + block.Y;
                int col = offsetCol + block.X;

                if (!IsWithinBounds(row, col))
                    return false;

                if (!IsEmpty(row, col))
                    return false;

                // Перевірка на дотик до інших фігур
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dy == 0 && dx == 0) continue;

                        int nRow = row + dy;
                        int nCol = col + dx;

                        if (IsWithinBounds(nRow, nCol) && board[nRow, nCol] >= 1)
                            return false;
                    }
                }
            }
            return true;
        }


        public void PlaceFigure(Figure figure, int baseRow, int baseCol) // метод для встановлення фігури на поле
        {
            foreach (var point in figure.GetPoints())
            {
                int row = baseRow + point.Y;
                int col = baseCol + point.X;
                board[row, col] = figure.Id;
            }
        }

        public void PlaceFigure(List<IntPoint> blocks, int offsetRow, int offsetCol, int id) // ще один метод для встановлення фігури на поле
        {
            foreach (var block in blocks)
            {
                int row = offsetRow + block.Y;
                int col = offsetCol + block.X;
                board[row, col] = id;
            }
        }
        public void RemoveFigure(List<IntPoint> blocks, int offsetRow, int offsetCol) // прибирання фігури з поля
        {
            foreach (var block in blocks)
            {
                int row = offsetRow + block.Y;
                int col = offsetCol + block.X;
                board[row, col] = EmptyCell;
            }
        }
        public void GenerateObstacles()  // метод для генерації перешкод
        {
            Random rand = new Random();
            for (int row = 0; row < Size; row++)
            {
                // Збираємо всі вільні клітинки у рядку
                var freeCols = new List<int>();
                for (int col = 0; col < Size; col++)
                {
                    if (board[row, col] == EmptyCell)
                        freeCols.Add(col);
                }
                while (freeCols.Count > 0)
                {
                    int index = rand.Next(freeCols.Count);
                    int chosenCol = freeCols[index];

                    if (CanPlaceObstacle(row, chosenCol))
                    {
                        PlaceObstacle(row, chosenCol);
                        break; // перешкода поставлена, виходимо з while
                    }
                    else
                    {
                        freeCols.RemoveAt(index); // виключаємо цю клітинку зі списку, бо вона не підходить
                    }
                }

            }
        }
        public void LeaveOnlyObstacles() // метод, аби лишити лише перешкоди та порожні клітинки на полі
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (board[row, col] != ObstacleCell)
                        board[row, col] = EmptyCell;
                }
            }
        }

        private bool CanPlaceObstacle(int row, int col) // перевірка на те, чи можливо поставити перешкоду
        {
            if (row == 0) // якщо перший рядок, то ставимо
                return true;
            else
            {
                for (int dx = -1; dx <= 1; dx++) 
                {
                    if (IsWithinBounds(row - 1, col + dx) && board[row - 1, col + dx] == ObstacleCell) // перевіряємо усі клітинки, що знаходяться поруч у сусідньому верхньому рядку
                        return false;
                }
            }
            return true;
        }

        private void PlaceObstacle(int row, int col) // метод для встановлення перешкоди
        {
            board[row, col] = ObstacleCell;
        }
    }
}
