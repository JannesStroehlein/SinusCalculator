using HandyControl.Tools;
using HandyControl.Themes;
using ScottPlot;
using System;
using System.Diagnostics;
using System.Windows;
using System.ComponentModel;
using HandyControl.Controls;

namespace SinusCalculator
{
    public partial class MainWindow
    {
        public FunctionProperties CurrentFunction;
        public RadianDegreeConverter CurrentRadianDegreeConverter;

        private double Graph_XMin = -6 * Math.PI;
        private double Graph_XMax = 6 * Math.PI;
        private double Graph_Step = 0.05;


        public MainWindow()
        {
            this.CurrentFunction = new FunctionProperties();
            this.CurrentFunction.Reset();
            this.CurrentFunction.onValueChanged += CurrentFunction_onValueChanged;
            this.CurrentRadianDegreeConverter = new RadianDegreeConverter
            {
                Degree = 0,
                Radian = 0
            };
            InitializeComponent();

            this.FunctionPropertyGrid.ShowSortButton = false;


            this.GraphXMinInput.Value = this.Graph_XMin;
            this.GraphXMaxInput.Value = this.Graph_XMax;
            this.GraphStepInput.Value = this.Graph_Step;

            this.Plot.plt.Style(ScottPlot.Style.Black);

            this.FunctionPropertyGrid.SelectedObject = this.CurrentFunction;
        }
        private void CurrentFunction_onValueChanged(object sender, EventArgs e)
        {
            this.FunctionEquasionBox.Text = this.CurrentFunction.GetFunctionTerm();
            //this.FunctionPropertyGrid.R
            if (this.AutoUpdateSwitch.IsChecked.Value)
                this.UpdatePlot();
        }
        private void RadianBox_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            this.CurrentRadianDegreeConverter.Radian = e.Info;
            this.DegreeBox.ValueChanged -= this.DegreeBox_ValueChanged;
            this.DegreeBox.Value = this.CurrentRadianDegreeConverter.Degree;
            this.DegreeBox.ValueChanged += this.DegreeBox_ValueChanged;
        }
        private void DegreeBox_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            this.CurrentRadianDegreeConverter.Degree = e.Info;
            this.RadianBox.ValueChanged -= this.RadianBox_ValueChanged;
            this.RadianBox.Value = this.CurrentRadianDegreeConverter.Radian;
            this.RadianBox.ValueChanged += this.RadianBox_ValueChanged;
        }

        private void ResetFunctionGenerator_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentFunction.Reset();
            this.FunctionPropertyGrid.SelectedObject = this.CurrentFunction;
        }
        private void ApplyGraph_Click(object sender, RoutedEventArgs e) => this.UpdatePlot();
        private void UpdatePlot()
        {
            int Steps = Convert.ToInt32((this.Graph_XMax - this.Graph_XMin) / this.Graph_Step);

            double[] xs = new double[Steps];
            double[] ys = new double[Steps];

            for (int i = 0; i < ys.Length; i++)
            {
                double X = this.Graph_XMax + i * this.Graph_Step;
                xs[i] = X;
                if (this.CurrentFunction.Cosinus)
                    ys[i] = this.CurrentFunction.Amplitude * (Math.Sin(this.CurrentFunction.Frequency * (X - this.CurrentFunction.XOffset)) + this.CurrentFunction.YOffset);
                else
                    ys[i] = this.CurrentFunction.Amplitude * (Math.Cos(this.CurrentFunction.Frequency * (X - this.CurrentFunction.XOffset)) + this.CurrentFunction.YOffset);
            }

            this.Plot.plt.Clear();
            this.Plot.plt.PlotScatter(xs, ys, label: "Funktion");
            this.Plot.Render();
        }
        private void GraphData_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            NumericUpDown numericUpDown = sender as NumericUpDown;

            switch ((string)numericUpDown.DataContext)
            {
                case "XMin":
                    if (e.Info < this.Graph_XMax - this.Graph_Step)
                        this.Graph_XMin = e.Info;
                    else
                        e.Handled = true;
                    break;
                case "XMax":
                    if (e.Info > this.Graph_XMin + this.Graph_Step)
                        this.Graph_XMax = e.Info;
                    else
                        e.Handled = true;
                    break;
                case "Step":
                    this.Graph_Step = e.Info;
                    break;
            }
            this.GraphXMinInput.Maximum = this.Graph_XMax - 0.01;
            this.GraphXMaxInput.Minimum = this.Graph_XMin + 0.01;
            if (this.AutoUpdateSwitch.IsChecked.Value)
                this.UpdatePlot();
        }
        private void ThemeSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (this.ThemeSwitch.IsChecked.Value)
            {
                ((App)Application.Current).UpdateSkin(HandyControl.Data.SkinType.Dark);
                this.Plot.plt.Style(ScottPlot.Style.Black);
            }
            else
            {
                if (HandyControl.Controls.MessageBox.Ask("Du versuchst gerade das dunkle Thema zu deaktivieren. Dies kann zu Augenschäden führen.\nMöchtest du fortfahren?", "Dunklen Modes deaktivieren?") == MessageBoxResult.OK)
                {
                    ((App)Application.Current).UpdateSkin(HandyControl.Data.SkinType.Default);
                    this.Plot.plt.Style(ScottPlot.Style.Default);
                }
                else
                    this.ThemeSwitch.IsChecked = true;
            }
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) => Process.Start(e.Uri.ToString());
    }
    public class RadianDegreeConverter
    {
        private double degree;
        private double radian;
        public double Degree
        {
            get => this.degree;
            set
            {
                this.radian = (value * (Math.PI / 180)) / Math.PI;
                this.degree = value;
            }
        }
        public double Radian 
        {
            get => this.radian; 
            set 
            {
                this.degree = value * Math.PI * (180 / Math.PI);
                this.radian = value;
            }
        }
    }
    public class FunctionProperties
    {
        private double amplitude;
        private double frequency;
        private double xOffset;
        private double yOffset;
        private bool cosinus;

        private bool EventsEnabled = true;
        
        public event EventHandler<EventArgs> onValueChanged;

        [Category("Generell")]
        [DisplayName("Cosinus")]
        [Description("Legt fest ob es eine Sinusfunktion oder Cosinusfunktion ist")]
        [DefaultValue(false)]
        public bool Cosinus
        {
            get => this.cosinus;
            set
            {
                this.cosinus = value;
                if (this.EventsEnabled)
                {
                    EventHandler<EventArgs> handler = this.onValueChanged;
                    if (handler != null)
                        handler(this, new EventArgs());
                }
            }
        }

        [Category("Form")]
        [DisplayName("Amplitude")]
        [Description("Verändert die Amplitude der Sinusfunktion")]
        [DefaultValue(1)]
        public double Amplitude 
        { 
            get => this.amplitude;
            set
            {
                this.amplitude = value;
                if (this.EventsEnabled)
                {
                    EventHandler<EventArgs> handler = this.onValueChanged;
                    if (handler != null)
                        handler(this, new EventArgs());
                }
            }
        }

        [Category("Form")]
        [DisplayName("Frequenz")]
        [Description("Verändert die Frequenz der Sinusfunktion")]
        [DefaultValue(1)]
        public double Frequency
        {
            get => this.frequency;
            set
            {
                this.frequency = value;
                if (this.EventsEnabled)
                {
                    EventHandler<EventArgs> handler = this.onValueChanged;
                    if (handler != null)
                        handler(this, new EventArgs());
                }
            }
        }

        [Category("Verschiebung")]
        [DisplayName("X Verschiebung")]
        [Description("Verschiebt die Funktion auf der X Achse")]
        [DefaultValue(0)]
        public double XOffset
        {
            get => this.xOffset;
            set
            {
                this.xOffset = value;
                if (this.EventsEnabled)
                {
                    EventHandler<EventArgs> handler = this.onValueChanged;
                    if (handler != null)
                        handler(this, new EventArgs());
                }
            }
        }

        [Category("Verschiebung")]
        [DisplayName("Y Verschiebung")]
        [Description("Verschiebt die Funktion auf der Y Achse")]
        [DefaultValue(0)]
        public double YOffset
        {
            get => this.yOffset;
            set
            {
                this.yOffset = value;
                if (this.EventsEnabled)
                {
                    EventHandler<EventArgs> handler = this.onValueChanged;
                    if (handler != null)
                        handler(this, new EventArgs());
                }
            }
        }

        public string GetFunctionTerm()
        {
            string ret = this.Amplitude != 1 ? this.Amplitude + "*(" : "";
            ret += !this.cosinus ? "sin(" : "cos(";
            ret += this.Frequency != 1 ? this.Frequency + "*X" : "X";
            ret += this.XOffset != 0 ? "-" + this.XOffset + ")" : ")";
            ret += this.YOffset != 0 ? "+" + this.YOffset : "";
            ret += this.Amplitude != 1 ? ")" : "";
            return ret;
        }
        public void Reset()
        {
            this.EventsEnabled = false;
            this.Cosinus = false;
            this.Amplitude = 1;
            this.Frequency = 1;
            this.XOffset = 0;
            this.EventsEnabled = true;
            this.YOffset = 0;
        }
    }
}
