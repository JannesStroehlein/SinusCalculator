using HandyControl.Data;
using HandyControl.Tools;
using System;
using System.Windows;

namespace SinusCalculator
{
    public partial class App
    {
        /// <summary>
        /// Wird beim Starten der App aufgerufen
        /// </summary>
        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            //Setzt die Sprache und das Thema
            ConfigHelper.Instance.SetLang("en");
            this.UpdateSkin(SkinType.Dark);
        }

        /// <summary>
        /// Setzt das Thema
        /// </summary>
        internal void UpdateSkin(SkinType skin)
        {
            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/HandyControl;component/Themes/Skin{skin.ToString()}.xaml")
            });
            this.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
            });
        }
    }
}
