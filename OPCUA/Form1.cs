using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpcUaHelper;
using OpcUaHelper.Forms;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Newtonsoft.Json;

namespace OPCUA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private OpcUaClient opcUaClient = new OpcUaClient();
        private async void button1_Click(object sender, EventArgs e)
        {
           

            try
            {

                //opcUaClient.UserIdentity = new Opc.Ua.UserIdentity("administrator", "123456");//
                //await opcUaClient.ConnectServer("opc.tcp://192.168.1.2:49320");
                await opcUaClient.ConnectServer("opc.tcp://office:4862");
                //MessageBox.Show("连接成功！");
                button1.BackColor = Color.Green;
                button2.BackColor = Color.LightGray;
        
            }
            catch(Exception ex)
            {
                ClientUtils.HandleException(Text, ex);
            }
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            opcUaClient.Disconnect();
            button1.BackColor = Color.LightGray;
            button2.BackColor = Color.Red;
        }
        

    private void button3_Click(object sender, EventArgs e)
        {

            if (!opcUaClient.Connected)
            {
                MessageBox.Show("请先连接OPC服务器！");
                return;
            }
            this.timer1.Enabled = true;
            //try
            //{
            //    Opc.Ua.DataValue value = opcUaClient.ReadNode("ns=2;s=模拟器示例.函数.Ramp1");
            //    textBox1.Text = value.ToString(); // 显示测试数据
            //}
            //catch (Exception ex)
            //{
            //    // 使用了opc ua的错误处理机制来处理错误，网络不通或是读取拒绝
            //    ClientUtils.HandleException(Text, ex);
            //}
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                bool IsSuccess = opcUaClient.WriteNode("ns=2;s=模拟器示例.函数.Ramp1", 123);
                textBox2.Text = IsSuccess.ToString(); ; // 显示True，如果成功的话
            }
            catch (Exception ex)
            {
                // 使用了opc ua的错误处理机制来处理错误，网络不通或是读取拒绝
                ClientUtils.HandleException(Text, ex);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!opcUaClient.Connected) { textBox1.Text = "连接已经断开！"; 
                return; }
            try
            {
                Opc.Ua.DataValue value = opcUaClient.ReadNode("ns=1;s=t|MyTagFloat");
                textBox1.Text = value.ToString(); // 显示测试数据
            }
            catch (Exception ex)
            {
                // 使用了opc ua的错误处理机制来处理错误，网络不通或是读取拒绝
                ClientUtils.HandleException(Text, ex);
            }
        }

        private void btBrowseSever_Click(object sender, EventArgs e)
        {
            using (FormBrowseServer form = new FormBrowseServer())
            {
                form.ShowDialog();
            }
        }

        private void btReadNest_Click(object sender, EventArgs e)
        {

            this.timer2.Enabled = true;

        }

        private void button5_Click(object sender, EventArgs e)
        {
            HttpWebRequest req = WebRequest.Create("https://office:34568/WinCCRestService/tagManagement/Value/MyTagFloat") as HttpWebRequest;

            //HttpWebRequest req = WebRequest.Create("https://office:34568/WinCCRestService") as HttpWebRequest;

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            req.Method = "GET";
            //req.Headers.Add("Content-Type", "application/json");

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            Stream str = res.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(str, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();

            MessageBox.Show(str.ToString());

        }
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;
            return true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //HttpWebRequest req = WebRequest.Create("https://office:34568/WinCCRestService/tagManagement/Value/MyTagFloat") as HttpWebRequest;

            HttpWebRequest req = WebRequest.Create("https://office:34568/WinCCRestService") as HttpWebRequest;

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            req.Method = "POST";
            req.ContentLength =0;
            //req.Headers.Add("Content-Type", "application/json");


            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            Stream str = res.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(str, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();

            MessageBox.Show(retString.ToString());

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            RestClient client = new RestClient("https://office:34568/WinCCRestService/tagManagement/Value/MyTagFloat");
            client.Authenticator = new HttpBasicAuthenticator("admin", "123456");
            var request = new RestRequest("statuses/home_timeline.json", DataFormat.Json);
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            request.AddHeader("Content-Type", "application/json");
            request.Timeout = 10000;
            request.AddHeader("Cache-Control", "no-cache");
            
            IRestResponse response = client.Execute(request);

            var content = response.Content;
            MyValue value = JsonConvert.DeserializeObject<MyValue>(content);
            
            textBox3.Text =String.Format("{0:0.00}",value.value);
        }
    }
    public class MyValue
    {
        public string Tagname { get; set; }
        public string datatype { get; set; }
        public float value { get; set; }
        public string timestamp { get; set; }
        public string qualitycode { get; set; }
    }
}
