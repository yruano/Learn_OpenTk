using System;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace BasicOpenTk
{
    public class Square : GameWindow
    {
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private VertexArray vertexArray;
        private int shaderprogramHandle;


        private float x = 384f;
        private float y = 400f;
        private float w = 512f;
        private float h = 256f;

        public Square(int width = 1280, int height = 768, string title = "Square")
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

            VertexPositionColor[] vertices = new VertexPositionColor[]
            {
                new VertexPositionColor(new Vector2(x, y + h),      new Color4(1f, 0f, 0f, 1f)),
                new VertexPositionColor(new Vector2(x + w, y + h),  new Color4(0f, 1f, 0f, 1f)),
                new VertexPositionColor(new Vector2(x + w, y),      new Color4(0f, 0f, 1f, 1f)),
                new VertexPositionColor(new Vector2(x, y),          new Color4(1f, 1f, 0f, 0f)),
            };

            uint[] indices = new uint[]
            {
                0, 1, 2, 0, 2, 3
            };

            this.vertexBuffer = new VertexBuffer(VertexPositionColor.vertexInfo, vertices.Length, true);
            this.vertexBuffer.SetData(vertices, vertices.Length);
            
            this.indexBuffer = new IndexBuffer(indices.Length, true);
            this.indexBuffer.SetData(indices, indices.Length);

            this.vertexArray = new VertexArray(this.vertexBuffer);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            this.IsVisible = true;

            GL.ClearColor(new Color4(0.3f, 0.4f, 0.5f, 1f));

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

            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderCode);
            GL.CompileShader(vertexShaderHandle);

            // vertexShader 프로그래밍 오류를 출력함
            string vertexShaderInfo = GL.GetShaderInfoLog(vertexShaderHandle);
            if (vertexShaderInfo != String.Empty)
            {
                Console.WriteLine(vertexShaderInfo);
            }

            int pixelShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(pixelShaderHandle, pixelShaderCode);
            GL.CompileShader(pixelShaderHandle);

            // pixelShader 프로그래밍 오류를 출력함
            string pixelShaderInfo = GL.GetShaderInfoLog(pixelShaderHandle);
            if (pixelShaderInfo != String.Empty)
            {
                Console.WriteLine(pixelShaderInfo);
            }

            this.shaderprogramHandle = GL.CreateProgram();

            GL.AttachShader(this.shaderprogramHandle, vertexShaderHandle);
            GL.AttachShader(this.shaderprogramHandle, pixelShaderHandle);

            GL.LinkProgram(this.shaderprogramHandle);

            GL.DetachShader(this.shaderprogramHandle, vertexShaderHandle);
            GL.DetachShader(this.shaderprogramHandle, pixelShaderHandle);

            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(pixelShaderHandle);

            // 화면크기를 가져옴
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);

            GL.UseProgram(this.shaderprogramHandle);
            int viewportSizeUniformLocation = GL.GetUniformLocation(this.shaderprogramHandle, "ViewportSize");
            GL.Uniform2(viewportSizeUniformLocation, (float)viewport[2], (float)viewport[3]);
            GL.UseProgram(0);

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            this.vertexArray?.Dispose();
            this.indexBuffer?.Dispose();
            this.vertexBuffer?.Dispose();

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
            GL.BindVertexArray(this.vertexArray.VertexArrayHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.indexBuffer.IndexBufferHandle);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            this.Context.SwapBuffers();
            base.OnRenderFrame(args);
        }
    }
}