using System;
using System.IO;

namespace MetalCombatReader
{
	internal class MetalCombatApu
	{
		internal static void Load()
		{
			if(!Directory.Exists("Apu"))
				Directory.CreateDirectory("Apu");

			var position = MetalCombatGame.ApuDataAddress;

			while (true)
			{
				var length = BitConverter.ToUInt16(Snes.Memory, position);
				position += 2;

				if (length == 0)
				{
					var startAddress = BitConverter.ToUInt16(Snes.Memory, position);

					Console.WriteLine("Apu.StartAddress = 0x" + startAddress.ToString("X4"));
					break;
				}

				var address = BitConverter.ToUInt16(Snes.Memory, position);
				position += 2;

				var data = new byte[length];

				Array.Copy(Snes.Memory, position, data, 0, length);

				File.WriteAllBytes("Apu/" + address.ToString("X4") + "-" + (address + length - 1).ToString("X4") + ".dat", data);

				Array.Copy(data, 0, Apu.Memory, address, length);

				position += length;
			}

			File.WriteAllBytes("Apu/Apu.dat", Apu.Memory);

			// Write BRR Samples
			position = MetalCombatGame.ApuDirectoryAddress;

			while (position < MetalCombatGame.ApuDirectoryAddress + MetalCombatGame.ApuDirectoryLength)
			{
				var address = BitConverter.ToUInt16(Apu.Memory, position);
				position += 2;
				var loop = BitConverter.ToUInt16(Apu.Memory, position);
				position += 2;

				var length = loop - address;

				if (length > 0)
				{
					var data = new byte[length];

					Array.Copy(Apu.Memory, address, data, 0, length);

					File.WriteAllBytes("Apu/" + address.ToString("X4") + "-" + (address + length - 1).ToString("X4") + ".brr", data);

					BrrEncoding.Data = data;

					BrrEncoding.Decode();

					WaveFile.Save("Apu/" + address.ToString("X4") + "-" + (address + length - 1).ToString("X4") + ".wav");
				}
			}
		}
	}
}