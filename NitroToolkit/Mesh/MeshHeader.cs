using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NitroToolkit.Mesh
{
    public class MeshHeader
    {
        public uint Identifier { get; private set; }
        public string Descriptor { get; private set; }
        public uint SubmeshCount { get; set; }
        public List<uint> SubmeshStartIndices { get; private set; }
        public uint TriangleIndexCount { get; set; }
        public uint VertexCount { get; set; }

        public MeshHeader()
        {
            Identifier = 0x06;
            Descriptor = "p3n4ccu2t3b3\0";
            SubmeshCount = 1;
            SubmeshStartIndices = new List<uint>
            {
                0
            };

            TriangleIndexCount = 0;
            VertexCount = 0;
        }

        public static MeshHeader Read(BinaryReader binaryReader)
        {
            var header = new MeshHeader
            {
                Identifier = binaryReader.ReadUInt32(),
                Descriptor = Encoding.UTF8.GetString(binaryReader.ReadBytes(13)),
                SubmeshCount = binaryReader.ReadUInt32(),
                SubmeshStartIndices = new List<uint>()
            };

            for (var i = 0; i < header.SubmeshCount; i++)
            {
                header.SubmeshStartIndices.Add(binaryReader.ReadUInt32());
            }

            header.TriangleIndexCount = binaryReader.ReadUInt32();
            header.VertexCount = binaryReader.ReadUInt32();

            return header;
        }

        public void Write(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Identifier);

            var bytes = Encoding.UTF8.GetBytes(Descriptor);
            binaryWriter.Write(bytes);

            binaryWriter.Write(SubmeshCount);
            foreach (var startIdx in SubmeshStartIndices)
            {
                binaryWriter.Write(startIdx);
            }
            binaryWriter.Write(TriangleIndexCount);
            binaryWriter.Write(VertexCount);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"  Identifier: 0x{Identifier:X8}\n");
            sb.Append($"  Descriptor: {Descriptor}\n");
            sb.Append($"  Submesh count: {SubmeshCount}\n");
            sb.Append("  Submesh start indices:\n");

            var i = 0;
            foreach (var x in SubmeshStartIndices)
            {
                sb.Append($"    {i}: {x}\n");
                i++;
            }
            sb.Append($"  Triangle indices: {TriangleIndexCount}\n");
            sb.Append($"  Vertex count: {VertexCount}\n");

            return sb.ToString();
        }
    }
}
