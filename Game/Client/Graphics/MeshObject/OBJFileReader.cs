using CubeEngine.Engine.Client.Graphics.Window;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CubeEngine.Engine.Client.Graphics.MeshObject
{
    public class OBJFileReader
    {
        public MeshInfo ReadOBJFile(string filePath)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();

            var faceVertexIndices = new List<int>();
            var faceUvIndices = new List<int>();
            var faceNormalIndices = new List<int>();

            try
            {
                using var sr = new StreamReader(filePath);
                string? line;

                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    line = line.Trim();

                    if (line.StartsWith("#"))
                        continue;

                    var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length == 0)
                        continue;

                    string cmd = tokens[0];

                    if (cmd == "v")
                    {
                        if (tokens.Length < 4) continue;

                        float x = float.Parse(tokens[1], CultureInfo.InvariantCulture);
                        float y = float.Parse(tokens[2], CultureInfo.InvariantCulture);
                        float z = float.Parse(tokens[3], CultureInfo.InvariantCulture);

                        vertices.Add(new Vector3(x, y, z));
                    }
                    else if (cmd == "vt")
                    {
                        if (tokens.Length < 3) continue;

                        float u = float.Parse(tokens[1], CultureInfo.InvariantCulture);
                        float v = float.Parse(tokens[2], CultureInfo.InvariantCulture);

                        uvs.Add(new Vector2(u, v));
                    }
                    else if (cmd == "vn")
                    {
                        if (tokens.Length < 4) continue;

                        float x = float.Parse(tokens[1], CultureInfo.InvariantCulture);
                        float y = float.Parse(tokens[2], CultureInfo.InvariantCulture);
                        float z = float.Parse(tokens[3], CultureInfo.InvariantCulture);

                        normals.Add(new Vector3(x, y, z));
                    }
                    else if (cmd == "f")
                    {
                        if (tokens.Length < 4) continue;

                        var refs = new List<(int v, int vt, int vn)>();

                        for (int i = 1; i < tokens.Length; i++)
                        {
                            var comps = tokens[i].Split('/');

                            int vIndex = ParseIndexSafe(comps, 0);
                            int vtIndex = ParseIndexSafe(comps, 1);
                            int vnIndex = ParseIndexSafe(comps, 2);

                            vIndex = NormalizeIndex(vIndex, vertices.Count);
                            vtIndex = NormalizeIndexAllowMissing(vtIndex, uvs.Count);
                            vnIndex = NormalizeIndexAllowMissing(vnIndex, normals.Count);

                            refs.Add((vIndex, vtIndex, vnIndex));
                        }

                        for (int i = 1; i < refs.Count - 1; i++)
                        {
                            AddFace(refs[0], refs[i], refs[i + 1]);
                        }
                    }

                    void AddFace((int v, int vt, int vn) a, (int v, int vt, int vn) b, (int v, int vt, int vn) c)
                    {
                        faceVertexIndices.Add(a.v);
                        faceUvIndices.Add(a.vt);
                        faceNormalIndices.Add(a.vn);

                        faceVertexIndices.Add(b.v);
                        faceUvIndices.Add(b.vt);
                        faceNormalIndices.Add(b.vn);

                        faceVertexIndices.Add(c.v);
                        faceUvIndices.Add(c.vt);
                        faceNormalIndices.Add(c.vn);
                    }
                }

                var unique = new Dictionary<(int, int, int), int>();
                var finalVerts = new List<VertexPositionNormalTexture>();
                var finalIndices = new List<int>();

                for (int i = 0; i < faceVertexIndices.Count; i++)
                {
                    var key = (faceVertexIndices[i], faceUvIndices[i], faceNormalIndices[i]);

                    if (!unique.TryGetValue(key, out int newIndex))
                    {
                        newIndex = finalVerts.Count;
                        unique[key] = newIndex;

                        Vector3 pos = vertices[key.Item1];
                        Vector3 normal = key.Item3 >= 0 && key.Item3 < normals.Count ? normals[key.Item3] : Vector3.Zero;
                        Vector2 uv = key.Item2 >= 0 && key.Item2 < uvs.Count ? uvs[key.Item2] : Vector2.Zero;

                        finalVerts.Add(new VertexPositionNormalTexture(pos, normal, uv));
                    }

                    finalIndices.Add(newIndex);
                }

                bool missingNormals = finalVerts.Any(v => v.Normal == Vector3.Zero);
                if (missingNormals)
                {
                    var accum = new Vector3[finalVerts.Count];

                    for (int i = 0; i < finalIndices.Count; i += 3)
                    {
                        int ia = finalIndices[i];
                        int ib = finalIndices[i + 1];
                        int ic = finalIndices[i + 2];

                        var pa = finalVerts[ia].Position;
                        var pb = finalVerts[ib].Position;
                        var pc = finalVerts[ic].Position;

                        var n = Vector3.Cross(pb - pa, pc - pa);
                        if (n.LengthSquared > 0)
                            n.Normalize();

                        accum[ia] += n;
                        accum[ib] += n;
                        accum[ic] += n;
                    }

                    for (int i = 0; i < finalVerts.Count; i++)
                    {
                        var n = accum[i];
                        if (n.LengthSquared > 0)
                            n.Normalize();
                        else
                            n = Vector3.UnitY;

                        var v = finalVerts[i];
                        finalVerts[i] = new VertexPositionNormalTexture(v.Position, n, v.TexCoord);
                    }
                }

                return new MeshInfo(finalVerts.ToArray(), finalIndices.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OBJ load failed: {ex.Message}");
                return new MeshInfo(Array.Empty<VertexPositionNormalTexture>(), Array.Empty<int>());
            }
        }

        private static int ParseIndexSafe(string[] comps, int idx)
        {
            if (idx >= comps.Length) return -1;
            if (string.IsNullOrEmpty(comps[idx])) return -1;

            return int.TryParse(comps[idx], NumberStyles.Integer, CultureInfo.InvariantCulture, out int v)
                ? v
                : -1;
        }

        private static int NormalizeIndex(int idx, int count)
        {
            if (idx == -1) return -1;
            if (idx < 0) return count + idx;
            return idx - 1;
        }

        private static int NormalizeIndexAllowMissing(int idx, int count)
        {
            if (idx == -1 || count == 0) return -1;
            if (idx < 0) return count + idx;
            return idx - 1;
        }
    }
}
