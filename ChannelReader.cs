using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetalCombatReader
{
	public static class ChannelReader
	{
		public static int Position;
		public static int Value;
		public static EventTypes EventType;
		public static int Special;
		public static int Note;
		public static int Length;
		public static int Duration;
		public static int Velocity;
		public static int Call;
		public static int Repeat;
		public static int Tempo;
		public static int Pan;
		public static int Phase;
		public static int Instrument;
		public static int PercussionInstrumentOffset;
		public static int Volume;
		public static int Fade;
		public static int Transpose;
		public static int Tuning;
		public static int PitchSlide;
		public static int Delay;
		public static InstrumentRecord[] Instruments;

		public static void Read()
		{
			Value = Apu.Memory[Position++];

			if (Value == RomSongs.EndTrack)
				EventType = EventTypes.Stop;
			else if (Value < RomSongs.FirstNote)
			{
				Length = Value;

				if ((Apu.Memory[Position] & 0x80) != 0x00)
					EventType = EventTypes.Length;
				else if ((Apu.Memory[Position] & 0x40) != 0x00)
				{
					EventType = EventTypes.LengthVelocity;
					Velocity = Apu.Memory[Position] & 0x3f;
					Position++;
				}
				else
				{
					EventType = EventTypes.LengthDuration;
					Duration = Apu.Memory[Position] & 0x3f;
					Position++;

					if ((Apu.Memory[Position] & 0x80) == 0x00)
					{
						EventType = EventTypes.LengthDurationVelocity;

						Velocity = Apu.Memory[Position] & 0x3f;

						Position++;
					}
				}
				//else
				//{
				//	EventType = EventTypes.LengthDurationVelocity;

				//	Duration = Apu.Memory[Position] >> 4;
				//	Velocity = Apu.Memory[Position] & 0x0f;

				//	Position++;
				//}
			}
			else if (Value <= RomSongs.LastNote)
			{
				EventType = EventTypes.Note;
				Note = Value - RomSongs.FirstNote;

				//if (Apu.Memory[Position] != 0xf9)
				//{
				Delay = 0;
				Duration = 0;
				PitchSlide = 0;
				//}
				//else
				//{
				//	Position++;

				//	Delay = Apu.Memory[Position++];
				//	Duration = Apu.Memory[Position++];
				//	PitchSlide = Apu.Memory[Position++] - RomSongs.FirstNote;
				//}
			}
			else if (Value == RomSongs.Tie)
				EventType = EventTypes.Tie;
			else if (Value <= RomSongs.LastRest)
				EventType = EventTypes.Rest;
			else if (Value <= RomSongs.LastPercussion)
			{
				EventType = EventTypes.Percussion;
				Note = Value - RomSongs.FirstPercussion;
			}
			else if (Value == 0xD6)
			{
				EventType = EventTypes.Instrument;
				Instrument = Apu.Memory[Position];
				Position += 1;
			}
			else if (Value == 0xD7)
			{
				EventType = EventTypes.Other;
				Position += 1;
			}
			else if (Value == 0xD8)
			{
				EventType = EventTypes.Other;
				Position += 2;
			}
			else if (Value == 0xD9)
			{
				EventType = EventTypes.Other;
				Position += 3;
			}
			else if (Value == 0xDA)
			{
				EventType = EventTypes.Other;
				Position += 0;
			}
			else if (Value == 0xDB)
			{
				EventType = EventTypes.Other;
				Position += 1;
			}
			else if (Value == 0xDC)
			{
				EventType = EventTypes.Other;
				Position += 2;
			}
			else if (Value == 0xDD)
			{
				EventType = EventTypes.Tempo;
				Tempo = Apu.Memory[Position];
				Position += 1;
			}
			else if (Value == 0xDE)
			{
				EventType = EventTypes.Other;
				Position += 2;
			}
			else if (Value == 0xDF)
			{
				EventType = EventTypes.Other;
				Position += 1;
			}
			else if (Value == 0xE0)
			{
				EventType = EventTypes.Other;
				Position += 1;
			}
			else if (Value == 0xE1)
			{
				EventType = EventTypes.Other;
				Position += 3;
			}
			else if (Value == 0xE2)
			{
				EventType = EventTypes.Other;
				Position += 0;
			}
			else if (Value == 0xE3)
			{
				EventType = EventTypes.Other;
				Position += 1;
			}
			else if (Value == 0xE4)
			{
				EventType = EventTypes.Other;
				Position += 2;
			}
			else if (Value == 0xE5)
			{
				EventType = EventTypes.Call;
				Call = BitConverter.ToUInt16(Apu.Memory, Position);
				Repeat = Apu.Memory[Position + 2];
				Position += 3;
			}
			else if (Value == 0xE6)
			{
				EventType = EventTypes.Other;
				Position += 1;
			}
			else if (Value == 0xE7)
			{
				EventType = EventTypes.Other;
				Position += 3;
			}
			else if (Value == 0xE8)
			{
				EventType = EventTypes.Other;
				Position += 3;
			}
			else if (Value == 0xE9)
			{
				EventType = EventTypes.Other;
				Position += 0;
			}
			else if (Value == 0xEA)
			{
				EventType = EventTypes.Other;
				Position += 1;
			}
			else if (Value == 0xEB)
			{
				EventType = EventTypes.Other;
				Position += 3;
			}
			else if (Value == 0xEC)
			{
				EventType = EventTypes.Other;
				Position += 0;
			}
			else if (Value == 0xED)
			{
				EventType = EventTypes.Other;
				Position += 3;
			}
			else if (Value == 0xEE)
			{
				EventType = EventTypes.Other;
				Position += 3;
			}
			else if (Value == 0xEF)
			{
				EventType = EventTypes.Other;
				Position += 3;
			}
			else if (Value == 0xF0)
			{
				EventType = EventTypes.Other;
				Position += 1;
			}
			else if (Value == 0xF1)
			{
				EventType = EventTypes.Other;
				Position += 0;
			}
			else if (Value == 0xF2)
			{
				EventType = EventTypes.Other;
				Position += 0;
			}
			else if (Value == 0xF3)
			{
				EventType = EventTypes.Other;
				Position += 0;
			}
			else if (Value == 0xF4)
			{
				EventType = EventTypes.Other;
				Position += 0;
			}
			else if (Value == 0xF5)
			{
				EventType = EventTypes.Other;
				Position += 1;
			}
			else if (Value == 0xF6)
			{
				EventType = EventTypes.Other;
				Position += 1;
			}
			else if (Value == 0xF7)
			{
				EventType = EventTypes.Other;
				Position += 1;
			}
			else if (Value == 0xF8)
			{
				EventType = EventTypes.Other;
				Position += 1;
			}
			else if (Value == 0xF9)
			{
				EventType = EventTypes.Other;
				Position += 0;
				Position += 24;
			}
			else if (Value == 0xFA)
			{
				EventType = EventTypes.LoadInstruments;
				var length = Apu.Memory[Position++];

				Instruments = new InstrumentRecord[length];

				for (var instrument = 0; instrument < length; instrument++)
				{
					Instruments[instrument].Value1 = Apu.Memory[Position++];
					Instruments[instrument].Value2 = Apu.Memory[Position++];
					Instruments[instrument].Value3 = Apu.Memory[Position++];
					Instruments[instrument].Value4 = Apu.Memory[Position++];
				}

				//Position += length * 4;
			}
			else if (Value == 0xFB)
			{
				EventType = EventTypes.FindInstrument;
				Instrument = Apu.Memory[Position];
				Position += 1;
			}
			else if (Value == 0xFC)
			{
				EventType = EventTypes.Other;
				Position += 2;
			}
			else if (Value == 0xFD)
			{
				EventType = EventTypes.Other;
				Position += 2;
			}
			else
			{
				EventType = EventTypes.Other;

				//Position += RomSongs.EventTypes[Value - RomSongs.FirstEvent].Length;

				System.Diagnostics.Debug.WriteLine("Unknown Event: " + Value.ToString("X2"));
			}
		}

		public enum EventTypes
		{
			Note,
			Tie,
			Rest,
			Length,
			LengthDurationVelocity,
			LengthDuration,
			LengthVelocity,
			Pan,
			PanFade,
			Percussion,
			Tempo,
			TempoFade,
			Call,
			Other,
			Stop,
			Instrument,
			PercussionInstrumentOffset,
			Volume,
			VolumeFade,
			Transpose,
			Tuning,
			PitchSlide,
			MasterVolume,
			MasterVolumeFade,
			TremoloOn,
			TremoloOff,
			Echo,
			VibratoOn,
			VibratoOff,
			GlobalTranspose,
			LoadInstruments,
			FindInstrument,
		}

		public struct InstrumentRecord
		{
			public int Value1;
			public int Value2;
			public int Value3;
			public int Value4;
		}
	}
}
