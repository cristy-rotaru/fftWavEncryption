using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace fftWavEncryption
{
    /// <summary>
    /// Interaction logic for LineChart.xaml
    /// </summary>
    public partial class LineChart : UserControl
    {
        private double lowLimit, highLimit;
        private int plotStep;
        private bool invertXAxis, invertYAxis;

        public double LowLimit
        {
            get
            {
                return this.lowLimit;
            }

            set
            {
                this.lowLimit = (double)value;
            }
        }

        public double HighLimit
        {
            get
            {
                return this.highLimit;
            }

            set
            {
                this.highLimit = (double)value;
            }
        }

        public int PlotStep
        {
            get
            {
                return this.plotStep;
            }

            set
            {
                this.plotStep = value;
            }
        }

        public bool InvertXAxis
        {
            get
            {
                return this.invertXAxis;
            }

            set
            {
                this.invertXAxis = value;
            }
        }

        public bool InvertYAxis
        {
            get
            {
                return this.invertYAxis;
            }

            set
            {
                this.invertYAxis = value;
            }
        }

        public LineChart()
        {
            InitializeComponent();

            this.lowLimit = 0;
            this.highLimit = 0;
            this.plotStep = 1;
            this.invertXAxis = false;
            this.invertYAxis = false;
        }

        public void Plot(float[] values)
        {
            if (this.highLimit <= this.lowLimit)
            {
                throw new Exception("Invalid limits.");
            }

            if (values == null)
            {
                throw new ArgumentNullException();
            }

            polylinePlot.Points.Clear();

            for (int i = 0; i < values.Length; i += this.plotStep)
            {
                double x = canvasPlot.ActualWidth / (values.Length - 1) * i;
                double y = canvasPlot.ActualHeight / (this.highLimit - this.lowLimit) * (values[i] - this.lowLimit);

                if (this.invertXAxis == true)
                {
                    x = canvasPlot.ActualWidth - x;
                }

                if (this.invertYAxis == false)
                {
                    y = canvasPlot.ActualHeight - y;
                }

                if (y < 0)
                {
                    y = 0;
                }
                else if (y > canvasPlot.ActualHeight)
                {
                    y = canvasPlot.ActualHeight;
                }

                polylinePlot.Points.Add(new Point(x, y));
            }
        }
    }
}
