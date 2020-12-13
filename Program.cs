using System;

namespace MetalCombatReader
{
	class Program
	{
		static void Main(string[] args)
		{
			Arguments.Parse(args);

			RomFile.Load(Arguments.Path);

			MetalCombatApu.Load();
		}
	}
}
