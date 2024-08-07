using System;
using OpenTK.Graphics.OpenGL;

namespace CubeEngine.Engine.Window
{
    public sealed class VertexArray : IDisposable
    {
        private bool disposed;

        public readonly int VertexArrayHandle;
        public readonly VertexBuffer VertexBuffer;

        public readonly IndexBuffer IndexBuffer;

        public VertexArray(VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            disposed = false;

            if (vertexBuffer is null)
            {
                throw new ArgumentException(nameof(vertexBuffer));
            }

            if (indexBuffer is null)
            {
                throw new ArgumentException(nameof(indexBuffer));
            }

            VertexBuffer = vertexBuffer;
            IndexBuffer = indexBuffer;

            int vertexSizeInBytes = VertexBuffer.VertexInfo.SizeInBytes;
            VertexAttribute[] attributes = VertexBuffer.VertexInfo.VertexAttributes;

            // Generate and bind the vertex array
            VertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayHandle);

            // Bind the vertex buffer and set vertex attributes
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer.VertexBufferHandle);

            for (int i = 0; i < attributes.Length; i++)
            {
                VertexAttribute attribute = attributes[i];

                GL.VertexAttribPointer(attribute.Index, attribute.ComponentCount, VertexAttribPointerType.Float, false, vertexSizeInBytes, attribute.Offset);
                GL.EnableVertexAttribArray(attribute.Index);

            }

            // Bind the element array buffer while the VAO is bound
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer.IndexBufferHandle);
            // Unbind the VAO (this will also unbind the IBO from the element array buffer target)
            GL.BindVertexArray(0);
        }

        ~VertexArray()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                return;
            }

            GL.BindVertexArray(0);
            GL.DeleteVertexArray(VertexArrayHandle);

            disposed = true;
            GC.SuppressFinalize(this);
        }

    }
}

