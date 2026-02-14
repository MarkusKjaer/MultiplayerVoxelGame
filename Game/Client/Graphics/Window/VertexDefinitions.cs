using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace CubeEngine.Engine.Client.Graphics.Window
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
    /*
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
    */

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 texCoord)
    {
        public readonly Vector3 Position { get; } = position;
        public readonly Vector3 Normal { get; } = normal;
        public readonly Vector2 TexCoord { get; } = texCoord;

        public static readonly VertexInfo vertexInfo = new VertexInfo(
            typeof(VertexPositionNormalTexture),
            new VertexAttribute("Position", 0, 3, 0),
            new VertexAttribute("Normal", 1, 3, 3 * sizeof(float)),
            new VertexAttribute("TexCoord", 2, 2, 6 * sizeof(float))
        );
    }

    /*
    public readonly struct VertexPositionTextureLayer(Vector3 position, Vector2 texCoord, float layer)
    {
        public readonly Vector3 Position { get; } = position;
        public readonly Vector2 TexCoord { get; } = texCoord;
        public readonly float Layer { get; } = layer;

        public static readonly VertexInfo vertexInfo = new VertexInfo(
            typeof(VertexPositionTextureLayer),
            new VertexAttribute("Position", 0, 3, 0),
            new VertexAttribute("TexCoord", 1, 2, 3 * sizeof(float)),
            new VertexAttribute("Layer", 2, 1, (3 + 2) * sizeof(float))
            );
    }
    */
    //public readonly struct VertexPositionNormalTextureLayer(Vector3 position, Vector3 normal, Vector2 texCoord, float layer)
    //{
    //    public readonly Vector3 Position { get; } = position;
    //    public readonly Vector3 Normal { get; } = normal;
    //    public readonly Vector2 TexCoord { get; } = texCoord;
    //    public readonly float Layer { get; } = layer;

    //    public static readonly VertexInfo vertexInfo = new VertexInfo(
    //        typeof(VertexPositionNormalTextureLayer),
    //        new VertexAttribute("Position", 0, 3, 0),
    //        new VertexAttribute("Normal", 1, 3, 3 * sizeof(float)),
    //        new VertexAttribute("TexCoord", 2, 2, (3 + 3) * sizeof(float)),
    //        new VertexAttribute("Layer", 3, 1, (3 + 3 + 2) * sizeof(float))
    //        );
    //}

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct VertexPositionNormalTextureLayerAO(
    Vector3 position,
    Vector3 normal,
    Vector2 texCoord,
    float layer,
    float ao)
    {
        public readonly Vector3 Position { get; } = position;
        public readonly Vector3 Normal { get; } = normal;
        public readonly Vector2 TexCoord { get; } = texCoord;
        public readonly float Layer { get; } = layer;
        public readonly float AO { get; } = ao;

        public static readonly VertexInfo vertexInfo = new VertexInfo(
            typeof(VertexPositionNormalTextureLayerAO),
            new VertexAttribute("Position", 0, 3, 0),
            new VertexAttribute("Normal", 1, 3, 3 * sizeof(float)),
            new VertexAttribute("TexCoord", 2, 2, 6 * sizeof(float)),
            new VertexAttribute("Layer", 3, 1, 8 * sizeof(float)),
            new VertexAttribute("AO", 4, 1, 9 * sizeof(float))
        );
    }
}

