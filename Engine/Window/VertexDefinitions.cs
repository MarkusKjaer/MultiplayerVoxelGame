using System;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Window
{
    public readonly struct VertexAttribute(string name, int index, int componentCount, int offset)
    {
        public readonly string Name = name;
        public readonly int Index = index;
        public readonly int ComponentCount = componentCount;
        public readonly int Offset = offset;
    }

    public sealed class VertexInfo
    {
        public readonly Type Type;
        public readonly int SizeInBytes;
        public readonly VertexAttribute[] VertexAttributes;

        public VertexInfo(Type type, params VertexAttribute[] attributes)
        {
            Type = type;
            SizeInBytes = 0;

            VertexAttributes = attributes;

            for (int i = 0; i < attributes.Length; i++)
            {
                VertexAttribute attribute = attributes[i];
                SizeInBytes += attribute.ComponentCount * sizeof(float);
            }
        }
    }

    public readonly struct VertexPositionColor(Vector3 position, Color4 color)
    {
        public readonly Vector3 Position = position;
        public readonly Color4 Color = color;

        public static readonly VertexInfo vertexInfo = new VertexInfo(
            typeof(VertexPositionColor),
            new VertexAttribute("Position", 0, 3, 0),
            new VertexAttribute("Color", 1, 4, 3 * sizeof(float))
            );
    }

    public readonly struct VertexPositionTexture(Vector3 position, Vector2 texCoord)
    {
        public readonly Vector3 Position = position;
        public readonly Vector2 TexCoord = texCoord;

        public static readonly VertexInfo vertexInfo = new VertexInfo(
            typeof(VertexPositionTexture),
            new VertexAttribute("Position", 0, 3, 0),
            new VertexAttribute("TexCoord", 1, 2, 3 * sizeof(float))
            );
    }
}

