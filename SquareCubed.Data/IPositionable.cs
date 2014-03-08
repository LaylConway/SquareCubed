﻿using OpenTK;

namespace SquareCubed.Data
{
	public interface IPositionable
	{
		Vector2 Center { get; }
		Vector2 Position { get; }
		float Rotation { get; }
	}
}