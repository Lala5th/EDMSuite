﻿using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

// This script is used to measure how the MOT loading is effected by the presence of the absorption beam. First it dumps the MOT, then loads it and takes two images,
// one of the MOT and one background.

public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 170000;
        Parameters["MOTStartTime"] = 1000;
        Parameters["MOTCoilsCurrent"] = 10.0;
        Parameters["NumberOfFrames"] = 2;
        Parameters["Frame0TriggerDuration"] = 100;
        Parameters["Frame0Trigger"] = 110000;
        Parameters["Frame1TriggerDuration"] = 100;
        Parameters["Frame1Trigger"] = 125000;
        Parameters["aom2Detuning"] = 200.875;
        Parameters["aom3Detuning"] = 200.875;
        Parameters["TSAcceleration"] = 10.0;
        Parameters["TSDeceleration"] = 10.0;
        Parameters["TSDistance"] = 0.0;
        Parameters["TSVelocity"] = 10.0;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);  // This is how you load "preset" patterns.

        p.Pulse(0, 0, 1, "AnalogPatternTrigger");  //NEVER CHANGE THIS!!!! IT TRIGGERS THE ANALOG PATTERN!

        p.AddEdge("CameraTrigger", 0, true);
        p.DownPulse((int)Parameters["Frame0Trigger"], 0, (int)Parameters["Frame0TriggerDuration"], "CameraTrigger");
        p.DownPulse((int)Parameters["Frame1Trigger"], 0, (int)Parameters["Frame1TriggerDuration"], "CameraTrigger");

        p.DownPulse(150000, 0, 50, "CameraTrigger");
        p.DownPulse(160000, 0, 50, "CameraTrigger");

        //switches off Zeeman and Absoroption beams during imaging, so that MOT is not reloaded and fluorescence images can be taken
        p.AddEdge("aom2enable", (int)Parameters["Frame0Trigger"], false);
        p.AddEdge("aom3enable", (int)Parameters["Frame0Trigger"], false);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);

        p.AddChannel("aom2frequency");
        p.AddChannel("aom3frequency");

        p.AddAnalogValue("coil0current", 0, 0);
        p.AddAnalogValue("aom2frequency", 0, (double)Parameters["aom2Detuning"]);
        p.AddAnalogValue("aom3frequency", 0, (double)Parameters["aom3Detuning"]);
        p.AddAnalogValue("coil0current", 120000, 0);

        p.SwitchAllOffAtEndOfPattern();
        return p;
    }

}
