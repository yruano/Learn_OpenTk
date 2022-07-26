using System;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace BasicOpenTk
{
    public class Game : GameWindow
    {
        private int vertexBufferHandle;
        private int shaderprogramHandle;
        private int vertexArrayHandle;

        public Game(int width = 1280, int height = 768, string title = "Triangle")
            : base(
                GameWindowSettings.Default,
                new NativeWindowSettings() 
                {
                    Title = title,
                    Size = new Vector2i(width, height),
                    StartVisible = false,
                    StartFocused = true,
                    API = ContextAPI.OpenGL,
                    Profile = ContextProfile.Core,
                    APIVersion = new Version(3, 3)

                })
        {
            this.CenterWindow();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            this.IsVisible = true;

            // 최적화에 필요 뒷면을 안그림, 속을 안 채움
            // GL.Enable(EnableCap.CullFace);
            // GL.CullFace(CullFaceMode.Back);
            // GL.FrontFace(FrontFaceDirection.Cw);

            GL.ClearColor(new Color4(0.3f, 0.4f, 0.5f, 1f));

            float[] vertices = new float[]
            {
                /* vertex0 */  0.0f,  0.5f, 0f, /* color0 (R) */ 1.0f, 0.0f, 0.0f,
                /* vertex1 */  0.5f, -0.5f, 0f, /* color1 (G) */ 0.0f, 1.0f, 0.0f,
                /* vertex2 */ -0.5f, -0.5f, 0f, /* color2 (B) */ 0.0f, 0.0f, 1.0f,
            };

            this.vertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            this.vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(this.vertexArrayHandle);

            // 알아두자
            // vertex array == vao
            // vertex buffer == vbo
            // -- index buffer (element array buffer) == ibo (ebo)

            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBufferHandle);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);

            string vertexShaderCode = @"
                #version 330 core
                
                layout (location = 0) in vec3 aPosition;
                layout (location = 1) in vec3 aColor;
                out vec3 color;
                
                void main()
                {
                    gl_Position = vec4(aPosition, 1f);
                    color = aColor;
                }
            ";

            string pixelShaderCode = @"
                #version 330 core

                in vec3 color;
                out vec4 pixelColor;

                void main()
                {
                    // pixelColor = vec4(0.8f, 0.8f, 0.1f, 1f);
                    pixelColor = vec4(color, 1f);
                }
            ";

            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderCode);
            GL.CompileShader(vertexShaderHandle);

            int pixelShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(pixelShaderHandle, pixelShaderCode);
            GL.CompileShader(pixelShaderHandle);

            this.shaderprogramHandle = GL.CreateProgram();

            GL.AttachShader(this.shaderprogramHandle, vertexShaderHandle);
            GL.AttachShader(this.shaderprogramHandle, pixelShaderHandle);

            GL.LinkProgram(this.shaderprogramHandle);

            GL.DetachShader(this.shaderprogramHandle, vertexShaderHandle);
            GL.DetachShader(this.shaderprogramHandle, pixelShaderHandle);

            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(pixelShaderHandle);

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(this.vertexArrayHandle);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(this.vertexBufferHandle);

            GL.UseProgram(0);
            GL.DeleteProgram(this.shaderprogramHandle);

            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(this.shaderprogramHandle);
            GL.BindVertexArray(this.vertexArrayHandle);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            this.Context.SwapBuffers();
            base.OnRenderFrame(args);
        }
    }
}
