using System;

namespace MetalCombatReader
{
	class Program
	{
		static void Main(string[] args)
		{
			Arguments.Parse(args);

			RomFile.Load(Arguments.Path);

			System.IO.File.WriteAllBytes(Arguments.Path + ".snes.bin", Snes.Memory);

			MetalCombatApu.Load();
		}
	}
}
