using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetalCombatReader
{
	public static class WaveFile
	{
		public static void Save(string path)
		{
			var stream2 = File.Create(path);
			var writer = new BinaryWriter(stream2);

			writer.Write(Encoding.ASCII.GetBytes("RIFF"));
			var fileLengthPosition = stream2.Position;
			writer.Write(0);
			writer.Write(Encoding.ASCII.GetBytes("WAVE"));

			writer.Write(Encoding.ASCII.GetBytes("fmt "));
			writer.Write(16);
			writer.Write((ushort)1);
			writer.Write((ushort)1);
			//writer.Write(32000);
			//writer.Write(64000);
			writer.Write(8000);
			writer.Write(32000);
			writer.Write((ushort)4);
			writer.Write((ushort)16);

			writer.Write(Encoding.ASCII.GetBytes("data"));
			var dataLengthPosition = stream2.Position;
			writer.Write(0);

			writer.Write(Sound.Data);

			writer.Flush();

			var fileLength = (int)stream2.Position;

			stream2.Position = dataLengthPosition;

			writer.Write(fileLength - 36);

			writer.Flush();

			stream2.Position = fileLengthPosition;

			writer.Write(fileLength - 8);

			writer.Flush();
			writer.Close();
			writer.Dispose();
		}
	}
}
