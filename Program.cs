﻿using OpenTK;
using Assimp;
using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using OpenTK.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.ComponentModel;


/*
Движок ещё в разработке!
Он не готов
*/




class MainSettingsEngine
{
    public GameWindow _Window;
    public NativeWindowSettings _WindowSettings;

    private ShaderSystem _Shader = new ShaderSystem();
    private Camera _Camera;
    private Import _Import;

    public List<float> _Vert = new List<float>();
    private int _VBO;
    private int _VAO;

    public void Activation()
    {
        _WindowSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(800,800),
            Title = "LineEngine"
        };

        _Window = new GameWindow(GameWindowSettings.Default, _WindowSettings);

        _Window.Unload += _Window_Unload;
        _Window.Load += _Window_Load;
        _Window.RenderFrame += _Window_RenderFrame;
        _Window.UpdateFrame += _Window_UpdateFrame;

        _Window.Run();
    }

    
    private void _Window_Load()
    {
        _Shader.UseAndIntilisation("D:\\LineEngine\\Core\\LineEngineCore\\Shader\\VertShader.glsl", "D:\\LineEngine\\Core\\LineEngineCore\\Shader\\FragShader.glsl");
        _Shader.Use();

        _Camera = new Camera(new Vector3(0.0f, 0.0f, 0.0f));


        _Import = new Import();
        _Import.ImportModel("Твоя модель/You model");
        GL.ClearColor(Color4.CornflowerBlue);
    }
   


    private void _Window_RenderFrame(FrameEventArgs obj)
    {

        float _ApsRat = 800f / 600f; 

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        

        _Shader.SetMatrix4("view", _Camera.GetView());
        _Shader.SetMatrix4("projection", _Camera.GetProjection(_ApsRat));

        Matrix4 _Model = Matrix4.Identity;
        _Shader.SetMatrix4("model", _Model);
        _Import.Draw(_Shader);
        GL.BindVertexArray(_VAO);
        GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 3);


        _Window.SwapBuffers();
    }

    private void _Window_UpdateFrame(FrameEventArgs obj)
    {
        //Движение камеры
        var _Input = _Window.KeyboardState;
        const float _CameraSpeed = 1.5f * 0.01f;

        if (_Input.IsKeyDown(Keys.W))
        {
            _Camera._Position += _Camera._Front * _CameraSpeed;
            Console.WriteLine($"Координаты(X){_Camera._Position.X} (Y){_Camera._Position.Y} (Z){_Camera._Position.Z}");
        }
        if (_Input.IsKeyDown(Keys.S))
        {
            _Camera._Position -= _Camera._Front * _CameraSpeed;
            Console.WriteLine($"Координаты(X){_Camera._Position.X} (Y){_Camera._Position.Y} (Z){_Camera._Position.Z}");
        }
        if (_Input.IsKeyDown(Keys.A))
        {
            _Camera._Position -= _Camera._Front * _CameraSpeed;
            Console.WriteLine($"Координаты(X){_Camera._Position.X} (Y){_Camera._Position.Y} (Z){_Camera._Position.Z}");
        }
        if (_Input.IsKeyDown(Keys.D))
        {
            _Camera._Position += _Camera._Front * _CameraSpeed;
            Console.WriteLine($"Координаты(X){_Camera._Position.X} (Y){_Camera._Position.Y} (Z){_Camera._Position.Z}");
        }

        var _Mouse = _Window.MouseState;
        const float _Sensitivity = 0.2f;

        _Camera._Yaw += _Mouse.Delta.X * _Sensitivity;
        _Camera._Pitch -= _Mouse.Delta.Y * _Sensitivity;
        _Camera._Pitch = MathHelper.Clamp(_Camera._Pitch, -89.0f,89.0f);

        _Camera.UpdateVector();
    }

    private void _Window_Unload()
    {
        
    }

    
}

//Импорт
class Import
{


    private List<float> _Vert = new List<float>();
    private int _VAO;
    private int _VBO;
    

    public void ImportModel(string _File)
    {
        var _Import = new AssimpContext();
        var _Scene = _Import.ImportFile(_File, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

        foreach (var _Mesh in _Scene.Meshes)
        {
            for (int i = 0; i < _Mesh.VertexCount; i++)
            {
                _Vert.Add(_Mesh.Vertices[i].X);
                _Vert.Add(_Mesh.Vertices[i].Y);
                _Vert.Add(_Mesh.Vertices[i].Z);

                if (_Mesh.HasNormals)
                {
                    _Vert.Add(_Mesh.Normals[i].X);
                    _Vert.Add(_Mesh.Normals[i].Y);
                    _Vert.Add(_Mesh.Normals[i].Z);
                }

                if (_Mesh.HasTextureCoords(0))
                {
                    _Vert.Add(_Mesh.TextureCoordinateChannels[0][i].X);
                    _Vert.Add(_Mesh.TextureCoordinateChannels[0][i].Y);
                }
                else
                {
                    _Vert.Add(0.0f);
                    _Vert.Add(0.0f);
                }
            }
        }
       
        

        

        _VBO = GL.GenBuffer();
        _VAO = GL.GenVertexArray();

        GL.BindVertexArray(_VAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);        
        GL.BufferData(BufferTarget.ArrayBuffer, _Vert.Count * sizeof(float), _Vert.ToArray(), BufferUsageHint.StaticDraw);

        int _Ride = 8 * sizeof(float);

        GL.VertexAttribPointer(0,3,VertexAttribPointerType.Float, false, _Ride, 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1,3,VertexAttribPointerType.Float, false, _Ride, 3*sizeof(float));
        GL.EnableVertexAttribArray(1);


        GL.VertexAttribPointer(2,2, VertexAttribPointerType.Float, false, _Ride, 6*sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.ClearColor(Color4.Black);
    }

    public void Draw(ShaderSystem _Shader)
    {
        _Shader.Use();
        GL.BindVertexArray(_VAO);
        GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, _Vert.Count /8);
    }

   
}


//Шейдеры
class ShaderSystem()
{
    public int _Hand;

    public void UseAndIntilisation(string _VertShaderPath, string _FragShaderPath)
    {
        string _VertSource = File.ReadAllText(_VertShaderPath);
        string _FragSource = File.ReadAllText(_FragShaderPath);

        int _VertShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(_VertShader, _VertSource);
        GL.CompileShader(_VertShader);
        CheckAllSystem(_VertShader, true, false);

        int _FragShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(_FragShader, _FragSource);
        GL.CompileShader(_FragShader);
        CheckAllSystem(_FragShader, true, false);

        _Hand = GL.CreateProgram();
        GL.AttachShader(_Hand, _VertShader);
        GL.AttachShader(_Hand, _FragShader);
        GL.LinkProgram(_Hand);
        CheckAllSystem(_Hand, false, true);

        GL.DetachShader(_Hand, _VertShader);
        GL.DetachShader(_Hand, _FragShader);
        GL.DeleteShader(_VertShader);
        GL.DeleteShader(_FragShader);
    }

    public void Use()
    {
        GL.UseProgram(_Hand);
    }

    private void CheckAllSystem(int _Shader, bool _CheckShader, bool _CheckLink)
    {
        
        var _Error = GL.GetError();
        if (_Error != OpenTK.Graphics.OpenGL4.ErrorCode.NoError)
        {
            throw new Exception(_Error.ToString());
        }




        if(_CheckShader == true)
        {
            GL.GetShader(_Shader, ShaderParameter.CompileStatus, out int _Success);
            if (_Success == 0)
            {
                string _LogInfo = GL.GetShaderInfoLog(_Shader);
                throw new Exception(_LogInfo);
            }
        }

        if(_CheckLink == true)
        {
            GL.GetProgram(_Shader, GetProgramParameterName.LinkStatus, out int _SuccessLink);
            if (_SuccessLink == 0)
            {
                string _LogInfoLink = GL.GetProgramInfoLog(_Shader);
                throw new Exception(_LogInfoLink);
            }
        }

    }

    public void SetMatrix4(string _Name, Matrix4 _Matrix)
    {
        int _Location = GL.GetUniformLocation(_Hand, _Name);
        GL.UniformMatrix4(_Location, false, ref _Matrix);
    }
}


class Start()
{
    static void Main()
    {
        MainSettingsEngine _Engine = new MainSettingsEngine();
        _Engine.Activation();
    }
}
