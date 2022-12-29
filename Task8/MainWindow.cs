using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using CompGraph.Render;
using CompGraph.GameObjects;
using CompGraph.Tools;

namespace CompGraph;


class MainWindow : GameWindow
{
    public MainWindow() : base(800, 600, new GraphicsMode(0, 0, 0, 0), "WASD - Movement, Space - Freeze, R - Reload Image", GameWindowFlags.Default, DisplayDevice.Default, 4, 5, GraphicsContextFlags.Debug)
    {}
    
    public const int MaxGameObjectsSpheres = 256;
    public const int MaxGameObjectsCuboids = 64;
    private const float Epsilon = 0.005f;
    private const float Fov = 100;
    private Matrix4 _inverseProjection;
    private readonly Vector2 _nearFarPlane = new(Epsilon, 1000f);
    private readonly Camera _camera = new Camera(new Vector3(0f, 1f, 0), new Vector3(0, 1, 0), 0f, 0f);
    private readonly List<BaseGameObject> _gameObjects = new();
    private ShaderProgram _finalProgram;
    private BufferObject _basicDataUbo;
    private BufferObject _gameObjectsUbo;
    private PathTracer _pathTracer;
    private PostProcessor _postProcessor;
    private SkyboxCreator _skyboxCreator;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        GL.DepthMask(false);
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.Multisample);
        GL.Enable(EnableCap.TextureCubeMapSeamless);

        VSync = VSyncMode.On;
        CursorVisible = false;
        CursorGrabbed = true;

        _skyboxCreator = new SkyboxCreator();
        _skyboxCreator.Render();

        var rayDepth = 12;
        var sampleCount = 5;
        var focalLength = 20f;
            
        _pathTracer = new PathTracer(new Texture(TextureTarget2d.Texture2D), Width, Height, rayDepth, sampleCount, focalLength);
        _postProcessor = new PostProcessor(Width, Height);
        _finalProgram = new ShaderProgram(
            new Shader(ShaderType.VertexShader, File.ReadAllText("Shaders/screen.vert")),
            new Shader(ShaderType.FragmentShader, File.ReadAllText("Shaders/result.frag")));
            
        _basicDataUbo = new BufferObject();
        _basicDataUbo.ImmutableAllocate(Vector4.SizeInBytes * 4 * 2 + Vector4.SizeInBytes, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
        _basicDataUbo.BindRange(BufferRangeTarget.UniformBuffer, 0, 0, _basicDataUbo.Size);

        _gameObjectsUbo = new BufferObject();
        _gameObjectsUbo.ImmutableAllocate(Sphere.GpuInstanceSize * MaxGameObjectsSpheres + Cuboid.GpuInstanceSize * MaxGameObjectsCuboids, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
        _gameObjectsUbo.BindRange(BufferRangeTarget.UniformBuffer, 1, 0, _gameObjectsUbo.Size);

        LoadScene();
    }

    private void LoadScene()
    {
        var light = new Material(Vector3.One, new Vector3(10.0f, 10.0f, 10.0f), Vector3.Zero, 0, 0, 0, 0, 0);
        var plane = new Cuboid(new Vector3(0, 0, 0), new Vector3(10, 0.1f, 10), _pathTracer.NumCuboids++, Material.Zero);
        var stick = new Cuboid(new Vector3(0, 2.5f, 0), new Vector3(0.5f, 5f, 0.5f), _pathTracer.NumCuboids++, Material.Zero);
        var sun = new Sphere(new Vector3(4, 6, 0), 0.5f, _pathTracer.NumSpheres++, light);
        
        _gameObjects.Add(plane);
        _gameObjects.Add(stick);
        _gameObjects.Add(sun);
        foreach (var objects in _gameObjects)
            objects.Upload(_gameObjectsUbo);
    }
    
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        if (Focused)
        {
            _pathTracer.Render();

            _postProcessor.Render(_pathTracer.Result);
            GL.Viewport(0, 0, Width, Height);
            Framebuffer.Bind(0);
            _postProcessor.Result.AttachSampler(0);
            _finalProgram.Use();
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                
            SwapBuffers();
        }

        base.OnRenderFrame(e);
    }
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        if (Focused)
        {
            KeyboardManager.Update();
            MouseManager.Update();
            
            if (KeyboardManager.IsKeyDown(Key.Escape))
                Close();

            if (KeyboardManager.IsKeyTouched(Key.F11))
                WindowState = WindowState == WindowState.Normal ? WindowState.Fullscreen : WindowState.Normal;

            if (KeyboardManager.IsKeyTouched(Key.Space))
            {
                isPaused = !isPaused;
                CursorVisible = !CursorVisible;
                CursorGrabbed = !CursorGrabbed;
                
                if (!CursorVisible)
                {
                    MouseManager.Update();
                    _camera.SetCameraVelocity(Vector3.Zero);
                }
            }

            if (KeyboardManager.IsKeyTouched(Key.R))
            {
                _pathTracer.ResetRenderer();
            }

            if (!CursorVisible)
            {
                _camera.ProcessInputs((float)e.Time, out bool frameChanged);
                if (frameChanged)
                    _pathTracer.ResetRenderer();
            }
            _basicDataUbo.SubData(Vector4.SizeInBytes * 4, Vector4.SizeInBytes * 4, _camera.View.Inverted());
            _basicDataUbo.SubData(Vector4.SizeInBytes * 8, Vector4.SizeInBytes, _camera.GetCameraPos());
        }
        base.OnUpdateFrame(e);
    }
        
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        _pathTracer.SetSize(Width, Height);
        _postProcessor.SetSize(Width, Height);
        _inverseProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), Width / (float)Height, _nearFarPlane.X, _nearFarPlane.Y).Inverted();
        _basicDataUbo.SubData(0, Vector4.SizeInBytes * 4, _inverseProjection);
        _pathTracer.ResetRenderer();
    }

    protected override void OnFocusedChanged(EventArgs e)
    {
        if (Focused)
            MouseManager.Update();
    }
}