using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetalCombatReader
{
	internal static class MetalCombatGame
	{
		internal static int ApuDataAddress = 0xB98000;
		internal static int ApuDirectoryAddress = 0x1B00;
		internal static int ApuDirectoryLength = 0x58;

		internal static int SongTableAddress = 0xBA8200;
		internal static int SongCount = 226;
		internal static int TrackTableAddress = 0xC200;
	}
}
