using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;

namespace BSK
{
    public partial class Wykresy : Form
    {
        private float[,] data;

        public Wykresy(float Frequency, float Height1, float Height2, bool LOS, float Phi, float BuildDist, float StreetWidth, float HeightObstacle, string Environment, float Result)
        {
            InitializeComponent(Result);
            float Distance, Intercept, Slope, Propagation;
            int i = 0;
            data = new float[498, 3];
            for (Distance = 0.02f; Distance <= 5; Distance += 0.01f)
            {
                Intercept = BSK.Propagacja.PropagateLkm(Frequency, Height1, Height2, Distance, LOS, Phi, BuildDist, StreetWidth, HeightObstacle, Environment);
                Slope = BSK.Propagacja.PropagateLaw(Height1, LOS, HeightObstacle);
                Propagation = (float)(Intercept + 10 * Slope * Math.Log10(Distance));
                data[i, 0] = Distance;
                data[i, 1] = Propagation;
                i++;
            }
            chart2.Series.Clear();
            chart2.Series.Add("Spadek");
            chart2.Series[0].ChartType = SeriesChartType.Line;
            chart2.ChartAreas[0].AxisX.LabelStyle.Format = "{F2}";
            chart2.ChartAreas[0].AxisX.Title = "Dystans[km]";
            chart2.ChartAreas[0].AxisY.Title = "Strata[dB]";

            chart2.Legends.Clear();

            for (int j = 0; j < i; j++)
            {
                chart2.Series[0].Points.AddXY(data[j,0], data[j, 1]);
            }

            i = 0;
            for (Frequency = 800f; Frequency <= 2000f; Frequency += 2.41f)
            {
                data[i, 2] = Frequency;
                i++;
            }

            chart1.Series.Clear();
            chart1.Series.Add("Spadek");
            chart1.Series[0].ChartType = SeriesChartType.Line;
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "{F2}";
            chart1.ChartAreas[0].AxisX.Title = "Częstotliwość[Hz]";
            chart1.ChartAreas[0].AxisY.Title = "Strata[dB]";

            chart1.Legends.Clear();

            for (int j = 0; j < i; j++)
            {
                chart1.Series[0].Points.AddXY(data[j, 2], data[j, 1]);
            }

        }
    }
}
