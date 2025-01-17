using System;
using System.Collections;

using ScanMaster.Acquire.Plugins;

namespace ScanMaster.Acquire.Plugin
{
	/// <summary>
	/// The plugin registry keeps track of the installed plugins. It's rather simple
	/// - it just keeps track of a table of plugins for each plugin type. It has some
	/// helper methods to get a plugin instance. When writing new plugins, they must
	/// be added to the constructor of this class by hand.
	/// </summary>
	public class PluginRegistry
	{
		private static PluginRegistry registry;

		private Hashtable scanOutputPlugins = new Hashtable();
		private Hashtable switchOutputPlugins = new Hashtable();
		private Hashtable patternPlugins = new Hashtable();
		private Hashtable yagPlugins = new Hashtable();
		private Hashtable shotGathererPlugins = new Hashtable();
		private Hashtable analogInputPlugins = new Hashtable();
        private Hashtable gpibInputPlugins = new Hashtable();
		private Hashtable wmlOutputPlugins = new Hashtable();

		private PluginRegistry()
		{
			// scanOutputPlugins
			scanOutputPlugins.Add("No scan", typeof(NullOutputPlugin));
			scanOutputPlugins.Add("Analog output", typeof(DAQMxAnalogOutputPlugin));
			scanOutputPlugins.Add("Synth frequency output", typeof(SynthFrequencyOutputPlugin));
			scanOutputPlugins.Add("Windfriek synth frequency output", typeof(WindfriekSynthFrequencyOutputPlugin));
			scanOutputPlugins.Add("Synth amplitude output", typeof(SynthAmplitudeOutputPlugin));
			scanOutputPlugins.Add("PG parameter scan", typeof(PGOutputPlugin));
            scanOutputPlugins.Add("TCL scan", typeof(TCLOutputPlugin));
			scanOutputPlugins.Add("WML scan", typeof(WMLOutputPlugin));
#if DECELERATOR
            scanOutputPlugins.Add("Deceleration hardware analog output", typeof(DecelerationHardwareAnalogOutputPlugin));
            patternPlugins.Add("MOTMaster", typeof(MMPatternPlugin));
#endif

#if EDM
            scanOutputPlugins.Add("NI Rfsg frequency output", typeof(NIRfsgFrequencyOutputPlugin));
            scanOutputPlugins.Add("NI Rfsg amplitude output", typeof(NIRfsgAmplitudeOutputPlugin));
            scanOutputPlugins.Add("EDM hardware control output", typeof(HardwareControllerOutputPlugin));
#endif
			// switchOutputPlugins
			switchOutputPlugins.Add("No switch", typeof(NullSwitchPlugin));
            switchOutputPlugins.Add("TTL switch", typeof(TTLSwitchPlugin));
			// patternPlugins
			patternPlugins.Add("No pattern", typeof(NullPGPlugin));
			patternPlugins.Add("Pulsed rf scan", typeof(PulsedRFScanPatternPlugin));
			patternPlugins.Add("Common raman", typeof(CommonRamanPatternPlugin));
			patternPlugins.Add("Pump-probe", typeof(PumpProbePatternPlugin));
            patternPlugins.Add("Pump-probe Dual CCD", typeof(DualCCDPatternPlugin));
			patternPlugins.Add("Leak Test, Dual CCD", typeof(LeakTestPatternPlugin));
			patternPlugins.Add("Leak Test Modified, Dual CCD", typeof(LeakTestPatternPluginModified));
			patternPlugins.Add("Leak Test with Dye laser, Dual CCD", typeof(LeakTestWithDyePatternPlugin));
			patternPlugins.Add("Pulsed rf scan with super pumping", typeof(SuperPumpingPulsedRFScanPatternPlugin));
            patternPlugins.Add("Super pumping pattern without pulsed rf", typeof(SuperPumpingPatternPlugin));
			patternPlugins.Add("Deceleration", typeof(DecelerationPatternPlugin));
			patternPlugins.Add("Guide", typeof(GuidePatternPlugin));
            patternPlugins.Add("Dual ablation", typeof(DualAblationPatternPlugin));
            patternPlugins.Add("Dual valve", typeof(DualValvePatternPlugin));
            patternPlugins.Add("Basic beam", typeof(BasicBeamPatternPlugin));
            patternPlugins.Add("Modulated aom", typeof(AomModulatedPatternPlugin));
            patternPlugins.Add("Level-controlled aom", typeof(AomLevelControlPatternPlugin));
            patternPlugins.Add("Imaging", typeof(ImagingPatternPlugin));
            patternPlugins.Add("MOT", typeof(MOTPatternPlugin));
            patternPlugins.Add("Flashlamps only", typeof(FlashlampsOnlyPatternPlugin));
            patternPlugins.Add("Zeeman Sisyphus", typeof(ZeemanSisyphusPatternPlugin));
			patternPlugins.Add("N shots", typeof(NshotsPatternPlugin));
			// yagPlugins
			yagPlugins.Add("No YAG", typeof(NullYAGPlugin));
			yagPlugins.Add("YAG on", typeof(DefaultYAGPlugin));
			yagPlugins.Add("Not-so-Brilliant YAG", typeof(NotInTheLeastBitBrilliantYAGPlugin));
            yagPlugins.Add("Quanta-Ray", typeof(QuantaRayYAGPlugin));
			// shotGathererPlugins
			shotGathererPlugins.Add("Constant, fake data", typeof(NullShotGathererPlugin));
			shotGathererPlugins.Add("Analog gatherer", typeof(AnalogShotGathererPlugin));
			shotGathererPlugins.Add("Modulated Analog gatherer", typeof(ModulatedAnalogShotGathererPlugin));
			shotGathererPlugins.Add("Buffered event counting gatherer", typeof(BufferedEventCountingShotGathererPlugin));
            shotGathererPlugins.Add("Image grabbing analog gatherer", typeof(ImageGrabbingAnalogShotGathererPlugin));
			// analog input plugins
			analogInputPlugins.Add("No analog input", typeof(NullAnalogInputPlugin));
			analogInputPlugins.Add("Analog input", typeof(DAQMxAnalogInputPlugin));
			analogInputPlugins.Add("Wavemeter input", typeof(WavemeterInputPlugin));
            //GPIB Input plugins
            gpibInputPlugins.Add("Single Counter input", typeof(SingleCounterInputPlugin));
            gpibInputPlugins.Add("No GPIB input", typeof(NullGPIBInputPlugin));
            gpibInputPlugins.Add("GPIB input", typeof(GPIBInputPlugin));
            

#if DECELERATOR
            analogInputPlugins.Add("Deceleration hardware analog input", typeof(DecelerationHardwareAnalogInputPlugin));
#endif
        }

		public static PluginRegistry GetRegistry()
		{
			if (registry == null) registry = new PluginRegistry();
			return registry;
		}

		public ScanOutputPlugin GetOutputPlugin(String type)
		{
			return (ScanOutputPlugin)InstantiatePlugin(scanOutputPlugins, type);
		}

		public String[] GetOutputPlugins()
		{
			return GetPluginNameList(scanOutputPlugins);
		}

		public SwitchOutputPlugin GetSwitchPlugin(String type)
		{
			return (SwitchOutputPlugin)InstantiatePlugin(switchOutputPlugins, type);
		}

		public String[] GetSwitchPlugins()
		{
			return GetPluginNameList(switchOutputPlugins);
		}

		public PatternPlugin GetPatternPlugin(String type)
		{
			return (PatternPlugin)InstantiatePlugin(patternPlugins, type);
		}

		public String[] GetPatternPlugins()
		{
			return GetPluginNameList(patternPlugins);
		}

		public YAGPlugin GetYAGPlugin(String type)
		{
			return (YAGPlugin)InstantiatePlugin(yagPlugins, type);
		}

		public String[] GetYAGPlugins()
		{
			return GetPluginNameList(yagPlugins);
		}

		public ShotGathererPlugin GetShotGathererPlugin(String type)
		{
			return (ShotGathererPlugin)InstantiatePlugin(shotGathererPlugins, type);
		}

		public String[] GetShotGathererPlugins()
		{
			return GetPluginNameList(shotGathererPlugins);
		}


		public AnalogInputPlugin GetAnalogPlugin(String type)
		{
			return (AnalogInputPlugin)InstantiatePlugin(analogInputPlugins, type);
		}


		
		public String[] GetAnalogPlugins()
		{
			return GetPluginNameList(analogInputPlugins);
		}

        public GPIBInputPlugin GetGPIBPlugin(String type)
        {
            return (GPIBInputPlugin)InstantiatePlugin(gpibInputPlugins, type);
        }

        public String[] GetGPIBPlugins()
        {
            return GetPluginNameList(gpibInputPlugins);
        }

		public WMLOutputPlugin GetWMLPlugins(string type)
        {
			return (WMLOutputPlugin)InstantiatePlugin(wmlOutputPlugins, type);
        }

		private object InstantiatePlugin(Hashtable plugins, String type)
		{
			Type pluginType = (Type)plugins[type];
			System.Reflection.ConstructorInfo info = pluginType.GetConstructor(new Type[] {});
			return info.Invoke(new object[] {});
		}

		private String[] GetPluginNameList(Hashtable plugins)
		{
			String[] keys = new String[plugins.Count];
			plugins.Keys.CopyTo(keys,0);
			return keys;
		}
	}
}
