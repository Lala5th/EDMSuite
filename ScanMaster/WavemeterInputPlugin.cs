using System;
using System.Collections;
using System.Xml.Serialization;
using System.Net;
using System.Net.Sockets;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.HAL;

using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin to capture wavemeter reading from WavemeterLock. Returns measured frequency-offset in GHz.
	/// </summary>
	[Serializable]
	public class WavemeterInputPlugin : AnalogInputPlugin
	{
		[NonSerialized]
		private double latestData;
		[NonSerialized]
		private WavemeterLock.Controller wavemeterContrller;
		[NonSerialized]
		private string serverComputerName;
		[NonSerialized]
		private string ipAddr;

		private string hostName = (String)System.Environment.GetEnvironmentVariables()["COMPUTERNAME"];

		protected override void InitialiseSettings()
		{
			settings["laser"] =  "Laser";
			settings["computer"] = hostName;
			settings["offset"] = 0.0;//Frequency offset in THz
		}

		public override void AcquisitionStarting()
		{
            if (!Environs.Debug)
            {
				serverComputerName = (string)settings["computer"];

				foreach (var addr in Dns.GetHostEntry(serverComputerName).AddressList)
				{
					if (addr.AddressFamily == AddressFamily.InterNetwork)
						ipAddr = addr.ToString();
				}

				EnvironsHelper eHelper = new EnvironsHelper(serverComputerName);

				wavemeterContrller = (WavemeterLock.Controller)(Activator.GetObject(typeof(WavemeterLock.Controller), "tcp://" + ipAddr + ":" + eHelper.wavemeterLockTCPChannel + "/controller.rem"));
			}
			
		}

		public override void ScanStarting()
		{
		}

		public override void ScanFinished()
		{
		}

		public override void AcquisitionFinished()
		{
		}
		
		public override void ArmAndWait()
		{
			lock (this)
			{
				if (!Environs.Debug)
				{
					latestData = 1000*(wavemeterContrller.getSlaveFrequency((string)settings["laser"]) - (double)settings["offset"]);
				}
			}
		}

		[XmlIgnore]
		public override ArrayList Analogs
		{
			get 
			{
				lock(this)
				{
					ArrayList a = new ArrayList();
					if (!Environs.Debug) a.Add(latestData);
					else a.Add(new Random().NextDouble());
					return a;
				}
			}
		}
	}
}
