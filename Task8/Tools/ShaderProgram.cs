using System;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace CompGraph.Tools;

struct Shader : IDisposable
{
    public readonly int Id;

    public Shader(ShaderType shaderType, string sourceCode)
    {
        Id = GL.CreateShader(shaderType);

        sourceCode = PreProcessIncludes(sourceCode);
        GL.ShaderSource(Id, sourceCode);
        GL.CompileShader(Id);

        var compileInfo = GL.GetShaderInfoLog(Id);
        if (compileInfo != string.Empty)
            Console.WriteLine(compileInfo);
    }
        
    private static string PreProcessIncludes(string s)
    {
        var includedContent = new StringBuilder(s.Length + 2000);
        using var stringReader = new StringReader(s);

        string line;
        while ((line = stringReader.ReadLine()) is not null)
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 9 && trimmed.Substring(0, 9) == "#include ")
            {
                var filePath = $"Shaders/{trimmed.Substring(9, trimmed.Length - 9)}.glsl";
                includedContent.Append(PreProcessIncludes(File.ReadAllText(filePath)));
            }
            else
            {
                includedContent.AppendLine(line);
            }
        }
        return includedContent.ToString();
    }

    public void Dispose() => GL.DeleteShader(Id);
}

class ShaderProgram : IDisposable
{
    private static int _lastBindedId = -1;

    private readonly int _id;
    public ShaderProgram(params Shader[] shaders)
    {
        _id = GL.CreateProgram();

        for (var i = 0; i < shaders.Length; i++)
            GL.AttachShader(_id, shaders[i].Id);

        GL.LinkProgram(_id);
        for (var i = 0; i < shaders.Length; i++)
        {
            GL.DetachShader(_id, shaders[i].Id);
            shaders[i].Dispose();
        }
    }

    public void Use()
    {
        if (_lastBindedId != _id)
        {
            GL.UseProgram(_id);
            _lastBindedId = _id;
        }
    }

    public void Upload(string name, Vector3 vector3) => GL.ProgramUniform3(_id, GetUniformLocation(name), vector3);
    public void Upload(string name, Vector2 vector2) => GL.ProgramUniform2(_id, GetUniformLocation(name), vector2);
    public void Upload(string name, float x) => GL.ProgramUniform1(_id, GetUniformLocation(name), x);
    public void Upload(int location, int x) => GL.ProgramUniform1(_id, location, x);
    public void Upload(string name, int x) => GL.ProgramUniform1(_id, GetUniformLocation(name), x);
    private int GetUniformLocation(string name) => GL.GetUniformLocation(_id, name);
    public void Dispose() => GL.DeleteProgram(_id);
}