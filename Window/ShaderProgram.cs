using System;
using System.Xml.Linq;
using OpenTK.Graphics.OpenGL;

namespace CubeEngine.Window
{
    public readonly struct ShaderUniform(string name, int location, ActiveUniformType type)
    {
        public readonly string Name = name;
        public readonly int Location = location;
        public readonly ActiveUniformType Type = type;
    }

    public readonly struct ShaderAttribute(string name, int location, ActiveAttribType type)
    {
        public readonly string Name = name;
        public readonly int Location = location;
        public readonly ActiveAttribType Type = type;
    }

    public sealed class ShaderProgram : IDisposable
    {
        private bool dispored;

        public readonly int ShaderProgramHandle;
        public readonly int VertexShaderHandle;
        public readonly int PixelShaderHandle;

        private readonly ShaderUniform[] uniforms;
        private readonly ShaderAttribute[] attributes;

        public ShaderProgram(string vertexShaderCode, string pixelShaderCode)
        {
            this.dispored = false;

            if (!ShaderProgram.CompileVertexShader(vertexShaderCode, out this.VertexShaderHandle, out string vertexShaderCompileError))
            {
                throw new ArgumentException(vertexShaderCompileError);
            }

            if (!ShaderProgram.CompilePixelShader(pixelShaderCode, out this.PixelShaderHandle, out string pixelShaderCompileError))
            {
                throw new ArgumentException(pixelShaderCompileError);
            }

            this.ShaderProgramHandle = ShaderProgram.CreateLinkProgram(VertexShaderHandle, PixelShaderHandle);

            this.uniforms = ShaderProgram.CreateUniformList(this.ShaderProgramHandle);
            this.attributes = ShaderProgram.CreateAttributeList(this.ShaderProgramHandle);


        }

        ~ShaderProgram()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (dispored)
            {
                return;
            }

            GL.DeleteShader(this.VertexShaderHandle);
            GL.DeleteShader(this.PixelShaderHandle);

            GL.UseProgram(0);
            GL.DeleteProgram(this.ShaderProgramHandle);

            dispored = true;
            GC.SuppressFinalize(this);
        }

        public ShaderUniform[] GetUniformList()
        {
            ShaderUniform[] result = new ShaderUniform[this.uniforms.Length];
            Array.Copy(this.uniforms, result, this.uniforms.Length);
            return result;
        }

        public ShaderAttribute[] GetAttributeList()
        {
            ShaderAttribute[] result = new ShaderAttribute[this.attributes.Length];
            Array.Copy(this.attributes, result, this.attributes.Length);
            return result;
        }

        public void SetUnitform(string name, float v1)
        {
            if (!this.GetShaderUnitform(name, out ShaderUniform uniform))
            {
                throw new ArgumentException("Name was not found");
            }

            if (uniform.Type != ActiveUniformType.Float)
            {
                throw new ArgumentException("Uniform type is not float");
            }

            GL.UseProgram(this.ShaderProgramHandle);
            GL.Uniform1(uniform.Location, v1);
            GL.UseProgram(0);
        }

        public void SetUnitform(string name, float v1, float v2)
        {
            if (!this.GetShaderUnitform(name, out ShaderUniform uniform))
            {
                throw new ArgumentException("Name was not found");
            }

            if (uniform.Type != ActiveUniformType.FloatVec2)
            {
                throw new ArgumentException("Uniform type is not float");
            }

            GL.UseProgram(this.ShaderProgramHandle);
            GL.Uniform2(uniform.Location, v1, v2);
            GL.UseProgram(0);
        }

        public void SetUnitform(string name, float v1, float v2, float v3)
        {
            if (!this.GetShaderUnitform(name, out ShaderUniform uniform))
            {
                throw new ArgumentException("Name was not found");
            }

            if (uniform.Type != ActiveUniformType.FloatVec3)
            {
                throw new ArgumentException("Uniform type is not float");
            }

            GL.UseProgram(this.ShaderProgramHandle);
            GL.Uniform3(uniform.Location, v1, v2, v3);
            GL.UseProgram(0);
        }

        public void SetUnitform(string name, OpenTK.Mathematics.Matrix4 matrix)
        {
            if (!this.GetShaderUnitform(name, out ShaderUniform uniform))
            {
                throw new ArgumentException("Name was not found");
            }

            if (uniform.Type != ActiveUniformType.FloatMat4)
            {
                throw new ArgumentException("Uniform type is not float");
            }

            GL.UseProgram(this.ShaderProgramHandle);
            GL.UniformMatrix4(uniform.Location, true, ref matrix);
            GL.UseProgram(0);
        }

        private bool GetShaderUnitform(string name, out ShaderUniform uniform)
        {
            uniform = new ShaderUniform();

            for (int i = 0; i < this.uniforms.Length; i++)
            {
                uniform = this.uniforms[i];

                if (name == uniform.Name)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CompileVertexShader(string vertexShaderCode, out int vertexShaderHandle, out string errorMesage)
        {
            errorMesage = string.Empty;

            vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderCode);
            GL.CompileShader(vertexShaderHandle);

            string vertexShaderInfo = GL.GetShaderInfoLog(vertexShaderHandle);

            if (vertexShaderInfo != String.Empty)
            {
                errorMesage = vertexShaderInfo;
                return false;
            }
            return true;
        }

        public static bool CompilePixelShader(string pixelShaderCode, out int pixelShaderHandle, out string errorMesage)
        {
            errorMesage = string.Empty;

            pixelShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(pixelShaderHandle, pixelShaderCode);
            GL.CompileShader(pixelShaderHandle);

            string pixelShaderInfo = GL.GetShaderInfoLog(pixelShaderHandle);

            if (pixelShaderInfo != String.Empty)
            {
                errorMesage = pixelShaderInfo;
                return false;
            }

            return true;
        }

        public static int CreateLinkProgram(int vertexShaderHandle, int pixelShaderHandle)
        {
            int shaderProgramHandle = GL.CreateProgram();

            GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(shaderProgramHandle, pixelShaderHandle);

            GL.LinkProgram(shaderProgramHandle);

            GL.DetachShader(shaderProgramHandle, vertexShaderHandle);
            GL.DetachShader(shaderProgramHandle, pixelShaderHandle);

            return shaderProgramHandle;
        }

        public static ShaderUniform[] CreateUniformList(int shaderProgramHandle)
        {
            GL.GetProgram(shaderProgramHandle, GetProgramParameterName.ActiveUniforms, out int unitformCount);

            ShaderUniform[] uniforms = new ShaderUniform[unitformCount];

            for (int i = 0; i < unitformCount; i++)
            {
                GL.GetActiveUniform(shaderProgramHandle, i, 256, out _, out _, out ActiveUniformType type, out string name);
                int location = GL.GetUniformLocation(shaderProgramHandle, name);
                uniforms[i] = new ShaderUniform(name, location, type);
            }

            return uniforms;
        }

        public static ShaderAttribute[] CreateAttributeList(int shaderProgramHandle)
        {
            GL.GetProgram(shaderProgramHandle, GetProgramParameterName.ActiveAttributes, out int attributeCount);

            ShaderAttribute[] attributes = new ShaderAttribute[attributeCount];

            for (int i = 0; i < attributeCount; i++)
            {
                GL.GetActiveAttrib(shaderProgramHandle, i, 256, out _, out _, out ActiveAttribType type, out string name);
                int location = GL.GetAttribLocation(shaderProgramHandle, name);
                attributes[i] = new ShaderAttribute(name, location, type);
            }

            return attributes;
        }
    }

}
