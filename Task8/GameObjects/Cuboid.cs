using System;
using CompGraph;
using CompGraph.Tools;
using OpenTK;

namespace CompGraph.GameObjects;

class Cuboid : BaseGameObject
{
    public const int GpuInstanceSize = 16 * 2 + Material.GpuInstanceSize;
    public Vector3 Dimensions;
    private readonly int _instance;

    public Cuboid(Vector3 position, Vector3 dimensions, int instance, Material material)
    {
        Material = material;
        Position = position;
        Dimensions = dimensions;
        _instance = instance;
    }

    protected override int BufferOffset => Sphere.GpuInstanceSize * MainWindow.MaxGameObjectsSpheres + _instance * GpuInstanceSize;
    private Vector3 Min => Position - Dimensions * 0.5f;
    private Vector3 Max => Position + Dimensions * 0.5f;
    private readonly Vector4[] _gpuData = new Vector4[GpuInstanceSize / Vector4.SizeInBytes];

    protected override Vector4[] GetGPUFriendlyData()
    {
        _gpuData[0].Xyz = Min;
        _gpuData[1].Xyz = Max;

        Array.Copy(Material.GetGPUFriendlyData(), 0, _gpuData, 2, _gpuData.Length - 2);
            
        return _gpuData;
    }
}