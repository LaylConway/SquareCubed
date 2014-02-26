﻿using System.Collections.Generic;
using Lidgren.Network;
using OpenTK;
using SquareCubed.Data;
using SquareCubed.Server.Worlds;

namespace SquareCubed.Server.Structures
{
	public class Structure
	{
		private World _world;

		public Structure()
		{
			Chunks = new List<Chunk>();
		}

		public World World
		{
			get { return _world; }
			set
			{
				var oldWorld = value;
				_world = value;

				// Update links in worlds
				if (oldWorld != null)
					oldWorld.UpdateStructureEntry(this);
				if (_world != null)
					_world.UpdateStructureEntry(this);
			}
		}

		public uint Id { get; set; }
		public List<Chunk> Chunks { get; set; }
		public Vector2 Position { get; set; }
	}

	public static class StructureExtensions
	{
		public static void Write(this NetOutgoingMessage msg, Structure structure)
		{
			// Add metadata and position
			msg.Write(structure.Id);
			msg.Write(structure.Position);

			// Add structure chunk data
			msg.Write(structure.Chunks.Count);
			foreach (var chunk in structure.Chunks)
				msg.Write(chunk);
		}
	}
}