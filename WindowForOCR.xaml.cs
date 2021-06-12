using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
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
using System.Xml.Serialization;
using Microsoft.Win32;
using aliyun_api_gateway_sdk.Constant;
using aliyun_api_gateway_sdk.Util;
using System.Security.Cryptography;

namespace WFOCR
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WindowForOCR : Window
    {

        public WindowForOCR()
        {
            InitializeComponent();
        }

        private void Button_Click_Output(object sender, RoutedEventArgs e)
        {
            string nowtime = DateTime.Now.ToString();

            nowtime = nowtime.Replace(" ", "");
            nowtime = nowtime.Replace("/", "");
            nowtime = nowtime.Replace(":", "");

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "文本文件 (*.txt;)|*.txt;",
                FileName = "OCRout" + nowtime + ".txt"
            };
            Nullable<bool> sfdresult = sfd.ShowDialog();
            
            if (sfdresult == true)
            {
                string outputfilelocat = sfd.ToString();
                outputfilelocat = outputfilelocat.Remove(0, 51);                

                using (StreamWriter sw = new StreamWriter(outputfilelocat))
                {
                    sw.WriteLine(textout.Text);
                }
            }
        }

        private void Button_Click_OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "图像文件 (*.png; *.jpg)|*.jpg;*.png"
            };
            Nullable<bool> ofdresult = ofd.ShowDialog();
           
            if (ofdresult == true)
            {
                string fileDialog = ofd.ToString();
                string fileLocat = fileDialog.Remove(0, 51);
                Filelocat.Text = fileLocat;
            }
            
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            //String
            string configure = "";
            string base64 = "";
            string method = "POST";            
            string img_file = Filelocat.Text;
            string appcode = "";
            string appKey = "";
            string appSecret = "";
            string url = "";
            //String

            if (System.IO.File.Exists("setting.xml"))
            {
                
                SerializableDictionary<string, string> set = new SerializableDictionary<string, string>();
                using (FileStream fs = new FileStream("setting.xml", FileMode.Open))
                {
                    XmlSerializer xmlFormatter = new XmlSerializer(typeof(SerializableDictionary<string, string>));
                    set = (SerializableDictionary<string, string>)xmlFormatter.Deserialize(fs);
                }
                
                if (set["Url"].Contains("https://"))
                {
                    if (set["Mode"] == "")
                    {
                        MessageBox.Show("请填写AppCode或AppKey", "警告");
                    }
                    else
                    {
                        //ModeAppCode
                        if (set["Mode"] == "AppCode")
                        {
                            if (set["AppCode"] == "")
                            {
                                MessageBox.Show("请填写AppCode", "警告");
                            }
                            else
                            {
                                if (Filelocat.Text == "")
                                {
                                    MessageBox.Show("请正确填写文件路径或文件Url","警告");
                                }
                                //Start
                                else
                                {

                                    //String
                                    string querys = "";
                                    appcode = set["AppCode"];
                                    url = set["Url"];
                                    //String

                                    if (img_file.StartsWith("http"))
                                    {
                                        base64 = img_file;
                                    }
                                    else
                                    {
                                        FileStream fs = new FileStream(img_file, FileMode.Open);
                                        BinaryReader br = new BinaryReader(fs);
                                        byte[] contentBytes = br.ReadBytes(Convert.ToInt32(fs.Length));
                                        base64 = System.Convert.ToBase64String(contentBytes);
                                    }

                                    string bodys;
                                    bodys = "{\"image\":\"" + base64 + "\"";
                                    if (configure.Length > 0)
                                    {
                                        bodys += ",\"configure\" :\"" + configure + "\"";
                                    }
                                    bodys += "}";


                                    HttpWebRequest httpRequest = null;
                                    HttpWebResponse httpResponse = null;

                                    if (0 < querys.Length)
                                    {
                                        url = url + "?" + querys;
                                    }

                                    if (url.Contains("https://"))
                                    {
                                        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                                        httpRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
                                    }
                                    else
                                    {
                                        httpRequest = (HttpWebRequest)WebRequest.Create(url);
                                    }

                                    httpRequest.Method = method;
                                    httpRequest.Headers.Add("Authorization", "APPCODE " + appcode);
                                    httpRequest.ContentType = "application/json; charset=UTF-8";

                                    if (0 < bodys.Length)
                                    {
                                        byte[] data = Encoding.UTF8.GetBytes(bodys);
                                        using (Stream stream = httpRequest.GetRequestStream())
                                        {
                                            stream.Write(data, 0, data.Length);
                                        }
                                    }
                                    try
                                    {
                                        httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                                    }
                                    catch (WebException ex)
                                    {
                                        httpResponse = (HttpWebResponse)ex.Response;
                                    }

                                    if (httpResponse.StatusCode != HttpStatusCode.OK)
                                    {
                                        textout.Text = ("http error code: " + httpResponse.StatusCode);
                                        textout.Text += ("error in header: " + httpResponse.GetResponseHeader("X-Ca-Error-Message"));
                                        textout.Text += ("error in body: ");
                                        Stream st = httpResponse.GetResponseStream();
                                        StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));
                                        textout.Text += (reader.ReadToEnd());
                                    }
                                    else
                                    {

                                        Stream st = httpResponse.GetResponseStream();
                                        StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));
                                        textout.Text = (reader.ReadToEnd());

                                    }
                                }
                            }
                        }

                        //ModeAppKey
                        else
                        {
                            if (set["AppKey"] == "")
                            {
                                MessageBox.Show("请填写AppSecret和AppKey", "警告");
                            }
                            else
                            {
                                if (set["AppSecret"] == "")
                                {
                                    MessageBox.Show("请填写AppSecret和AppKey", "警告");
                                }
                                else
                                {
                                    if (Filelocat.Text == "")
                                    {
                                        MessageBox.Show("请正确填写文件路径或文件Url", "警告");
                                    }
                                    //Start
                                    else
                                    {

                                        url = set["Url"];
                                        appKey = set["AppKey"];
                                        appSecret = set["AppSecret"];



                                        if (img_file.StartsWith("http"))
                                        {
                                            base64 = img_file;
                                        }
                                        else
                                        {
                                            FileStream fs = new FileStream(img_file, FileMode.Open);
                                            BinaryReader br = new BinaryReader(fs);
                                            byte[] contentBytes = br.ReadBytes(Convert.ToInt32(fs.Length));
                                            base64 = System.Convert.ToBase64String(contentBytes);
                                        }
                                        string bodys;
                                        bodys = "{\"image\":\"" + base64 + "\"";
                                        if (configure.Length > 0)
                                        {
                                            bodys += ",\"configure\" :\"" + configure + "\"";
                                        }
                                        bodys += "}";

                                        Dictionary<string, string> headers = new Dictionary<string, string>();
                                        Dictionary<string, string> querys = new Dictionary<string, string>();
                                        Dictionary<string, string> bodys_map = new Dictionary<string, string>();
                                        List<string> signHeader = new List<string>();

                                        //设定Content-Type，根据服务器端接受的值来设置
                                        headers.Add(HttpHeader.HTTP_HEADER_CONTENT_TYPE, ContentType.CONTENT_TYPE_JSON);
                                        //设定Accept，根据服务器端接受的值来设置
                                        headers.Add(HttpHeader.HTTP_HEADER_ACCEPT, ContentType.CONTENT_TYPE_JSON);

                                        //注意：如果有非Form形式数据(body中只有value，没有key)；如果body中是key/value形式数据，不要指定此行
                                        headers.Add(HttpHeader.HTTP_HEADER_CONTENT_MD5, MessageDigestUtil.Base64AndMD5(Encoding.UTF8.GetBytes(bodys)));

                                        //注意：业务body部分
                                        bodys_map.Add("", bodys);

                                        //指定参与签名的header            
                                        signHeader.Add(SystemHeader.X_CA_TIMESTAMP);

                                        Uri myUri = new Uri(url);
                                        try
                                        {
                                            using (HttpWebResponse response = HttpUtil.HttpPost(myUri.Scheme + "://" + myUri.Host, myUri.AbsolutePath, appKey, appSecret, 30000, headers, querys, bodys_map, signHeader))
                                            {
                                                if (response.StatusCode != HttpStatusCode.OK)
                                                {
                                                    textout.Text = ("http error code: " + response.StatusCode);
                                                    textout.Text += ("error in header: " + response.GetResponseHeader("X-Ca-Error-Message"));
                                                    textout.Text += "error in body: ";
                                                    Stream st = response.GetResponseStream();
                                                    StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));
                                                    textout.Text += (reader.ReadToEnd());
                                                }
                                                else
                                                {

                                                    Stream st = response.GetResponseStream();
                                                    StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));
                                                    textout.Text = (reader.ReadToEnd());
                                                    textout.Text += (Constants.LF);

                                                }
                                            }
                                        }
                                        catch(WebException ex)
                                        {
                                            MessageBox.Show(ex.ToString());
                                            MessageBox.Show("ApiUrl错误,请重新填写Url","错误");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("请正确填写Url", "警告");
                }
            }
            else
            {
                MessageBox.Show("请进行配置","警告");
            }

            

        }

        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        private void TextBox_Filelocat(object sender, TextChangedEventArgs e)
        {
            
        }

        private void TextBox_Output(object sender, TextChangedEventArgs e)
        {
            
        }

        private void Button_Click_Setting(object sender, RoutedEventArgs e)
        {
            Setting setting = new Setting();
            setting.ShowDialog();
        }

        private string SimplifyOutput(string input, string start, string end)
        {
            Dictionary<int, int> sd = new Dictionary<int, int>();

            int result = input.IndexOf(start);

            if (result >= 0)
            {
                int st = 0;
                int ed = 0;

                while (st > -1)
                {
                    st = input.IndexOf(start, st + 1);
                    ed = input.IndexOf(end, ed + 1);
                    sd.Add(st, ed);
                }
            }
            foreach (KeyValuePair<int, int> kv in sd )
            {
                if (kv.Key != kv.Value)
                {
                    string subs = input.Substring(kv.Key, kv.Value - kv.Key);
                    string space = "";
                    space = space.PadLeft(subs.Length);
                    input = input.Replace(subs, space);
                }
            }

            input = input.Replace(" ", "");
            input = input.Remove(0, 60);
            input = input.Replace("},", "\n");
            input = input.Replace("}],\"success\":true}", "");
            string output = input;

            return output;
        }

        private void Button_Click_SimpOut(object sender, RoutedEventArgs e)
        {
            if (textout.Text.StartsWith("{\"request_id\":\""))
            {
                string nowtime = DateTime.Now.ToString();

                nowtime = nowtime.Replace(" ", "");
                nowtime = nowtime.Replace("/", "");
                nowtime = nowtime.Replace(":", "");

                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "文本文件 (*.txt;)|*.txt;",
                    FileName = "SimpOCRout" + nowtime + ".txt"
                };
                Nullable<bool> sfdresult = sfd.ShowDialog();

                if (sfdresult == true)
                {
                    string outputfilelocat = sfd.ToString();
                    outputfilelocat = outputfilelocat.Remove(0, 51);

                    string simo = SimplifyOutput(textout.Text, "{\"rect\":", "\"word\":\"");

                    using (StreamWriter sw = new StreamWriter(outputfilelocat))
                    {
                        sw.WriteLine(simo);
                    }
                }
            }
            else
            {
                MessageBox.Show("简化导出仅成功输出时可用", "警告");
            }
        }
    }
}
