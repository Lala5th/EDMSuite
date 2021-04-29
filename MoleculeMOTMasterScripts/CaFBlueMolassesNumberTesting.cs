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
        Parameters["PatternLength"] = 50000;
        Parameters["TCLBlockStart"] = 2000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 8000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["ExpansionTime"] = 650;
        Parameters["MolassesDelay"] = 100;
        Parameters["MolassesHoldTime"] = 200;
        Parameters["MolassesRampDuration"] = 100;
        Parameters["MolassesLowIntensityHoldDuration"] = 900;

        // Camera
        Parameters["Frame0TriggerDuration"] = 10;
        Parameters["WaitBeforeImage"] = 0;

        //
        Parameters["MOTLoadDuration"] = 4000;

        // Slowing
        Parameters["slowingAOMOnStart"] = 240; //started from 250
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1520;//started from 1520
        Parameters["slowingAOMOffDuration"] = 40000;
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1520;//1520
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 380;// 380;
        Parameters["SlowingChirpDuration"] = 1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -1.25; //-1.25

        // Slowing field
        Parameters["slowingCoilsValue"] = 0.42; //1.1;
        Parameters["slowingCoilsOffTime"] = 4000;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 6.0;// 1.195; // 1.202;// 1.219;
        Parameters["yShimLoadCurrent"] = 0.0;// -0.155; //2.4
        Parameters["zShimLoadCurrent"] = 1.5; //0.26

        // v0 Light Intensity
        Parameters["v0IntensityRampDuration"] = 100;
        Parameters["MOTHoldTime"] = 1000;
        Parameters["v0IntensityRampStartValue"] = 5.6;
        Parameters["v0IntensityRampEndValue"] = 7.5;
        Parameters["v0IntensityMolassesValue"] = 5.6;

        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyNewValue"] = 20.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)

        //v0aomCalibrationValues
        Parameters["calibGradient"] = 11.4;


    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int motLoadEndTime = (int)Parameters["MOTLoadDuration"];
        int motSwitchOffTime = motLoadEndTime + (int)Parameters["v0IntensityRampDuration"] + (int)Parameters["MOTHoldTime"];
        int molassesStartTime = motSwitchOffTime + (int)Parameters["MolassesDelay"];
        int molassesRampTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];
        int releaseTime = molassesRampTime + (int)Parameters["MolassesRampDuration"] + (int)Parameters["MolassesLowIntensityHoldDuration"];
        int imagingLightOnTime = releaseTime + 100;
        int cameraTrigger2 = imagingLightOnTime + 300;

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns. 

        
        p.Pulse(patternStartBeforeQ, motSwitchOffTime, (int)Parameters["MolassesDelay"], "v00MOTAOM"); //pulse off the MOT light whilst MOT fields are turning off
        p.Pulse(patternStartBeforeQ, releaseTime, cameraTrigger2 - releaseTime, "v00MOTAOM"); //pulse off the MOT light to release the cloud
        //p.Pulse(patternStartBeforeQ, 4000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        p.Pulse(patternStartBeforeQ, motSwitchOffTime - (int)Parameters["MOTHoldTime"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        p.Pulse(patternStartBeforeQ, cameraTrigger2, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for 2nd frame
        


        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        int motLoadEndTime = (int)Parameters["MOTLoadDuration"];
        int motSwitchOffTime = motLoadEndTime + (int)Parameters["v0IntensityRampDuration"] + (int)Parameters["MOTHoldTime"];
        int molassesStartTime = motSwitchOffTime + (int)Parameters["MolassesDelay"];
        int molassesRampTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];
        int releaseTime = molassesRampTime + (int)Parameters["MolassesRampDuration"] + (int)Parameters["MolassesLowIntensityHoldDuration"];
        int imagingLightOnTime = releaseTime + 100;
        int cameraTrigger2 = imagingLightOnTime + 300;
        
        // Add Analog Channels
        p.AddChannel("v00Intensity");
        p.AddChannel("v00Frequency");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("v00EOMAmp");

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, 1.0);
        p.AddAnalogValue("MOTCoilsCurrent", motSwitchOffTime, -0.05);
        p.AddAnalogValue("MOTCoilsCurrent", cameraTrigger2 - 100, 1.0);
        p.AddAnalogValue("MOTCoilsCurrent", cameraTrigger2 + 1000, 0.0);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", motLoadEndTime, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", molassesStartTime, (double)Parameters["v0IntensityMolassesValue"]);
        p.AddAnalogValue("v00Intensity", releaseTime, (double)Parameters["v0IntensityRampEndValue"]);

        // F=0
        p.AddAnalogValue("v00EOMAmp", 0, 4.85);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, 10.0 - (double)Parameters["v0FrequencyStartValue"] / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", motSwitchOffTime, 10.0 - (double)Parameters["v0FrequencyNewValue"] / (double)Parameters["calibGradient"]);//jump to blue detuning
        p.AddAnalogValue("v00Frequency", releaseTime, 10.0 - (double)Parameters["v0FrequencyStartValue"] / (double)Parameters["calibGradient"]); //jump aom frequency back to normal for imaging
        

        return p;
    }

}
