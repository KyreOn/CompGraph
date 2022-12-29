using System;
using OpenTK.Graphics.OpenGL4;

namespace CompGraph.Tools;

public class BufferObject : IDisposable
{
    private readonly int _id;
    public int Size { get; private set; }

    public BufferObject() => GL.CreateBuffers(1, out _id);
    public void BindRange(BufferRangeTarget bufferRangeTarget, int index, int offset, int size) => GL.BindBufferRange(bufferRangeTarget, index, _id, (IntPtr)offset, size);
    public void SubData<T>(int offset, int size, T data) where T : struct => GL.NamedBufferSubData(_id, (IntPtr)offset, size, ref data);
    public void SubData<T>(int offset, int size, T[] data) where T : struct => GL.NamedBufferSubData(_id, (IntPtr)offset, size, data);

    public void ImmutableAllocate(int size, IntPtr data, BufferStorageFlags bufferStorageFlags)
    {
        GL.NamedBufferStorage(_id, size, data, bufferStorageFlags);
        Size = size;
    }

    public void Dispose() => GL.DeleteBuffer(_id);
}