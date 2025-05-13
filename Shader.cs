using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace minecraft
{
    public class Shader
    {
        public readonly int Handle;
        private Dictionary<string, int> _uniforms;

        public Shader(string vertPath, string fragPath)
        {
            try
            {
                var shaderData = File.ReadAllText(vertPath);
                var vertexShader = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(vertexShader, shaderData);
                CompileShader(vertexShader);

                shaderData = File.ReadAllText(fragPath);
                var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(fragmentShader, shaderData);
                CompileShader(fragmentShader);

                Handle = GL.CreateProgram();

                GL.AttachShader(Handle, vertexShader);
                GL.AttachShader(Handle, fragmentShader);

                LinkProgram(Handle);

                LoadAttributeLocations();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading shader: {ex.Message}");
                throw;
            }
        }

        private void LoadAttributeLocations()
        {
            try
            {
                GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
                _uniforms = new Dictionary<string, int>();

                for (var i = 0; i < numberOfUniforms; i++)
                {
                    var key = GL.GetActiveUniform(Handle, i, out _, out _);
                    var location = GL.GetUniformLocation(Handle, key);
                    _uniforms.Add(key, location);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading attribute locations: {ex.Message}");
                throw;
            }
        }

        public void Use()
        {
            try
            {
                GL.UseProgram(Handle);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error using shader program: {ex.Message}");
                throw;
            }
        }

        private static void CompileShader(int shader)
        {
            try
            {
                GL.CompileShader(shader);

                GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
                if (code == 0)
                {
                    GL.GetShaderInfoLog(shader, out string infoLog);
                    throw new Exception($"Shader compilation failed: {infoLog}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error compiling shader: {ex.Message}");
                throw;
            }
        }

        private static void LinkProgram(int program)
        {
            try
            {
                GL.LinkProgram(program);
                GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
                if (code == 0)
                {
                    GL.GetProgramInfoLog(program, out string infoLog);
                    throw new Exception($"Program link failed: {infoLog}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error linking program: {ex.Message}");
                throw;
            }
        }

        public int GetAttribLocation(string attribName)
        {
            try
            {
                return GL.GetAttribLocation(Handle, attribName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting attribute location: {ex.Message}");
                throw;
            }
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            try
            {
                GL.UseProgram(Handle);
                GL.UniformMatrix4(_uniforms[name], true, ref data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting matrix uniform '{name}': {ex.Message}");
                throw;
            }
        }

        public void SetInt(string name, int data)
        {
            try
            {
                GL.UseProgram(Handle);
                GL.Uniform1(_uniforms[name], data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting integer uniform '{name}': {ex.Message}");
                throw;
            }
        }

        public void SetVector3(string name, Vector3 v)
        {
            try
            {
                GL.UseProgram(Handle);
                GL.Uniform3(_uniforms[name], v);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting color uniform '{name}': {ex.Message}");
                throw;
            }
        }

        public void SetFloat(string name, float v)
        {
            try
            {
                GL.UseProgram(Handle);
                GL.Uniform1(_uniforms[name], v);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting color uniform '{name}': {ex.Message}");
                throw;
            }
        }
    }
}
