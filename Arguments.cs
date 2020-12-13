using System;

namespace MetalCombatReader
{
	internal class Arguments
	{
		internal static string Path;

		internal static void Parse(string[] args)
		{
			Path = args[0];
		}
	}
}