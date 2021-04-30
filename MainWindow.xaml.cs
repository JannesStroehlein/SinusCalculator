/*
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
using System.Collections.Generic;
using System.Linq;
using SinusCalculator.Data;
using System.Collections.ObjectModel;

namespace SinusCalculator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public FunctionProperties CurrentFunction;
        public RadianDegreeConverter CurrentRadianDegreeConverter;

        public ObservableCollection<CalculationData> Calculations { get; private set; }

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
            this.CurrentRadianDegreeConverter = new RadianDegreeConverter();

            //Initialisieren der Liste
            this.Calculations = new ObservableCollection<CalculationData>();

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
                double X = this.Graph_XMin + i * this.Graph_Step;
                double Y;
                xs[i] = X;
                
                if (this.CurrentFunction.Cosinus)
                    Y = this.CurrentFunction.Amplitude * (Math.Sin(this.CurrentFunction.Frequency * (X - this.CurrentFunction.XOffset)) + this.CurrentFunction.YOffset);
                else
                    Y = this.CurrentFunction.Amplitude * (Math.Cos(this.CurrentFunction.Frequency * (X - this.CurrentFunction.XOffset)) + this.CurrentFunction.YOffset);
                ys[i] = Y;
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
        /// <summary>
        /// Wird Aufgerufen, wenn der Berechnen Button für den X-Werte-Finder gedrückt wird
        /// </summary>
        private void YFinderCalcButton_Click(object sender, RoutedEventArgs e) => this.CalcXValues();
        /// <summary>
        /// Berechnet und zeigt die X Werte für den X-Werte-Finder an
        /// </summary>
        public void CalcXValues()
        {
            double Start = this.YFinderStartX.Value * Math.PI;
            double End = this.YFinderEndX.Value * Math.PI;
            double Value = this.YFinderYValue.Value;

            this.YFinderOutputBox.Text = "";
            CalculationResponse<double[]> values = this.CalcClampedXValuesForYValue(Start, End, Value);
            if (values != null)
            {
                if (values.Data.Length > 0)
                {
                    foreach (double xValue in values.Data)
                        this.YFinderOutputBox.Text += xValue == values.Data[values.Data.Length - 1] ? Math.Round(xValue, 3) + "π" : Math.Round(xValue, 3) + "π, ";
                    this.Calculations.Add(values.Calculation);
                }
                else
                    this.YFinderOutputBox.Text = "Kein Ergebniss";
            }
            else
                this.YFinderOutputBox.Text = "Kein Ergebniss";
        }
        /// <summary>
        /// Findet alle X Werte für einen Y Wert in einem bestimmten Bereich einer Sinusfunktion
        /// </summary>
        /// <param name="Start">Der minimal X Wert</param>
        /// <param name="End">Der maximal X Wert</param>
        /// <param name="Value">Der gesuchte Y Wert</param>
        /// <returns>Die Gefundenen Werte oder falls es zu einem Fehler kommt null</returns>
        public CalculationResponse<double[]> CalcClampedXValuesForYValue(double Start, double End, double Value)
        {
            CalculationData calculation = new CalculationData("X Werte von Y Wert ermitteln", "Alle X werte für einen genannten Y Wert berechnen");
            
            //Fehler Verhindern
            if (Value > this.CurrentFunction.Amplitude)
                return null;
            
            //Den Ersten X-Punkt wo Y den gesuchten Wert hat (berücksichtigt Sinus und Cosinus)
            double StartRadian = this.CurrentFunction.Cosinus ? Math.Acos(Value) : Math.Asin(Value);
            calculation.Steps.Add(
                new CalculationStep(
                    "Startpunkt errechnen",
                    "Mit asinus den ersten Punkt errechnen. Dieser ist auch eines der Ergebnisse!",
                    GetIfRounded(StartRadian) 
                    ? this.CurrentFunction.Cosinus ? string.Format("cos⁻¹({0})≈{1}", Value, Math.Round(StartRadian, 3)) : string.Format("sin⁻¹({0})≈{1}", Value, Math.Round(StartRadian, 3))
                    : this.CurrentFunction.Cosinus ? string.Format("cos⁻¹({0})={1}", Value, Math.Round(StartRadian, 3)) : string.Format("sin⁻¹({0})={1}", Value, Math.Round(StartRadian, 3))
                ));
            //Fehler Verhindern
            if (double.IsNaN(StartRadian))
                return null;

            List<double> Matches = new List<double>();

            //Den Ersten X-Wert zu den Ergebnissen hinzufügen
            Matches.Add(StartRadian);

            //double JumpCount = End - Start / 2 * Math.PI;

            //Die Periodenlänge berechnen
            double JumpSize = 2 * Math.PI / this.CurrentFunction.Frequency;
            if (this.CurrentFunction.Frequency != 1)
            {
                calculation.Steps.Add(
                    new CalculationStep(
                        "Periodenlänge errechnen",
                        "2π/Frequenz teilen",
                        GetIfRounded(JumpSize) 
                        ? string.Format("2π/{0}≈{1}", this.CurrentFunction.Frequency, Math.Round(JumpSize, 3))
                        : string.Format("2π/{0}={1}", this.CurrentFunction.Frequency, Math.Round(JumpSize, 3))
                    ));
            }

            //Die periodischen Wiederholungen von Startpunkt nach links berechnen und dabei auf den minimum Wert achten 
            for (double x = StartRadian - JumpSize; x > Start; x -= JumpSize)
            {
                Matches.Add(x);
                //Die gespiegelte X-Position herausfinden
                double MirroredPoint = Math.PI - x;
                if (MirroredPoint > Start)
                    Matches.Add(MirroredPoint);
            }        

            //Die periodischen Wiederholungen von Startpunkt nach rechts berechnen und dabei auf den minimum Wert achten 
            for (double x = StartRadian + JumpSize; x < End; x += JumpSize)
            {
                Matches.Add(x);
                //Die gespiegelte X-Position herausfinden
                double MirroredPoint = Math.PI - x;
                if (MirroredPoint < End)
                    Matches.Add(MirroredPoint);
            }

            calculation.Steps.Add(
                new CalculationStep(
                    "X Werte berechnen",
                    "Die Periodenlänge X-Mal mit dem Startpunkt addieren/subtrahieren. Anschließend die Spiegelung des Wertes Berechnen.",
                    string.Format("{0}+X*{1} oder {0}-X*{1}", Math.Round(StartRadian, 3), Math.Round(JumpSize, 3)) + "\n"
                     + string.Format("π-({0}+X*{1})", Math.Round(StartRadian, 3), Math.Round(JumpSize, 3))
                ));

            for (int i = 0; i < Matches.Count; i++)
                Matches[i] = Matches[i] / Math.PI;

            //Liste nach Größe sortieren
            return new CalculationResponse<double[]>(Matches.OrderBy(d => d).ToArray(), calculation);
        }
        /// <summary>
        /// Wird aufgerufen wenn eine Taste in der YFinderYValue Box gedrückt wird
        /// </summary>
        private void YFinderYValue_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                this.CalcXValues();
        }
        /// <summary>
        /// Wird aufgerufen, wenn der Rechnungen leeren Button geklickt wird
        /// </summary>
        private void ClearCalculations_Click(object sender, RoutedEventArgs e) => this.Calculations.Clear();
        /// <summary>
        /// Wird aufgerufen, wenn bei der DegreeBox eine Taste gedrückt wird
        /// </summary>
        private void DegreeBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                CalculationResponse<double> radians = this.CurrentRadianDegreeConverter.DegreeToRadian(this.DegreeBox.Value);
                this.RadianBox.Value = radians.Data;
                this.Calculations.Add(radians.Calculation);
            }
        }
        /// <summary>
        /// Wird aufgerufen, wenn bei der RadianBox eine Taste gedrückt wird
        /// </summary>
        private void RadianBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                CalculationResponse<double> degrees = this.CurrentRadianDegreeConverter.RadianToDegree(this.RadianBox.Value);
                this.DegreeBox.Value = degrees.Data;
                this.Calculations.Add(degrees.Calculation);
            }
        }

        /// <summary>
        /// Evaluiert, ob eine Zahl gerundet wurde.
        /// </summary>
        /// <param name="n">Die Zahl die gerundet werden soll</param>
        /// <param name="places">Die Anzahl an Nachkommastellen auf die gerundet werden soll</param>
        /// <returns>True wenn die Zahl gerundet wurde, False wenn nicht</returns>
        public static bool GetIfRounded(double n, int places = 3)
        {
            string a = n.ToString();
            string b = Math.Round(n, places).ToString();

            return a != b;
        }
    }
    /// <summary>
    /// Klasse mit Methoden zur Konvertierung von Radian und Grad
    /// </summary>
    public class RadianDegreeConverter
    {
        public CalculationResponse<double> DegreeToRadian(double degree)
        {
            CalculationData calculation = new CalculationData("Grad zu Radian", "Umrechnung vom Winkelmaß ins Bogenmaß");
            double radian = (degree * (Math.PI / 180)) / Math.PI;
            calculation.Steps.Add(
                new CalculationStep(
                    "Umrechnen",
                    "Zum Umrechnen muss die Zahl im Gradmaß einfach nur mit π/180 multipliziert werden und anschließend durch π geteilt werden.",
                    MainWindow.GetIfRounded(radian)
                    ? string.Format("({0}*(π/180))/π≈{1}π", degree, Math.Round(radian, 3))
                    : string.Format("({0}*(π/180))/π={1}π", degree, radian)
                    ));
            return new CalculationResponse<double>(radian, calculation);
        }
        public CalculationResponse<double> RadianToDegree(double radian)
        {
            CalculationData calculation = new CalculationData("Radian zu Grad", "Umrechnung vom Bogenmaß ins Winkelmaß");
            double degree = radian * Math.PI * (180 / Math.PI);
            calculation.Steps.Add(
                new CalculationStep(
                    "Umrechnen",
                    "Zum Umrechnen muss die Zahl im Bogenmaß einfach nur mit 180/π multiplizieren.",
                    MainWindow.GetIfRounded(radian)
                    ? string.Format("{0}π*(180/π)≈{1}°", radian, Math.Round(degree, 3))
                    : string.Format("{0}π*(180/π)={1}°", radian, degree)
                    ));
            return new CalculationResponse<double>(degree, calculation);
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