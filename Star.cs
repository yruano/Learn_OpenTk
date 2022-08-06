using System;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace BasicOpenTk
{
    public class Star : GameWindow
    {
        private VertexBuffer vertexBuffer;
        private ShaderProgram shaderProgram;
        private VertexArray vertexArray;
        private IndexBuffer indexBuffer;

        public Star(int width = 1280, int height = 768, string title = "Triangle")
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

            GL.ClearColor(new Color4(0.3f, 0.4f, 0.5f, 1f));

            int x = 540;
            int y = 284;
            int w = 300;
            int h = 300;

            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            vertices[0] = new VertexPositionTexture(new Vector2(x, y + h), new Vector2(0, 1));
            vertices[1] = new VertexPositionTexture(new Vector2(x + w, y + h), new Vector2(1, 1));
            vertices[2] = new VertexPositionTexture(new Vector2(x + w, y), new Vector2(1, 0));
            vertices[3] = new VertexPositionTexture(new Vector2(x, y), new Vector2(0, 0));

            uint[] indices = new uint[6];

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;

            this.vertexBuffer = new VertexBuffer(VertexPositionTexture.vertexInfo, vertices.Length, true);
            this.vertexBuffer.SetData(vertices, vertices.Length);

            this.indexBuffer = new IndexBuffer(indices.Length, true);
            this.indexBuffer.SetData(indices, indices.Length);

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
            this.IsVisible = true;

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
                uniform float ColorFactor;
                
                layout (location = 0) in vec2 aPosition;
                layout (location = 1) in vec2 Uv;

                out vec2 v_Uv;
                
                void main()
                {
                    float nx = aPosition.x / ViewportSize.x * 2f - 1f;
                    float ny = aPosition.y / ViewportSize.y * 2f - 1f;
                    
                    v_Uv = Uv;

                    gl_Position = vec4(nx, ny, 0f, 1f);
                }
            ";

            string pixelShaderCode = @"
                #version 330 core

                in vec2 v_Uv;

                vec2 Uv = v_Uv;

                vec4 color = vec4(0.0f, 0.0f, 0.0f, 1.0f);
                float r1 = 0.4;
                float r2 = 0.2;

                vec4 color1 = vec4(0.0f, 1.0f, 0.0f, 1.0f);
                vec4 color2 = vec4(1.0f, 0.0f, 0.0f, 1.0f);
                vec4 color3 = vec4(0.0f, 0.0f, 1.0f, 1.0f);

                out vec4 pixelColor;

                void main()
                {
                    float line = step((0.7 * Uv.x) - 0.1169, Uv.y);
                    float line1 = step((-0.7 * Uv.x) + 0.5891, Uv.y);
                    float line2 = step(Uv.y, 0.5891);
                    float line3 = step(Uv.y, (3 * Uv.x) - 0.5);
                    float line4 = step(Uv.y, (-3 * Uv.x) + 2.5);

                    float all_line = line * line1 * line2 * line3 * line4;

                    float leg = (1 - line2) * (1 - line4);
                    float leg1 = (1 - line) * (1 - line4);
                    float leg2 = (1 - line) * (1 - line1);
                    float leg3 = (1 - line1) * (1 - line3);
                    float leg4 = (1 - line2) * (1 - line3);

                    color = (1 - leg) * (1 - leg1) * (1 - leg2) * (1 - leg3) * (1 - leg4) * color1;

                    pixelColor = color;
                }
            ";

            //  별의 안을 체움
            // color = color2 + ((1 - line2) * ((1 - line3) + (1 - line4)) * (color1 - color2) + (1 - line4) * ((1 - line2) + (1 - line)) * (color1 - color2) + (1 - line) * ((1 - line1) + (1 - line4)) * (color1 - color2) + (1 - line1) * ((1 - line3) + (1 - line)) * (color1 - color2));

            // 원 쉐이더
            // vec2 center = vec2(0.5, 0.5);

            // float len = length(Uv - center);

            // float is_out1 = step(r1, len);
            // float is_out2 = step(r2, len);

            // color = (1 - is_out1) * color1 + (is_out1 * color2) + (1 - is_out2) * (color3 - color1);

            // pixelColor = color;

            this.shaderProgram.InitShaderProgram(vertexShaderCode, pixelShaderCode);

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            this.vertexArray.Dispose();
            this.indexBuffer.Dispose();
            this.vertexBuffer.Dispose();

            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            // 화면크기를 가져옴
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);

            this.shaderProgram.SetUniform("ViewportSize", (float)viewport[2], (float)viewport[3]);

            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(this.shaderProgram.ShaderprogramHandle);
            GL.BindVertexArray(this.vertexArray.VertexArrayHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.indexBuffer.IndexBufferHandle);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            this.Context.SwapBuffers();
            base.OnRenderFrame(args);
        }
    }
}
