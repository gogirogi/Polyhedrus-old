﻿using AudioLib.Modules;
using Polyhedrus.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Polyhedrus.Modules
{
	public class Modulator
	{
		double _fsInv;
		double _samplerate;
		public double Samplerate
		{
			get { return _samplerate; }
			set
			{
				_samplerate = value;
				_fsInv = 1.0 / _samplerate;
				Envelope.Samplerate = value;
				Lfo.Samplerate = value;
			}
		}

		public double Output;

		Ahdsr Envelope;
		LFO Lfo;

		private bool FreePhase;
		private double OffsetValue;
		private bool Gate;

		private double DelaySamples;
		private double DelayAccumulator;		

		public Modulator(double samplerate)
		{
			Envelope = new Ahdsr(samplerate);
			Lfo = new LFO(samplerate);
		}

		public void SetParameter(ModulatorParams parameter, object value)
		{
			double val = Convert.ToDouble(value);

			switch(parameter)
			{
				case ModulatorParams.Attack:
					Envelope.SetParameter(Ahdsr.StageAttack, val);
					break;
				case ModulatorParams.Hold:
					Envelope.SetParameter(Ahdsr.StageHold, val);
					break;
				case ModulatorParams.Decay:
					Envelope.SetParameter(Ahdsr.StageDecay, val);
					break;
				case ModulatorParams.Sustain:
					Envelope.SetParameter(Ahdsr.StageSustain, val);
					break;
				case ModulatorParams.Release:
					Envelope.SetParameter(Ahdsr.StageRelease, val);
					break;
				case ModulatorParams.Delay:
					DelaySamples = val * 1000 * _fsInv;
					break;
				case ModulatorParams.Frequency:
					Lfo.FreqHz = val;
					break;
				case ModulatorParams.FreePhase:
					FreePhase = (bool)value;
					break;
				case ModulatorParams.Offset:
					OffsetValue = val;
					break;
				case ModulatorParams.Shape:
					Lfo.Shape = val;
					break;
				case ModulatorParams.Phase:
					Lfo.StartPhase = val;
					break;
				case ModulatorParams.TempoSync:
					Lfo.TempoSync = (bool)value;
					break;
				case ModulatorParams.Wave:
					Lfo.SelectedWave = (LFO.Wave)(value);
					break;
			}
		}

		public void UpdateStepsize()
		{
			Lfo.UpdateStepsize();
		}

		public void Reset()
		{
			Envelope.Reset();
			if (!FreePhase)
				Lfo.Reset();
		}

		public void SetGate(bool state)
		{
			if(state && !Gate) // turning on
			{
				Gate = true;
				DelayAccumulator = 0;
			}
			else if (Gate && !state) // turning off
			{
				Gate = false;
				Envelope.Gate = false;
			}
		}

		public double Process(int sampleCount)
		{
			if(DelayAccumulator > DelaySamples)
				Envelope.Gate = Gate;

			DelayAccumulator += sampleCount;

			var lfo = (Lfo.SelectedWave == LFO.Wave.None) ? 1.0 : Lfo.Process(sampleCount);
			var env = Envelope.Process(sampleCount);

			Output = (lfo + OffsetValue) * env;
			return Output;
		}
	}
}
