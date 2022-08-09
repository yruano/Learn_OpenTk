using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using ImGuiNET;
using Dear_ImGui_Sample;

namespace BasicOpenTk
{
    public abstract class Game
    {
        protected GameWindow? gameWindow;
        protected string WindowTitle { get; set; }
        protected int InitialWindowWidth { get; set; }
        protected int InitialWindowHeight { get; set; }

        private GameWindowSettings _gameWindowSettings = GameWindowSettings.Default;
        private NativeWindowSettings _nativeWindowSettings = NativeWindowSettings.Default;

        protected ImGuiController? controller;

        public Game(string windowTitle, int initialWindowWidth, int initialWindowHeight)
        {
            WindowTitle = windowTitle;
            InitialWindowWidth = initialWindowWidth;
            InitialWindowHeight = initialWindowHeight;

            _nativeWindowSettings.Size = new Vector2i(initialWindowWidth, initialWindowHeight);
            _nativeWindowSettings.Title = windowTitle;
        }

        public void Run()
        {
            gameWindow = new GameWindow(_gameWindowSettings, _nativeWindowSettings);
            GameTime gameTime = new();

            Initialize();

            gameWindow.Load += () =>
            {
                controller = new ImGuiController(gameWindow.Size.X, gameWindow.Size.Y);
                LoadContent();
            };

            gameWindow.UpdateFrame += (FrameEventArgs eventArgs) =>
            {
                double time = eventArgs.Time;
                gameTime.ElapsedGameTime = TimeSpan.FromMilliseconds(time);
                gameTime.TotalGameTime += TimeSpan.FromMilliseconds(time);
                Update(gameTime);
            };

            gameWindow.RenderFrame += (FrameEventArgs eventArgs) =>
            {
                controller!.Update(gameWindow, (float)eventArgs.Time);

                Render(gameTime);
                controller!.Render();
                gameWindow.SwapBuffers();
            };

            gameWindow.Resize += (ResizeEventArgs) =>
            {
                GL.Viewport(0, 0, gameWindow.Size.X, gameWindow.Size.Y);
                controller!.WindowResized(gameWindow.Size.X, gameWindow.Size.Y);
            };

            gameWindow.TextInput += (TextInputEventArgs eventArgs) =>
            {
                controller!.PressChar((char)eventArgs.Unicode);
            };
            gameWindow.MouseWheel += (MouseWheelEventArgs eventArgs) =>
            {
                controller!.MouseScroll(eventArgs.Offset);
            };

            gameWindow.Run();
        }

        ~Game()
        {
            gameWindow!.Dispose();
        }

        protected abstract void Initialize();
        protected abstract void LoadContent();
        protected abstract void Update(GameTime gameTime);
        protected abstract void Render(GameTime gameTime);
    }
}