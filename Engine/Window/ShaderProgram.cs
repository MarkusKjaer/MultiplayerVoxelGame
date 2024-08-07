using System;
using System.Xml.Linq;
using OpenTK.Graphics.OpenGL;

namespace CubeEngine.Engine.Window
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
            dispored = false;

            if (!CompileVertexShader(vertexShaderCode, out VertexShaderHandle, out string vertexShaderCompileError))
            {
                throw new ArgumentException(vertexShaderCompileError);
            }

            if (!CompilePixelShader(pixelShaderCode, out PixelShaderHandle, out string pixelShaderCompileError))
            {
                throw new ArgumentException(pixelShaderCompileError);
            }

            ShaderProgramHandle = CreateLinkProgram(VertexShaderHandle, PixelShaderHandle);

            uniforms = CreateUniformList(ShaderProgramHandle);
            attributes = CreateAttributeList(ShaderProgramHandle);


        }

        ~ShaderProgram()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (dispored)
            {
                return;
            }

            GL.DeleteShader(VertexShaderHandle);
            GL.DeleteShader(PixelShaderHandle);

            GL.UseProgram(0);
            GL.DeleteProgram(ShaderProgramHandle);

            dispored = true;
            GC.SuppressFinalize(this);
        }

        public ShaderUniform[] GetUniformList()
        {
            ShaderUniform[] result = new ShaderUniform[uniforms.Length];
            Array.Copy(uniforms, result, uniforms.Length);
            return result;
        }

        public ShaderAttribute[] GetAttributeList()
        {
            ShaderAttribute[] result = new ShaderAttribute[attributes.Length];
            Array.Copy(attributes, result, attributes.Length);
            return result;
        }

        public void SetUnitform(string name, float v1)
        {
            if (!GetShaderUnitform(name, out ShaderUniform uniform))
            {
                throw new ArgumentException("Name was not found");
            }

            if (uniform.Type != ActiveUniformType.Float)
            {
                throw new ArgumentException("Uniform type is not float");
            }

            GL.UseProgram(ShaderProgramHandle);
            GL.Uniform1(uniform.Location, v1);
            GL.UseProgram(0);
        }

        public void SetUnitform(string name, float v1, float v2)
        {
            if (!GetShaderUnitform(name, out ShaderUniform uniform))
            {
                throw new ArgumentException("Name was not found");
            }

            if (uniform.Type != ActiveUniformType.FloatVec2)
            {
                throw new ArgumentException("Uniform type is not float");
            }

            GL.UseProgram(ShaderProgramHandle);
            GL.Uniform2(uniform.Location, v1, v2);
            GL.UseProgram(0);
        }

        public void SetUnitform(string name, float v1, float v2, float v3)
        {
            if (!GetShaderUnitform(name, out ShaderUniform uniform))
            {
                throw new ArgumentException("Name was not found");
            }

            if (uniform.Type != ActiveUniformType.FloatVec3)
            {
                throw new ArgumentException("Uniform type is not float");
            }

            GL.UseProgram(ShaderProgramHandle);
            GL.Uniform3(uniform.Location, v1, v2, v3);
            GL.UseProgram(0);
        }

        public void SetUnitform(string name, OpenTK.Mathematics.Matrix4 matrix)
        {
            if (!GetShaderUnitform(name, out ShaderUniform uniform))
            {
                throw new ArgumentException("Name was not found");
            }

            if (uniform.Type != ActiveUniformType.FloatMat4)
            {
                throw new ArgumentException("Uniform type is not float");
            }

            GL.UseProgram(ShaderProgramHandle);
            GL.UniformMatrix4(uniform.Location, true, ref matrix);
            GL.UseProgram(0);
        }

        private bool GetShaderUnitform(string name, out ShaderUniform uniform)
        {
            uniform = new ShaderUniform();

            for (int i = 0; i < uniforms.Length; i++)
            {
                uniform = uniforms[i];

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

            if (vertexShaderInfo != string.Empty)
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

            if (pixelShaderInfo != string.Empty)
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
