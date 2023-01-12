using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NitroToolkit.Mesh
{
    public class Mesh
    {
        public MeshHeader Header { get; }
        public List<Triangle> Triangles { get; }
        public List<float[]> Vertices { get; }
        public List<float[]> Normals { get; }
        public List<byte[]> ColorMasks { get; }
        public List<float[]> UVs { get; }
        public List<float[]> Tangents { get; }
        public List<float[]> Bitangents { get; }
        public List<byte> StrayBytes { get; }

        public string FileName { get; }

        public Mesh()
        {
            Triangles = new List<Triangle>();
            StrayBytes = new List<byte>();

            Vertices = new List<float[]>();
            Normals = new List<float[]>();
            ColorMasks = new List<byte[]>();
            UVs = new List<float[]>();
            Tangents = new List<float[]>();
            Bitangents = new List<float[]>();
        }

        public Mesh(string fileName)
        {
            FileName = fileName;

            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    Header = MeshHeader.Read(br);

                    for (var i = 0; i < Header.TriangleIndexCount / 3; i++)
                    {
                        var tri = new Triangle(br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16());
                        Triangles.Add(tri);
                    }

                    for (var i = 0; i < Header.VertexCount; i++)
                    {
                        Vertices.Add(new[] { br.ReadSingle(), br.ReadSingle(), br.ReadSingle() });
                        Normals.Add(new[] { br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle() });
                        ColorMasks.Add(br.ReadBytes(4));

                        // V = (1f - value): 
                        // because the textures seem to be flipped
                        // after reading without inverting the value
                        UVs.Add(new[] { br.ReadSingle(), 1f - br.ReadSingle() });
                        Tangents.Add(new[] { br.ReadSingle(), br.ReadSingle(), br.ReadSingle() });

                        Bitangents.Add(new[] { br.ReadSingle(), br.ReadSingle(), br.ReadSingle() });
                    }

                    var bytesLeft = br.BaseStream.Length - br.BaseStream.Position;
                    for (var i = 0; i < bytesLeft; i++)
                    {
                        StrayBytes.Add(br.ReadByte());
                    }
                }
            }
        }

        public void Write()
        {
            Write(FileName);
        }

        public void Write(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    Header.Write(bw);

                    foreach (var triangle in Triangles)
                    {
                        bw.Write(triangle.A);
                        bw.Write(triangle.B);
                        bw.Write(triangle.C);
                    }

                    for (var i = 0; i < Header.VertexCount; i++)
                    {
                        bw.Write(Vertices[i][0]);
                        bw.Write(Vertices[i][1]);
                        bw.Write(Vertices[i][2]);

                        bw.Write(Normals[i][0]);
                        bw.Write(Normals[i][1]);
                        bw.Write(Normals[i][2]);
                        bw.Write(Normals[i][3]);

                        bw.Write(ColorMasks[i][0]);
                        bw.Write(ColorMasks[i][1]);
                        bw.Write(ColorMasks[i][2]);
                        bw.Write(ColorMasks[i][3]);

                        bw.Write(UVs[i][0]);
                        bw.Write(1f - UVs[i][1]);

                        bw.Write(Tangents[i][0]);
                        bw.Write(Tangents[i][1]);
                        bw.Write(Tangents[i][2]);

                        bw.Write(Bitangents[i][0]);
                        bw.Write(Bitangents[i][1]);
                        bw.Write(Bitangents[i][2]);
                    }

                    foreach (var strayByte in StrayBytes)
                    {
                        bw.Write(strayByte);
                    }
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Header:\n{Header}\n");

            sb.Append(TriangleInfo());
            sb.Append(VertexInfo());
            sb.Append(NormalInfo());
            sb.Append(ColorMaskInfo());
            sb.Append(UVInfo());
            sb.Append(TangentInfo());
            sb.Append(BitangentInfo());

            return sb.ToString();
        }

        public string TriangleInfo()
        {
            var sb = new StringBuilder();

            sb.Append("Triangles:\n");
            foreach (var tri in Triangles)
            {
                sb.Append($"  [A: {tri.A}, B: {tri.B}, C: {tri.C}]\n");
            }

            return sb.ToString();
        }

        public string VertexInfo()
        {
            var sb = new StringBuilder();

            sb.Append("Vertices:\n");
            foreach (var ver in Vertices)
            {
                sb.Append($"  [X: {ver[0]}, Y: {ver[1]}, Z: {ver[2]}]\n");
            }

            return sb.ToString();
        }

        public string NormalInfo()
        {
            var sb = new StringBuilder();

            sb.Append("Normals:\n");
            foreach (var n in Normals)
            {
                sb.Append($"  [X: {n[0]}, Y: {n[1]}, Z: {n[2]}, W: {n[3]}]\n");
            }

            return sb.ToString();
        }

        public string ColorMaskInfo()
        {
            var sb = new StringBuilder();

            sb.Append("Color masks:\n");
            foreach (var cc in ColorMasks)
            {
                sb.Append($"  [#{cc[0]:X2}{cc[1]:X2}{cc[2]:X2}{cc[3]:X2}] /rgba({cc[0]:D3}, {cc[1]:D3}, {cc[2]:D3}, {cc[3]:D3})/\n");
            }

            return sb.ToString();
        }

        public string UVInfo()
        {
            var sb = new StringBuilder();

            sb.Append("UVs:\n");
            foreach (var uv in UVs)
            {
                sb.Append($"  [U: {uv[0]}, V: {uv[1]}]\n");
            }

            return sb.ToString();
        }

        public string TangentInfo()
        {
            var sb = new StringBuilder();

            sb.Append("Tangents:\n");
            foreach (var tangent in Tangents)
            {
                sb.Append($"  [X: {tangent[0]}, Y: {tangent[1]}, Z: {tangent[2]}, (W 1.0; implicit)]\n");
            }

            return sb.ToString();
        }

        public string BitangentInfo()
        {
            var sb = new StringBuilder();

            sb.Append("Bitangents:\n");
            foreach (var b in Bitangents)
            {
                sb.Append($"  [X: {b[0]}, Y: {b[1]}, Z: {b[2]}]\n");
            }

            return sb.ToString();
        }
    }
}
