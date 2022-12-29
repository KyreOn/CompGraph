using System;
using OpenTK.Graphics.OpenGL4;

namespace CompGraph.Tools;

class Framebuffer : IDisposable
{
    private static int _lastBindedId = -1;

    private readonly int _id;
    public Framebuffer()
    {
        GL.CreateFramebuffers(1, out _id);
    }

    public void AddRenderTarget(FramebufferAttachment framebufferAttachment, Texture texture) => GL.NamedFramebufferTexture(_id, framebufferAttachment, texture.Id, 0);

    public void Bind(FramebufferTarget framebufferTarget = FramebufferTarget.Framebuffer)
    {
        if (_lastBindedId != _id)
        {
            GL.BindFramebuffer(framebufferTarget, _id);
            _lastBindedId = _id;
        }  
    }

    public static void Bind(int id, FramebufferTarget framebufferTarget = FramebufferTarget.Framebuffer)
    {
        if (_lastBindedId != id)
        {
            GL.BindFramebuffer(framebufferTarget, id);
            _lastBindedId = id;
        }
    }

    public void Dispose() => GL.DeleteFramebuffer(_id);
}