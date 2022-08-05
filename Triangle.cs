using System;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace BasicOpenTk
{
    public class Triangle : GameWindow
    {
        private VertexBuffer vertexBuffer;
        private VertexArray vertexArray;
        private ShaderProgram shaderProgram;

        public Triangle(int width = 1280, int height = 768, string title = "Triangle")
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

            this.IsVisible = true;

            GL.ClearColor(new Color4(0.3f, 0.4f, 0.5f, 1f));

            int x = 540;
            int y = 284;

            VertexPositionColor[] vertices = new VertexPositionColor[]
            {
                new VertexPositionColor(new Vector2(x + 200, y - 200), new Color4(1f, 0f, 0f, 1f)),
                new VertexPositionColor(new Vector2(x - 200, y - 200), new Color4(1f, 0f, 0f, 1f)),
                new VertexPositionColor(new Vector2(x      , y - 200), new Color4(1f, 0f, 0f, 1f)),
            };


            this.vertexBuffer = new VertexBuffer(VertexPositionColor.vertexInfo, vertices.Length, true);
            this.vertexBuffer.SetData(vertices, vertices.Length);

            this.vertexArray = new VertexArray(this.vertexBuffer);

            this.shaderProgram = new ShaderProgram();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            // 최적화에 필요 뒷면을 안그림, 속을 안 채움
            // GL.Enable(EnableCap.CullFace);
            // GL.CullFace(CullFaceMode.Back);
            // GL.FrontFace(FrontFaceDirection.Cw);

            // 알아두자
            // vertex array == vao
            // vertex buffer == vbo
            // -- index buffer (element array buffer) == ibo (ebo)

            string vertexShaderCode = @"
                #version 330 core

                uniform vec2 ViewportSize;

                layout (location = 0) in vec2 aPosition;
                layout (location = 1) in vec3 aColor;

                out vec3 color;
                
                void main()
                {
                    float nx = aPosition.x / ViewportSize.x * 2f - 1f;
                    float ny = aPosition.y / ViewportSize.y * 2f - 1f;
                    
                    gl_Position = vec4(nx, ny, 0f, 1f);

                    color = aColor;
                }
            ";

            string pixelShaderCode = @"
                #version 330 core

                in vec3 color;
                out vec4 pixelColor;

                void main()
                {
                    pixelColor = vec4(color, 1f);
                }
            ";

            this.shaderProgram.InitShaderProgram(vertexShaderCode, pixelShaderCode);

            // 화면크기를 가져옴
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);

            this.shaderProgram.SetUniform("ViewportSize", (float)viewport[2], (float)viewport[3]);

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            this.vertexArray.Dispose();
            this.vertexBuffer.Dispose();

            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(this.shaderProgram.ShaderprogramHandle);
            GL.BindVertexArray(this.vertexArray.VertexArrayHandle);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            this.Context.SwapBuffers();
            base.OnRenderFrame(args);
        }
    }
}
