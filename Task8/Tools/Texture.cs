using System;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace CompGraph.Tools;

class Texture : IDisposable
{
    private enum TextureDimension
    {
        One = 1,
        Two = 2,
        Three = 3,
    }

    public readonly int Id;
    public readonly TextureTarget Target;
    private readonly TextureDimension _dimension;
    public int Width { get; private set; }
    public PixelInternalFormat PixelInternalFormat { get; private set; }

    public Texture(TextureTarget2d textureTarget2D)
    {
        Target = (TextureTarget)textureTarget2D;
        _dimension = TextureDimension.Two;

        GL.CreateTextures(Target, 1, out Id);
    }

    public void SetFilter(TextureMinFilter minFilter, TextureMagFilter magFilter)
    {
        GL.TextureParameter(Id, TextureParameterName.TextureMinFilter, (int)minFilter);
        GL.TextureParameter(Id, TextureParameterName.TextureMagFilter, (int)magFilter);
    }

    private void Bind() => GL.BindTexture(Target, Id);

    public void AttachImage(int unit, int level, bool layered, int layer, TextureAccess textureAccess, SizedInternalFormat sizedInternalFormat) 
        => GL.BindImageTexture(unit, Id, level, layered, layer, textureAccess, sizedInternalFormat);

    public void AttachSampler(int unit) => GL.BindTextureUnit(unit, Id);

    public void MutableAllocate(int width, int height, int depth, PixelInternalFormat pixelInternalFormat)
    {
        Bind();
        switch (_dimension)
        {
            case TextureDimension.One:
                GL.TexImage1D(Target, 0, pixelInternalFormat, width, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                Width = width;
                break;

            case TextureDimension.Two:
                if (Target == TextureTarget.TextureCubeMap)
                    for (var i = 0; i < 6; i++)
                        GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, pixelInternalFormat, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                else
                    GL.TexImage2D(Target, 0, pixelInternalFormat, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                Width = width;
                break;

            case TextureDimension.Three:
                GL.TexImage3D(Target, 0, pixelInternalFormat, width, height, depth, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                Width = width;
                break;

            default:
                return;
        }
        PixelInternalFormat = pixelInternalFormat;
    }

    public void Dispose() => GL.DeleteTexture(Id);
}