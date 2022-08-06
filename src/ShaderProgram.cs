using System;
using OpenTK.Graphics.OpenGL;

namespace BasicOpenTk
{
    public readonly struct ShaderUniform
    {
        public readonly string Name;
        public readonly int location;
        public readonly ActiveUniformType Type;

        public ShaderUniform(string name, int location, ActiveUniformType type)
        {
            this.Name = name;
            this.location = location;
            this.Type = type;
        }
    }

    public readonly struct ShaderAttribute
    {
        public readonly string Name;
        public readonly int location;
        public readonly ActiveAttribType Type;

        public ShaderAttribute(string name, int location, ActiveAttribType type)
        {
            this.Name = name;
            this.location = location;
            this.Type = type;
        }
    }

    public sealed class ShaderProgram : IDisposable
    {
        private bool disposed;
        public int ShaderprogramHandle;
        public int VertexShaderHandle;
        public int PixelShaderHandle;
        private ShaderUniform[]? uniforms;
        private ShaderAttribute[]? attributes;

        public ShaderProgram()
        {
            this.disposed = false;
            ShaderprogramHandle = 0;
        }

        public void InitShaderProgram(string vertexShaderCode, string pixelShaderCode)
        {
            if (!ShaderProgram.CompileVertexShader(vertexShaderCode, out this.VertexShaderHandle, out string vertexShaderCompileError))
            {
                throw new ArgumentException(vertexShaderCompileError);
            }

            if (!ShaderProgram.CompilePixelShader(pixelShaderCode, out this.PixelShaderHandle, out string pixelShaderCompileError))
            {
                throw new ArgumentException(pixelShaderCompileError);
            }

            this.ShaderprogramHandle = ShaderProgram.CreateLinkProgram(this.VertexShaderHandle, this.PixelShaderHandle);

            this.uniforms = ShaderProgram.CreateUniformList(this.ShaderprogramHandle);
            this.attributes = ShaderProgram.CreateAttributeList(this.ShaderprogramHandle);
        }

        ~ShaderProgram()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            GL.DeleteShader(this.VertexShaderHandle);
            GL.DeleteShader(this.PixelShaderHandle);

            GL.UseProgram(0);
            GL.DeleteProgram(this.ShaderprogramHandle);

            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        public ShaderUniform[] GetUniformList()
        {
            ShaderUniform[] result = new ShaderUniform[this.uniforms!.Length];
            Array.Copy(this.uniforms, 0, result, 0, this.uniforms.Length);
            return result;
        }

        public ShaderAttribute[] GetAttributeList()
        {
            ShaderAttribute[] result = new ShaderAttribute[this.attributes!.Length];
            Array.Copy(this.attributes, result, this.attributes.Length);
            return result;
        }

        public void SetUniform(String name, float v1)
        {
            if (!this.GetShaderUniform(name, out ShaderUniform uniform))
            {
                throw new ArgumentException($"\"{name}\" was not found");
            }

            if (uniform.Type != ActiveUniformType.Float)
            {
                throw new AbandonedMutexException("Uniforms type is not float.");
            }

            GL.UseProgram(this.ShaderprogramHandle);
            GL.Uniform1(uniform.location, v1);
            GL.UseProgram(0);
        }

        public void SetUniform(String name, float v1, float v2)
        {
            if (!this.GetShaderUniform(name, out ShaderUniform uniform))
            {
                throw new ArgumentException($"\"{name}\" was not found");
            }

            if (uniform.Type != ActiveUniformType.FloatVec2)
            {
                throw new AbandonedMutexException("Uniforms type is not FloatVec2.");
            }

            // Uniform세팅하고 프로그램을 실행하고십어서 이렇게해둔것이다 (유튜브아제)
            GL.UseProgram(this.ShaderprogramHandle);
            GL.Uniform2(uniform.location, v1, v2);
            GL.UseProgram(0);
        }


        private bool GetShaderUniform(String name, out ShaderUniform uniform)
        {
            uniform = new ShaderUniform();

            for (int i = 0; i < this.uniforms!.Length; i++)
            {
                uniform = this.uniforms[i];

                if (name == uniform.Name)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CompileVertexShader(string vertexShaderCode, out int vertexShaderHandle, out string errorMessage)
        {
            errorMessage = string.Empty;

            vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderCode);
            GL.CompileShader(vertexShaderHandle);

            // vertexShader 프로그래밍 오류를 출력함
            string vertexShaderInfo = GL.GetShaderInfoLog(vertexShaderHandle);
            if (vertexShaderInfo != String.Empty)
            {
                errorMessage = vertexShaderInfo;
                return false;
            }
            return true;
        }

        public static bool CompilePixelShader(string pixelShaderCode, out int pixelShaderHandle, out string errorMessage)
        {
            errorMessage = string.Empty;

            pixelShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(pixelShaderHandle, pixelShaderCode);
            GL.CompileShader(pixelShaderHandle);

            // pixelShader 프로그래밍 오류를 출력함
            string pixelShaderInfo = GL.GetShaderInfoLog(pixelShaderHandle);
            if (pixelShaderInfo != String.Empty)
            {
                errorMessage = pixelShaderInfo;
                return false;
            }
            return true;
        }

        public static int CreateLinkProgram(int vertexShaderHandle, int pixelShaderHandle)
        {
            int shaderprogramHandle = GL.CreateProgram();

            GL.AttachShader(shaderprogramHandle, vertexShaderHandle);
            GL.AttachShader(shaderprogramHandle, pixelShaderHandle);

            GL.LinkProgram(shaderprogramHandle);

            GL.DetachShader(shaderprogramHandle, vertexShaderHandle);
            GL.DetachShader(shaderprogramHandle, pixelShaderHandle);

            return shaderprogramHandle;
        }

        public static ShaderUniform[] CreateUniformList(int shaderProgramHandle)
        {
            GL.GetProgram(shaderProgramHandle, GetProgramParameterName.ActiveUniforms, out int uniformCount);

            ShaderUniform[] Uniforms = new ShaderUniform[uniformCount];

            for (int i = 0; i < uniformCount; i++)
            {
                GL.GetActiveUniform(shaderProgramHandle, i, 256, out _, out _, out ActiveUniformType type, out string name);
                int location = GL.GetUniformLocation(shaderProgramHandle, name);
                Uniforms[i] = new ShaderUniform(name, location, type);
            }

            return Uniforms;
        }

        public static ShaderAttribute[] CreateAttributeList(int shaderProgramHandle)
        {
            GL.GetProgram(shaderProgramHandle, GetProgramParameterName.ActiveUniforms, out int attributeCount);

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