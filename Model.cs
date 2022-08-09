using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace BasicOpenTk
{
    public class Model : Game
    {
        private VertexBuffer? vertexBuffer;
        private VertexArray? vertexArray;
        private ShaderProgram? shaderProgram;

        public Model(string windowTitle, int initialWindowWidth, int initialWindowHeight) 
        : base(windowTitle, initialWindowWidth, initialWindowHeight) {}

        protected override void Initialize() {}

        protected override void LoadContent()
        {
           GL.ClearColor(new Color4(0.3f, 0.4f, 0.5f, 1f));

            float x = InitialWindowWidth / 2;
            float y = InitialWindowHeight /2;

            VertexPositionColor[] vertices = new VertexPositionColor[]
            {
                new VertexPositionColor(new Vector2(x + 200, y      ), new Color4(1f, 0f, 0f, 1f)),
                new VertexPositionColor(new Vector2(x - 200, y      ), new Color4(1f, 0f, 0f, 1f)),
                new VertexPositionColor(new Vector2(x      , y + 200), new Color4(1f, 0f, 0f, 1f)),
            };

            this.vertexBuffer = new VertexBuffer(VertexPositionColor.vertexInfo, vertices.Length, true);
            this.vertexBuffer.SetData(vertices, vertices.Length);

            this.vertexArray = new VertexArray(this.vertexBuffer);

            this.shaderProgram = new ShaderProgram();

            string vertexShaderCode = @"
                #version 330 core

                uniform vec2 ViewportSize;

                layout (location = 0) in vec2 aPosition;
                layout (location = 1) in vec4 aColor;

                out vec4 color;
                
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

                in vec4 color;
                out vec4 pixelColor;

                void main()
                {
                    pixelColor = color;
                }
            ";

            this.shaderProgram.InitShaderProgram(vertexShaderCode, pixelShaderCode);
        }

        protected override void Update(GameTime gameTime)
        {
            // 화면크기를 가져옴
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
            
        }

        protected override void Render(GameTime gameTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            this.shaderProgram!.SetUniform("ViewportSize", (float)gameWindow!.Size.X, (float)gameWindow!.Size.Y);
            GL.UseProgram(this.shaderProgram!.ShaderprogramHandle);
            GL.BindVertexArray(this.vertexArray!.VertexArrayHandle);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }
    }
}