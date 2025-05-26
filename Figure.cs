namespace Pentagon
{
    public class Figure
    {
        public int Id { get; }
        public List<IntPoint> Blocks { get; private set; }  // список локальних координат блоків для усіх варіантів розташування фігур 
        public int BlockSize { get; private set; } 

        public Figure(int id, List<IntPoint> blocks, int blockSize)
        {
            Id = id;
            Blocks = blocks;
            BlockSize = blockSize;
        }

        public void RotateClockwise()   // Обертає фігуру на 90 градусів
        {             
              Blocks = Blocks.Select(p => new IntPoint(-p.Y, p.X)).ToList();           
        }         
        public List<IntPoint> GetPoints() => Blocks;               
    }
}
