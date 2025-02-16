using CubeEngine.Engine.MeshObject;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace CubeEngine.Engine.Window
{
    public sealed class CubeGameWindow : GameWindow
    {
        private static CubeGameWindow _instance;
        public static CubeGameWindow Instance 
        { 
            get 
            { 
                return _instance; 
            } 
            private set
            {
                _instance = value;
            }
        }

        public GameScene CurrentGameScene { get; private set; }

        public int WindowWidth { get; private set; }
        public int Windowheight { get; private set; }

        public CubeGameWindow(GameScene currentGameScene, int width = 1920, int height = 1080, string title = "Game1") : base(
            GameWindowSettings.Default,
            new NativeWindowSettings()
            {
                Title = title,
                ClientSize = new Vector2i(width, height),
                WindowBorder = WindowBorder.Fixed,
                StartVisible = false,
                StartFocused = true,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(3, 3),
            })
        {
            Instance = this;
            CurrentGameScene = currentGameScene;
            WindowWidth = width;
            Windowheight = height;

            CenterWindow();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            IsVisible = true;
            GL.Enable(EnableCap.DepthTest);

            base.OnLoad();
        }

        protected override void OnUnload()
        {

            var currentGameObjects = CurrentGameScene.GameObjects;

            for (int i = 0; i < currentGameObjects.Count; i++)
            {
                CurrentGameScene.RemoveGameObject(currentGameObjects[i]);
            }
            
            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            
            CurrentGameScene.Update();

            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(new Color4(0.1f, 0.8f, 0.8f, 1f));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            CurrentGameScene.Render();

            Context.SwapBuffers();

            base.OnRenderFrame(args);
        }
    }
}
