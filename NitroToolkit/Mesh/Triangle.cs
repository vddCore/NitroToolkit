namespace NitroToolkit.Mesh
{
    public class Triangle
    {
        public ushort A { get; }
        public ushort B { get; }
        public ushort C { get; }

        public Triangle(ushort a, ushort b, ushort c)
        {
            A = a;
            B = b;
            C = c;
        }
    }
}
