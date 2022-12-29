using System;
using OpenTK;

namespace CompGraph.Tools;

class Material
{
    public static Material Zero => new(Vector3.One, Vector3.Zero, Vector3.Zero, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
    public const int GpuInstanceSize = 16 * 4;

    public Vector3 Albedo;
    public Vector3 Emissive;
    public Vector3 AbsorbanceColor;
    public float SpecularChance;
    public float SpecularRoughness;
    public float IOR;
    public float RefractionChance;
    public float RefractionRoughnes;
    public Material(Vector3 albedo, Vector3 emissive, Vector3 refractionColor, float specularChance, float specularRoughness, float indexOfRefraction, float refractionChance, float refractionRoughnes)
    {
        Albedo = albedo;
        Emissive = emissive;
        AbsorbanceColor = refractionColor;
        SpecularChance = Math.Clamp(specularChance, 0.0f, 1.0f);
        SpecularRoughness = specularRoughness;
        IOR = Math.Max(indexOfRefraction, 1.0f);
        RefractionChance = Math.Clamp(refractionChance, 0.0f, 1.0f - SpecularChance);
        RefractionRoughnes = refractionRoughnes;
    }

    private readonly Vector4[] _gpuData = new Vector4[GpuInstanceSize / Vector4.SizeInBytes];
    public Vector4[] GetGPUFriendlyData()
    {
        _gpuData[0].Xyz = Albedo;
        _gpuData[0].W = SpecularChance;
           
        _gpuData[1].Xyz = Emissive;
        _gpuData[1].W = SpecularRoughness;

        _gpuData[2].Xyz = AbsorbanceColor;
        _gpuData[2].W = RefractionChance;

        _gpuData[3].X = RefractionRoughnes;
        _gpuData[3].Y = IOR;

        return _gpuData;
    }

    private static readonly Random Random = new();
    public static Material GetRndMaterial()
    {
        var isEmissive = Random.NextDouble() < 0.2;
        return new Material(RndVector3(), isEmissive ? RndVector3() : Vector3.Zero, RndVector3() * 2.0f, (float)Random.NextDouble() * 0.5f, (float)Random.NextDouble(), (float)Random.NextDouble() + 1, (float)Random.NextDouble() * 0.5f, (float)Random.NextDouble());
    }

    private static Vector3 RndVector3() => new((float)Random.NextDouble(), (float)Random.NextDouble(), (float)Random.NextDouble());
}