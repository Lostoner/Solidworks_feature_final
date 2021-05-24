using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetFeatures
{
    public partial class Form2 : Form
    {
        public string path;
        public int cancel = 1;         //作为改变 暂停/继续 键的标志,同时也是提取特征进程的启停标志
        private System.Windows.Forms.Timer tm = new System.Windows.Forms.Timer();       //计时器
        //自动重置事件类  
        //主要用到其两个方法 WaitOne() 和 Set() , 前者阻塞当前线程，后者通知阻塞线程继续往下执行
        AutoResetEvent autoEvent = new AutoResetEvent(false);

        public Form2(string f1path)
        {
            this.path = f1path;
            InitializeComponent();
            progressBar1.Maximum = 1000;                                                               //进度条最大值
            progressBar1.Value = progressBar1.Minimum = 0;                                  //进度条最小值与当前值

            tm.Interval = 1;
            tm.Tick += new EventHandler(tm_Tick);
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        //计时器 事件
        void tm_Tick(object sender, EventArgs e)
        {
            autoEvent.Set(); //通知阻塞的线程继续执行
        }

        //开始
        private void button1_Click(object sender, EventArgs e)
        {
            tm.Start();

            Thread t2 = new Thread(new ParameterizedThreadStart(function));         //创建线程
            t2.IsBackground = true;
            t2.Start(this.path);                  //开始线程            
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.cancel = -this.cancel;
            if (this.cancel == -1)
            {
                tm.Stop();
                button2.Text = "继续";
            }
            else
            {
                tm.Start();
                button2.Text = "暂停";
            }
        }


        private delegate void DeFun(int ipos);            //委托，用于传参
        private void SetPos(int ipos)                   //委托实例化，用于调整进度条的值
        {
            if (this.progressBar1.InvokeRequired)
            {
                DeFun df = new DeFun(SetPos);
                this.Invoke(df, new object[] { ipos });
            }
            else
            {
                this.progressBar1.Value = ipos;                                                           //更改进度条的值
            }
        }

        private delegate void Daili(string ipath);
        private void SetFilePath(string ipath)
        {
            if (this.textBox1.InvokeRequired)
            {
                Daili dl = new Daili(SetFilePath);
                this.Invoke(dl, new object[] { ipath });
            }
            else
            {
                textBox1.AppendText(ipath + Environment.NewLine);                                                           //更改进度条的值
            }            
        }

        //获取文件夹中的所有文件
        private string[] getFiles(string Dirpath)
        {
            string[] files = null;
            string[] dirs = Directory.GetDirectories(Dirpath);
            if(dirs.Length == 0)
            {                
                files = Directory.GetFiles(Dirpath);                
                return files;
            }
            else
            {
                string[] fs;
                foreach(string dir in dirs)
                {                    
                    fs = getFiles(dir);
                    if (files != null)
                        files = files.Concat(fs).ToArray();
                    else
                        files = fs;
                }
                if (files != null)
                    files = files.Concat(Directory.GetFiles(Dirpath)).ToArray();
                else
                    files = Directory.GetFiles(Dirpath);
                return files;
            }            
        }

        private void function(object data)                                                               //线程所调用的函数
        {
            string file_path = data.ToString();
            //string[] files = Directory.GetFiles(file_path);
            string[] files = null;
            files = getFiles(file_path);
            int num = files.Length, cou = 0;        //用于进度条的设置               
            
            AllNodeFeatures nodFeas = new AllNodeFeatures();

            foreach (string fil in files)
            {                
                if (this.cancel == -1)                
                    autoEvent.WaitOne();  //阻塞当前线程，等待通知以继续执行                  
                else
                {
                    //Thread.CurrentThread.Suspend();                    
                }
                Debug.Print("--------------------------------------------------------------------------------------------------");
                Debug.Print(fil);
                SetFilePath(fil);
                cou++;
                OutputOneFileClass file = new OutputOneFileClass(fil);

                //ISldWorks swApp = FileClass.ConnectToSolidWorks();
                //swApp.OpenDoc(fil, (int)swDocumentTypes_e.swDocPART);
                //FileClass.TestFunction(fil);                                                                 //所调用的针对单个模型文件的操作函数，也就是后续仅需编写该函数便可。

                file.TraverseFeatureGraph(true);
                nodFeas.SaveInFixed(file);

                //file.print();
                //swApp.CloseDoc(fil);

                file.close();

                SetPos((int)((float)cou / (float)num * 1000));
            }
        }


    }
}
