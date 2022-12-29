using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using CompGraph.Tools;

namespace CompGraph.Render;

class PathTracer
{
    private int _numSpheres;
    public int NumSpheres
    {
        get => _numSpheres;

        set
        {
            _numSpheres = value;
            ShaderProgram.Upload("uboGameObjectsSize", new Vector2(value, NumCuboids));
        }
    }
    private int _numCuboids;
    public int NumCuboids
    {
        get => _numCuboids;

        set
        {
            _numCuboids = value;
            ShaderProgram.Upload("uboGameObjectsSize", new Vector2(NumSpheres, value));
        }
    }
    private int RayDepth { init => ShaderProgram.Upload("rayDepth", value); }
    private int SPP { init => ShaderProgram.Upload("SPP", value); }
    private float FocalLength { init => ShaderProgram.Upload("focalLength", value); }
    
    private readonly Texture _environmentMap;
    public readonly Texture Result;
    private readonly Framebuffer _framebuffer;
    private static readonly ShaderProgram ShaderProgram = new(
        new Shader(ShaderType.VertexShader, File.ReadAllText("Shaders/screen.vert")),
        new Shader(ShaderType.FragmentShader, File.ReadAllText("Shaders/pathTracing.frag")));
    private int _thisRenderNumFrame;
        
    public PathTracer(Texture environmentMap, int width, int height, int rayDepth, int spp, float focalLength)
    {
        Result = new Texture(TextureTarget2d.Texture2D);
        Result.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
        Result.MutableAllocate(width, height, 1, PixelInternalFormat.Rgba32f);
        _framebuffer = new Framebuffer();
        _framebuffer.AddRenderTarget(FramebufferAttachment.ColorAttachment0, Result);
        RayDepth = rayDepth;
        SPP = spp;
        FocalLength = focalLength;
        _environmentMap = environmentMap;
    }
        
    public void Render()
    {
        ShaderProgram.Use();
        ShaderProgram.Upload(0, _thisRenderNumFrame++);
        _environmentMap.AttachSampler(1);
        _framebuffer.Bind();
        Result.AttachSampler(0);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    public void SetSize(int width, int height)
    {
        ResetRenderer();
        Result.MutableAllocate(width, height, 1, Result.PixelInternalFormat);
    }

    public void ResetRenderer() => _thisRenderNumFrame = 0;
}