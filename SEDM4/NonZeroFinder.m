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
BeginPackage["SEDM4`NonZeroFinder`","SEDM4`EDMSuite`","SEDM4`Statistics`","NETLink`"];


(* ::Input::Initialization:: *)
buildGatedChannelTable::usage="buildGatedChannelTable[tofChannelSet_, gateLow_, gateHigh_, switches_] takes a TOF channel set, extracts all possible channels from the list of switches, and gates each channel based on gateLow and gateHigh.";
showSortedTable::usage="showSortedTable[channelTable_, switches_, trimLevel_] takes a channel table and the switches used to generate it, sorts the table by the channel mean divided by the channel error, then displays the channels that have a mean/error larger than a threshold trimLevel."
showDynamicTable::usage="showDynamicTable[channelTable_, switches_] shows a sorted channel table (see showSortedTable) in a Manipulate environment, allowing for changes in the threshold for displaying channels.";


(* ::Input::Initialization:: *)



(* ::Input::Initialization:: *)
Begin["`Private`"];


(* ::Input::Initialization:: *)
stateList=Reverse[Thread[IntegerDigits[#,2,4]==1]&/@Range[0,15]];
channelNames={1->"E",2->"B",3->"RF",4->"MW"};
modeHeads=Join[
StringJoin@@#&/@(stateList/.{True->"T",False->"F"}),
{"None"},
StringJoin@@(Flatten[Table[If[#[[n]]==1,n/.channelNames,""],{n,1,4}]])&/@(IntegerDigits[#,2,4]&/@Range[1,15])
];
specialChannels={"SIG"};


(* ::Input::Initialization:: *)



(* ::Input::Initialization:: *)
chan[n_,switches_]:=Pick[switches,Thread[IntegerDigits[n,2,Length[switches]]==1]]


(* ::Input::Initialization:: *)
getChannels[switches_]:=DeleteCases[Join[{#}&/@{"SIG"},chan[#,switches]&/@Range[1,2^Length[switches]-1]],{"E","B"}]


(* ::Input::Initialization:: *)
getTOFWithError[tofWithErr_]:=Transpose[{tofWithErr@Times,tofWithErr@Data,tofWithErr@Errors}]


(* ::Input::Initialization:: *)
gatedWeightedMean[tofWithErr_,trimLow_,trimHigh_]:=Module[{trimmedTOF},
trimmedTOF=Select[getTOFWithError[tofWithErr],trimLow<=#[[1]]<=trimHigh&];
weightedMean[{#[[2]],#[[3]]}&/@trimmedTOF]
]


(* ::Input::Initialization:: *)
labelChannelTable[chanTab_,switches_]:=MapThread[Join[{#1},#2]&,{getChannels[switches],chanTab}]
sortChannelTable[chanTab_]:=Sort[chanTab,(#1[[4]]>#2[[4]])&]
trimChannelTable[chanTab_,trimLevel_]:=Select[chanTab,#[[4]]>trimLevel&]


(* ::Input::Initialization:: *)
buildGatedChannelTable[tofWithErr_,trimLow_,trimHigh_,switches_]:={#[[1]],#[[2]],Abs[#[[1]]/(#[[2]]+10^-9)]}&/@(gatedWeightedMean[tofWithErr@GetChannel[#],trimLow,trimHigh]&/@getChannels[switches]);


(* ::Input::Initialization:: *)
getRowColor[chanTab_]:=If[#[[4]]>10,Red,(ColorData["TemperatureMap"][#[[4]]/10])]&/@chanTab;


(* ::Input::Initialization:: *)
showSortedTable[chanTab_,switches_,trimLevel_]:=Module[{labelledChanTab,trimmedSortedTab,cols},
labelledChanTab=labelChannelTable[chanTab,switches];
trimmedSortedTab=trimChannelTable[sortChannelTable[labelledChanTab],trimLevel];
cols=getRowColor[trimmedSortedTab];
Grid[
Join[
{{"Chan","Weighted mean","Error","mean/err"}},Map[PaddedForm[#,3]&,trimmedSortedTab,{2}]],
Dividers->All,Background->{None,Join[{LightGray},cols]},ItemStyle->Directive[FontSize->12,FontFamily->"LucidaConsole"]]
]


(* ::Input::Initialization:: *)
showDynamicTable[chanTab_,switches_]:=Manipulate[showSortedTable[chanTab,switches,trimLevel],{{trimLevel,4,"Trim level"},2,5}]


(* ::Input::Initialization:: *)
End[];
EndPackage[];