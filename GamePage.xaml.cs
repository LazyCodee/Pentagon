using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Pentagon
{
    public partial class GamePage : Page
    {
        private readonly bool _enableBarriers;
        private bool isDragging = false;        
        private Canvas? draggedFigureCanvas = null;
        private Figure? draggedFigure = null;
        private GameBoard gameBoard;       
        private Solver _solver;
        private List<TextBlock> rowHints = new();
        private List<TextBlock> columnHints = new();
        private List<Rectangle> previewRects = new();  
        private Stack<Move> moveHistory = new Stack<Move>();   // стек для зберігання інформації про додані на поле фігури
        private Stack<int> _userPlacedFiguresIds = new Stack<int>();  // стек для зберігання id фішур, що були додані на поле користувачем
        private int _placedFigures = 0;   // к-сть встановлених фігур
        private bool _isSolved = false; // чи було хоч раз встановлено всі фігури на поле
        public GamePage(bool enableBarriers)
        {
            InitializeComponent();           
            InitializeGameField();
            GenerateFigurePanel();
            DoSolving();
            _enableBarriers = enableBarriers;
            InitializeLogicalBoard();                  
        }
        private void GenerateFigurePanel()  //генерація області з фігурами
        {
            for (int id = 1; id <= 12; id++)
                RemoveFigureFromPanelById(id);
            List<Figure> figures = AllFigures.CreateAllFigures();
            for (int i = 0; i < figures.Count; i++)            
               AddFigureToPanel(figures, i);            
        }
        private void InitializeGameField()  // генерація ігрового поля
        {
            gameBoard = new GameBoard();
            GameFieldGrid.RowDefinitions.Clear();
            GameFieldGrid.ColumnDefinitions.Clear();
            GameFieldGrid.Children.Clear();
            rowHints.Clear();
            columnHints.Clear();

            // Створюємо 13 рядків і 13 стовпців
            for (int i = 0; i < gameBoard.Size + 1; i++)
            {
                GameFieldGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
                GameFieldGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            }

            // Основне поле 12x12
            for (int row = 0; row < gameBoard.Size; row++)
            {
                for (int col = 0; col < gameBoard.Size; col++)
                {
                    var cell = new Border
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(0.5),
                        Background = Brushes.White
                    };

                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);
                    GameFieldGrid.Children.Add(cell);                   
                }
            }
            // клітинки для підрахунку зайнятих клітинок у рядку
            for (int row = 0; row < gameBoard.Size; row++)
            {
                var hint = new TextBlock
                {
                    Text = "0",
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(hint, row);
                Grid.SetColumn(hint, gameBoard.Size);
                GameFieldGrid.Children.Add(hint);
                rowHints.Add(hint);
            }
            // клітинки для підрахунку зайнятих клітинок у стовпці
            for (int col = 0; col < gameBoard.Size; col++)
            {
                var hint = new TextBlock
                {
                    Text = "0",
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(hint, gameBoard.Size);
                Grid.SetColumn(hint, col);
                GameFieldGrid.Children.Add(hint);
                columnHints.Add(hint);
            }

            // Нижній правий кут 
            var corner = new Border
            {
                Background = Brushes.LightGray
            };
            Grid.SetRow(corner, gameBoard.Size);
            Grid.SetColumn(corner, gameBoard.Size);
            GameFieldGrid.Children.Add(corner);

        }
        // створення контроллера для керування фігурами
        private UIElement CreateFigureControl(Figure figure)
        {
           int blockSize = figure.BlockSize;

            Canvas canvas = new Canvas
            {
                Width = 5 * blockSize,
                Height = 5 * blockSize,
                Margin = new Thickness(3),
                Tag = figure
            };

            canvas.MouseLeftButtonDown += FigureCanvas_MouseLeftButtonDown;
            canvas.MouseMove += FigureCanvas_MouseMove;
            canvas.MouseLeftButtonUp += FigureCanvas_MouseLeftButtonUp;

            canvas.MouseRightButtonDown += (s, e) =>
            {
                if (s is Canvas c && c.Tag is Figure f)
                {
                    f.RotateClockwise();
                    RedrawFigure(c, f);
                }

            };
            RedrawFigure(canvas, figure);

            return canvas;
        }
        private void RedrawFigure(Canvas canvas, Figure figure) // перемалювання фігури (після повороту)
        {
            int blockSize = figure.BlockSize;
            canvas.Children.Clear();
            double minX = figure.Blocks.Min(p => p.X);
            double minY = figure.Blocks.Min(p => p.Y);

            foreach (IntPoint block in figure.Blocks)
            {
                Rectangle rect = new Rectangle
                {
                    Width = blockSize,
                    Height = blockSize,
                    Fill = Brushes.MediumSlateBlue,  //Колір фігури 
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.4                    
                };

                Canvas.SetLeft(rect, (block.X - minX) * blockSize);
                Canvas.SetTop(rect, (block.Y - minY) * blockSize);
                canvas.Children.Add(rect);
            }
        }                    

        private void FigureCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) // якщо натиснуто ЛКМ на фігуру
        {
            if (sender is Canvas canvas && canvas.Tag is Figure figure)
            {
                draggedFigureCanvas = canvas;
                draggedFigure = figure;
                isDragging = true;              
                canvas.CaptureMouse();
                Cursor = Cursors.Hand;  // зміна курсору на тип "Рука"
            }
        }

        private void FigureCanvas_MouseMove(object sender, MouseEventArgs e) // якщо натиснуто на фігуру і йде рух курсором
        {
            if (!isDragging || draggedFigureCanvas == null || draggedFigure == null) return;

            Point position = e.GetPosition(GameFieldGrid);
            int cellSize = draggedFigure.BlockSize;
            int row = (int)(position.Y / cellSize);
            int col = (int)(position.X / cellSize);
            ClearPreview();
            if (col >= 0 && col < 12 && row >= 0 && row < 12)
            {
                bool isInside = true;

                // Перевіряємо, чи всі точки фігури в межах
                foreach (IntPoint p in draggedFigure.Blocks)
                {
                    int x = col + p.X;
                    int y = row + p.Y;
                    if (x < 0 || x >= 12 || y < 0 || y >= 12)
                    {
                        isInside = false;
                        break;
                    }
                }

                // Перевіряємо, чи можна поставити фігуру (тільки якщо вона в межах)
                bool canPlace = isInside && gameBoard.CanPlaceFigure(draggedFigure.Blocks, row, col);

                // Визначаємо колір заливки (червоний або синій)
                SolidColorBrush fillBrush = (!isInside || !canPlace)
                    ? new SolidColorBrush(Color.FromArgb(128, 255, 0, 0))  // червоний
                    : new SolidColorBrush(Color.FromArgb(128, 100, 149, 237)); // синій
                foreach (IntPoint p in draggedFigure.Blocks)
                {
                    int x = col + p.X;
                    int y = row + p.Y;

                    if (x < 0 || x >= 12 || y < 0 || y >= 12) continue;

                    Rectangle previewRect = new Rectangle
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Fill = fillBrush,    //колір відображення фігури
                        Stroke = Brushes.Gray,
                        StrokeThickness = 0.4
                    };

                    Grid.SetRow(previewRect, y);
                    Grid.SetColumn(previewRect, x);
                    GameFieldGrid.Children.Add(previewRect);
                    previewRects.Add(previewRect);
                }
            }            
        }

        private void FigureCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) // якщо припинено зажаття ЛКМ
        {
            if (!isDragging || draggedFigureCanvas == null || draggedFigure == null || gameBoard == null)
                return;            

            Point dropPosition = e.GetPosition(GameFieldGrid);
            int cellSize = draggedFigure.BlockSize;
            int row = (int)(dropPosition.Y / cellSize);
            int col = (int)(dropPosition.X / cellSize);            

            if (col >= 0 && col < 12 && row >= 0 && row < 12)
            {
                if (gameBoard.CanPlaceFigure(draggedFigure, row, col)) // якщо можна встановити фігуру, то встановлюємо
                {
                    gameBoard.PlaceFigure(draggedFigure, row, col);
                    RenderFigureOnBoard(draggedFigure, row, col, cellSize); //відобразити фігуру на полі
                    RemoveFigureFromPanel(draggedFigure);                   // прибрати фігуру з панелі
                    moveHistory.Push(new Move(draggedFigure, row, col));    //додати запис до стеку доданих фігур
                    _userPlacedFiguresIds.Push(draggedFigure.Id);           // додати id фігури до стеку фігур, доданих користувачем
                    _placedFigures++;
                    UpdatePlaceSolveAvailability();
                    UpdateUndoButtonState();
                    UpdateHints();
                }               
            }
            isDragging = false;
            draggedFigureCanvas.ReleaseMouseCapture();
            draggedFigureCanvas = null;
            draggedFigure = null;
            ClearPreview();
            CheckIfSolved();
            Cursor = Cursors.Arrow; // зміна курсору на звичайний
        }
        private void ClearPreview()
        {
            foreach (var rect in previewRects)
            {
                GameFieldGrid.Children.Remove(rect);
            }
            previewRects.Clear();
        }        
        private void BackToMenuBtn_Click(object sender, RoutedEventArgs e) // робота кнопки для повернення у стартове меню
        {
            MessageBoxResult result = MessageBox.Show(                      // очікування підтвердження від користувача
               "Do you want to return to the menu? Current game will be lost.",
               "Return to Menu",
               MessageBoxButton.YesNo,
               MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes && Application.Current.MainWindow is MainWindow mw)
                mw.MainFrame.Navigate(new StartMenu());
        }                   

        private void UndoButton_Click(object sender, RoutedEventArgs e) // робота кнопки Undo
        {
            UndoLastMove();
        }        
        private void PlaceButton_Click(object sender, RoutedEventArgs e) // робота кнопки Place (встановлення однієї фігури на поле)
        {
            if (_solver == null || _placedFigures >= 12) return;

            int targetId = _placedFigures + 1;
            var coords = FindFigureCoordinates(_solver.GiveSolvedField(), targetId);
            if (coords == null) return;

            foreach (var point in coords)
                gameBoard[point.Y, point.X] = targetId;

            RenderFigureFromAbsoluteCoords(coords); // генеруємо фігуру на полі
            RemoveFigureFromPanelById(targetId);    // видаляємо фігуру з панелі
            var relativePoints = coords.Select(p => new IntPoint(p.X - coords[0].X, p.Y - coords[0].Y)).ToList();
            var figure = new Figure(targetId, relativePoints, 30);
            moveHistory.Push(new Move(figure, coords[0].Y, coords[0].X)); //робимо запис у стек
            _placedFigures++;
            UpdateHints();
            UpdateUndoButtonState();
            UpdatePlaceSolveAvailability();
            CheckIfSolved();
        }
        private void SolveButton_Click(object sender, RoutedEventArgs e) // робота кнопки Solve (встановлення усіх фігур на поле)
        {
            if (_solver == null) return;
            foreach (var move in moveHistory)  // прибираємо усі фігури, що були раніше поставлені на поле
                RemoveFigureFromBoard(move.Figure.Id); 
            _userPlacedFiguresIds.Clear();
            moveHistory.Clear();
            gameBoard = new GameBoard(_solver.GiveSolvedField()); // копіюємо поле розв’язку            
            var rendered = new HashSet<int>();   // множина id фігур, що вже були згенеровані на полі
            var movesToPush = new List<(int id, List<IntPoint> coords)>();
            for (int i = 0; i < gameBoard.Size; i++)
            {
                for (int j = 0; j < gameBoard.Size; j++)
                {
                    int id = gameBoard[i, j];
                    if (id >= 1 && id <= 12 && !rendered.Contains(id))
                    {
                        var coords = FindFigureCoordinates(gameBoard, id);
                        if (coords != null)
                        {
                            movesToPush.Add((id, coords));
                            rendered.Add(id);
                        }                                                 
                    }
                }
            }
            foreach (var (id, coords) in movesToPush.OrderBy(m => m.id))
            {
                RenderFigureFromAbsoluteCoords(coords);               // генеруємо фігуру на полі
                RemoveFigureFromPanelById(id);                        // видаляємо фігуру з панелі

                var relativePoints = coords
                    .Select(p => new IntPoint(p.X - coords[0].X, p.Y - coords[0].Y))
                    .ToList();

                var figure = new Figure(id, relativePoints, 30);              
                moveHistory.Push(new Move(figure, coords[0].Y, coords[0].X)); // додаємо запис до стеку

            }
            _placedFigures = 12; // всі фігури вже використані
            UpdateHints();
            UpdateUndoButtonState();
            UpdatePlaceSolveAvailability();
            CheckIfSolved();
        }      

        private void RestartButton_Click(object sender, RoutedEventArgs e) // робота кнопки Restart
        {
            ResetGame();
        }
        private void UndoLastMove() // скасування останнього хожу
        {         
            Move lastMove = moveHistory.Pop();    // беремо інформацію про останню додану фігуру
            RemoveFigureFromBoard(lastMove.Figure.Id);    // Видаляємо фігуру з поля           
            AddFigureToPanel(lastMove.Figure);            // Додаємо назад у панель фігур
            if (_userPlacedFiguresIds.Count > 0 && _userPlacedFiguresIds.Peek() == lastMove.Figure.Id) // якщо id фігури є у іншому стеку, то видалити його
                _userPlacedFiguresIds.Pop();
            _placedFigures--;
            UpdateUndoButtonState();
            UpdateHints();
            UpdatePlaceSolveAvailability();
        }

        private void RemoveFigureFromBoard(int figureId) // прибирання фігури з поля за id
        {
            if (gameBoard == null) return;
            var toRemove = new List<UIElement>();  // збираємо список елементів для видалення
            foreach (UIElement child in GameFieldGrid.Children)
            {
                if (child is Rectangle rect && rect.Fill == Brushes.MediumSlateBlue)  //пошук прямокутників для видалення фігури
                {
                    int row = Grid.GetRow(rect);
                    int col = Grid.GetColumn(rect);
                    if (gameBoard[row, col] == figureId)   // якщо знайдено клітинку, зайняту цією фігурою
                    {
                        toRemove.Add(rect);   // додати прямокутник до списку елементів для видалення
                        gameBoard[row, col] = 0;
                    }
                }
            }
            foreach (var elem in toRemove)             // видалення усіх прямокутників
                GameFieldGrid.Children.Remove(elem);

        }

        private void AddFigureToPanel(List<Figure> figs, int index) // метод для додавання фігури на понель
        {
            UIElement figureControl = CreateFigureControl(figs[index]);
            int row = index / 4;
            int col = index % 4;
            Grid.SetRow(figureControl, row);
            Grid.SetColumn(figureControl, col);
            FigureGrid.Children.Add(figureControl);
        }
        private void AddFigureToPanel(Figure figure) // перевантажений метод додавання фігури на панель
        {
            UIElement figureControl = CreateFigureControl(figure);
            int index = figure.Id - 1;
            int row = index / 4;
            int col = index % 4;
            Grid.SetRow(figureControl, row);
            Grid.SetColumn(figureControl, col);
            FigureGrid.Children.Add(figureControl);
        }
        private void UpdateUndoButtonState() // оновлення стану кнопки Undo
        {
            UndoButton.IsEnabled = moveHistory.Count > 0;
        }
        private void UpdateHints() // оновлення значення заповнених клітинок у рядку/стовпці
        {
            if (gameBoard == null) return;

            for (int row = 0; row < 12; row++)
            {
                int count = 0;
                for (int col = 0; col < 12; col++)
                {
                    int value = gameBoard[row, col];
                    if (value >= 1) count++;
                }
                rowHints[row].Text = count.ToString();
            }

            for (int col = 0; col < 12; col++)
            {
                int count = 0;
                for (int row = 0; row < 12; row++)
                {
                    int value = gameBoard[row, col];
                    if (value >= 1) count++;
                }
                columnHints[col].Text = count.ToString();
            }
        }
        private void UpdatePlaceSolveAvailability() // оновлення стану кнопок Place та Solve 
        {
            bool isUserModified = _userPlacedFiguresIds.Count > 0;
            bool isBoardFull = moveHistory.Count == 12;
            // кнопки доступні, якщо ще не було розміщено жодної фігури користувачем, а також не усі фігури на полі
            PlaceButton.IsEnabled = !isUserModified && !isBoardFull && _solver != null;
            SolveButton.IsEnabled = !isUserModified && !isBoardFull && _solver != null;
        }
        private void CheckIfSolved()  // перевірка на те, чи всі фігури є на полі
        {
            if (moveHistory.Count == 12 && !_isSolved)
            {
                MessageBox.Show(                      // повідомлення для користувача
                     "Congratulations! All figures are placed!",
                     "Victory",
                      MessageBoxButton.OK,
                      MessageBoxImage.Information);
                _isSolved = true;
            }
        }

        private void RenderFigureOnBoard(Figure figure, int row, int col, int blockSize) // створення зображення фігури на полі
        {
            foreach (var point in figure.Blocks)
            {
                int cellRow = row + point.Y;
                int cellCol = col + point.X;

                Rectangle rect = new Rectangle
                {
                    Width = 30,
                    Height = 30,
                    Fill = Brushes.MediumSlateBlue, // колір блоку
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.4
                };
                Grid.SetRow(rect, cellRow);
                Grid.SetColumn(rect, cellCol);
                GameFieldGrid.Children.Add(rect);
            }
        }
        private void RenderFigureFromAbsoluteCoords(List<IntPoint> coords) // генерація фігури на полі через координати її блоків
        {
            foreach (var point in coords)
            {
                Rectangle rect = new Rectangle
                {
                    Width = 30,
                    Height = 30,
                    Fill = Brushes.MediumSlateBlue,    //колір блоку
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.4
                };

                Grid.SetRow(rect, point.Y);
                Grid.SetColumn(rect, point.X);
                GameFieldGrid.Children.Add(rect);
            }
        }
        private void RemoveFigureFromPanel(Figure figure) // видалити фігуру з панелі
        {
            UIElement elementToRemove = null;
            foreach (UIElement elem in FigureGrid.Children)
            {
                if (elem is Canvas canvas && canvas.Tag is Figure f && f.Id == figure.Id) // якщо знайдено фігуру для вмдалення
                {
                    elementToRemove = elem;
                    break;
                }
            }

            if (elementToRemove != null)
                FigureGrid.Children.Remove(elementToRemove);
        }
        private void RemoveFigureFromPanelById(int id)
        {
            UIElement elementToRemove = null;
            foreach (UIElement elem in FigureGrid.Children)
            {
                if (elem is Canvas canvas && canvas.Tag is Figure f && f.Id == id)
                {
                    elementToRemove = elem;
                    break;
                }
            }

            if (elementToRemove != null)
                FigureGrid.Children.Remove(elementToRemove);
        }
        private List<IntPoint> FindFigureCoordinates(GameBoard board, int figureId) // метод для знаходження координат блоків фігури на полі
        {
            var result = new List<IntPoint>();
            for (int row = 0; row < board.Size; row++)
            {
                for (int col = 0; col < board.Size; col++)
                {
                    if (board[row, col] == figureId)
                        result.Add(new IntPoint(col, row));
                }
            }
            return result.Count == 5 ? result : null;
        }
        private void DoSolving()  // процес пошуку розв'язку
        {
            _solver = new Solver(AllFigures.CreateAllFiguresRotations(), new GameBoard(gameBoard));
            _solver.TrySolveWithRestart();
            PlaceButton.IsEnabled = true;
            SolveButton.IsEnabled = true;
        }      
        private void DrawObstacle(int x, int y) // розташування перешкод на полі
        {
            var rect = new Rectangle
            {
                Width = 30,
                Height = 30,
                Fill = Brushes.Gray,
                Opacity = 0.6
            };
            Grid.SetColumn(rect, x);
            Grid.SetRow(rect, y);
            GameFieldGrid.Children.Add(rect);
        }
        private void InitializeLogicalBoard() // ініціалізація логічного поля
        {
            var tempBoard = new GameBoard(_solver.GiveSolvedField());
            if (_enableBarriers)
            {
                tempBoard.GenerateObstacles();
                tempBoard.LeaveOnlyObstacles();
                for (int row = 0; row < tempBoard.Size; row++)
                {
                    for (int col = 0; col < tempBoard.Size; col++)
                    {
                        if (tempBoard[row, col] == -1)
                            DrawObstacle(col, row);
                    }
                }
            }
            else
                tempBoard.Clear();
            gameBoard = tempBoard;
        }

        private void ResetGame() // повернення до початкового стану гри
        {
            InitializeGameField();
            GenerateFigurePanel();
            InitializeLogicalBoard();
            moveHistory.Clear();
            _userPlacedFiguresIds.Clear();
            _placedFigures = 0;
            UpdateUndoButtonState();
            UpdatePlaceSolveAvailability();
            _isSolved = false;
        }       
    }
}
