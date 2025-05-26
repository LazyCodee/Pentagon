namespace Pentagon
{
    public static class AllFigures // усі варіанти розміщення фігур
    {
        public static List<Figure> CreateAllFigures() //створення усіх фігур (без врахування положень при повороті)
        {
            return new List<Figure>
        {
            // 1. "T"-подібна, повернута вниз
            new Figure(1, new List<IntPoint>
            {
                new IntPoint(-1, -1), new IntPoint(0, -1), new IntPoint(1, -1),
                new IntPoint(0, 0),
                new IntPoint(0, 1)
            }, 30),

            // 2. Східці
            new Figure(2, new List<IntPoint>
            {
                new IntPoint(1, -1),
                new IntPoint(0, 0), new IntPoint(1, 0),
                new IntPoint(-1, 1), new IntPoint(0, 1)
            }, 30), 

            // 3. Гачок
            new Figure(3, new List<IntPoint>
            {
                new IntPoint(0, -1), new IntPoint(1, -1),
                new IntPoint(0, 0),
                new IntPoint(-1, 1), new IntPoint(0, 1)
            }, 30), 

            // 4. L-подібна фігура
            new Figure(4, new List<IntPoint>
            {
                new IntPoint(0, -3),
                new IntPoint(0, -2),
                new IntPoint(0, -1),
                new IntPoint(0, 0), new IntPoint(1, 0)
            }, 30), 

            // 5. П-подібна фігура
            new Figure(5, new List<IntPoint>
            {
                new IntPoint(-1, -1), new IntPoint(0, -1), new IntPoint(1, -1),
                new IntPoint(-1, 0), new IntPoint(1, 0)
            }, 30),

            // 6. Стовп + ліва бічна гілка
            new Figure(6, new List<IntPoint>
            {
                new IntPoint(0, -2),
                new IntPoint(0, -1), new IntPoint(1, -1),
                new IntPoint(1, 0),
                new IntPoint(1, 1)
            }, 30), 

            // 7. Прямий кут
            new Figure(7, new List<IntPoint>
            {
                new IntPoint(-1, -1), new IntPoint(0, -1), new IntPoint(1, -1),
                new IntPoint(-1, 0),
                new IntPoint(-1, 1)
            }, 30), 

            // 8. Фігура цікавенька
            new Figure(8, new List<IntPoint>
            {
                new IntPoint(0, 0), new IntPoint(1, 0),
                new IntPoint(-1, 1), new IntPoint(0, 1), new IntPoint(1, 1)
            }, 30), 

            // 9. Високий ключ
            new Figure(9, new List<IntPoint>
            {
                new IntPoint(1, -2),
                new IntPoint(0, -1), new IntPoint(1, -1),
                new IntPoint(1, 0),
                new IntPoint(1, 1)
            }, 30), 

            // 10. Теж дуже цікавеньке
            new Figure(10, new List<IntPoint>
            {
                new IntPoint(-1, -1), new IntPoint(0, -1),
                new IntPoint(0, 0), new IntPoint(1, 0),
                new IntPoint(0, 1)
            }, 30), 

            // 11. Стовп
            new Figure(11, new List<IntPoint>
            {
                new IntPoint(0, -2),
                new IntPoint(0, -1),
                new IntPoint(0, 0),
                new IntPoint(0, 1),
                new IntPoint(0, 2)
            }, 30),

            // 12. Хрест
            new Figure(12, new List<IntPoint>
            {
                new IntPoint(0, -1),
                new IntPoint(-1, 0), new IntPoint(0, 0), new IntPoint(1, 0),
                new IntPoint(0, 1)
            }, 30)
        };
        }

        public static List<List<List<IntPoint>>> CreateAllFiguresRotations() //створення усіх фігур (з врахування положень при повороті)
        {
            List<Figure> figures = CreateAllFigures();
            List<List<List<IntPoint>>> result = new List<List<List<IntPoint>>>();

            foreach (var figure in figures)
            {
                List<List<IntPoint>> rotations = new List<List<IntPoint>>();
                List<IntPoint> currentRot = figure.GetPoints();

                for (int i = 0; i < 4; i++)
                {
                    List<IntPoint> normalized = Normalize(currentRot);
                    if (!IsDuplicate(normalized, rotations))
                        rotations.Add(normalized);
                    currentRot = Rotate(currentRot);

                }
                result.Add(rotations);
            }
            return result;
        }
        private static List<IntPoint> Rotate(List<IntPoint> points) //поворот фігури на 90 градусів праворуч
        {
            List<IntPoint> rotated = new List<IntPoint>();
            foreach (var point in points)
                rotated.Add(new IntPoint(-point.Y, point.X));
            return rotated;
        }

        private static List<IntPoint> Normalize(List<IntPoint> points) // нормалізація координат (аби не було від'ємних)
        {
            int minX = int.MaxValue, minY = int.MaxValue;
            foreach (var point in points)
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
            }
            List<IntPoint> normalized = new List<IntPoint>();
            foreach (var point in points)
                normalized.Add(new IntPoint(point.X - minX, point.Y - minY));
            return normalized;

        }

        private static bool AreShapesEqual(List<IntPoint> shape1, List<IntPoint> shape2) // встановлення того, чи є фігури однаковими 
        {
            if (shape1.Count != shape2.Count)
                return false;

            foreach (var p1 in shape1)
            {
                bool found = false;
                foreach (var p2 in shape2)
                {
                    if (p1.X == p2.X && p1.Y == p2.Y)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return false;
            }

            return true;
        }

        private static bool IsDuplicate(List<IntPoint> shape, List<List<IntPoint>> existingShapes) //метод для перевірки того, чи обрана фігура вже міститься у існуючому списку фігур
        {
            foreach (var existing in existingShapes)
            {
               if (AreShapesEqual(existing, shape))
                    return true;                
            }
            return false;

        }
    }
}
