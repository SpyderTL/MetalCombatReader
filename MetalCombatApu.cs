using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace MetalCombatReader
{
	internal class MetalCombatApu
	{
		internal static void Load()
		{
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

			// Write Song Data
			position = MetalCombatGame.SongTableAddress;

			for (var song = 0; song < MetalCombatGame.SongCount; song++)
			{
				var source = Snes.Memory[position++] | (Snes.Memory[position++] << 8) | (Snes.Memory[position++] << 16);
				var length = Snes.Memory[position++] | (Snes.Memory[position++] << 8);
				var destination = Snes.Memory[position++] | (Snes.Memory[position++] << 8);
				var unknown = Snes.Memory[position++];

				if (length > 0)
				{
					var data = new byte[length];

					Array.Copy(Snes.Memory, source + 0xB90000, data, 0, length);

					Console.WriteLine("Apu/Song" + song.ToString("X2"));

					File.WriteAllBytes("Apu/Song" + song.ToString("X2") + " " + destination.ToString("X4") + "-" + (destination + length - 1).ToString("X4") + ".bin", data);

					if (destination == MetalCombatGame.TrackTableAddress || true)
					{
						Array.Copy(data, 0, Apu.Memory, destination, length);

						var branches = new List<int>();

						SongReader.Position = destination;

						using var writer = XmlWriter.Create("Apu/Song" + song.ToString("X2") + ".xml", new XmlWriterSettings { Indent = true, IndentChars = "\t" });
						writer.WriteStartDocument();

						writer.WriteStartElement("song");
						writer.WriteAttributeString("address", SongReader.Position.ToString("X4"));

						while (true)
						{
							var songPosition = SongReader.Position;

							SongReader.Read();

							if (SongReader.EventType == SongReader.EventTypes.Track)
							{
								TrackReader.Position = SongReader.TrackPosition;
								TrackReader.Read();

								writer.WriteStartElement("track");
								writer.WriteAttributeString("address", songPosition.ToString("X4"));
								writer.WriteAttributeString("start", SongReader.TrackPosition.ToString("X4"));

								for (var channel = 0; channel < TrackReader.Channels.Length; channel++)
								{
									ChannelReader.Position = TrackReader.Channels[channel];
									writer.WriteStartElement("channel");
									writer.WriteAttributeString("index", (channel + 1).ToString());
									writer.WriteAttributeString("address", ChannelReader.Position.ToString("X4"));

									var reading = true;

									while (reading)
									{
										var channelPosition = ChannelReader.Position;
										ChannelReader.Read();

										switch (ChannelReader.EventType)
										{
											case ChannelReader.EventTypes.Note:
												writer.WriteStartElement("note");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteString(ChannelReader.Note.ToString("X2"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.Call:
												writer.WriteStartElement("call");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteString(ChannelReader.Call.ToString("X4"));
												writer.WriteEndElement();

												branches.Add(ChannelReader.Call);
												break;

											case ChannelReader.EventTypes.Echo:
												writer.WriteStartElement("echo");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.GlobalTranspose:
												writer.WriteStartElement("globalTranspose");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.Instrument:
												writer.WriteStartElement("instrument");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteString(ChannelReader.Instrument.ToString("X2"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.Length:
												writer.WriteStartElement("length");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteString(ChannelReader.Length.ToString("X2"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.LengthDuration:
												writer.WriteStartElement("parameters");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteAttributeString("length", ChannelReader.Length.ToString("X2"));
												writer.WriteAttributeString("duration", ChannelReader.Duration.ToString("X2"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.LengthVelocity:
												writer.WriteStartElement("parameters");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteAttributeString("length", ChannelReader.Length.ToString("X2"));
												writer.WriteAttributeString("velocity", ChannelReader.Velocity.ToString("X2"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.LengthDurationVelocity:
												writer.WriteStartElement("parameters");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteAttributeString("length", ChannelReader.Length.ToString("X2"));
												writer.WriteAttributeString("duration", ChannelReader.Duration.ToString("X2"));
												writer.WriteAttributeString("velocity", ChannelReader.Velocity.ToString("X2"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.LoadInstruments:
												writer.WriteStartElement("loadInstruments");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteAttributeString("length", ChannelReader.Instruments.Length.ToString("X2"));

												for (var instrument = 0; instrument < ChannelReader.Instruments.Length; instrument++)
												{
													writer.WriteStartElement("instrument");
													writer.WriteAttributeString("index", instrument.ToString("X2"));
													writer.WriteAttributeString("value1", ChannelReader.Instruments[instrument].Value1.ToString("X2"));
													writer.WriteAttributeString("value2", ChannelReader.Instruments[instrument].Value2.ToString("X2"));
													writer.WriteAttributeString("value3", ChannelReader.Instruments[instrument].Value3.ToString("X2"));
													writer.WriteAttributeString("value4", ChannelReader.Instruments[instrument].Value4.ToString("X2"));
													writer.WriteEndElement();
												}

												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.FindInstrument:
												writer.WriteStartElement("findInstrument");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteString(ChannelReader.Instrument.ToString("X2"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.MasterVolume:
												writer.WriteStartElement("masterVolume");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteString(ChannelReader.Volume.ToString("X2"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.MasterVolumeFade:
												writer.WriteStartElement("masterVolumeFade");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.Pan:
												writer.WriteStartElement("pan");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteString(ChannelReader.Pan.ToString("X2"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.PanFade:
												writer.WriteStartElement("panFade");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.Percussion:
												writer.WriteStartElement("percussion");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteString(ChannelReader.Note.ToString("X2"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.PercussionInstrumentOffset:
												writer.WriteStartElement("percussionInstrumentOffset");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteString(ChannelReader.PercussionInstrumentOffset.ToString("X2"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.PitchSlide:
												writer.WriteStartElement("pitchSlide");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteString(ChannelReader.PitchSlide.ToString("X2"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.Rest:
												writer.WriteStartElement("rest");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.Tempo:
												writer.WriteStartElement("tempo");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteString(ChannelReader.Tempo.ToString("X2"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.TempoFade:
												writer.WriteStartElement("tempoFade");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.Tie:
												writer.WriteStartElement("tie");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.Transpose:
												writer.WriteStartElement("transpose");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteString(ChannelReader.Transpose.ToString("X2"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.TremoloOff:
												writer.WriteStartElement("tremoloOff");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.TremoloOn:
												writer.WriteStartElement("tremoloOn");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.Tuning:
												writer.WriteStartElement("tuning");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteString(ChannelReader.Tuning.ToString("X2"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.VibratoOff:
												writer.WriteStartElement("vibratoOff");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.VibratoOn:
												writer.WriteStartElement("vibratoOn");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.Volume:
												writer.WriteStartElement("volume");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteString(ChannelReader.Volume.ToString("X2"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.VolumeFade:
												writer.WriteStartElement("volumeFade");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteEndElement();
												break;

											case ChannelReader.EventTypes.Stop:
												writer.WriteStartElement("stop");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteEndElement();
												reading = false;
												break;

											case ChannelReader.EventTypes.Other:
												writer.WriteStartElement("other");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteString(ChannelReader.Value.ToString("X2"));
												writer.WriteEndElement();
												break;

											default:
												writer.WriteStartElement("unknown");
												writer.WriteAttributeString("address", channelPosition.ToString("X4"));
												writer.WriteEndElement();
												reading = false;
												break;
										}
									}

									writer.WriteEndElement();
								}

								writer.WriteEndElement();
							}
							else if (SongReader.EventType == SongReader.EventTypes.Loop)
							{
								writer.WriteStartElement("loop");
								writer.WriteAttributeString("address", songPosition.ToString("X4"));
								writer.WriteAttributeString("target", SongReader.LoopPosition.ToString("X4"));
								writer.WriteEndElement();
								break;
							}
							else if (SongReader.EventType == SongReader.EventTypes.Repeat)
							{
								writer.WriteStartElement("repeat");
								writer.WriteAttributeString("address", songPosition.ToString("X4"));
								writer.WriteAttributeString("target", SongReader.LoopPosition.ToString("X4"));
								writer.WriteAttributeString("count", SongReader.RepeatCount.ToString());
								writer.WriteEndElement();
							}
							else if (SongReader.EventType == SongReader.EventTypes.Stop)
							{
								writer.WriteStartElement("stop");
								writer.WriteAttributeString("address", songPosition.ToString("X4"));
								writer.WriteEndElement();
								break;
							}
							else
								break;
						}

						// Write Branches
						branches = branches.Distinct().ToList();
						branches.Sort();

						foreach (var branch in branches)
						{
							ChannelReader.Position = branch;

							writer.WriteStartElement("subroutine");
							writer.WriteAttributeString("address", branch.ToString("X4"));

							var reading = true;

							while (reading)
							{
								var channelPosition = ChannelReader.Position;
								ChannelReader.Read();

								switch (ChannelReader.EventType)
								{
									case ChannelReader.EventTypes.Note:
										writer.WriteStartElement("note");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteString(ChannelReader.Note.ToString("X2"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.Call:
										writer.WriteStartElement("call");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteString(ChannelReader.Call.ToString("X4"));
										writer.WriteEndElement();

										//branches.Add(ChannelReader.Call);
										break;

									case ChannelReader.EventTypes.Echo:
										writer.WriteStartElement("echo");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.GlobalTranspose:
										writer.WriteStartElement("globalTranspose");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.Instrument:
										writer.WriteStartElement("instrument");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteString(ChannelReader.Instrument.ToString("X2"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.Length:
										writer.WriteStartElement("length");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteString(ChannelReader.Length.ToString("X2"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.LengthDurationVelocity:
										writer.WriteStartElement("parameters");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteAttributeString("length", ChannelReader.Length.ToString("X2"));
										writer.WriteAttributeString("duration", ChannelReader.Duration.ToString("X2"));
										writer.WriteAttributeString("velocity", ChannelReader.Velocity.ToString("X2"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.LengthDuration:
										writer.WriteStartElement("parameters");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteAttributeString("length", ChannelReader.Length.ToString("X2"));
										writer.WriteAttributeString("duration", ChannelReader.Duration.ToString("X2"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.LengthVelocity:
										writer.WriteStartElement("parameters");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteAttributeString("length", ChannelReader.Length.ToString("X2"));
										writer.WriteAttributeString("velocity", ChannelReader.Velocity.ToString("X2"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.LoadInstruments:
										writer.WriteStartElement("loadInstruments");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteAttributeString("length", ChannelReader.Instruments.Length.ToString("X2"));

										for (var instrument = 0; instrument < ChannelReader.Instruments.Length; instrument++)
										{
											writer.WriteStartElement("instrument");
											writer.WriteAttributeString("index", instrument.ToString("X2"));
											writer.WriteAttributeString("value1", ChannelReader.Instruments[instrument].Value1.ToString("X2"));
											writer.WriteAttributeString("value2", ChannelReader.Instruments[instrument].Value2.ToString("X2"));
											writer.WriteAttributeString("value3", ChannelReader.Instruments[instrument].Value3.ToString("X2"));
											writer.WriteAttributeString("value4", ChannelReader.Instruments[instrument].Value4.ToString("X2"));
											writer.WriteEndElement();
										}

										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.MasterVolume:
										writer.WriteStartElement("masterVolume");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteString(ChannelReader.Volume.ToString("X2"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.MasterVolumeFade:
										writer.WriteStartElement("masterVolumeFade");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.Pan:
										writer.WriteStartElement("pan");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteString(ChannelReader.Pan.ToString("X2"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.PanFade:
										writer.WriteStartElement("panFade");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.Percussion:
										writer.WriteStartElement("percussion");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteString(ChannelReader.Note.ToString("X2"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.PercussionInstrumentOffset:
										writer.WriteStartElement("percussionInstrumentOffset");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteString(ChannelReader.PercussionInstrumentOffset.ToString("X2"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.PitchSlide:
										writer.WriteStartElement("pitchSlide");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteString(ChannelReader.PitchSlide.ToString("X2"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.Rest:
										writer.WriteStartElement("rest");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.Tempo:
										writer.WriteStartElement("tempo");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteString(ChannelReader.Tempo.ToString("X2"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.TempoFade:
										writer.WriteStartElement("tempoFade");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.Tie:
										writer.WriteStartElement("tie");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.Transpose:
										writer.WriteStartElement("transpose");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteString(ChannelReader.Transpose.ToString("X2"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.TremoloOff:
										writer.WriteStartElement("tremoloOff");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.TremoloOn:
										writer.WriteStartElement("tremoloOn");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.Tuning:
										writer.WriteStartElement("tuning");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteString(ChannelReader.Tuning.ToString("X2"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.VibratoOff:
										writer.WriteStartElement("vibratoOff");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.VibratoOn:
										writer.WriteStartElement("vibratoOn");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.Volume:
										writer.WriteStartElement("volume");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteString(ChannelReader.Volume.ToString("X2"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.VolumeFade:
										writer.WriteStartElement("volumeFade");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteEndElement();
										break;

									case ChannelReader.EventTypes.Stop:
										writer.WriteStartElement("stop");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteEndElement();
										reading = false;
										break;

									case ChannelReader.EventTypes.Other:
										writer.WriteStartElement("other");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteString(ChannelReader.Value.ToString("X2"));
										writer.WriteEndElement();
										break;

									default:
										writer.WriteStartElement("unknown");
										writer.WriteAttributeString("address", channelPosition.ToString("X4"));
										writer.WriteEndElement();
										reading = false;
										break;
								}
							}

							writer.WriteEndElement();
						}

						writer.WriteEndDocument();
						writer.Flush();
						writer.Close();
					}
				}
			}
		}
	}
}