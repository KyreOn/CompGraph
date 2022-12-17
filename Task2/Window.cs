using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using CompGraph.Common;
using OpenTK.Mathematics;

namespace CompGraph;

enum Dir
{
    North,
    South,
    West,
    East,
    All
}
public class Window : GameWindow
{
    private int _vertexBufferObject;

    private int _vertexArrayObject;

    private Shader _shader;
        
    private int _elementBufferObject;

    private int _animationTick;
    
    private Camera _camera;

    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings)
    {
    }
    
    
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);
        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();
        if (KeyboardState.IsKeyDown(Keys.W))
        {
            _camera.Position += _camera.Front * (float)e.Time; // Forward
        }

        if (KeyboardState.IsKeyDown(Keys.S))
        {
            _camera.Position -= _camera.Front * (float)e.Time; // Backwards
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, Size.X, Size.Y);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);
        _animationTick++;
        
        LoadPolygon(4, 90);
        DrawPolygon(10);
    }
    
    private void LoadFigure(float[] vertices, uint[] indices)
    {
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        
        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
            
        _elementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            
        _shader = new Shader("../../../Shaders/vertex_shader.glsl", "../../../Shaders/fragment_shader.glsl");
        _shader.Use();
    }
    private void LoadTriangle()
    {
        var vertices = new float[]
        {
            -0.5f, -0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            0.0f, 0.5f, 0.0f
        };

        var indices = new uint[]
        {
            0, 1, 2
        };
        
        LoadFigure(vertices, indices);
    }
    private void LoadRectangle()
    {
        var vertices = new float[]
        {
            0.5f, 0.5f, 0.0f, // top right
            0.5f, -0.5f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, // bottom left
            -0.5f, 0.5f, 0.0f, // top left
        };
        
        var indices = new uint[]
        {
            0, 1, 3,
            1, 2, 3 
        };
        LoadFigure(vertices, indices);
    }
    private void LoadPolygon(int size, float angle, float scale = 1, float xOffset = 0, float yOffset = 0)
    {
        var curAngle = MathHelper.DegreesToRadians(angle);
        var angleDiff = 2 * MathF.PI / size;
        var vertices = new float[3 * (size + 2)];
        vertices[0] = xOffset;
        vertices[1] = yOffset;
        for (var i = 3; i < 3 * (size + 2); i+=3)
        {
            vertices[i] = xOffset + scale * 0.5f * MathF.Cos(curAngle);
            vertices[i + 1] = yOffset + scale * 0.5f * MathF.Sin(curAngle);
            vertices[i + 2] = 0;
            curAngle += angleDiff;
        }

        uint curIndex = 1;
        var indices = new uint[size * 3];
        for (var i = 0; i < size * 3; i+=3)
        {
            indices[i] = 0;
            indices[i + 1] = curIndex;
            indices[i + 2] = curIndex + 1;
            curIndex++;
        }
        LoadFigure(vertices, indices);
    }
    private void LoadPolygonWithRot(int size, float scale = 0, float ancX = 0, float ancY = 0)
    {
        var radius = MathF.Sqrt(ancX * ancX + ancY * ancY);
        var curAngle = 0f;
        var angleDiff = 2 * MathF.PI / size;
        var vertices = new float[3 * (size + 2)];
        vertices[0] = ancX + radius;
        vertices[1] = ancY;
        for (var i = 3; i < 3 * (size + 2); i+=3)
        {
            vertices[i] = vertices[0] + scale * 0.5f * MathF.Cos(curAngle + 0.005f * _animationTick);
            vertices[i + 1] = vertices[1] + scale * 0.5f * MathF.Sin(curAngle + 0.005f * _animationTick);
            vertices[i + 2] = 0;
            curAngle += angleDiff;
        }

        var startX = vertices[0];
        var startY = vertices[1];
        vertices[0] = ancX + radius * MathF.Cos(0.005f * _animationTick);
        vertices[1] = ancY + radius * MathF.Sin(0.005f * _animationTick);
        for (var i = 3; i < 3 * (size + 2); i += 3)
        {
            vertices[i] += vertices[0] - startX;
            vertices[i + 1] += vertices[1] - startY;
        }
        
        uint curIndex = 1;
        var indices = new uint[size * 3];
        for (var i = 0; i < size * 3; i+=3)
        {
            indices[i] = 0;
            indices[i + 1] = curIndex;
            indices[i + 2] = curIndex + 1;
            curIndex++;
        }
        LoadFigure(vertices, indices);
    }
    private void LoadPolygonWithFunc(int size, float scale = 0)
    {
        var curAngle = 0f;
        var angleDiff = 2 * MathF.PI / size;
        var vertices = new float[3 * (size + 2)];
        for (var i = 3; i < 3 * (size + 2); i+=3)
        {
            vertices[i] = scale * 0.5f * MathF.Cos(curAngle);
            vertices[i + 1] = scale * 0.5f * MathF.Sin(curAngle);
            vertices[i + 2] = 0;
            curAngle += angleDiff;
        }
        
        var startX = vertices[0];
        var startY = vertices[1];
        vertices[0] = -1f + 0.005f * _animationTick;
        vertices[1] = 0.5f * MathF.Sin(0.05f * _animationTick);
        
        for (var i = 3; i < 3 * (size + 2); i += 3)
        {
            vertices[i] += vertices[0] - startX;
            vertices[i + 1] += vertices[1] - startY;
        }

        uint curIndex = 1;
        var indices = new uint[size * 3];
        for (var i = 0; i < size * 3; i+=3)
        {
            indices[i] = 0;
            indices[i + 1] = curIndex;
            indices[i + 2] = curIndex + 1;
            curIndex++;
        }
        LoadFigure(vertices, indices);
    }
    private void LoadSquare(float xOffset = 0, float yOffset = 0, float scale = 1)
    {
        LoadPolygon(4, scale, xOffset, yOffset);
    }

    private void LoadFractal(float xC, float yC, float scale, int iter, Dir dir)
    {
        LoadSquare(xC, yC, scale);
        DrawPolygon(4);

        if (iter <= 0) return;
        
        if (dir is not Dir.North or Dir.All) LoadFractal(xC, yC + 1.4f * scale / 4 / MathF.Sin(MathF.Atan(1)), scale / 2.5f,  iter - 1, Dir.South);
        if (dir is not Dir.West or Dir.All) LoadFractal(xC + 1.4f * scale / 4 / MathF.Sin(MathF.Atan(1)), yC, scale / 2.5f,  iter - 1, Dir.East);
        if (dir is not Dir.South or Dir.All) LoadFractal(xC, yC - 1.4f * scale / 4 / MathF.Sin(MathF.Atan(1)), scale / 2.5f,  iter - 1, Dir.North);
        if (dir is not Dir.East or Dir.All) LoadFractal(xC - 1.4f * scale / 4 / MathF.Sin(MathF.Atan(1)), yC, scale / 2.5f,  iter - 1, Dir.West);
    }

    private void DrawFigure(int indCount)
    {
        //GL.Clear(ClearBufferMask.ColorBufferBit);

        _shader.Use();
            
        GL.BindVertexArray(_vertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, indCount, DrawElementsType.UnsignedInt, 0);

        SwapBuffers();
    }
    private void DrawTriangle()
    {
        DrawFigure(3);
    }
    private void DrawRectangle()
    {
        DrawFigure(6);
    }
    private void DrawPolygon(int size)
    {
        DrawFigure(size*3);
    }
    private void DrawCircle()
    {
        DrawPolygon(1000);
    }
}  