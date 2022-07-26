using System;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace BasicOpenTk
{
    public class Square : GameWindow
    {
        private int vertexBufferHandle;
        private int shaderprogramHandle;
        private int vertexArrayHandle;
        private int indexBufferHandle;

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

            float x = 384f;
            float y = 400f;
            float w = 512f;
            float h = 256f;

            // float[] vertices = new float[]
            // {
            //     /* vertex0 */  x    , y + h, /* color0 (R) */ 1.0f, 0.0f, 0.0f,
            //     /* vertex1 */  x + w, y + h, /* color1 (G) */ 0.0f, 1.0f, 0.0f,
            //     /* vertex2 */  x + w, y    , /* color2 (B) */ 0.0f, 0.0f, 1.0f,
            //     /* vertex3 */  x    , y    , /* color3 (y) */ 1.0f, 1.0f, 0.0f,
            // };

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

            int vertexSizeInBytes = VertexPositionColor.vertexInfo.SizeInBytes;

            this.vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(this.vertexArrayHandle);

            this.vertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * vertexSizeInBytes, vertices, BufferUsageHint.StaticDraw);

            this.indexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.indexBufferHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            
            VertexAttribute attr0 = VertexPositionColor.vertexInfo.VertexAttributes[0];
            VertexAttribute attr1 = VertexPositionColor.vertexInfo.VertexAttributes[1];
            
            GL.VertexAttribPointer(attr0.Index, attr0.ComponentCount, VertexAttribPointerType.Float, false, vertexSizeInBytes, attr0.Offset);
            GL.VertexAttribPointer(attr1.Index, attr1.ComponentCount, VertexAttribPointerType.Float, false, vertexSizeInBytes, attr1.Offset);
            
            GL.EnableVertexAttribArray(attr0.Index);
            GL.EnableVertexAttribArray(attr1.Index);

            GL.BindVertexArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);


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
                    // pixelColor = vec4(0.8f, 0.8f, 0.1f, 1f);
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
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(this.vertexArrayHandle);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DeleteBuffer(this.indexBufferHandle);

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
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            this.Context.SwapBuffers();
            base.OnRenderFrame(args);
        }
    }
}