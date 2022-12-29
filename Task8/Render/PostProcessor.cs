using System.IO;
using OpenTK.Graphics.OpenGL4;
using CompGraph.Tools;

namespace CompGraph.Render;

class PostProcessor
{
    public readonly Texture Result;
    private readonly Framebuffer _framebuffer;
    private readonly ShaderProgram _shaderProgram;

    private static readonly Shader VertexShader = new(ShaderType.VertexShader, File.ReadAllText("Shaders/screen.vert"));
    private static readonly Shader FragmentShader = new(ShaderType.FragmentShader, File.ReadAllText("Shaders/postprocess.frag"));

    public PostProcessor(int width, int height)
    {
        _framebuffer = new Framebuffer();
            
        Result = new Texture(TextureTarget2d.Texture2D);
        Result.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        Result.MutableAllocate(width, height, 1, PixelInternalFormat.Rgba8);

        _framebuffer.AddRenderTarget(FramebufferAttachment.ColorAttachment0, Result);
        _shaderProgram = new ShaderProgram(VertexShader, FragmentShader);
    }

    public void Render(params Texture[] textures)
    {
        _framebuffer.Bind();
        _shaderProgram.Use();

        for (var i = 0; i < textures.Length; i++)
            textures[i].AttachSampler(i);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    public void SetSize(int width, int height) => Result.MutableAllocate(width, height, 1, Result.PixelInternalFormat);
}