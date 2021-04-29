﻿/*
 * Diese Datei beinhaltet die Interaktionslokig für MainWindow.xaml
 * Die Interaktionslogik legt fest, was passiert wenn der Nutzer mit der Nutzeroberfläche agiert.
 * Dazu gehört:
 * - Texte ändern
 * - Farbschema ändern
 * - Werte anzeigen
 */
using System;
using System.Diagnostics;
using System.Windows;
using System.ComponentModel;
using HandyControl.Controls;

namespace SinusCalculator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public FunctionProperties CurrentFunction;
        public RadianDegreeConverter CurrentRadianDegreeConverter;

        private double Graph_XMin = -6 * Math.PI;
        private double Graph_XMax = 6 * Math.PI;
        private double Graph_Step = 0.05;

        /// <summary>
        /// Einstiegspunkt für wenn das Fenster gestartet wird
        /// </summary>
        public MainWindow()
        {
            //Werte festlegen bevor sie angezeit werden
            this.CurrentFunction = new FunctionProperties();
            this.CurrentFunction.Reset();
            this.CurrentFunction.onValueChanged += CurrentFunction_onValueChanged;
            this.CurrentRadianDegreeConverter = new RadianDegreeConverter
            {
                Degree = 0,
                Radian = 0
            };

            //Laden des Fensters
            InitializeComponent();

            //Werte festlegen
            this.FunctionPropertyGrid.ShowSortButton = false;

            this.GraphXMinInput.Value = this.Graph_XMin;
            this.GraphXMaxInput.Value = this.Graph_XMax;
            this.GraphStepInput.Value = this.Graph_Step;

            //Den DarkMode für den Graphen anschalten
            this.Plot.plt.Style(ScottPlot.Style.Black);

            this.FunctionPropertyGrid.SelectedObject = this.CurrentFunction;
        }
        /// <summary>
        /// Wird aufgerufen, wenn sich die aktuelle Sinusfuntion ändert
        /// 
        /// Ist dafür zuständig den Graphen und die angezeigte Funktionsgleichung zu aktuallisieren.
        /// </summary>
        private void CurrentFunction_onValueChanged(object sender, EventArgs e)
        {
            this.FunctionEquasionBox.Text = this.CurrentFunction.GetFunctionTerm();
            if (this.AutoUpdateSwitch.IsChecked.Value)
                this.UpdatePlot();
        }
        
        /// <summary>
        /// Dafür zuständig den Radian - Grad Umrechner zu betreiben
        /// </summary>
        private void RadianBox_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            this.CurrentRadianDegreeConverter.Radian = e.Info;
            this.DegreeBox.ValueChanged -= this.DegreeBox_ValueChanged; //Notwendig um eine Rückkopplung zu vermeiden
            this.DegreeBox.Value = this.CurrentRadianDegreeConverter.Degree;
            this.DegreeBox.ValueChanged += this.DegreeBox_ValueChanged;
        }
        /// <summary>
        /// Dafür zuständig den Radian - Grad Umrechner zu betreiben
        /// </summary>
        private void DegreeBox_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            this.CurrentRadianDegreeConverter.Degree = e.Info;
            this.RadianBox.ValueChanged -= this.RadianBox_ValueChanged; //Notwendig um eine Rückkopplung zu vermeiden
            this.RadianBox.Value = this.CurrentRadianDegreeConverter.Radian;
            this.RadianBox.ValueChanged += this.RadianBox_ValueChanged;
        }
        /// <summary>
        /// Wird aufgerufen wenn der Reset-Knopf gedrückt wurde und setzt die Werte der Funktion wieder auf den Standard
        /// </summary>
        private void ResetFunctionGenerator_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentFunction.Reset();
            this.FunctionPropertyGrid.SelectedObject = this.CurrentFunction;
        }
        /// <summary>
        /// Wird aufgerufen wenn der Button zum Aktuallisieren des Graphens gedrückt wurde
        /// </summary>
        private void ApplyGraph_Click(object sender, RoutedEventArgs e) => this.UpdatePlot(); //Direkte Weiterleitung an die UpdatePlot Methode
        /// <summary>
        /// Errechnet die Werte für den Graphen und zeigt diese dann an
        /// </summary>
        private void UpdatePlot()
        {
            int Steps = Convert.ToInt32((this.Graph_XMax - this.Graph_XMin) / this.Graph_Step);

            //Das sind Arrays also Listen mit einer bestimmten Länge
            //Ein double(-precision) ist ein Datentyp mit dem Kommazahlen gespeichert werden
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

            //Nutzerinterface aktuallisieren
            this.Plot.plt.Clear();
            this.Plot.plt.PlotScatter(xs, ys, label: "Funktion");
            this.Plot.Render();
        }
        /// <summary>
        /// Wird aufgerufen wenn sich die Einstellungen für den Graphen geändert haben
        /// </summary>
        private void GraphData_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            //Den Typen des object sender zu NumericUpDown ändern
            NumericUpDown numericUpDown = sender as NumericUpDown;

            //Anhand des definierten DataContext herausfinden welches NumericUpDown sich geändert hat
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
            //Neue Minimum und Maximum Werte für das Nutzerinterface setzten um Fehler vorzubeugen
            this.GraphXMinInput.Maximum = this.Graph_XMax - 0.01;
            this.GraphXMaxInput.Minimum = this.Graph_XMin + 0.01;

            //Graphen aktuallisieren
            if (this.AutoUpdateSwitch.IsChecked.Value)
                this.UpdatePlot();
        }
        /// <summary>
        /// Wird aufgerufen, wenn Schalter für den Dunklen Modus geklickt wird
        /// </summary>
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
        /// <summary>
        /// Wird aufgerufen wenn ein Link geklickt wird
        /// </summary>
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) => Process.Start(e.Uri.ToString()); //Einfach den Browser mit der Adresse starten
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
    /// <summary>
    /// Klasse zur Speicherung der Eigenschaften der Sinusfunktion 
    /// </summary>
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
            //Starke Nutzung von Inline-If/Else
            //Schema: Bedingung ? Wenn Wahr : Wenn Falsch;
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
