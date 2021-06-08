using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace WFOCR
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : Window
    {
        private string mode = "";

        public Setting()
        {
            InitializeComponent();

            SerializableDictionary<string, string> setting = new SerializableDictionary<string, string>();

            setting.Add("Url", "");
            setting.Add("AppCode", "");
            setting.Add("AppSecret", "");
            setting.Add("AppKey", "");
            setting.Add("Mode", "");

            if (File.Exists("setting.xml"))
            {
                using (FileStream fs = new FileStream("setting.xml", FileMode.Open))
                {
                    XmlSerializer xmlFormatter = new XmlSerializer(typeof(SerializableDictionary<string, string>));
                    setting = (SerializableDictionary<string, string>)xmlFormatter.Deserialize(fs);
                }
                tbau.Text = setting["Url"];
                tbac.Text = setting["AppCode"];
                tbas.Text = setting["AppSecret"];
                tbak.Text = setting["AppKey"];
                this.mode = setting["Mode"];

                if (setting["Mode"] == "AppCode")
                {
                    cbac.IsChecked = true;
                    tbac.IsEnabled = true;
                    tbac.Background = new LinearGradientBrush(Colors.White, Colors.White, 0);
                }
                else
                {
                    if (setting["Mode"] == "AppKey") 
                    {
                    cbak.IsChecked = true;
                    tbak.IsEnabled = true;
                    tbak.Background = new LinearGradientBrush(Colors.White, Colors.White, 0);
                    tbas.IsEnabled = true;
                    tbas.Background = new LinearGradientBrush(Colors.White, Colors.White, 0); 
                    }                    
                }

            }
            else
            {
                using (FileStream fs = new FileStream("setting.xml", FileMode.Create))
                {
                    XmlSerializer xmlFormatter = new XmlSerializer(typeof(SerializableDictionary<string, string>));
                    xmlFormatter.Serialize(fs, setting );
                }
            }
        }

        private void AppCode_Checked(object sender, RoutedEventArgs e)
        {
            if (cbak.IsChecked == true)
            {
                cbak.IsChecked = false;
            }
            tbac.IsEnabled = true;
            tbac.Background = new LinearGradientBrush(Colors.White, Colors.White, 0);

            this.mode = "AppCode";
        }

        private void AppKey_Checked(object sender, RoutedEventArgs e)
        {
            if (cbac.IsChecked ==true)
            {
                cbac.IsChecked = false;               
            } 
            tbak.IsEnabled = true;
            tbak.Background = new LinearGradientBrush(Colors.White, Colors.White, 0);
            tbas.IsEnabled = true;
            tbas.Background = new LinearGradientBrush(Colors.White, Colors.White, 0);

            this.mode = "AppKey";

        }

        private void AppCode_Unchecked(object sender, RoutedEventArgs e)
        {
            tbac.IsEnabled = false;
            tbac.Background = new LinearGradientBrush(Colors.Silver, Colors.Silver, 0);
        }

        private void AppKey_Unchecked(object sender, RoutedEventArgs e)
        {
            tbak.IsEnabled = false;
            tbak.Background = new LinearGradientBrush(Colors.Silver, Colors.Silver, 0);
            tbas.IsEnabled = false;
            tbas.Background = new LinearGradientBrush(Colors.Silver, Colors.Silver, 0);
        }

        private void Click_Canecl(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Click_OK(object sender, RoutedEventArgs e)
        {

            using (FileStream fs = new FileStream("setting.xml", FileMode.Create))
            {

                SerializableDictionary<string, string> setting = new SerializableDictionary<string, string>
                {
                    ["Url"] = tbau.Text,
                    ["AppCode"] = tbac.Text,
                    ["AppSecret"] = tbas.Text,
                    ["AppKey"] = tbak.Text,
                    ["Mode"] = this.mode
                };

                XmlSerializer xmlFormatter = new XmlSerializer(typeof(SerializableDictionary<string, string>));
                xmlFormatter.Serialize(fs, setting);
                
            }
            this.DialogResult = true;            
        }
    }
    

}
