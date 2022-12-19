using System;
using System.Data;
using CompGraph.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Security.Principal;
using OpenTK.Audio.OpenAL.Extensions.Creative.EFX;

namespace CompGraph
{
    class Figure
    {
        public int Vao;
        public Vector3 Pos;
        public Vector3 Rot;
        public Vector3 scale;
        public Vector3 Col;
        public int indCount;
    }
    // In this tutorial we take the code from the last tutorial and create some level of abstraction over it allowing more
    // control of the interaction between the light and the material.
    // At the end of the web version of the tutorial we also had a bit of fun creating a disco light that changes
    // color of the cube over time.
    public class Window : GameWindow
    {
        private float timer;
        private List<Figure> figures = new List<Figure>();
        private List<Figure> lights = new List<Figure>();
        private static float _lightCircleRadius = 5.0f;
        private Vector3 _lightPos = new Vector3(_lightCircleRadius, 1.0f, 0.0f);

        private int _vertexBufferObject;
        
        private int _vaoModel;

        private int _vaoLamp;

        private Shader _lampShader;

        private Shader _lightingShader;

        private Camera _camera;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        private PrimitiveType _primitiveType = PrimitiveType.Triangles;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);

            _camera = new Camera(Vector3.Zero, Size.X / (float)Size.Y);

            var angleStep = 2 * MathF.PI / 13;
            
            
            //LoadCylinder(0.5f, 1.0f, 3.0f, 100, new Vector3(0.0f, -1.7f, 0.0f), Vector3.Zero, Vector3.One);
            //LoadSphere(0.5f, new Vector3(0.0f, 1.5f - 1.7f, 0.0f), Vector3.One);
            
            //LoadPyramid(2f, 3f, 3, new Vector3(0.0f, 3f / 2, 0.0f), Vector3.Zero, GetRandColor());
            //LoadPyramid(2f, -2f, 3, new Vector3(0.0f, -2f / 2, 0.0f), Vector3.Zero, GetRandColor());
            LoadParallelepiped(1.0f, 1.0f, 1.0f, 30, new Vector3(8.0f * MathF.Cos(angleStep), 0, 8.0f * MathF.Sin(angleStep)), Vector3.Zero, GetRandColor());
            LoadPrism(0.5f, 1.0f, 2.0f, new Vector3(8.0f * MathF.Cos(2 * angleStep), 0, 8.0f * MathF.Sin(2 * angleStep)), Vector3.Zero, GetRandColor());
            LoadOctahedron(1.0f, new Vector3(8.0f * MathF.Cos(3 * angleStep), 0, 8.0f * MathF.Sin(3 * angleStep)), Vector3.Zero, GetRandColor());
            LoadPyramid(1.0f, 2.0f, 3, new Vector3(8.0f * MathF.Cos(4 * angleStep), 0, 8.0f * MathF.Sin(4 * angleStep)), Vector3.Zero, GetRandColor());
            LoadCone(1.0f, 2.0f, new Vector3(8.0f * MathF.Cos(5 * angleStep), 0, 8.0f * MathF.Sin(5 * angleStep)), Vector3.Zero, GetRandColor());
            LoadCylinder(0.75f, 1.0f, 2.0f, 100, new Vector3(8.0f * MathF.Cos(6 * angleStep), 0, 8.0f * MathF.Sin(6 * angleStep)), Vector3.Zero, GetRandColor());
            LoadSphere(1.0f, new Vector3(8.0f * MathF.Cos(7 * angleStep), 0, 8.0f * MathF.Sin(7 * angleStep)),  GetRandColor());
            LoadSpring(4, 2.0f, 0.15f, 1.0f, new Vector3(8.0f * MathF.Cos(8 * angleStep), 0, 8.0f * MathF.Sin(8 * angleStep)), Vector3.Zero, GetRandColor());
            LoadTorus(0.5f, 1.0f, new Vector3(8.0f * MathF.Cos(9 * angleStep), 0, 8.0f * MathF.Sin(9 * angleStep)), Vector3.Zero, GetRandColor());
            LoadTetrahedron(1.0f, new Vector3(8.0f * MathF.Cos(10 * angleStep), 0, 8.0f * MathF.Sin(10 * angleStep)), Vector3.Zero, GetRandColor());
            LoadHexahedron(1.0f, new Vector3(8.0f * MathF.Cos(11 * angleStep), 0,  8.0f * MathF.Sin(11 * angleStep)), Vector3.Zero, GetRandColor());
            LoadIcosahedron(1.0f, new Vector3(8.0f * MathF.Cos(12 * angleStep), 0, 8.0f * MathF.Sin(12 * angleStep)), Vector3.Zero, GetRandColor());
            LoadDodecahedron(1.0f, new Vector3(8.0f * MathF.Cos(13 * angleStep), 0, 8.0f * MathF.Sin(13 * angleStep)), Vector3.Zero, GetRandColor());
            
            LoadLightSphere(2.0f);
            CursorState = CursorState.Grabbed;
        }

        private Vector3 GetRandColor()
        {
            var rand = new Random();
            var r = (float)rand.NextDouble();
            var g = (float)rand.NextDouble();
            var b = (float)rand.NextDouble();
            return new Vector3(r, g, b);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            DrawAllFigures();
            
            GL.BindVertexArray(lights[0].Vao);
            _lampShader.Use();
            timer += (float)e.Time;
            var angle = 0.5f *  timer % (2 * MathF.PI);
            _lightPos = new Vector3(_lightCircleRadius * MathF.Cos(angle), 1.0f, _lightCircleRadius * MathF.Sin(angle));
            Matrix4 lampMatrix = Matrix4.Identity;
            lampMatrix *= Matrix4.CreateScale(0.2f);
            lampMatrix *= Matrix4.CreateTranslation(_lightPos);

            _lampShader.SetMatrix4("model", lampMatrix);
            _lampShader.SetMatrix4("view", _camera.GetViewMatrix());
            _lampShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            GL.DrawElements(PrimitiveType.Triangles, lights[0].indCount, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        private void LoadLightSphere(float radius)
        {
            var vertices = new List<float>();
            var normals = new List<float>();
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
            LoadLight(vertices.ToArray(), indices.ToArray());
        }
        private void LoadLight(float[] vertices, uint[] indices)
        {
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            
            var vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            
            var positionLocation = _lampShader.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            var indicesBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indicesBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            
            var light = new Figure();
            light.Vao = vao;
            light.indCount = indices.Length;
            lights.Add(light);
        }
        private void LoadFigure(float[] vertices, float[] normals, uint[] indices, Vector3 position, Vector3 rotation, Vector3 color)
        {
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            _lightingShader = new Shader("../../../Shaders/vertex_shader.glsl", "../../../Shaders/lighting_shader.glsl");
            _lampShader = new Shader("../../../Shaders/vertex_shader.glsl", "../../../Shaders/fragment_shader.glsl");
            
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

            var indicesBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indicesBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            
            var figure = new Figure();
            figure.Vao = vao;
            figure.Pos = position;
            figure.Rot = rotation;
            figure.Col = color;
            figure.indCount = indices.Length;
            
            figures.Add(figure);
        }

        private void LoadParallelepiped(float length, float width, float height, float angle, Vector3 pos, Vector3 rot, Vector3 col)
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
                -hLength, hHeight, -hWidth,
                hLength, hHeight, -hWidth,
                hLength, hHeight, hWidth,
                -hLength, hHeight, hWidth,
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
            
            LoadFigure(vertices.ToArray(), normals.ToArray(), indices.ToArray(), pos, rot, col);
        }
        private void LoadPyramid(float radius, float height, int size, Vector3 pos, Vector3 rot, Vector3 col)
        {
            var vertices = new List<float>();
            var normals = new List<float>();
            var indices = new List<uint>();
            
            var invLength = 1 / radius;
            var curAngle = 0f;
            var angleDiff = 2 * MathF.PI / size;
            vertices.Add(0);
            vertices.Add(height / 2);
            vertices.Add(0);
            normals.Add(0);
            normals.Add(height / 2);
            normals.Add(0);
            
            vertices.Add(0);
            vertices.Add(-height / 2);
            vertices.Add(0);
            normals.Add(0);
            normals.Add(-height / 2);
            normals.Add(0);
            for (var i = 6; i < 3 * (size + 3); i+=3)
            {
                var x = radius * MathF.Cos(curAngle);
                var y = radius * MathF.Sin(curAngle);
                vertices.Add(x);
                vertices.Add(-height / 2);
                vertices.Add(y);

                normals.Add(x * invLength);
                normals.Add(-height / 2);
                normals.Add(y * invLength);
                
                curAngle += angleDiff;
            }

            uint curIndex = 2;
            for (var i = 0; i < (size + 1) * 3; i+=3)
            {
                indices.Add(0);
                indices.Add(curIndex);
                indices.Add(curIndex + 1);
                curIndex++;
            }
            
            vertices.Add(0);
            vertices.Add(0);
            vertices.Add(0);
            
            normals.Add(0);
            normals.Add(1);
            normals.Add(0);

            curIndex = 2;
            for (var i = 0; i < (size + 1) * 3; i+=3)
            {
                indices.Add(1);
                indices.Add(curIndex);
                indices.Add(curIndex + 1);
                curIndex++;
            }
            LoadFigure(vertices.ToArray(), normals.ToArray(), indices.ToArray(), pos, rot, col);
        }
        private void LoadPrism(float topRad, float botRad, float height, Vector3 pos, Vector3 rot, Vector3 col)
        {
            LoadCylinder(topRad, botRad, height, 4, pos, rot, col);
        }

        private void LoadCone(float radius, float height, Vector3 pos, Vector3 rot, Vector3 col)
        {
            LoadPyramid(radius, height, 100, pos, rot, col);
        }
        private void LoadCylinder(float topRad, float botRad, float height, uint size, Vector3 pos, Vector3 rot, Vector3 col)
        {
            var vertices = new List<float>();
            var normals = new List<float>();
            var indices = new List<uint>();

            var topInvLength = 1 / topRad;
            var botInvLength = 1 / botRad;

            var curAngle = 0f;
            var angleDiff = 2 * MathF.PI / size;
            
            vertices.AddRange(new []{0f, -height / 2,0f});
            normals.AddRange(new []{0f, -height / 2, 0f});
            
            vertices.AddRange(new []{0f, height / 2,0f});
            normals.AddRange(new []{0f, height / 2, 0f});

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

            LoadFigure(vertices.ToArray(), normals.ToArray(), indices.ToArray(), pos, rot, col);
        }
        private void LoadSphere(float radius, Vector3 pos, Vector3 col)
        {
            var vertices = new List<float>();
            var normals = new List<float>();
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
            LoadFigure(vertices.ToArray(), normals.ToArray(), indices.ToArray(), pos, new Vector3(0,0,0), col);
        }
        private void LoadSpring(float rounds, float height, float thickness, float radius, Vector3 pos, Vector3 rot, Vector3 col)
        {
            var vertices = new List<float>();
            var normals = new List<float>();
            var indices = new List<uint>();
            var pointCount = 0;
            var slices = 100;
            var step = 5;

            for (var i = -slices; i <= rounds * 360 + step; i += step)
            {
                for (var j = 0; j < slices; j ++)
                {
                    var t = (float)i / 360 + (float)j / slices * step / 360;
                    t = MathF.Max(0.0f, MathF.Min(rounds, t));
                    var a1 = t * MathF.PI * 2;
                    var a2 = (float)j / slices * MathF.PI * 2;
                    var d = radius + thickness * MathF.Cos(a2);
                    vertices.Add(d * MathF.Cos(a1));
                    vertices.Add(d * MathF.Sin(a1));
                    vertices.Add(thickness * MathF.Sin(a2) + height * t / rounds);
                    pointCount++;
                    normals.Add(MathF.Cos(a1));
                    normals.Add(MathF.Sin(a1));
                    normals.Add(MathF.Sin(a2));
                }
            }
            vertices.RemoveRange(0, 0);
            for (uint i = 0; i < (uint)vertices.Count / 3 - slices; ++i)
            {
                indices.Add(i);
                indices.Add(i + 1);
                indices.Add(i + (uint)slices);
                
                indices.Add(i + 1);
                indices.Add(i + (uint)slices);
                indices.Add(i + (uint)slices + 1);
            }

            var firstCenter = new Vector3();
            var secondCenter = new Vector3();
            for (var i = 0; i < slices * 3; i += 3)
            {
                firstCenter[0] += vertices[i];
                firstCenter[1] += vertices[i + 1];
                firstCenter[2] += vertices[i + 2];

                secondCenter[0] += vertices[vertices.Count - 3 - i];
                secondCenter[1] += vertices[vertices.Count - 2 - i];
                secondCenter[2] += vertices[vertices.Count - 1 - i];
            }
            firstCenter /= slices;
            secondCenter /= slices;
            vertices.AddRange(new []{firstCenter.X, firstCenter.Y, firstCenter.Z});
            vertices.AddRange(new []{secondCenter.X, secondCenter.Y, secondCenter.Z});
            for (uint i = 0; i < slices - 1; i++)
            {
                indices.AddRange(new[] {(uint) pointCount, i, i + 1});
            }
            for (uint i = 0; i < slices - 1; i++)
            {
                indices.AddRange(new[]{(uint)pointCount + 1, (uint)pointCount - 1 - i,(uint)pointCount - 2 - i});
            }
            LoadFigure(vertices.ToArray(), normals.ToArray(), indices.ToArray(), pos, rot, col);
        }
        private void LoadTorus(float innerRad, float outerRad, Vector3 pos, Vector3 rot, Vector3 col)
        {
            var vertices = new List<float>();
            var normals = new List<float>();
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
            
            LoadFigure(vertices.ToArray(), normals.ToArray(), indices.ToArray(), pos, rot, col);
        }

        private void LoadTetrahedron(float radius, Vector3 pos, Vector3 rot, Vector3 col)
        {
            var vertices = new List<float>()
            {
                //0, radius, 0,
                //radius * MathF.Cos(2 *MathF.PI / 3), -radius / 2, radius * MathF.Sin(MathF.PI / 3),
                //radius * MathF.Cos(4 * MathF.PI / 3), -radius / 2, radius * MathF.Sin(2 * MathF.PI / 3),
                //radius * MathF.Cos(2 * MathF.PI), -radius / 2, radius * MathF.Sin(2 * MathF.PI)
                radius, radius, radius,
                -radius, radius , -radius,
                -radius, -radius, radius,
                radius, -radius, -radius
            };

            var normals = new List<float>()
            {
                1, 1, 1,
                -1, 1 , -1,
                -1, -1, 1,
                1, -1, -1
            };

            var indices = new List<uint>()
            {
                0, 1, 2,
                0, 2, 3,
                0, 3, 1,
                1, 2, 3
            };
            
            LoadFigure(vertices.ToArray(), normals.ToArray(), indices.ToArray(), pos, rot, col);
        }

        private void LoadHexahedron(float radius, Vector3 pos, Vector3 rot, Vector3 col)
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
            
            LoadFigure(vertices.ToArray(), normals.ToArray(), indices.ToArray(), pos, rot, col);

        }
        private void LoadOctahedron(float radius, Vector3 pos, Vector3 rot, Vector3 col)
        {
            var vertices = new List<float>();
            var normals = new List<float>();

            vertices.AddRange(new []{0, radius, 0});
            vertices.AddRange(new []{0, -radius, 0});
            normals.AddRange(new []{0, 1f, 0});
            normals.AddRange(new []{0, -1f, 0});

            for (var i = 0; i < 4; i++)
            {
                var x = radius * MathF.Cos(i * MathF.PI / 2);
                var z = radius * MathF.Sin(i * MathF.PI / 2);
                
                vertices.AddRange(new []{x, 0, z});
                normals.AddRange(new []{x * 1 / radius, 0, z * 1 / radius});
            }

            var indices = new List<uint>
            {
                0, 2, 3,
                0, 3, 4, 
                0, 4, 5,
                0, 5, 2,
                
                1, 2, 3,
                1, 3, 4, 
                1, 4, 5,
                1, 5, 2
            };
            
            LoadFigure(vertices.ToArray(), normals.ToArray(), indices.ToArray(), pos, rot, col);
        }
        private void LoadIcosahedron(float radius, Vector3 pos, Vector3 rot, Vector3 col)
        {
            var vertices = new List<float>();
            var normals = new List<float>();

            var phi = (1f + MathF.Sqrt(5)) / 2f;
            var r = radius;
            
            foreach (var i in new int[] { -1, 1 })
            {
                foreach (var j in new int[] { -1, 1 })
                {
                    vertices.AddRange(new []{0, i * r, phi * j * r});
                    vertices.AddRange(new []{i * r, phi * j * r, 0});
                    vertices.AddRange(new []{phi * j * r, 0, i * r});
                    
                    normals.AddRange(new []{0, i * r, phi * j * r});
                    normals.AddRange(new []{i * r, phi * j * r, 0});
                    normals.AddRange(new []{phi * j * r, 0, i * r});
                }
            }
            var indices = new List<uint>()
            {
                0, 7, 1,
                3, 1, 7,
                5, 7, 0,
                0, 1, 2,
                5, 0, 6,
                0, 2, 6,
                8, 1, 3,
                3, 7, 11,
                8, 3, 9,
                9, 3, 11,
                2, 1, 8,
                11, 7, 5,
                6, 2, 4,
                4, 2, 8,
                4, 8, 9,
                9, 11, 10,
                10, 11, 5,
                10, 5, 6, 
                4, 9, 10, 
                10, 6, 4
            };
            
            LoadFigure(vertices.ToArray(), normals.ToArray(), indices.ToArray(), pos, rot, col);
        }

        private void LoadDodecahedron(float radius,Vector3 pos, Vector3 rot, Vector3 col)
        {
            var vertices = new List<float>();
            var normals = new List<float>();
            var indices = new List<uint>();

            var phi = (1f + MathF.Sqrt(5)) / 2f;
            var points = new List<Vector3>();
            foreach (var i in new int[] {-1, 1})
            {
                foreach (var j in new int[] {-1, 1})
                {
                    foreach (var k in new int[] {-1, 1})
                    {
                        vertices.AddRange(new[] {(float) i * radius, (float) j * radius, (float) k * radius});
                        normals.AddRange(new[] {(float) i, (float) j, (float) k});
                        points.Add(new Vector3(i, j, k));
                    }

                    vertices.AddRange(new[] {0, phi * i * radius, 1 / phi * j * radius});
                    vertices.AddRange(new[] {1 / phi * i * radius, 0, phi * j * radius});
                    vertices.AddRange(new[] {phi * i * radius, 1 / phi * j * radius, 0});

                    normals.AddRange(new[] {0, phi * i, 1 / phi * j});
                    normals.AddRange(new[] {1 / phi * i, 0, phi * j});
                    normals.AddRange(new[] {phi * i, 1 / phi * j, 0});

                    points.Add(new Vector3(0, phi * i * radius, 1 / phi * j * radius));
                    points.Add(new Vector3(1 / phi * i * radius, 0, phi * j * radius));
                    points.Add(new Vector3(phi * i * radius, 1 / phi * j * radius, 0));
                }
            }

            var faces = new List<int[]>()
            {
                new int[] {0, 2, 7, 1, 4},
                new int[] {2, 10, 14, 11, 7},
                new int[] {1, 7, 11, 18, 8},
                new int[] {10, 2, 0, 3, 13},
                new int[] {0, 4, 9, 5, 3},
                new int[] {4, 1, 8, 6, 9},
                new int[] {11, 14, 19, 16, 18},
                new int[] {10, 13, 15, 19, 14},
                new int[] {13, 3, 5, 12, 15},
                new int[] {8, 18, 16, 17, 6},
                new int[] {9, 6, 17, 12, 5},
                new int[] {19, 15, 12, 17, 16},
            };
            uint centerCounter = 20;
            for (var i = 0; i < faces.Count; i++)
            {
                var p1 = points[faces[i][0]];
                var p2 = points[faces[i][1]];
                var p3 = points[faces[i][2]];
                var p4 = points[faces[i][3]];
                var p5 = points[faces[i][4]];

                var center = (p1 + p2 + p3 + p4 + p5) / 5;
                vertices.AddRange(new[] {center.X, center.Y, center.Z});
                normals.AddRange(new[] {center.X, center.Y, center.Z});
                indices.AddRange(new[] {(uint) faces[i][0], (uint) faces[i][1], centerCounter});
                indices.AddRange(new[] {(uint) faces[i][1], (uint) faces[i][2], centerCounter});
                indices.AddRange(new[] {(uint) faces[i][2], (uint) faces[i][3], centerCounter});
                indices.AddRange(new[] {(uint) faces[i][3], (uint) faces[i][4], centerCounter});
                indices.AddRange(new[] {(uint) faces[i][4], (uint) faces[i][0], centerCounter});
                centerCounter++;
            }

            LoadFigure(vertices.ToArray(), normals.ToArray(), indices.ToArray(), pos, rot, col);
        }

        private void DrawAllFigures()
        {
            foreach (var figure in figures)
            {
                //figure.Pos = new Vector3(figure.Pos.X, MathF.Sin((timer) % (MathF.PI * 2)), figure.Pos.Z);
                Console.WriteLine(figure.Pos.Y);
                //figure.Rot = new Vector3(figure.Rot.X, (figure.Rot.Y + timer / 100000) % (2 * MathF.PI) , figure.Rot.Z);
                //figure.scale = new Vector3(MathF.Sin(timer % (2 * MathF.PI)), MathF.Sin(timer % (2 * MathF.PI)), MathF.Sin(timer % (2 * MathF.PI)));
                GL.BindVertexArray(figure.Vao);
                var eulerRot = new Vector3(MathHelper.RadiansToDegrees(figure.Rot.X),
                    MathHelper.RadiansToDegrees(figure.Rot.Y),
                    MathHelper.RadiansToDegrees(figure.Rot.Z));
                var model = Matrix4.Identity;
                model *= Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(eulerRot));
                model *= Matrix4.CreateTranslation(figure.Pos);
                //model *= Matrix4.CreateScale(figure.scale);
                _lightingShader.Use();

                _lightingShader.SetMatrix4("model", Matrix4.Identity);
                _lightingShader.SetMatrix4("view", _camera.GetViewMatrix());
                _lightingShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

                _lightingShader.SetVector3("viewPos", _camera.Position);

                
                _lightingShader.SetVector3("material.ambient", new Vector3(1.0f, 0.5f, 0.31f));
                _lightingShader.SetVector3("material.diffuse", new Vector3(1.0f, 0.5f, 0.31f));
                _lightingShader.SetVector3("material.specular", new Vector3(0.0f, 0.0f, 0.0f));
                _lightingShader.SetFloat("material.shininess", 32.0f);
                
                Vector3 lightColor;
                
                lightColor.X = figure.Col.X;
                lightColor.Y = figure.Col.Y;
                lightColor.Z = figure.Col.Z;
                
                Vector3 ambientColor = lightColor * new Vector3(0.5f);
                Vector3 diffuseColor = lightColor * new Vector3(0.8f);

                _lightingShader.SetVector3("light.position", _lightPos);
                _lightingShader.SetVector3("light.ambient", ambientColor);
                _lightingShader.SetVector3("light.diffuse", diffuseColor);
                _lightingShader.SetVector3("light.specular", new Vector3(1.0f, 1.0f, 1.0f));
                
                _lightingShader.SetMatrix4("model", model);
                GL.DrawElements(_primitiveType, figure.indCount, DrawElementsType.UnsignedInt, 0);
                
                lightColor.X = 1;
                lightColor.Y = 1;
                lightColor.Z = 1;
                
                ambientColor = lightColor * new Vector3(1f);
                diffuseColor = lightColor * new Vector3(1f);

                _lightingShader.SetVector3("light.position", _lightPos);
                _lightingShader.SetVector3("light.ambient", ambientColor);
                _lightingShader.SetVector3("light.diffuse", diffuseColor);
                _lightingShader.SetVector3("light.specular", new Vector3(1.0f, 1.0f, 1.0f));
                //GL.DrawElements(PrimitiveType.LineLoop, figure.indCount, DrawElementsType.UnsignedInt, 0);
            }
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

            if (input.IsKeyReleased(Keys.LeftAlt))
                _primitiveType = _primitiveType == PrimitiveType.LineLoop
                    ? PrimitiveType.Triangles
                    : PrimitiveType.LineLoop;
            const float cameraSpeed = 1.5f;
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
}