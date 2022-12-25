using System;
using CompGraph.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;

namespace CompGraph;

class Figure
{
    public int Vao;
    public Vector3 Pos;
    public Vector3 Rot;
    public Vector3 Col;
    public int IndCount;
    public Texture Texture;
}
// In this tutorial we focus on how to set up a scene with multiple lights, both of different types but also
// with several point lights
public class Window : GameWindow
{
    private float _timer;
    private readonly Vector3[] _pointLightPositions =
    {
        new Vector3(10, 20, 0),
        new Vector3(-10, 20, 0),
        new Vector3(0, 20, 10),
        new Vector3(0, 20, -10),
        new Vector3(10, -10, 0),
        new Vector3(-10, -10, 0),
        new Vector3(0, -10, 10),
        new Vector3(0, -10, -10),
    };

    private List<Figure> _figures = new List<Figure>();

    private int _vertexBufferObject;

    private int _vaoModel;

    private int _vaoLamp;

    private Shader _lampShader;
    private Shader _lightingShader;

    private Texture _metalTexture;
    private Texture _leatherTexture;
    private Texture _bladeTexture;
    private Texture _bladeShineTexture;
    private Texture _specularMap;
    
    private Camera _camera;

    private bool _firstMove = true;

    private Vector2 _lastPos;

    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings)
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        GL.Enable(EnableCap.DepthTest);
        
        _lightingShader = new Shader("../../../Shaders/vert_shader.glsl", "../../../Shaders/light_shader.glsl");
        _lampShader = new Shader("../../../Shaders/vert_shader.glsl", "../../../Shaders/frag_shader.glsl");

        //{
        //    _vaoLamp = GL.GenVertexArray();
        //    GL.BindVertexArray(_vaoLamp);
//
        //    var positionLocation = _lampShader.GetAttribLocation("aPos");
        //    GL.EnableVertexAttribArray(positionLocation);
        //    GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        //}

        _metalTexture = Texture.LoadFromFile("../../../Resources/metal.png");
        _leatherTexture = Texture.LoadFromFile("../../../Resources/leather.png");
        _bladeTexture = Texture.LoadFromFile("../../../Resources/black.png");
        _bladeShineTexture = Texture.LoadFromFile("../../../Resources/blackShine.png");
        _specularMap = Texture.LoadFromFile("../../../Resources/metal.png");

        _camera = new Camera(Vector3.UnitY * 10 + Vector3.UnitX * 3, Size.X / (float)Size.Y);

        CursorState = CursorState.Grabbed;
        
        #region Particles

        var partCount = 63;
        for (var i = 0; i < partCount; i++)
        {
            var horStep = 0.25f;
            var vertStep = 0.25f;
            var x = 1f * MathF.Cos(horStep * i);
            var y = 8 + (i * vertStep) % 16;
            var z = 1.5f * MathF.Sin(horStep * i);
                
            LoadSphere(0.1f, new Vector3(x, y, z), _leatherTexture);
        }
        
        //for (var i = 0; i < partCount; i++)
        //{
        //    var horStep = 0.25f;
        //    var vertStep = 0.25f;
        //    var x = -0.75f * MathF.Cos(horStep * i);
        //    var y = 8 + (i * vertStep) % 16;
        //    var z = 1.75f * MathF.Sin(horStep * i);
        //        
        //    LoadSphere(0.1f, new Vector3(x, y, z), _leatherTexture);
        //}

        #endregion
        #region Grip

            LoadCylinder(0.5f, 0.4f, 4, 100, Vector3.Zero, Vector3.Zero, _leatherTexture);
            LoadCylinder(0.4f, 0.5f, 2.5f, 100, new Vector3(0, 3.25f, 0), Vector3.Zero, _leatherTexture);
            LoadCylinder(0.45f, 0.45f, 0.25f, 100, new Vector3(0, 4.5f, 0), Vector3.Zero, _leatherTexture);
            LoadCylinder(0.4f, 0.5f, 0.05f, 100, new Vector3(0, -2.0025f, 0), Vector3.Zero, _metalTexture);
            LoadCylinder(0.5f, 0.5f, 0.05f, 100, new Vector3(0, -2.05f, 0), Vector3.Zero, _metalTexture);
            LoadSphere(0.65f, new Vector3(0, -2.5f, 0), _metalTexture);
            LoadCylinder(0.2f, 0.2f, 0.1f, 100, new Vector3(0, -3.15f, 0), Vector3.Zero, _metalTexture);

        #endregion
        #region Guard

            LoadCylinder(0.39f, 0.38f, 1.09f, 100, new Vector3(0, 5.0f, 0), Vector3.Zero, _metalTexture);
            LoadPrism(0.05f, 0.3925f, 5, new Vector3(0, 6.8f, -2.165f), new Vector3((120f) * MathF.PI / 180,(0) * MathF.PI / 180 , (180) * MathF.PI / 180), _metalTexture);
            LoadPrism(0.05f, 0.3925f, 5, new Vector3(0, 6.8f, 2.165f), new Vector3((60f) * MathF.PI / 180,(0) * MathF.PI / 180 , (0) * MathF.PI / 180), _metalTexture);
            LoadParallelepiped(0.37f, 0.45f, 2.0f, -10, new Vector3(0.19f, 6.67f, 0f), new Vector3(), _metalTexture);
            LoadParallelepiped(0.37f, 0.45f, 2.0f, 10, new Vector3(-0.19f, 6.67f, 0f), new Vector3(), _metalTexture);
            LoadTent(0.75f, 0.4f, 0.15f, new Vector3(0f, 5.6f, 0f), new Vector3(MathF.PI, 90 * MathF.PI / 180 ,0), _metalTexture);
            LoadPrism(0.2025f, 0.38f, 1.97f,new Vector3(0, 6.685f, 0.2225f), new Vector3(0, (90) * MathF.PI / 180, 0), _metalTexture);
            LoadPrism(0.2025f, 0.38f, 1.97f, new Vector3(0, 6.685f, -0.2225f), new Vector3(0, -(90) * MathF.PI / 180, 0), _metalTexture);
            LoadTorus(0.045f, 0.175f, new Vector3(0, 8.14f, 4.48f), new Vector3(0, 80.125f, 0), _metalTexture);
            LoadTorus(0.045f, 0.175f, new Vector3(0, 8.14f + 1.5f * 0.25f, 4.48f + 0.5f * 0.25f), new Vector3(0, 80.125f, 0), _metalTexture);
            LoadTorus(0.045f, 0.175f, new Vector3(0, 8.14f - 0.5f * 0.25f, 4.48f + 1.5f * 0.25f), new Vector3(0, 80.125f, 0), _metalTexture);
            LoadTorus(0.045f, 0.175f, new Vector3(0, 8.14f + 0.25f , 4.48f + 0.5f), new Vector3(0, 80.125f, 0), _metalTexture);
            LoadTorus(0.045f, 0.175f, new Vector3(0, 8.14f, -4.48f), new Vector3(0, 80.125f, 0), _metalTexture);
            LoadTorus(0.045f, 0.175f, new Vector3(0, 8.14f + 1.5f * 0.25f, -4.48f - 0.5f * 0.25f), new Vector3(0, 80.125f, 0), _metalTexture);
            LoadTorus(0.045f, 0.175f, new Vector3(0, 8.14f - 0.5f * 0.25f, -4.485f - 1.5f * 0.25f), new Vector3(0, 80.125f, 0), _metalTexture);
            LoadTorus(0.045f, 0.175f, new Vector3(0, 8.14f + 0.25f , -4.48f - 0.5f), new Vector3(0, 80.125f, 0), _metalTexture);
        #endregion
        #region Blade
        
            //LoadPyramidWithSquareAngle(0.2f, 0.7f, 20f, new Vector3(0f, 16, -0.35f), new Vector3(0, (90) * MathF.PI / 180, 0), _bladeTexture);
            //LoadPyramidWithSquareAngle(0.2f, 0.7f, 20f, new Vector3(0f, 16, 0.35f), new Vector3(0, -(90) * MathF.PI / 180, 0), _bladeTexture);
            LoadPrismWithSquareAngle(0.15f, 1f, 0.2f, 1.37f, 18f, new Vector3(0f, 15f, 0), new Vector3(0, 0, 0), _bladeShineTexture);
            LoadPyramid(0.15f, 1f, 1f, 4,  new Vector3(0, 15f + 9.5f, 0), new Vector3(0, 90 * MathF.PI / 180, 0), _bladeTexture);
            //LoadPrismWithSquareAngle(0f, 0, 0.15f, 1.2f, 0.5f, new Vector3(0, 16 + 10.25f, 0), Vector3.Zero, _bladeTexture);
        #endregion

            
    }

    private void LoadFigure(float[] vertices, float[] normals, float[] texCoords, uint[] indices, Vector3 pos, Vector3 rot, Texture texture)
    {
        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
        
        var vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        var positionLocation = _lightingShader.GetAttribLocation("aPos");
        GL.EnableVertexAttribArray(positionLocation);
        GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

        var normalsBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, normalsBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, normals.Length * sizeof(float), normals, BufferUsageHint.StaticDraw);
        var normalLocation = _lightingShader.GetAttribLocation("aNormal");
        GL.EnableVertexAttribArray(normalLocation);
        GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

        var texCoordsBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, texCoordsBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, texCoords.Length * sizeof(float), texCoords, BufferUsageHint.StaticDraw);
        var texCoordLocation = _lightingShader.GetAttribLocation("aTexCoords");
        GL.EnableVertexAttribArray(texCoordLocation);
        GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

        var indicesBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indicesBuffer);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            
        var figure = new Figure();
        figure.Vao = vao;
        figure.Pos = pos;
        figure.Rot = rot;
        figure.IndCount = indices.Length;
        figure.Texture = texture;
        _figures.Add(figure);
    }

    private void LoadPyramid(float length, float width, float height, int size, Vector3 pos, Vector3 rot, Texture texture)
        {
            var vertices = new List<float>();
            var normals = new List<float>();
            var texCoords = new List<float>();
            var indices = new List<uint>();
            
            var invLength = 1 / length;
            var curAngle = 0f;
            var angleDiff = 2 * MathF.PI / size;

            vertices.Add(0);
            vertices.Add(-height / 2);
            vertices.Add(0);
            normals.Add(0);
            normals.Add(-height / 2);
            normals.Add(0);
            texCoords.Add(0);
            texCoords.Add(0);
            
            
            for (var i = 3; i < 3 * (size + 1); i+=3)
            {
                var x = width * MathF.Cos(curAngle);
                var y = length * MathF.Sin(curAngle);
                vertices.Add(x);
                vertices.Add(-height / 2);
                vertices.Add(y);

                var iLength = 1 / MathF.Sqrt(x * x + y * y + 1 * 1);
                normals.Add(x * iLength);
                normals.Add(-1 * iLength);
                normals.Add(y * iLength);
                
                texCoords.Add(0);
                texCoords.Add(0);

                vertices.Add(0);
                vertices.Add(height / 2);
                vertices.Add(0);
                
                normals.Add(0);
                normals.Add(1);
                normals.Add(0);
                
                texCoords.Add(0.5f);
                texCoords.Add(1);
                
                curAngle += angleDiff;
                x = width * MathF.Cos(curAngle);
                y = length * MathF.Sin(curAngle);
                
                vertices.Add(x);
                vertices.Add(-height / 2);
                vertices.Add(y);
                iLength = 1 / MathF.Sqrt(x * x + y * y + 1 * 1);
                normals.Add(x * iLength);
                normals.Add(-height / 2);
                normals.Add(y * iLength);
                
                texCoords.Add(1);
                texCoords.Add(0);
            }

            uint curIndex = 1;
            for (var i = 0; i < size; i++)
            {
                indices.Add(curIndex);
                indices.Add(curIndex + 1);
                indices.Add(curIndex + 2);
                curIndex+= 3;
            }

            curIndex = 1;
            for (var i = 0; i < size; i++)
            {
                indices.Add(0);
                indices.Add(curIndex);
                indices.Add(curIndex + 2);
                curIndex+=3;
            }

            LoadFigure(vertices.ToArray(), normals.ToArray(), texCoords.ToArray(), indices.ToArray(), pos, rot, texture);
        }
    private void LoadParallelepiped(float length, float width, float height, float angle, Vector3 pos, Vector3 rot, Texture texture)
        {
            var hLength = length / 2;
            var hWidth = width / 2;
            var hHeight = height / 2;

            var vertices = new List<float>
            {
                -hLength, -hHeight, -hWidth,
                hLength, -hHeight, -hWidth,
                hLength, -hHeight, hWidth,
                -hLength, -hHeight, hWidth,
                -hLength + MathF.Sin(MathHelper.DegreesToRadians(angle)), hHeight, -hWidth,
                hLength + MathF.Sin(MathHelper.DegreesToRadians(angle)), hHeight, -hWidth,
                hLength + MathF.Sin(MathHelper.DegreesToRadians(angle)), hHeight, hWidth,
                -hLength + MathF.Sin(MathHelper.DegreesToRadians(angle)), hHeight, hWidth,
            };
            
            var normals = new List<float>
            {
                -hLength, -hHeight, -hWidth,
                hLength, -hHeight, -hWidth,
                hLength, -hHeight, hWidth,
                -hLength, -hHeight, hWidth,
                -hLength + MathF.Sin(MathHelper.DegreesToRadians(angle)), hHeight, -hWidth,
                hLength + MathF.Sin(MathHelper.DegreesToRadians(angle)), hHeight, -hWidth,
                hLength + MathF.Sin(MathHelper.DegreesToRadians(angle)), hHeight, hWidth,
                -hLength + MathF.Sin(MathHelper.DegreesToRadians(angle)), hHeight, hWidth,
            };
            var texCoords = new List<float>()
            {
                0, 0,
                1, 0,
                0, 0,
                1, 0,
                0, 1,
                1, 1,
                0, 1,
                1, 1,
            };
            var indices = new List<uint>()
            {
                0, 1, 5,
                0, 4, 5,
                1, 2, 6,
                1, 5, 6,
                2, 3, 7,
                2, 6, 7,
                3, 0, 4,
                3, 7, 4,
                0, 1, 2, 
                0, 2, 3,
                4, 5, 6,
                4, 6, 7
            };
            
            LoadFigure(vertices.ToArray(), normals.ToArray(), texCoords.ToArray(), indices.ToArray(), pos, rot, texture);
        }
    private void LoadHexahedron(float radius, Vector3 pos, Vector3 rot, Texture texture)
    {
        var vertices = new List<float>()
        {
            -radius, radius, -radius,
            -radius, radius, radius,
            radius, radius, radius,
            radius, radius, -radius,
                
            -radius, -radius, -radius,
            -radius, -radius, radius,
            radius, -radius, radius,
            radius, -radius, -radius
        };

        var normals = new List<float>()
        {
            -1, 1, -1,
            -1, 1, 1,
            1, 1, 1,
            1, 1, -1,

            -1, -1, -1,
            -1, -1, 1,
            1, -1, 1,
            1, -1, -1
        };

        var texCoords = new List<float>
        {
            0, 1,
            1, 1,
            0, 1,
            1, 1,
            
            0, 0,
            1, 0,
            0, 0,
            1, 0
        };

        var indices = new List<uint>()
        {
            0, 1, 2,
            0, 2, 3,

            4, 5, 6,
            4, 6, 7,

            0, 1, 4,
            1, 4, 5,
                
            1, 2, 5,
            2, 5, 6,
                
            2, 3, 6,
            3, 6, 7,
                
            3, 0, 7,
            0, 7, 4
        };
            
        LoadFigure(vertices.ToArray(), normals.ToArray(), texCoords.ToArray(), indices.ToArray(), pos, rot, texture);

    }
    private void LoadCylinder(float topRad, float botRad, float height, uint size, Vector3 pos, Vector3 rot, Texture texture)
        {
            var vertices = new List<float>();
            var normals = new List<float>();
            var texCoords = new List<float>();
            var indices = new List<uint>();

            var topInvLength = 1 / topRad;
            var botInvLength = 1 / botRad;

            var curAngle = 0f;
            var angleDiff = 2 * MathF.PI / size;
            
            vertices.AddRange(new []{0f, -height / 2,0f});
            normals.AddRange(new []{0f, -height / 2, 0f});
            texCoords.AddRange(new []{0f,0f});
            vertices.AddRange(new []{0f, height / 2,0f});
            normals.AddRange(new []{0f, height / 2, 0f});
            texCoords.AddRange(new []{0f,1f});
            for (var i = 0; i < size; i++)
            {
                var x = botRad * MathF.Cos(curAngle);
                var y = botRad * MathF.Sin(curAngle);
                vertices.Add(x);
                vertices.Add(-height / 2);
                vertices.Add(y);

                normals.Add(x * botInvLength);
                normals.Add(0);
                normals.Add(y * botInvLength);
                
                texCoords.Add((float)i / size);
                texCoords.Add(0);
                curAngle += angleDiff;
            }

            curAngle = 0;
            for (var i = 0; i < size; i++)
            {
                var x = topRad * MathF.Cos(curAngle);
                var y = topRad * MathF.Sin(curAngle);
                vertices.Add(x);
                vertices.Add(height / 2);
                vertices.Add(y);

                normals.Add(x * topInvLength);
                normals.Add(0);
                normals.Add(y * topInvLength);
                
                texCoords.Add((float)i / size);
                texCoords.Add(1);
                
                curAngle += angleDiff;
            }
            
            uint curIndex = 2;
            for (var i = 0; i < size; i++)
            {
                if (i == size - 1)
                {
                    indices.Add(0);
                    indices.Add(curIndex);
                    indices.Add(2);
                }
                else
                {
                    indices.Add(0);
                    indices.Add(curIndex);
                    indices.Add(curIndex + 1);
                }
                curIndex++;
            }

            curIndex = 2 + size;
            for (var i = 0; i < size; i++)
            {
                if (i == size - 1)
                {
                    indices.Add(1);
                    indices.Add(curIndex);
                    indices.Add(2 + size);
                }
                else
                {
                    indices.Add(1);
                    indices.Add(curIndex);
                    indices.Add(curIndex + 1);
                }
                curIndex++;
            }

            curIndex = 2;
            for (var i = 0; i < size; i++)
            {
                var k1 = curIndex;
                var k2 = curIndex + size;
                
                if (i == size - 1)
                {
                    indices.Add(k1);
                    indices.Add(2);
                    indices.Add(k2);
                    indices.Add(2);
                    indices.Add(k2);
                    indices.Add(2 + size);
                }
                else
                {
                    indices.Add(k1);
                    indices.Add(k1 + 1);
                    indices.Add(k2);
                    indices.Add(k1 + 1);
                    indices.Add(k2);
                    indices.Add(k2 + 1);
                }
                curIndex++;
            }

            LoadFigure(vertices.ToArray(), normals.ToArray(), texCoords.ToArray(), indices.ToArray(), pos, rot, texture);
        }
    private void LoadCylinderWithSquareAngle(float topWidth, float topLength, float botWidth, float botLength, float height, uint size, Vector3 pos, Vector3 rot, Texture texture)
        {
            var vertices = new List<float>();
            var normals = new List<float>();
            var texCoords = new List<float>();
            var indices = new List<uint>();

            var topInvLength = 1 / topLength;
            var botInvLength = 1 / botLength;

            var curAngle = 0f;
            var angleDiff = 2 * MathF.PI / size;
            
            vertices.AddRange(new []{0f, -height / 2,0f});
            normals.AddRange(new []{0f, -height / 2, 0f});
            texCoords.AddRange(new []{0f,0f});
            vertices.AddRange(new []{0, height / 2, 0});
            normals.AddRange(new []{0f, height / 2, 0f});
            texCoords.AddRange(new []{0f,1f});
            for (var i = 0; i < size; i++)
            {
                var x = botWidth * MathF.Cos(curAngle);
                var y = botLength * MathF.Sin(curAngle);
                vertices.Add(x);
                vertices.Add(-height / 2);
                vertices.Add(y);

                normals.Add(x * botInvLength);
                normals.Add(0);
                normals.Add(y * botInvLength);
                
                texCoords.Add((float)i / size);
                texCoords.Add(0);
                curAngle += angleDiff;
            }

            curAngle = 0;
            for (var i = 0; i < size; i++)
            {
                var x = topWidth * MathF.Cos(curAngle);
                var y = topLength * MathF.Sin(curAngle);
                vertices.Add(x);
                vertices.Add(height / 2);
                vertices.Add(y);

                normals.Add(x * topInvLength);
                normals.Add(0);
                normals.Add(y * topInvLength);
                
                texCoords.Add((float)i / size);
                texCoords.Add(1);
                
                curAngle += angleDiff;
            }
            
            uint curIndex = 2;
            for (var i = 0; i < size; i++)
            {
                if (i == size - 1)
                {
                    indices.Add(0);
                    indices.Add(curIndex);
                    indices.Add(2);
                }
                else
                {
                    indices.Add(0);
                    indices.Add(curIndex);
                    indices.Add(curIndex + 1);
                }
                curIndex++;
            }

            curIndex = 2 + size;
            for (var i = 0; i < size; i++)
            {
                if (i == size - 1)
                {
                    indices.Add(1);
                    indices.Add(curIndex);
                    indices.Add(2 + size);
                }
                else
                {
                    indices.Add(1);
                    indices.Add(curIndex);
                    indices.Add(curIndex + 1);
                }
                curIndex++;
            }

            curIndex = 2;
            for (var i = 0; i < size; i++)
            {
                var k1 = curIndex;
                var k2 = curIndex + size;
                
                if (i == size - 1)
                {
                    indices.Add(k1);
                    indices.Add(2);
                    indices.Add(k2);
                    indices.Add(2);
                    indices.Add(k2);
                    indices.Add(2 + size);
                }
                else
                {
                    indices.Add(k1);
                    indices.Add(k1 + 1);
                    indices.Add(k2);
                    indices.Add(k1 + 1);
                    indices.Add(k2);
                    indices.Add(k2 + 1);
                }
                curIndex++;
            }

            LoadFigure(vertices.ToArray(), normals.ToArray(), texCoords.ToArray(), indices.ToArray(), pos, rot, texture);
        }
    private void LoadPrism(float topRad, float botRad, float height, Vector3 pos, Vector3 rot, Texture texture)
    {
        LoadCylinder(topRad, botRad, height, 4, pos, rot, texture);
    }
    private void LoadPrismWithSquareAngle(float topWidth, float topLength, float botWidth, float botLength, float height, Vector3 pos, Vector3 rot, Texture texture)
    {
        LoadCylinderWithSquareAngle(topWidth, topLength, botWidth, botLength, height, 4, pos, rot, texture);
    }
    private void LoadSphere(float radius, Vector3 pos, Texture texture)
    {
        var vertices = new List<float>();
        var normals = new List<float>();
        var texCoords = new List<float>();
        var indices = new List<uint>();
        var sectorCount = 100;
        var stackCount = 100;
        var invLength = 1f / radius;

        var sectorStep = 2 * MathF.PI / sectorCount;
        var stackStep = MathF.PI / stackCount;

        for (var i = 0; i <= stackCount; ++i)
        {
            var stackAngle = MathF.PI / 2 - i * stackStep;
            var xy = radius * MathF.Cos(stackAngle);
            var z = radius * MathF.Sin(stackAngle);

            for (var j = 0; j <= sectorCount; ++j)
            {
                var sectorAngle = j * sectorStep;

                var x = xy * MathF.Cos(sectorAngle);
                var y = xy * MathF.Sin(sectorAngle);
                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(z);

                var nx = x * invLength;
                var ny = y * invLength;
                var nz = z * invLength;
                normals.Add(nx);
                normals.Add(ny);
                normals.Add(nz);
                    
                var s = (float)j / sectorCount;
                var t = (float)i / stackCount;
                texCoords.Add(s);
                texCoords.Add(t);
            }
        }

        for (var i = 0; i < stackCount; ++i)
        {
            var k1 = i * (sectorCount + 1);
            var k2 = k1 + sectorCount + 1;

            for (var j = 0; j < sectorCount; ++j, ++k1, ++k2)
            {
                if (i != 0)
                {
                    indices.Add((uint)k1);
                    indices.Add((uint)k2);
                    indices.Add((uint)k1 + 1);
                }

                if (i != stackCount - 1)
                {
                    indices.Add((uint)k1 + 1);
                    indices.Add((uint)k2);
                    indices.Add((uint)k2 + 1);
                }
            }
        }
        LoadFigure(vertices.ToArray(), normals.ToArray(), texCoords.ToArray(), indices.ToArray(), pos, new Vector3(0,0,0), texture);
    }
    private void LoadPyramidWithSquareAngle(float length, float width, float height, Vector3 pos, Vector3 rot, Texture texture)
        {
            var vertices = new List<float>();
            var normals = new List<float>();
            var texCoords = new List<float>();
            var indices = new List<uint>();
            
            var invLength = 1 / length;
            var curAngle = 0f;
            var angleDiff = 2 * MathF.PI / 3;

            vertices.Add(0);
            vertices.Add(-height / 2);
            vertices.Add(0);
            normals.Add(0);
            normals.Add(-height / 2);
            normals.Add(0);
            texCoords.Add(0);
            texCoords.Add(0);
            
            for (var i = 3; i < 3 * (4); i+=3)
            {
                var x = width * MathF.Cos(curAngle);
                var y = length * MathF.Sin(curAngle);
                vertices.Add(x);
                vertices.Add(-height / 2);
                vertices.Add(y);
                var iLength = 1 / MathF.Sqrt(x * x + y * y + 1 * 1);
                normals.Add(x * iLength);
                normals.Add(-1 * iLength);
                normals.Add(y * iLength);
                
                texCoords.Add(0);
                texCoords.Add(0);

                vertices.Add(width * MathF.Cos(angleDiff));
                vertices.Add(height / 2);
                vertices.Add(0);
                
                normals.Add(0);
                normals.Add(height / 2);
                normals.Add(0);
                
                texCoords.Add(1);
                texCoords.Add(1);
                
                curAngle += angleDiff;
                x = width * MathF.Cos(curAngle);
                y = length * MathF.Sin(curAngle);
                
                vertices.Add(x);
                vertices.Add(-height / 2);
                vertices.Add(y);
                iLength = 1 / MathF.Sqrt(x * x + y * y + 1 * 1);
                normals.Add(x * iLength);
                normals.Add(-1 * iLength);
                normals.Add(y * iLength);
                
                texCoords.Add(1);
                texCoords.Add(0);
            }

            uint curIndex = 1;
            for (var i = 0; i < 3; i++)
            {
                indices.Add(curIndex);
                indices.Add(curIndex + 1);
                indices.Add(curIndex + 2);
                curIndex+= 3;
            }

            curIndex = 1;
            for (var i = 0; i < 3; i++)
            {
                indices.Add(0);
                indices.Add(curIndex);
                indices.Add(curIndex + 2);
                curIndex+=3;
            }

            LoadFigure(vertices.ToArray(), normals.ToArray(), texCoords.ToArray(), indices.ToArray(), pos, rot, texture);
        }
    private void LoadTent(float length, float width, float height, Vector3 pos, Vector3 rot, Texture texture)
    {
        var hLength = length / 2;
        var hWidth = width / 2;
        var hHeight = height / 2;
        var vertices = new List<float>()
        {
            -hWidth, -hHeight, -hLength,
            hWidth, -hHeight, -hLength,
            hWidth, -hHeight, hLength,
            -hWidth, -hHeight, hLength,
                
            0, hHeight, -hLength,
            0, hHeight, hLength,
        };

        var normals = new List<float>()
        {
            -hWidth, -hHeight, -hLength,
            hWidth, -hHeight, -hLength,
            hWidth, -hHeight, hLength,
            -hWidth, -hHeight, hLength,
                
            0, hHeight, -hLength,
            0, hHeight, hLength,
        };

        var texCoords = new List<float>()
        {
            0, 0,
            0, 0,
            1, 0,
            1, 0,
            0, 1,
            1, 1
        };
        
        var indices = new List<uint>()
        {
            0, 1, 2,
            0, 2, 3,

            0, 1, 4,
            2, 3, 5,

            0, 3, 4,
            3, 4, 5,

            1, 2, 4,
            2, 4, 5
        };
            
        LoadFigure(vertices.ToArray(), normals.ToArray(), texCoords.ToArray(), indices.ToArray(), pos, rot, texture);
    }
    private void LoadTorus(float innerRad, float outerRad, Vector3 pos, Vector3 rot, Texture texture)
        {
            var vertices = new List<float>();
            var normals = new List<float>();
            var texCoords = new List<float>();
            var indices = new List<uint>();
            
            uint sliceCount = 100;
            uint loopCount = 100;

            for (var i = 0; i <= sliceCount; ++i)
            {
                var sliceAngle = 2 * MathF.PI * i / sliceCount;
                var sliceRad = outerRad + innerRad * MathF.Cos(sliceAngle);

                for (var j = 0; j <= loopCount; ++j)
                {
                    var loopAngle = 2 * MathF.PI * j / loopCount;

                    var x = sliceRad * MathF.Cos(loopAngle);
                    var y = sliceRad * MathF.Sin(loopAngle);
                    var z = innerRad * MathF.Sin(sliceAngle);

                    var tx = -MathF.Sin(loopAngle);
                    var ty = MathF.Cos(loopAngle);
                    var tz = 0;

                    var sx = MathF.Cos(loopAngle) * (-MathF.Sin(sliceAngle));
                    var sy = MathF.Sin(loopAngle) * (-MathF.Sin(sliceAngle));
                    var sz = MathF.Cos(sliceAngle);

                    var nx = ty * sz - tz * sy;
                    var ny = tz * sz - tx * sz;
                    var nz = tx * sy - ty * sx;
                    vertices.AddRange(new []{x, y , z});
                    normals.AddRange(new []{nx, ny, nz});
                    texCoords.AddRange(new[]{(float)j / loopCount, (float)i / sliceCount});
                }
            }
            
            var verticesPerSlice = loopCount + 1;
            for (uint i = 0; i < sliceCount; ++i)
            {
                var v1 = i * verticesPerSlice;
                var v2 = v1 + verticesPerSlice;

                for (var j = 0; j < loopCount; ++j)
                {
                    indices.AddRange(new []{v1, v1 + 1, v2});
                    indices.AddRange(new []{v2, v1 + 1, v2 + 1});
                    v1++;
                    v2++;
                }
            }
            
            LoadFigure(vertices.ToArray(), normals.ToArray(), texCoords.ToArray(), indices.ToArray(), pos, rot, texture);
        }
    private void DrawAllFigures()
    {
        for (int i = 0; i < _pointLightPositions.Length; i++)
        {
            _lightingShader.SetVector3($"pointLights[{i}].position", _pointLightPositions[i]);
            _lightingShader.SetVector3($"pointLights[{i}].ambient", new Vector3(0.05f, 0.05f, 0.05f));
            _lightingShader.SetVector3($"pointLights[{i}].diffuse", new Vector3(0.8f, 0.8f, 0.8f));
            _lightingShader.SetVector3($"pointLights[{i}].specular", new Vector3(1.0f, 1.0f, 1.0f));
            _lightingShader.SetFloat($"pointLights[{i}].constant", 1.0f);
            _lightingShader.SetFloat($"pointLights[{i}].linear", 0.09f);
            _lightingShader.SetFloat($"pointLights[{i}].quadratic", 0.032f);
        }

        for (var i = 0; i < 63; i++)
        {
            var x = _figures[i].Pos.X;
            var y = _figures[i].Pos.Y;
            var z = _figures[i].Pos.Z;
            x = 0.75f * MathF.Cos(0.4f * i + _timer);
            y = 8 + (i * 0.25f + 2 * _timer) % 16;
            z = 1.75f * MathF.Sin(0.4f * i + _timer);
            _figures[i].Pos = new Vector3(x, y, z);
        }
        
        //for (var i = 60; i < 120; i++)
        //{
        //    var x = _figures[i].Pos.X;
        //    var z = _figures[i].Pos.Z;
        //    x = -0.75f * MathF.Cos(0.5f * i - _timer);
        //    z = 1.75f * MathF.Sin(0.5f * i - _timer);
        //    _figures[i].Pos = new Vector3(x, _figures[i].Pos.Y, z);
        //}
        foreach (var figure in _figures)
        {
            GL.BindVertexArray(figure.Vao);
            
            figure.Texture.Use(TextureUnit.Texture0);
            _specularMap.Use(TextureUnit.Texture1);
            _lightingShader.Use();

            _lightingShader.SetMatrix4("view", _camera.GetViewMatrix());
            _lightingShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            _lightingShader.SetVector3("viewPos", _camera.Position);

            _lightingShader.SetInt("material.diffuse", 0);
            _lightingShader.SetInt("material.specular", 1);
            _lightingShader.SetVector3("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
            _lightingShader.SetFloat("material.shininess", 32.0f);
            
            _lightingShader.SetVector3("dirLight.direction", new Vector3(0, -1.0f, 0));
            _lightingShader.SetVector3("dirLight.ambient", new Vector3(0.3f, 0.3f, 0.3f));
            _lightingShader.SetVector3("dirLight.diffuse", new Vector3(0.4f, 0.4f, 0.4f));
            _lightingShader.SetVector3("dirLight.specular", new Vector3(0.5f, 0.5f, 0.5f));

            _lightingShader.SetVector3("spotLight.position", _camera.Position);
            _lightingShader.SetVector3("spotLight.direction", _camera.Front);
            _lightingShader.SetVector3("spotLight.ambient", new Vector3(0.0f, 0.0f, 0.0f));
            _lightingShader.SetVector3("spotLight.diffuse", new Vector3(1.0f, 1.0f, 1.0f));
            _lightingShader.SetVector3("spotLight.specular", new Vector3(1.0f, 1.0f, 1.0f));
            _lightingShader.SetFloat("spotLight.constant", 1.0f);
            _lightingShader.SetFloat("spotLight.linear", 0.09f);
            _lightingShader.SetFloat("spotLight.quadratic", 0.032f);
            _lightingShader.SetFloat("spotLight.cutOff", MathF.Cos(MathHelper.DegreesToRadians(12.5f)));
            _lightingShader.SetFloat("spotLight.outerCutOff", MathF.Cos(MathHelper.DegreesToRadians(17.5f)));
            
            var eulerRot = new Vector3(figure.Rot.X,
                figure.Rot.Y,
                figure.Rot.Z);
            Matrix4 model = Matrix4.Identity;
            model *= Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(eulerRot));
            model *= Matrix4.CreateTranslation(figure.Pos);
            
            _lightingShader.SetMatrix4("model", model);

            GL.DrawElements(PrimitiveType.Triangles, figure.IndCount, DrawElementsType.UnsignedInt, 0);
        }
    }
    
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        _timer += (float)e.Time;
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        DrawAllFigures();
        
        //GL.BindVertexArray(_vaoLamp);

        //_lampShader.Use();

        //_lampShader.SetMatrix4("view", _camera.GetViewMatrix());
        //_lampShader.SetMatrix4("projection", _camera.GetProjectionMatrix());
        // We use a loop to draw all the lights at the proper position
        //for (int i = 0; i < _pointLightPositions.Length; i++)
        //{
        //    Matrix4 lampMatrix = Matrix4.CreateScale(0.2f);
        //    lampMatrix = lampMatrix * Matrix4.CreateTranslation(_pointLightPositions[i]);

        //    _lampShader.SetMatrix4("model", lampMatrix);

            //GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        //}

        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (!IsFocused)
        {
            return;
        }

        var input = KeyboardState;

        if (input.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        const float cameraSpeed = 5.0f;
        const float sensitivity = 0.2f;

        if (input.IsKeyDown(Keys.W))
        {
            _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
        }
        if (input.IsKeyDown(Keys.S))
        {
            _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
        }
        if (input.IsKeyDown(Keys.A))
        {
            _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
        }
        if (input.IsKeyDown(Keys.D))
        {
            _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
        }
        if (input.IsKeyDown(Keys.Space))
        {
            _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
        }
        if (input.IsKeyDown(Keys.LeftShift))
        {
            _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
        }

        var mouse = MouseState;

        if (_firstMove)
        {
            _lastPos = new Vector2(mouse.X, mouse.Y);
            _firstMove = false;
        }
        else
        {
            var deltaX = mouse.X - _lastPos.X;
            var deltaY = mouse.Y - _lastPos.Y;
            _lastPos = new Vector2(mouse.X, mouse.Y);

            _camera.Yaw += deltaX * sensitivity;
            _camera.Pitch -= deltaY * sensitivity;
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        _camera.Fov -= e.OffsetY;
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, Size.X, Size.Y);
        _camera.AspectRatio = Size.X / (float)Size.Y;
    }
}