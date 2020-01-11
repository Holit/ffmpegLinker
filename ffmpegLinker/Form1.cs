using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;


namespace ffmpegLinker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public void CreatXmlWithInfo(string xmlPath,string executePath,bool is_copytoclip,bool is_UseShellExecute,bool is_CreateNoWindow,bool exitafterend)
        {
            XElement xElement = new XElement(
                new XElement("configuration",
                    new XElement("support",
                        new XElement("execute",new XAttribute("path", executePath))
                                ),
                    new XElement("config",
                        new XElement("prompt", new XAttribute("copytoclip", is_copytoclip)),
                        new XElement("operation", new XAttribute("UseShellExecute", is_UseShellExecute), 
                                                  new XAttribute("CreateNoWindow", is_CreateNoWindow), 
                                                  new XAttribute("exitafterend", exitafterend))
                                )
                        )
                );
            //需要指定编码格式，否则在读取时会抛：根级别上的数据无效。 第 1 行 位置 1异常
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding(false);
            settings.Indent = true;
            XmlWriter xw = XmlWriter.Create(xmlPath, settings);
            xElement.Save(xw);
            //写入文件
            xw.Flush();
            xw.Close();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = "C://";
            fileDialog.Filter = "m4s音频文件 (audio.m4s)|audio.m4s";
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = fileDialog.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = "C://";
            fileDialog.Filter = "m4s视频文件 (video.m4s)|video.m4s";
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox2.Text = fileDialog.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = "C://";
            fileDialog.Filter = "可执行文件(ffmpeg.exe)|ffmpeg.exe";
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                if(fileDialog.FileName != "")
                {
                    this.textBox3.Text = fileDialog.FileName;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string audio_file_path = textBox1.Text;
            string vedio_file_path = textBox2.Text;
            string execute_file_path = textBox3.Text;
            string filename = textBox4.Text;
            if(execute_file_path == "" || vedio_file_path == "" || audio_file_path == "" || filename== "")
            {
                toolStripStatusLabel1.Text = "存在一个或多个参数为零";
            }
            else
            {
                CreatXmlWithInfo("ffmpegLinker.config", execute_file_path,checkBox1.Checked,checkBox2.Checked,checkBox3.Checked,checkBox4.Checked);
                string targetpath = vedio_file_path.Replace("video.m4s", filename + ".mp4");
                if (!File.Exists(targetpath))
                {

                    string str = execute_file_path + " -i " + vedio_file_path + " -i " + audio_file_path + " -codec copy " + targetpath;

                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = "cmd.exe";
                    p.StartInfo.UseShellExecute = checkBox2.Checked;//是否使用操作系统shell启动
                    p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                    p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                    p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                    p.StartInfo.CreateNoWindow = checkBox3.Checked;//不显示程序窗口
                    p.Start();//启动程序

                    //向cmd窗口发送输入信息
                    p.StandardInput.WriteLine(str + "&exit");

                    p.StandardInput.AutoFlush = true;

                    p.WaitForExit(9000);//等待程序执行完退出进程
                    p.Close();

                    this.toolStripStatusLabel1.Text = "合并已完成";
                    if(checkBox4.Checked == true)
                    {
                        MessageBox.Show("文件合并已完成", "消息");
                        Application.Exit();
                    }
                }
                else
                {
                    this.toolStripStatusLabel1.Text = "文件已存在";
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if(File.Exists("ffmpegLinker.config"))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("ffmpegLinker.config");
                XmlElement rootElem = xmlDoc.DocumentElement;
                XmlNodeList executeNodes = rootElem.GetElementsByTagName("execute"); 
                
                string path = ((XmlElement)executeNodes.Item(0)).GetAttribute("path");
                if(((XmlElement)executeNodes.Item(0)).GetAttribute("copytoclip") == "true")
                {
                    checkBox1.Checked = true;
                }
                if (((XmlElement)executeNodes.Item(0)).GetAttribute("UseShellExecute") == "true")
                {
                    checkBox2.Checked = true;
                }
                if (((XmlElement)executeNodes.Item(0)).GetAttribute("CreateNoWindow") == "true")
                {
                    checkBox3.Checked = true;
                }
                if (((XmlElement)executeNodes.Item(0)).GetAttribute("exitafterend") == "true")
                {
                    checkBox4.Checked = true;
                }

                if (File.Exists(path))
                {
                    this.textBox3.Text = path;
                    this.toolStripStatusLabel1.Text = "配置文件已加载 就绪";
                }
                else
                {
                    this.toolStripStatusLabel1.Text = "配置文件存在一个或多个错误 就绪";
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string audio_file_path = textBox1.Text;
            string vedio_file_path = textBox2.Text;
            string execute_file_path = textBox3.Text;
            string filename = textBox4.Text;
            if (execute_file_path == "" || vedio_file_path == "" || audio_file_path == "" || filename == "")
            {
                toolStripStatusLabel1.Text = "存在一个或多个参数为零";
            }
            else
            {
                string targetpath = vedio_file_path.Replace("video.m4s", filename + ".mp4");
                string str = execute_file_path + " -i " + vedio_file_path + " -i " + audio_file_path + " -codec copy " + targetpath;
                textBox5.Text = str;
                if(checkBox1.Checked)
                {
                    Clipboard.SetText(str);
                    this.toolStripStatusLabel1.Text = "生成了命令行并已复制到了剪贴板";
                }
                else
                {
                    this.toolStripStatusLabel1.Text = "生成了命令行";
                }
            }

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
