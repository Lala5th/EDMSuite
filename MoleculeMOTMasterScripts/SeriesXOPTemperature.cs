﻿using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;
using System.Collections;

using DAQ.Pattern;
using DAQ.Analog;

// This script is supposed to be the basic script for loading a molecule MOT.
// Note that times are all in units of the clock periods of the two pattern generator boards (at present, both are 10us).
// All times are relative to the Q switch, though note that this is not the first event in the pattern.
public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 150000;
        Parameters["TCLBlockStart"] = 2000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 8000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        // Camera
        Parameters["CameraTriggerDuration"] = 10;
        Parameters["CameraExposure"] = 1000;

        // Delays
        Parameters["MolassesDelay"] = 100;
        Parameters["WaitBeforeImage"] = 300;
        Parameters["OPDelay"] = 100;
        Parameters["MQTStartDelay"] = 0;
        Parameters["ExpansionTime"] = 800;
        Parameters["v00ShutterOpeningDelay"] = 1950; // 840
        Parameters["v00ShutterClosingDelay"] = 900; // 0
        Parameters["bXShutterOpeningDelay"] = 1260;
        Parameters["bXShutterClosingDelay"] = 1600;
        Parameters["OPShutterOpeningDelay"] = 1240;
        Parameters["OPShutterClosingDelay"] = 1700;

        // Slowing
        Parameters["slowingAOMOnStart"] = 240; //started from 250
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1520;//started from 1520
        Parameters["slowingAOMOffDuration"] = (int)Parameters["PatternLength"] - 10000;
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1520;//1520
        Parameters["slowingRepumpAOMOffDuration"] = (int)Parameters["PatternLength"] - 10000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 380;// 380;
        Parameters["SlowingChirpDuration"] = 1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -1.25; //-1.25

        // Slowing field
        Parameters["slowingCoilsValue"] = 0.42; //1.1;
        Parameters["slowingCoilsOffTime"] = 4000;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -1.35;// -1.35 is zero
        Parameters["yShimLoadCurrent"] = -1.92;// -1.92 is zero
        Parameters["zShimLoadCurrent"] = -0.22;// -0.22 is zero
        // shim fields during OP
        Parameters["xShimOPCurrent"] = -1.35;// -1.35 is zero
        Parameters["yShimOPCurrent"] = 5.0;// -1.92 is zero
        Parameters["zShimOPCurrent"] = -0.22;// -0.22 is zero

        // v0 Light intensity
        Parameters["v0IntensityRampStartValue"] = 5.6;
        Parameters["v0IntensityRampEndValue"] = 8.0;
        Parameters["v0IntensityCMOTValue"] = 8.0;
        Parameters["v0IntensityMolassesValue"] = 5.6;

        // v0 light durations
        Parameters["MOTLoadDuration"] = 5000;
        Parameters["v0IntensityRampDuration"] = 100;
        Parameters["MOTHoldDuration"] = 1000;
        Parameters["CMOTRampDuration"] = 800;
        Parameters["CMOTHoldDuration"] = 100;
        Parameters["MolassesHoldDuration"] = 250;

        // v0 Light Frequency (0.0 for 114.1MHz)
        Parameters["v0FrequencyMOTValue"] = 0.0;
        Parameters["v0FrequencyCMOTValue"] = 3.5;
        Parameters["v0FrequencyMolassesValue"] = 18.0;

        // B field
        Parameters["MOTFieldValue"] = 1.0;
        Parameters["CMOTFieldValue"] = 2.0;
        Parameters["MQTFieldValue"] = 1.2;
        Parameters["MQTHoldDuration"] = 100000;

        // OP
        Parameters["OPDuration"] = 300;

        //v0aomCalibrationValues
        Parameters["calibGradient"] = 11.4;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int motLoadEndTime = patternStartBeforeQ + (int)Parameters["MOTLoadDuration"];
        int motRampEndTime = motLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        int cmotRampStartTime = motRampEndTime + (int)Parameters["MOTHoldDuration"];
        int cmotRampEndTime = cmotRampStartTime + (int)Parameters["CMOTRampDuration"];
        int cmotEndTime = cmotRampEndTime + (int)Parameters["CMOTHoldDuration"];
        int molassesStartTime = cmotEndTime + (int)Parameters["MolassesDelay"];
        int molassesEndTime = molassesStartTime + (int)Parameters["MolassesHoldDuration"];
        int opStartTime = molassesEndTime + (int)Parameters["OPDelay"];
        int opEndTime = opStartTime + (int)Parameters["OPDuration"];
        int mqtStartTime = opEndTime + (int)Parameters["MQTStartDelay"];
        int releaseTime = mqtStartTime + (int)Parameters["MQTHoldDuration"];
        int imagingLightOnTime = releaseTime + (int)Parameters["ExpansionTime"];
        int imageTime = imagingLightOnTime + (int)Parameters["WaitBeforeImage"];
        int endOfTime = imageTime + 3000;

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);
        //V00 AOM switch:
        p.Pulse(0, cmotEndTime, (int)Parameters["MolassesDelay"], "v00MOTAOM");
        p.Pulse(0, molassesEndTime, imagingLightOnTime - molassesEndTime, "v00MOTAOM");
        //Camera triggers:
        p.Pulse(0, imageTime, (int)Parameters["CameraTriggerDuration"], "cameraTrigger");

        // Optical Pumping AOM
        p.AddEdge("dipoleTrapAOM", 0, true);
        p.AddEdge("dipoleTrapAOM", opStartTime, false);
        p.AddEdge("dipoleTrapAOM", opEndTime, true);
        // bX shutter
        p.Pulse(0, motLoadEndTime - (int)Parameters["bXShutterClosingDelay"],
                   endOfTime, "bXSlowingShutter");
        // v00 shutter
        p.Pulse(0, molassesEndTime - (int)Parameters["v00ShutterClosingDelay"],
                   imagingLightOnTime - molassesEndTime - (int)Parameters["v00ShutterOpeningDelay"], "v00MOTShutter");
        // Optical Pumping shutter
        p.AddEdge("rb2DMOTShutter", 0, false);
        p.AddEdge("rb2DMOTShutter", opEndTime - (int)Parameters["OPShutterClosingDelay"], true);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        int motLoadEndTime = (int)Parameters["MOTLoadDuration"];
        int motRampEndTime = motLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        int cmotRampStartTime = motRampEndTime + (int)Parameters["MOTHoldDuration"];
        int cmotRampEndTime = cmotRampStartTime + (int)Parameters["CMOTRampDuration"];
        int cmotEndTime = cmotRampEndTime + (int)Parameters["CMOTHoldDuration"];
        int molassesStartTime = cmotEndTime + (int)Parameters["MolassesDelay"];
        int molassesEndTime = molassesStartTime + (int)Parameters["MolassesHoldDuration"];
        int opStartTime = molassesEndTime + (int)Parameters["OPDelay"];
        int opEndTime = opStartTime + (int)Parameters["OPDuration"];
        int mqtStartTime = opEndTime + (int)Parameters["MQTStartDelay"];
        int releaseTime = mqtStartTime + (int)Parameters["MQTHoldDuration"];
        int imagingLightOnTime = releaseTime + (int)Parameters["ExpansionTime"];
        int imageTime = imagingLightOnTime + (int)Parameters["WaitBeforeImage"];
        int endOfTime = imageTime + 3000;

        // Add Analog Channels
        p.AddChannel("v00Intensity");
        p.AddChannel("v00Frequency");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("v00EOMAmp");

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", motRampEndTime, 0.0);
        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTFieldValue"]);
        p.AddLinearRamp("MOTCoilsCurrent", cmotRampStartTime, (int)Parameters["CMOTRampDuration"], (double)Parameters["CMOTFieldValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", cmotRampEndTime, -0.05);
        p.AddAnalogValue("MOTCoilsCurrent", mqtStartTime, (double)Parameters["MQTFieldValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", releaseTime, -0.05);
        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);
        //Shim fields for OP
        p.AddAnalogValue("xShimCoilCurrent", molassesEndTime, (double)Parameters["xShimOPCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", molassesEndTime, (double)Parameters["yShimOPCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", molassesEndTime, (double)Parameters["zShimOPCurrent"]);
        //Shim fields for MQT
        p.AddAnalogValue("xShimCoilCurrent", mqtStartTime, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", mqtStartTime, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", mqtStartTime, (double)Parameters["zShimLoadCurrent"]);
        // v0 Intensity
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", motLoadEndTime, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", cmotRampStartTime, (double)Parameters["v0IntensityCMOTValue"]);
        p.AddAnalogValue("v00Intensity", cmotEndTime, (double)Parameters["v0IntensityMolassesValue"]);
        p.AddAnalogValue("v00Intensity", releaseTime, (double)Parameters["v0IntensityRampStartValue"]);
        // v0 Frequency
        p.AddAnalogValue("v00Frequency", 0, 10.0 - (double)Parameters["v0FrequencyMOTValue"] / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", cmotRampStartTime, 10.0 - (double)Parameters["v0FrequencyCMOTValue"] / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", cmotEndTime, 10.0 - (double)Parameters["v0FrequencyMolassesValue"] / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", releaseTime, 10.0 - (double)Parameters["v0FrequencyMOTValue"] / (double)Parameters["calibGradient"]);
        // v0 frequency with EOM
        p.AddAnalogValue("v00EOMAmp", 0, 4.85);


        return p;
    }

}
