(* ::Package:: *)

(************************************************************************)
(* This file was generated automatically by the Mathematica front end.  *)
(* It contains Initialization cells from a Notebook file, which         *)
(* typically will have the same name as this file except ending in      *)
(* ".nb" instead of ".m".                                               *)
(*                                                                      *)
(* This file is intended to be loaded into the Mathematica kernel using *)
(* the package loading commands Get or Needs.  Doing so is equivalent   *)
(* to using the Evaluate Initialization Cells menu command in the front *)
(* end.                                                                 *)
(*                                                                      *)
(* DO NOT EDIT THIS FILE.  This entire file is regenerated              *)
(* automatically each time the parent Notebook file is saved in the     *)
(* Mathematica front end.  Any changes you make to this file will be    *)
(* overwritten.                                                         *)
(************************************************************************)



(* ::Input::Initialization:: *)
BeginPackage["SEDM4`Database`","SEDM4`EDMSuite`","NETLink`","JLink`"];


(* ::Input::Initialization:: *)
addFileToDatabase::usage="Adds the given block file to the database. By default it demodulates the block data point-by-point (TOF demodulation). If the gated option is set to true, then the block data is gated prior to demodulation according to the gate configuration. These configurations are specified in SirCachealot and have to be loaded prior to demodulation.";
addFilesToDatabase::usage="Adds multiple block files to the database, with a progress bar.";
addGatedDBlockFromTOFDBlock::usage="Gets a TOF-demodulated block from the database, gates the channels according to a gate configuration, then adds the gated block to the database.";
removeDBlock::usage="Removes the block with the given UID from the database.";
addTagToBlock::usage="Associates a tag with a particular block. This association persists in the database unless explicitly removed (i.e. it doesn't go away when you re-analyse/remove dblocks etc.)";
removeTagFromBlock::usage="Removes a tag from a block.";
getDBlock::usage="Gets a block from the database.";
getGatedChannelAndError::usage="This function gives the mean and error of an analysis channel for a given block and detector. The analysis channel is specified as a list of switches (strings).";
getGatedChannel::usage="";
getGatedError::usage="";
getTOFChannelWithError::usage="";
getTOFChannelTimes::usage=""
getTOFChannelData::usage=""
getTOFChannelErrors::usage=""
selectByCluster::usage="";
selectByTag::usage="";
uidsForTag::usage="";
uidsForAnalysisTag::usage="";
uidsForCluster::usage="";
uidsForGateConfigTag::usage="";
uidsForMachineState::usage="";
machineStateForCluster::usage="";


(* ::Input::Initialization:: *)
analysisProgress=0;


(* ::Input::Initialization:: *)
Begin["`Private`"];


(* ::Input::Initialization:: *)



(* ::Input::Initialization:: *)



(* ::Input::Initialization:: *)
addFileToDatabase[file_,gated_:False,gateConfig_:""]:=If[gated,$sirCachealot@AddBlock[file],$sirCachealot@AddGatedBlock[file,gateConfig]]


(* ::Input::Initialization:: *)
addFilesToDatabase[files_,gated_:False,gateConfig_]:=Module[{},
Do[
CheckAbort[
addFileToDatabase[files[[i]],gated,gateConfig],
Print["Failed to add file: "<>files[[i]]]
];

(* Update the progress dialog *)
SEDM3`Database`analysisProgress = i/Length[files];
,
{i,Length[files]}
]
]


(* ::Input::Initialization:: *)
addGatedDBlockFromTOFDBlock[uid_,gateConfig_]:=Module[{tofDBlock},
NETBlock[
tofDBlock=getDBlock[uid];
CheckAbort[
$sirCachealot@GateTOFDemodulatedBlock[tofDBlock,gateConfig],
Print["Failed to gate demodulated block: "<>uid]
];
]
]


(* ::Input::Initialization:: *)
removeDBlock[uidToRemove_]:=$sirCachealot@DBlockStore@RemoveDBlock[uidToRemove]


(* ::Input::Initialization:: *)
addTagToBlock[cluster_,index_,tagToAdd_]:=$sirCachealot@DBlockStore@AddTagToBlock[cluster,index,tagToAdd]


(* ::Input::Initialization:: *)
removeTagFromBlock[cluster_,index_,tagToRemove_]:=$sirCachealot@DBlockStore@RemoveTagFromBlock[cluster,index,tagToRemove]


(* ::Input::Initialization:: *)
getDBlock[uid_]:=$sirCachealot@DBlockStore@GetDBlock[uid]

getGatedChannelAndError[channel_,detector_,dblock_]:=dblock@GetChannelValueAndError[channel,detector]
getGatedChannel[channel_,detector_,dblock_]:=getChannelAndError[channel,detector,dblock][[1]]
getGatedError[channel_,detector_,dblock_]:=getChannelAndError[channel,detector,dblock][[2]]

getTOFChannelWithError[channel_,detector_,dblock_]:=dblock@GetTOFChannelWithError[channel,detector]
getTOFChannelTimes[channel_,detector_,dblock_]:=getTOFChannelWithError[channel,detector,dblock][[1]]
getTOFChannelData[channel_,detector_,dblock_]:=getTOFChannelWithError[channel,detector,dblock][[2]]
getTOFChannelErrors[channel_,detector_,dblock_]:=getTOFChannelWithError[channel,detector,dblock][[3]]


(* ::Input::Initialization:: *)
selectByCluster[clusterName_]:=Module[{dbs},
dbs=$sirCachealot@DBlockStore@GetDBlock[#]&/@$sirCachealot@DBlockStore@GetUIDsByCluster[clusterName];
Sort[dbs,(#1@TimeStamp@Ticks) < (#2@TimeStamp@Ticks)&]
]


(* ::Input::Initialization:: *)
selectByTag[tag_]:=$sirCachealot@DBlockStore@GetDBlock[#]&/@$sirCachealot@DBlockStore@GetUIDsByTag[tag]


(* ::Input::Initialization:: *)
uidsForTag[tag_]:=$sirCachealot@DBlockStore@GetUIDsByTag[tag]
uidsForTag[tag_,uidsIn_]:=$sirCachealot@DBlockStore@GetUIDsByTag[tag,uidsIn]
uidsForAnalysisTag[tag_]:=$sirCachealot@DBlockStore@GetUIDsByAnalysisTag[tag]
uidsForAnalysisTag[tag_,uidsIn_]:=$sirCachealot@DBlockStore@GetUIDsByAnalysisTag[tag,uidsIn]
uidsForGateConfigTag[tag_]:=$sirCachealot@DBlockStore@GetUIDsByGateTag[tag]
uidsForGateConfigTag[tag_,uidsIn_]:=$sirCachealot@DBlockStore@GetUIDsByGateTag[tag,uidsIn]
uidsForCluster[cluster_]:=$sirCachealot@DBlockStore@GetUIDsByCluster[cluster]
uidsForCluster[cluster_,uidsIn_]:=$sirCachealot@DBlockStore@GetUIDsByCluster[cluster,uidsIn]
uidsForMachineState[eState_,bState_, rfState_,mwState_]:=$sirCachealot@DBlockStore@GetUIDsByMachineState[eState,bState,rfState,mwState];
uidsForMachineState[eState_,bState_,rfState_,mwState_,uidsIn_]:=$sirCachealot@DBlockStore@GetUIDsByMachineState[eState,bState,rfState,mwState,uidsIn]
machineStateForCluster[clusterName_]:=NETBlock[
Module[{uid,dblock},
uid=uidsForCluster[clusterName][[1]];
dblock=getDBlock[uid];
{
dblock@Config@Settings["eState"],
dblock@Config@Settings["bState"],
dblock@Config@Settings["rfState"],
dblock@Config@Settings["mwState"]
}
]]



(* ::Input::Initialization:: *)
End[];
EndPackage[];
