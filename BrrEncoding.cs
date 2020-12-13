using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetalCombatReader
{
	public static class BrrEncoding
	{
		public static byte[] Data;

		public static void Decode()
		{
			using var stream = new MemoryStream(Data);
			using var reader = new BinaryReader(stream);

			var data2 = new List<short>();

			while (stream.Position < stream.Length)
			{
				var header = reader.ReadByte();

				var range = header >> 4;
				var filter = (header >> 2) & 0x3;
				var loop = (header & 0x2) != 0;
				var end = (header & 0x1) != 0;

				var data = reader.ReadBytes(8);
				var samples = data.SelectMany(x => new int[] { SignedNibbleToInt(x >> 4), SignedNibbleToInt(x & 0xf) }).ToArray();

				foreach (var sample in samples)
				{
					var value = sample << range;

					switch (filter)
					{
						case 1:
							if (data2.Count > 0)
								value += (int)(data2[data2.Count - 1] * (15.0f / 16.0f));
							break;

						case 2:
							if (data2.Count > 1)
								value += (int)((data2[data2.Count - 1] * (61.0f / 32.0f)) - (data2[data2.Count - 2] * (15.0f / 16.0f)));
							break;

						case 3:
							if (data2.Count > 1)
								value += (int)((data2[data2.Count - 1] * (115.0f / 64.0f)) - (data2[data2.Count - 2] * (13.0f / 16.0f)));
							break;
					}

					data2.Add((short)value);
				}

				if (end)
					break;
			}

			using var stream2 = new MemoryStream();
			using var writer = new BinaryWriter(stream2);

			for (var index = 0; index < data2.Count; index++)
				writer.Write(data2[index]);

			writer.Flush();
			Sound.Data = stream2.ToArray();
		}

		private static int SignedNibbleToInt(int value)
		{
			if (value < 8)
				return value;

			return value - 16;
		}
	}
}
