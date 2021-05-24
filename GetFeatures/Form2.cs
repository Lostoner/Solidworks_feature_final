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
        //public static string dir = "E:\\Users\\Cao-silver\\z-MyDataSet";
        //public static string dir = "D:\\MyDataSet";
        public static string dir = "D:\\MyDataSet";
        
        public static string path_graph_indicator = dir + "\\graph_indicator.txt";
        public static string path_A = dir + "\\A.txt";
        public static string path_graph_label = dir + "\\graph_labels.txt";
        public static string path_node_attributes = dir + "\\node_attributes.txt";
        public static string path_Tree = dir + "\\Tree.txt";

        public static int graph_ID = 0;
        public static int node_ID = 1;
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
            //string[] files = getFiles(file_path);
            
            //获取目录内包括子目录内，所有文件
            var files = Directory.GetFiles(file_path, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".sldprt") || s.EndsWith(".SLDPRT"));
            int num = files.Count(), cou = 0;        //用于进度条的设置            

            DataFiles DataSet = new DataFiles(Form2.dir);

            change();
            //获取现有文件的最大node__TD，Graph_ID
            getAdvanceNodeID();
            getAdvanceGraphID();
            

            foreach (string fil in files)
            {
                string[] resAry = fil.Split(new string[] { "\\" }, StringSplitOptions.None);
                string res = resAry[resAry.Length - 1];
                if (res[0] == '~' || res[1] == '$')
                {
                    Debug.WriteLine("\n找到一个副本文件！！！");
                    continue;
                }
                    
                if (this.cancel == -1)                
                    autoEvent.WaitOne();  //阻塞当前线程，等待通知以继续执行                  
                else
                {
                    //Thread.CurrentThread.Suspend();                    
                }
                Debug.Print("--------------------------------------------------------------------------------------------------");
                Debug.Print(res);
                Debug.Print(fil);
                Debug.Print("--------------------------------------------------------------------------------------------------");
                SetFilePath(fil);
                cou++;
                
                OutputOneFileClass file = new OutputOneFileClass(fil);
                
                //file.hasImported(true);
                //file.hasImported2(true);

                //待定//ISldWorks swApp = FileClass.ConnectToSolidWorks();
                //待定//swApp.OpenDoc(fil, (int)swDocumentTypes_e.swDocPART);
                //待定//FileClass.TestFunction(fil);                                                                 //所调用的针对单个模型文件的操作函数，也就是后续仅需编写该函数便可。

                graph_ID++;

                //写入txt的方法↓——边关系、图标签；节点图索引
                file.TraverseFeatureGraph(true, false);
                file.writeToTxt_GraphLabel(path_graph_label);

                //写入txt的方法↓——节点属性
                DataSet.Node_Features_SaveOutputFixed(file);

                //待定//file.print();
                //待定//swApp.CloseDoc(fil);

                //TODO:输出

                file.close();

                SetPos((int)((float)cou / (float)num * 1000));
            }
            SetFilePath("\n\n终于，全部搞定了——应提文件数：" + num + "，获取文件数：" + cou);
            Debug.WriteLine("\n所有sldprt文件都提取完了，开不开心！！！~~~\n");
            Debug.WriteLine("应提文件数：" + num + "，获取文件数：" + cou);
        }

        /// <summary>
        /// 检查是否存在文件夹,如不存在创建文件夹
        /// 同时检查文件夹中相应文件是否存在，若不存在，则创建文件
        /// </summary>
        public void change()
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (!File.Exists(path_node_attributes))
            {
                FileStream fs = File.Create(path_node_attributes);
                fs.Close();
            }
            if (!File.Exists(path_graph_indicator))
            {
                FileStream fs = File.Create(path_graph_indicator);
                fs.Close();
            }
            if (!File.Exists(path_graph_label))
            {
                FileStream fs = File.Create(path_graph_label);
                fs.Close();
            }
            if (!File.Exists(path_A))
            {
                FileStream fs = File.Create(path_A);
                fs.Close();
            }
        }

        //此方法用于获取数据集最大节点编号，注意！！！，有一个bug：若是出现数据集最后一个零件只有单特征（则没有边关系），
        //则会导致错过一位节点序号，操作时须谨慎，以防数据集混乱
        //最初初始化：node_ID = 1，graph_ID = 0
        void getAdvanceNodeID()
        {
            FileStream file = new FileStream(path_A, FileMode.Open);
            //file.Seek(0, SeekOrigin.Begin);
            if (file.Length > 0)
            {
                StreamReader sr = new StreamReader(file, Encoding.Default);
                string data = sr.ReadToEnd();
                string[] resAry = data.Split(new string[] { ",", "\n" }, StringSplitOptions.None);
                string res = resAry[resAry.Length - 3];
                node_ID = Convert.ToInt32(res) + 1;
                //node_ID = data[data.Length - 2] - '0';                
                sr.Close();
            }
            Debug.WriteLine("node_ID：" + node_ID);

            file.Close();
        }
        void getAdvanceGraphID()
        {
            FileStream file = new FileStream(path_graph_indicator, FileMode.Open);
            //file.Seek(0, SeekOrigin.Begin);
            if (file.Length > 0)
            {
                StreamReader sr = new StreamReader(file, Encoding.Default);
                string data = sr.ReadToEnd();
                string[] resAry = data.Split(new string[] { "\n" }, StringSplitOptions.None);
                string res = resAry[resAry.Length - 2];
                graph_ID = Convert.ToInt32(res);
                //node_ID = data[data.Length - 2] - '0';                
                sr.Close();
            }
            Debug.WriteLine("Graph_ID：" + graph_ID);

            file.Close();
        }

    }
}
