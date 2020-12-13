using System;
using System.IO;

namespace MetalCombatReader
{
	internal class RomFile
	{
		internal static byte[] Data;

		internal static void Load(string path)
		{
			Data = File.ReadAllBytes(path);

			Rom.Data = new byte[Data.Length - 0x200];

			Array.Copy(Data, 0x0200, Rom.Data, 0, Rom.Data.Length);

			SnesRom.Load();
		}
	}
}