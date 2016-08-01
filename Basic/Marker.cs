
namespace Basic
{
    public struct Marker
    {
        public int Pointer { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public Marker(int pointer, int line, int column)
            : this()
        {
            Pointer = pointer;
            Line = line;
            Column = Column;
        }
    }
}
