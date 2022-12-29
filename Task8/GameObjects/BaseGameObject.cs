using OpenTK;
using CompGraph.Tools;

namespace CompGraph.GameObjects;

abstract class BaseGameObject
{
    public Material Material;
    public Vector3 Position;
    protected abstract int BufferOffset { get; }

    protected abstract Vector4[] GetGPUFriendlyData();

    public void Upload(BufferObject buffer)
    {
        var data = GetGPUFriendlyData();
        buffer.SubData(BufferOffset, Vector4.SizeInBytes * data.Length, data);
    }
}