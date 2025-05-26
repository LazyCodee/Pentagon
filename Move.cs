namespace Pentagon
{
    public class Move // клас для запису інформації про додані на поле фігури
    {
        public Figure Figure { get; }
        public int BaseRow { get; }
        public int BaseColumn { get; }
        public Move (Figure figure, int row, int col)
        {
            Figure = new Figure(figure.Id, figure.Blocks.ToList(), figure.BlockSize);
            BaseRow = row;
            BaseColumn = col;
        }
    }
}
