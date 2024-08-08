using CubeEngine.Engine.Window;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CubeEngine.Engine.MeshObject
{
    public class OBJFileReader
    {
        public MeshInfo ReadOBJFile(string filePath)
        {
            MeshInfo meshInfoToReturn = new();

            try
            {
                using StreamReader sr = new StreamReader(filePath);
                string? line;

                List<int> vertexIndices = [], normalIndices = [];
                Dictionary<int, int> uvIndices = [];

                List <Vector3> vertices = [];
                List<int> indices = [];
                List<Vector3> normals = [];

                List<Vector2> uvs = [];

                while ((line = sr.ReadLine()) != null)
                {
                    var firstWord = Regex.Match(line, @"^([\w\-]+)").Value;

                    if (string.Equals(firstWord, "v"))
                    {
                        Vector3 vertex;

                        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        vertex.X = float.Parse(parts[1]);
                        vertex.Y = float.Parse(parts[2]);
                        vertex.Z = float.Parse(parts[3]);

                        vertices.Add(vertex);
                    }
                    else if (string.Equals(firstWord, "vt"))
                    {
                        Vector2 uv;

                        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        uv.X = float.Parse(parts[1]);
                        uv.Y = float.Parse(parts[2]);

                        uvs.Add(uv);
                    }
                    else if (string.Equals(firstWord, "vn"))
                    {
                        
                        Vector3 normal;

                        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        normal.X = float.Parse(parts[1]);
                        normal.Y = float.Parse(parts[2]);
                        normal.Z = float.Parse(parts[3]);

                        normals.Add(normal);
                        
                    }
                    else if (string.Equals(firstWord, "f"))
                    {
                        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length == 4)
                        {
                            ThreeFace(parts, vertexIndices, uvIndices, normalIndices);
                        }

                        if (parts.Length == 5)
                        {
                            FourFace(parts, vertexIndices, uvIndices, normalIndices);
                        }

                    }
                }
                List<VertexPositionTexture> verticePositionTextures = [];

                for (int i = 0; i < vertices.Count; i++)
                {
                    verticePositionTextures.Add(new(vertices[i], uvs[uvIndices[vertexIndices[i]]]));
                }

                List<Vector3> outVertices = [];

                meshInfoToReturn = new([.. verticePositionTextures], [.. vertexIndices]);
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }


            return meshInfoToReturn;
        }

        private void ThreeFace(string[] parts, List<int> vertexIndices, Dictionary<int, int> uvIndices, List<int> normalIndices)
        {
            int[] vertexIndex = new int[3],
                    uvIndex = new int[3],
                    normalIndex = new int[3];

            string[] firstVertex = parts[1].Split('/', StringSplitOptions.RemoveEmptyEntries);
            string[] secondVertex = parts[2].Split('/', StringSplitOptions.RemoveEmptyEntries);
            string[] thirdVertex = parts[3].Split('/', StringSplitOptions.RemoveEmptyEntries);

            vertexIndex[0] = int.Parse(firstVertex[0]);
            vertexIndex[1] = int.Parse(secondVertex[0]);
            vertexIndex[2] = int.Parse(thirdVertex[0]);

            uvIndex[0] = int.Parse(firstVertex[1]);
            uvIndex[1] = int.Parse(secondVertex[1]);
            uvIndex[2] = int.Parse(thirdVertex[1]);

            normalIndex[0] = int.Parse(firstVertex[2]);
            normalIndex[1] = int.Parse(secondVertex[2]);
            normalIndex[2] = int.Parse(thirdVertex[2]);

            for (int i = 0; i < 3; i++)
            {
                vertexIndices.Add(vertexIndex[i] - 1);
                if(!uvIndex.Contains(i))
                {
                    uvIndices.TryAdd(vertexIndex[i] - 1, uvIndex[i] - 1);
                }
                normalIndices.Add(normalIndex[i] - 1);
            }
            /*
            vertexIndices.Add(vertexIndex[0] - 1);
            vertexIndices.Add(vertexIndex[1] - 1);
            vertexIndices.Add(vertexIndex[2] - 1);

            uvIndices.Add(vertexIndex[0] - 1, uvIndex[0] - 1);
            uvIndices.Add(vertexIndex[1] - 1, uvIndex[1] - 1);
            uvIndices.Add(vertexIndex[2] - 1, uvIndex[2] - 1);

            normalIndices.Add(normalIndex[0] - 1);
            normalIndices.Add(normalIndex[1] - 1);
            normalIndices.Add(normalIndex[2] - 1);
            */
        }
        private void FourFace(string[] parts, List<int> vertexIndices, Dictionary<int, int> uvIndices, List<int> normalIndices)
        {
            int[] vertexIndex = new int[6],
                    uvIndex = new int[6],
                    normalIndex = new int[6];

            string[] firstVertex = parts[1].Split('/', StringSplitOptions.RemoveEmptyEntries);
            string[] secondVertex = parts[2].Split('/', StringSplitOptions.RemoveEmptyEntries);
            string[] thirdVertex = parts[3].Split('/', StringSplitOptions.RemoveEmptyEntries);
            string[] fourthVertex = parts[4].Split('/', StringSplitOptions.RemoveEmptyEntries);

            vertexIndex[0] = int.Parse(firstVertex[0]);
            vertexIndex[1] = int.Parse(secondVertex[0]);
            vertexIndex[2] = int.Parse(thirdVertex[0]);
            vertexIndex[3] = int.Parse(thirdVertex[0]);
            vertexIndex[4] = int.Parse(fourthVertex[0]);
            vertexIndex[5] = int.Parse(firstVertex[0]);

            uvIndex[0] = int.Parse(firstVertex[1]);
            uvIndex[1] = int.Parse(secondVertex[1]);
            uvIndex[2] = int.Parse(thirdVertex[1]);
            uvIndex[3] = int.Parse(thirdVertex[1]);
            uvIndex[4] = int.Parse(fourthVertex[1]);
            uvIndex[5] = int.Parse(firstVertex[1]);

            normalIndex[0] = int.Parse(firstVertex[2]);
            normalIndex[1] = int.Parse(secondVertex[2]);
            normalIndex[2] = int.Parse(thirdVertex[2]);
            normalIndex[3] = int.Parse(thirdVertex[2]);
            normalIndex[4] = int.Parse(fourthVertex[2]);
            normalIndex[5] = int.Parse(firstVertex[2]);
            
            for (int i = 0; i < 6; i++)
            {
                vertexIndices.Add(vertexIndex[i] - 1);
                if (!uvIndex.Contains(i))
                {
                    uvIndices.TryAdd(vertexIndex[i] - 1, uvIndex[i] - 1);
                }
                normalIndices.Add(normalIndex[i] - 1);
            }
        }
    }
}
