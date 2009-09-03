using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;

using Analysis.EDM;
using EDMConfig;

namespace EDMBlockHead
{
    public partial class LiveViewer : Form
    {
        private Controller controller;

        int blockCount = 1;
        double clusterVariance = 0;
        double clusterVarianceNormed = 0;
        double blocksPerDay = 240;
 

        public LiveViewer(Controller c)
        {
            InitializeComponent();
            controller = c;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateStatusText("EDMErr\t" + "normedErr\t" + "B\t" + "DB\t" + "DB/SIG" + "\t" + Environment.NewLine);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        //public void Clear()
        //{
        //}

        public void AddDBlock(DemodulatedBlock dblock)
        {
            QuickEDMAnalysis analysis = QuickEDMAnalysis.AnalyseDBlock(dblock);

            //Append LiveViewer text with edm errors, B, DB & DB/SIG
            AppendStatusText(
                (Math.Pow(10, 26) * analysis.RawEDMErr).ToString("G3")
                + "\t" + (Math.Pow(10, 26) * analysis.RawEDMErrNormed).ToString("G3")
                + "\t\t" + (analysis.BValAndErr[0]).ToString("N2")
                + "\t" + (analysis.DBValAndErr[0]).ToString("N2")
                + "\t" + (analysis.DBValAndErr[0] / analysis.SIGValAndErr[0]).ToString("N3")
                + Environment.NewLine);

            // Rollings values of edm error
            clusterVariance = 
                ((clusterVariance * (blockCount - 1)) + analysis.RawEDMErr * analysis.RawEDMErr) / blockCount;
            double edmPerDay = Math.Sqrt(clusterVariance / blocksPerDay);
            clusterVarianceNormed =
                ((clusterVarianceNormed * (blockCount - 1)) 
                + analysis.RawEDMErrNormed * analysis.RawEDMErrNormed) / blockCount;
            double edmPerDayNormed = Math.Sqrt(clusterVarianceNormed / blocksPerDay);

            UpdateClusterStatusText(
                "errorPerDay: " + edmPerDay.ToString("E3") 
                + "\terrorPerDayNormed: " + edmPerDayNormed.ToString("E3")
                + Environment.NewLine + "block count: " + blockCount);

            //Update Plots
            AppendToSigScatter(new double[] { blockCount }, new double[] { analysis.SIGValAndErr[0] });
            AppendToBScatter(new double[] { blockCount }, new double[] { analysis.BValAndErr[0] });
            AppendToDBScatter(new double[] { blockCount }, new double[] { analysis.DBValAndErr[0] });
            AppendToEDMScatter(new double[] { blockCount }, 
                new double[] { Math.Pow(10, 26) * analysis.RawEDMErr });
            AppendToEDMNormedScatter(new double[] { blockCount },
                new double[] { Math.Pow(10, 26) * analysis.RawEDMErrNormed });
            AppendSigmaToSIGScatter(new double[] { blockCount },
                new double[] { analysis.SIGValAndErr[0] + analysis.SIGValAndErr[1] },
                new double[] { analysis.SIGValAndErr[0] - analysis.SIGValAndErr[1] });
            AppendSigmaToBScatter(new double[] { blockCount },
                new double[] { analysis.BValAndErr[0] + analysis.BValAndErr[1] },
                new double[] { analysis.BValAndErr[0] - analysis.BValAndErr[1] });
            AppendSigmaToDBScatter(new double[] { blockCount },
                new double[] { analysis.DBValAndErr[0] + analysis.DBValAndErr[1] },
                new double[] { analysis.DBValAndErr[0] - analysis.DBValAndErr[1] });
            AppendToNorthLeakageScatter(new double[] { blockCount },
                new double[] { analysis.NorthCurrentValAndError[0] });
            AppendToSouthLeakageScatter(new double[] { blockCount },
                new double[] { analysis.SouthCurrentValAndError[0] });
            AppendToNorthLeakageErrorScatter(new double[] { blockCount },
                new double[] { analysis.NorthCurrentValAndError[1] });
            AppendToSouthLeakageErrorScatter(new double[] { blockCount },
                new double[] { analysis.SouthCurrentValAndError[1] });

            blockCount = blockCount + 1;
        }

        private void resetEdmErrRunningMeans()
        {
            blockCount = 1;
            clusterVariance = 0;
            clusterVarianceNormed = 0;
            UpdateClusterStatusText("errorPerDay: " + 0 + "\terrorPerDayNormed: " + 0
                + Environment.NewLine + "block count: " + 0);
            UpdateStatusText("EDMErr\t" + "normedErr\t" + "B\t" + "DB\t" + "DB/SIG" + "\t" + Environment.NewLine);
            ClearSIGScatter();
            ClearBScatter();
            ClearDBScatter();
            ClearEDMErrScatter();
            ClearLeakageScatters();
        }

        #region UI methods

        private void UpdateStatusText(string newText)
        {
            SetTextBox(statusText, newText);
        }

        private void AppendStatusText(string newText)
        {
            SetTextBox(statusText, statusText.Text + newText);
        }

        private void UpdateClusterStatusText(string newText)
        {
            SetTextBox(clusterStatusText, newText);
        }

        private void AppendToSigScatter(double[] x, double[] y)
        {
            PlotXYAppend(sigScatterGraph, sigPlot, x, y);
        }

        private void AppendToBScatter(double[] x, double[] y)
        {
            PlotXYAppend(bScatterGraph, bPlot, x, y);
        }

        private void AppendToDBScatter(double[] x, double[] y)
        {
            PlotXYAppend(dbScatterGraph, dbPlot, x, y);
        }

        private void AppendToNorthLeakageScatter(double[] x, double[] y)
        {
            PlotXYAppend(leakageGraph, northLeakagePlot, x, y);
        }

        private void AppendToSouthLeakageScatter(double[] x, double[] y)
        {
            PlotXYAppend(leakageGraph, southLeakagePlot, x, y);
        }
 
        private void AppendToEDMScatter(double[] x, double[] y)
        {
            PlotXYAppend(edmErrorScatterGraph, edmErrorPlot, x, y);
        }

        private void AppendToEDMNormedScatter(double[] x, double[] y)
        {
            PlotXYAppend(edmErrorScatterGraph, edmNormedErrorPlot, x, y);
        }

        private void AppendSigmaToSIGScatter(double[] x, double[] yPlusSigma, double[] yMinusSigma)
        {
            PlotXYAppend(sigScatterGraph, sigSigmaHi, x, yPlusSigma);
            PlotXYAppend(sigScatterGraph, sigSigmaLo, x, yMinusSigma);
        }

        private void AppendSigmaToBScatter(double[] x, double[] yPlusSigma, double[] yMinusSigma)
        {
            PlotXYAppend(bScatterGraph, bSigmaHi, x, yPlusSigma);
            PlotXYAppend(bScatterGraph, bSigmaLo, x, yMinusSigma);
        }

        private void AppendSigmaToDBScatter(double[] x, double[] yPlusSigma, double[] yMinusSigma)
        {
            PlotXYAppend(dbScatterGraph, dbSigmaHi, x, yPlusSigma);
            PlotXYAppend(dbScatterGraph, dbSigmaLo, x, yMinusSigma);
        }

        private void AppendToNorthLeakageErrorScatter(double[] x, double[] y)
        {
            PlotXYAppend(leakageErrorGraph, northLeakageVariancePlot, x, y);
        }

        private void AppendToSouthLeakageErrorScatter(double[] x, double[] y)
        {
            PlotXYAppend(leakageErrorGraph, southLeakageVariancePlot, x, y);
        }

        private void ClearSIGScatter()
        {
            ClearNIGraph(sigScatterGraph);
        }

        private void ClearBScatter()
        {
            ClearNIGraph(bScatterGraph);
        }

        private void ClearDBScatter()
        {
            ClearNIGraph(dbScatterGraph);
        }

        private void ClearEDMErrScatter()
        {
            ClearNIGraph(edmErrorScatterGraph);
        }

        private void ClearLeakageScatters()
        {
            ClearNIGraph(leakageGraph);
            ClearNIGraph(leakageErrorGraph);
        }

        #endregion


        #region Click Handler

        private void resetRunningMeans_Click(object sender, EventArgs e)
        {
            resetEdmErrRunningMeans();
        }

        #endregion

        #region Thread safe handlers

        private void SetTextBox(TextBox textBox, string text)
        {
            textBox.Invoke(new SetTextDelegate(SetTextHelper), new object[] { textBox, text });
        }

        private delegate void SetTextDelegate(TextBox textBox, string text);

        private void SetTextHelper(TextBox textBox, string text)
        {
            textBox.Text = text;
        }

        private delegate void PlotXYDelegate(double[] x, double[] y);

        private void PlotXYAppend(Graph graph, ScatterPlot plot, double[] x, double[] y)
        {
            graph.Invoke(new PlotXYDelegate(plot.PlotXYAppend), new Object[] { x, y });
        }

        private void PlotXY(Graph graph, ScatterPlot plot, double[] x, double[] y)
        {
            graph.Invoke(new PlotXYDelegate(plot.PlotXY), new Object[] { x, y });
        }

        private delegate void ClearDataDelegate();
        private void ClearNIGraph(Graph graph)
        {
            graph.Invoke(new ClearDataDelegate(graph.ClearData));
        }

        #endregion    
    }

}