namespace Pentagon
{
    internal class Solver
    {
        private readonly List<List<List<IntPoint>>> _allVariants;  //усі ротації фігур
        private List<List<List<IntPoint>>> _uniqueVariants; // ротації фігур у рандомізованому порядку
        private GameBoard _board;
        private int[] _figureOrder;  // порядок розміщення фігур на полі
        private const int maxTries = 5000000;  // максимальна кількість спроб розставити фігуру

        public Solver(List<List<List<IntPoint>>> allVariants, GameBoard board)
        {
            _allVariants = allVariants;
            _board = board;
            int count = allVariants.Count;
            _uniqueVariants = new List<List<List<IntPoint>>>(count);
            _figureOrder = GenerateRandomOrder(count);
        }
        // генерація порядку додавання фігур на поле
        private int[] GenerateRandomOrder(int count) 
        {
            Random rand = new Random();
            int[] order = new int[count];
            for (int i = 0; i < count; i++)
                order[i] = i + 1;

            for (int i = count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                int temp = order[j];
                order[j] = order[i];
                order[i] = temp;
            }
            return order;
        }

        // метод для гарантованого розв'язку головоломки
        public bool TrySolveWithRestart()
        {
            long tries = 0;
            bool solved = false;
            while (!solved)
            {
                tries = 0;
                _board.Clear();
                int count = _allVariants.Count;
                _figureOrder = GenerateRandomOrder(count);
                _uniqueVariants.Clear();
                foreach (int index in _figureOrder)
                    _uniqueVariants.Add(_allVariants[index - 1]);
                solved = Solve(0, ref tries, maxTries);
            }
            return solved;
        }
        // основний метод розв'язку головоломки (бектрекінг з рекурсією)
        public bool Solve(int figureIndex, ref long tries, long maxTries)
        {
            if (tries > maxTries)
                return false;

            if (figureIndex == _uniqueVariants.Count)                        
                return true;
            
            var figureVariants = _uniqueVariants[figureIndex];
            int figureId = _figureOrder[figureIndex];

            for (int row = 0; row < _board.Size; row++)
            {
                for (int col = 0; col < _board.Size; col++)
                {
                    foreach (var rotation in figureVariants)
                    {
                        tries++;
                        if (tries > maxTries)
                            return false;

                        if (_board.CanPlaceFigure(rotation, row, col))
                        {
                            _board.PlaceFigure(rotation, row, col, figureId);

                            if (Solve(figureIndex + 1, ref tries, maxTries))
                                return true;
                            _board.RemoveFigure(rotation, row, col);
                        }
                    }
                }
            }
            return false;
        }

        public GameBoard GiveSolvedField() => _board;    
       
    }
}
