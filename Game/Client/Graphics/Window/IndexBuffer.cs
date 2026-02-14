using System;
using OpenTK.Graphics.OpenGL;

namespace CubeEngine.Engine.Client.Graphics.Window
{
    public sealed class IndexBuffer : IDisposable
    {
        public static readonly int MinVertexCount = 1;
        public static readonly int MaxVertexCount = 250_0000;

        public bool disposed;

        public readonly int IndexBufferHandle;
        public readonly int IndexCount;
        public readonly bool IsStatic;

        public IndexBuffer(int indexCount, bool isStatic = true)
        {
            disposed = false;

            IsStatic = isStatic;

            if (indexCount < MinVertexCount ||
                indexCount > MaxVertexCount)
            {
                throw new ArgumentOutOfRangeException(nameof(indexCount));
            }

            IndexCount = indexCount;

            BufferUsageHint hint = BufferUsageHint.StaticDraw;
            if (!IsStatic)
            {
                hint = BufferUsageHint.StreamDraw;
            }

            IndexBufferHandle = GL.GenBuffer();

            // Prevent modifying the ELEMENT_ARRAY_BUFFER binding of any currently-bound VAO:
            int prevVao = GL.GetInteger(GetPName.VertexArrayBinding);
            GL.BindVertexArray(0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, IndexCount * sizeof(int), nint.Zero, hint);

            // Restore previous VAO binding
            GL.BindVertexArray(prevVao);
        }

        ~IndexBuffer()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (disposed) return;

            // Deleting the buffer is sufficient; avoid binding 0 here which would alter any bound VAO's stored binding.
            GL.DeleteBuffer(IndexBufferHandle);

            disposed = true;
            GC.SuppressFinalize(this);
        }

        public void SetData(int[] data, int count)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            if (count <= 0 ||
                count > IndexCount ||
                count > data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            // Prevent modifying the ELEMENT_ARRAY_BUFFER binding of any currently-bound VAO:
            int prevVao = GL.GetInteger(GetPName.VertexArrayBinding);
            GL.BindVertexArray(0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, nint.Zero, count * sizeof(int), data);

            // Restore previous VAO binding
            GL.BindVertexArray(prevVao);
        }
    }
}