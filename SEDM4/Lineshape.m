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



BeginPackage["SEDM3`Lineshape`"];


supersonicLineshape::usage="lineshape[vm,temp] gives the normalised lineshape as a function of applied magnetic field in T. The lineshape is calculated for a beam with mean velocity vm and temperature temp. It returns an interpolating function.";


Begin["`Private`"];

param ={ \[HBar] ->  1.0545*10^-34,
\[Mu]-> 9.28483*10^-24,
k-> 1.38066*10^-23,
coherenceLength -> .625 (* m *),
m->  193 *  1.67262*10^-27(*kg*),
RFLength ->  3*10^-2};

supersonicLineshape[vmean_,temperature_]:=Module[{range,supersonicLineshape},
range=500;
mono[v_,B_]:=Cos[b]^4+Cos[(B  \[Mu])/\[HBar] coherenceLength/v]^2 Sin[b]^4;
Off[Reduce::"ratnz"];
pnorm[vm_,temp_]=Integrate[  v^3Exp[-(v-vm)^2/((2 k temp)/m)]/.param,{v,0,\[Infinity]},Assumptions->vm\[Element] Reals && vm>0&&temp\[Element]Reals&&temp>0];
On[Reduce::"ratnz"];
p[v_,vm_,temp_] := v^3Exp[-(v-vm)^2/((2 k temp)/m)]/.param;
unnormedWeightedMono[B_,v_,vm_,temp_]:=p[v,vm,temp] mono[v,B]/.b-> \[Pi]/2 v/vm;
beamIntegral[B_,vm_,temp_] := 1/pnorm[vm,temp]NIntegrate[ unnormedWeightedMono[B,v,vm,temp]/.param ,{v, vm-100,vm+100},MaxRecursion->25];

Off[General::"unfl"];Off[NIntegrate::"inum"];Off[FunctionInterpolation::"ncvb"];Off[NIntegrate::"inumr"];
supersonicLineshape=FunctionInterpolation[beamIntegral[B,vmean,temperature],{B,-range*10^-9,range*10^-9}];
On[General::"unfl"];On[NIntegrate::"inum"];On[FunctionInterpolation::"ncvb"];On[NIntegrate::"inumr"];

supersonicLineshape
];


End[];
EndPackage[];
