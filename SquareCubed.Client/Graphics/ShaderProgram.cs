﻿using System;
using System.Diagnostics.Contracts;
using System.IO;
using OpenTK.Graphics.OpenGL;
using SquareCubed.Common.Utils;

namespace SquareCubed.Client.Graphics
{
	public class ShaderException : Exception
	{
		public int OpenGlId { get; set; }
		public int OpenGlResult { get; set; }
		public string OpenGlLog { get; set; }

		public ShaderException(string message, Exception inner = null)
			:base(message, inner)
		{
		}
	}


	public sealed class ShaderProgram : IDisposable
	{
		private static Logger _logger = new Logger("Shaders");

		private int _program = -1;

		public ShaderProgram(string vertPath, string fragPath)
		{
			Contract.Requires<ArgumentNullException>(
				vertPath != null,
				"ShaderProgram requires a vertex shader path!");
			Contract.Requires<ArgumentNullException>(
				vertPath != null,
				"ShaderProgram requires a fragment shader path!");

			// Create the shaders in OpenGL
			var vertexShader = GL.CreateShader(ShaderType.VertexShader);
			var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

			try
			{
				// Compile the shader sources
				CompileShader(vertexShader, vertPath);
				CompileShader(fragmentShader, fragPath);

				// Create a new shader program and link the shaders to it
				_program = GL.CreateProgram();
				GL.AttachShader(_program, vertexShader);
				GL.AttachShader(_program, fragmentShader);
				GL.LinkProgram(_program);

				// Make sure linking was successful
				int linkResult;
				GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out linkResult);
				if (linkResult != 1)
				{
					// Unsuccessful, we have to throw an error
					throw new ShaderException("Shader failed to compile!")
					{
						OpenGlId = _program,
						OpenGlResult = linkResult,
						OpenGlLog = GL.GetProgramInfoLog(_program)
					};
				}

				_logger.LogInfo("Created new shader program {0}!", _program);
			}
			catch (Exception)
			{
				// Clean up anything we created before we leave
				Dispose();
				throw;
			}
			finally
			{
				// The shaders are now in the program, so we can remove the originals
				GL.DeleteShader(vertexShader);
				GL.DeleteShader(fragmentShader);
			}
		}

		private static void CompileShader(int shader, string sourcePath)
		{
			// Read the shader files into memory
			var src = File.ReadAllText(sourcePath);

			// Compile the shader
			GL.ShaderSource(shader, src);
			GL.CompileShader(shader);

			// Make sure compilation was successful
			int compileResult;
			GL.GetShader(shader, ShaderParameter.CompileStatus, out compileResult);
			if (compileResult == 1) return;

			// Unsuccessful, we have to throw an error
			throw new ShaderException("Shader failed to compile!")
			{
				OpenGlId = shader,
				OpenGlResult = compileResult,
				OpenGlLog = GL.GetShaderInfoLog(shader)
			};
		}

		~ShaderProgram()
		{
			Dispose();
		}

		public void Dispose()
		{
			// We only have unmanaged resources
			if (_program != -1) GL.DeleteProgram(_program);

			GC.SuppressFinalize(this);
		}
	}
}