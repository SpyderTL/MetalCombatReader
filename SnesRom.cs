using System;

namespace MetalCombatReader
{
	internal class SnesRom
	{
		internal static void Load()
		{
			Snes.Memory = new byte[0x1000000];

			for (var bank = 0x00; bank < 0x7E; bank++)
			{
				if (Rom.Data.Length > bank * 0x8000)
					Array.Copy(Rom.Data, bank * 0x8000, Snes.Memory, (bank * 0x10000) + 0x8000, 0x8000);
			}

			for (var bank = 0xFE; bank < 0x100; bank++)
			{
				if (Rom.Data.Length > bank * 0x8000)
					Array.Copy(Rom.Data, bank * 0x8000, Snes.Memory, (bank * 0x10000) + 0x8000, 0x8000);
			}

			Array.Copy(Snes.Memory, 0, Snes.Memory, 0x800000, 0x7E0000);
		}
	}
}