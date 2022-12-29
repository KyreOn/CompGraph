using System;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using CompGraph.Tools;

namespace CompGraph.Render;

class SkyboxCreator
{
    private static int ISteps { set => ShaderProgram.Upload("iSteps", value); }
    private static int JSteps { set => ShaderProgram.Upload("jSteps", value); }
    private static float LightIntensity { set => ShaderProgram.Upload("lightIntensity", Math.Max(value, 0.0f)); }

    private static float Time
    {
        set => ShaderProgram.Upload("lightPos", new Vector3(0.0f, MathF.Sin(MathHelper.DegreesToRadians(value * 360.0f)), 
            MathF.Cos(MathHelper.DegreesToRadians(value * 360.0f))) * 149600000e3f);
    }
        
    public readonly Texture Result;
    private static readonly ShaderProgram ShaderProgram = new(new Shader(ShaderType.ComputeShader, File.ReadAllText("Shaders/skybox.comp")));

    public SkyboxCreator()
    {
        Result = new Texture(TextureTarget2d.TextureCubeMap);
        Result.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Linear);
        Result.MutableAllocate(256, 256, 1, PixelInternalFormat.Rgba32f);
            
        var bufferObject = new BufferObject();
        bufferObject.ImmutableAllocate(Vector4.SizeInBytes * 4 * 7 + Vector4.SizeInBytes, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
        bufferObject.BindRange(BufferRangeTarget.UniformBuffer, 2, 0, bufferObject.Size);

        var invProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), 1, 0.1f, 10f).Inverted();
        var invViews = new[]
        {
            Camera.GenerateMatrix(Vector3.Zero, new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)).Inverted(), // PositiveX
            Camera.GenerateMatrix(Vector3.Zero, new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)).Inverted(), // NegativeX
               
            Camera.GenerateMatrix(Vector3.Zero, new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)).Inverted(), // PositiveY
            Camera.GenerateMatrix(Vector3.Zero, new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, -1.0f)).Inverted(), // NegativeY

            Camera.GenerateMatrix(Vector3.Zero, new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, -1.0f, 0.0f)).Inverted(), // PositiveZ
            Camera.GenerateMatrix(Vector3.Zero, new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, -1.0f, 0.0f)).Inverted(), // NegativeZ
        };

        bufferObject.SubData(0, Vector4.SizeInBytes * 4, invProjection);
        bufferObject.SubData(Vector4.SizeInBytes * 4, Vector4.SizeInBytes * 4 * invViews.Length, invViews);

        Time = 0.5f;
        ISteps = 50;
        JSteps = 15;
        LightIntensity = 15.0f;
    }
        
    public void Render()
    {
        Result.AttachImage(0, 0, true, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);
        ShaderProgram.Use();

        GL.DispatchCompute((Result.Width + 8 - 1) / 8, (Result.Width + 8 - 1) / 8, 6);
        GL.MemoryBarrier(MemoryBarrierFlags.TextureFetchBarrierBit);
    }
}