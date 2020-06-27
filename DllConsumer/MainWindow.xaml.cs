using System;
using System.Windows;
using System.Windows.Media;
using Dll;
using SettingsTools;

namespace DllConsumer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            AppSettings.RegisterAssembly(typeof(Class1));
            AppSettings.RegisterAssembly(typeof(AppSettings));
            AppSettings.ThrowExceptionWhenValuesInDifferentConfigsDontMatch = false;
            //AppSettings.IgnoreGlobalExeConfig = true;
            
            
            // ReSharper disable once PossibleNullReferenceException
            AppSettings.RegisterConverter(typeof(Color), s => (Color)ColorConverter.ConvertFromString(s));
            
            AppSettings.RegisterConverter(typeof(System.Drawing.Color), s => s.StartsWith("#") ? System.Drawing.ColorTranslator.FromHtml(s) : System.Drawing.Color.FromName(s));
            


            try
            {
                //Закоменченное не удалять, проверено-работает
                //MessageBox.Show(AppSettings.GetSetting<string>("хуй", "по умолчанию хуй"));
                //MessageBox.Show(AppSettings.GetSetting<int>("NumberOne", -1).ToString());
                //MessageBox.Show(AppSettings.GetSetting<double>("FloatNumber", -1).ToString(CultureInfo.InvariantCulture));
                //MessageBox.Show(AppSettings.GetSetting<bool>("Bule").ToString());
                //BtnButton.Background = new SolidColorBrush(AppSettings.GetSetting<Color>("Kolor"));
                BtnButton.Background = new SolidColorBrush(AppSettings.GetSetting<Color>("NoSuchSetting", Colors.Black));

                MessageBox.Show(AppSettings.GetSetting<double>("FloatError").ToString());

                MessageBox.Show(AppSettings.GetSetting<System.Drawing.Color>("SignatureTotalColor").ToString());
                MessageBox.Show(AppSettings.GetSetting<System.Drawing.Color>("SignatureAgreeColor").ToString());
                MessageBox.Show(AppSettings.GetSetting<System.Drawing.Color>("No such winformcolor setting", System.Drawing.Color.Indigo).ToString());

                if (AppSettings.HasErrors)
                {
                    MessageBox.Show(string.Join("\n", AppSettings.Errors));
                }


            }
            catch (Exception exception)
            {
                MessageBox.Show("nishmagla: " + exception.Message);
            }

            

            //ConfigurationManager.
            //MessageBox.Show(ConfigurationManager.AppSettings["DropOffDir"]);
            //MessageBox.Show(Class1.GlobalExePath());
            //MessageBox.Show(Class1.ThisDllPath());
        }




    }
}
