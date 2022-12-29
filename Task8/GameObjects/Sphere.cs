using System;
using CompGraph;
using CompGraph.Tools;
using OpenTK;

namespace CompGraph.GameObjects;

class Sphere : BaseGameObject
{
    public const int GpuInstanceSize = 16 + Material.GpuInstanceSize;

    private readonly int _instance;
    private readonly float _radius;
    public Sphere(Vector3 position, float radius, int instance, Material material)
    {
        Material = material;
        Position = position;
        _radius = radius;
        _instance = instance;
    }

    protected override int BufferOffset => 0 + _instance * GpuInstanceSize;
    private readonly Vector4[] _gpuData = new Vector4[GpuInstanceSize / Vector4.SizeInBytes];

    protected override Vector4[] GetGPUFriendlyData()
    {
        _gpuData[0].Xyz = Position;
        _gpuData[0].W = _radius;

        Array.Copy(Material.GetGPUFriendlyData(), 0, _gpuData, 1, _gpuData.Length - 1);

        return _gpuData;
    }
}