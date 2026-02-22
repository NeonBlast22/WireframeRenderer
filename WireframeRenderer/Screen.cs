using System;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using Silk.NET.Input;

namespace WireframeRenderer;

public struct Color
{
    public float R;
    public float G;
    public float B;
    public float A;
    
    public Color(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color(float r, float g, float b)
    {
        R = r;
        G = g;
        B = b;
        A = 1.0f;
    }
}
public static class Screen
{
    private static IWindow window;
    private static GL gl;
    private static uint texture;
    private static uint vao, vbo, ebo;
    private static uint shaderProgram;
    private static byte[] pixels;
    private static int _width;
    private static int _height;
    private static IInputContext input;
    
    
    public delegate void UpdateEvent(double deltaTime);
    public delegate void StartEvent();
    
    public delegate void KeyEvent(Key key);
    
    public static event UpdateEvent? OnUpdate;
    public static event StartEvent? OnStart;
    
    public static event KeyEvent? OnKeyEvent;

    public static void Initialize(int width, int height, string title)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = title;
        options.VSync = false;
        options.FramesPerSecond = 0;
        options.UpdatesPerSecond = 0;
        _width = width;
        _height = height;

        window = Window.Create(options);

        window.Load += OnLoad;
        window.Render += OnRender;
        window.Closing += OnClose;
        
        window.Run();
    }

    public static bool GetKey(Key key)
    {
        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            if (input.Keyboards[i].IsKeyPressed(key)) return true;
        }

        return false;
    }
    

    private static void OnLoad()
    {
        gl = GL.GetApi(window);
        
        // Create pixel buffer (RGBA format)
        pixels = new byte[_width * _height * 4];
        
        // Create shaders
        string vertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec2 aPos;
            layout (location = 1) in vec2 aTexCoord;
            out vec2 TexCoord;
            void main()
            {
                gl_Position = vec4(aPos, 0.0, 1.0);
                TexCoord = aTexCoord;
            }
        ";
        
        string fragmentShaderSource = @"
            #version 330 core
            out vec4 FragColor;
            in vec2 TexCoord;
            uniform sampler2D texture1;
            void main()
            {
                FragColor = texture(texture1, TexCoord);
            }
        ";
        
        uint vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vertexShader, vertexShaderSource);
        gl.CompileShader(vertexShader);
        
        uint fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fragmentShader, fragmentShaderSource);
        gl.CompileShader(fragmentShader);
        
        shaderProgram = gl.CreateProgram();
        gl.AttachShader(shaderProgram, vertexShader);
        gl.AttachShader(shaderProgram, fragmentShader);
        gl.LinkProgram(shaderProgram);
        
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
        
        // Fullscreen quad vertices
        float[] vertices = {
            // positions   // texCoords
            -1.0f,  1.0f,  0.0f, 0.0f,
             1.0f,  1.0f,  1.0f, 0.0f,
             1.0f, -1.0f,  1.0f, 1.0f,
            -1.0f, -1.0f,  0.0f, 1.0f
        };
        
        uint[] indices = { 0, 1, 2, 2, 3, 0 };
        
        vao = gl.GenVertexArray();
        vbo = gl.GenBuffer();
        ebo = gl.GenBuffer();
        
        gl.BindVertexArray(vao);
        
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        unsafe
        {
            fixed (float* v = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
            }
        }
        
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        unsafe
        {
            fixed (uint* i = indices)
            {
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
            }
        }
        
        unsafe
        {
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)0);
            gl.EnableVertexAttribArray(0);
            
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)(2 * sizeof(float)));
            gl.EnableVertexAttribArray(1);
        }
        
        // Create texture
        texture = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, texture);
        
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
        
        unsafe
        {
            fixed (byte* ptr = pixels)
            {
                gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, 
                    (uint)_width, (uint)_height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            }
        }
        
        Start();
    }

    public static void SetPixel(int x, int y, Color color)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height) return;
        
        int index = (y * _width + x) * 4;
        pixels[index] = (byte)(color.R * 255);
        pixels[index + 1] = (byte)(color.G * 255);
        pixels[index + 2] = (byte)(color.B * 255);
        pixels[index + 3] = (byte)(color.A * 255);
    }

    public static void ClearScreen()
    {
        pixels = new byte[_width * _height * 4]; 
    }

    private static void OnRender(double deltaTime)
    {
        Update(deltaTime);
        
        gl.Clear(ClearBufferMask.ColorBufferBit);
        
        // Update texture
        gl.BindTexture(TextureTarget.Texture2D, texture);
        unsafe
        {
            fixed (byte* ptr = pixels)
            {
                gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 
                    (uint)_width, (uint)_height, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            }
        }
        
        // Draw
        gl.UseProgram(shaderProgram);
        gl.BindVertexArray(vao);
        
        unsafe
        {
            gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
        }
    }

    private static void OnClose()
    {
        gl.DeleteTexture(texture);
        gl.DeleteVertexArray(vao);
        gl.DeleteBuffer(vbo);
        gl.DeleteBuffer(ebo);
        gl.DeleteProgram(shaderProgram);
        gl.Dispose();
    }

    private static void Start()
    {
        input = window.CreateInput();
        OnStart?.Invoke();
    }
    
    private static void Update(double deltaTime)
    {
        OnUpdate?.Invoke(deltaTime);
    }
}