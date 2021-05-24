using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json;


namespace GetFeatures
{
    public class ReturnJson1
    {     
        private List<outSketchNode1> returnSketchList = new List<outSketchNode1>();

        public List<outSketchNode1> ReturnSketchList { get => returnSketchList; set => returnSketchList = value; }
    }

    public class outSketchNode1
    {
        private List<Box> boxes = new List<Box>();
             
        public List<Box> Boxes { get => boxes; set => boxes = value; }
    }

    public class Box
    {
        //**包围盒物理属性
        private double rightUpX;          //右上角起点横坐标
        private double rightUpY;
        private double leftDownX;            //左下角起点的横坐标
        private double leftDownY;
        private double lengthAxisX;            //沿横轴走向
        private double lengthAxisY;
        private int plane;                     //描述草图位于哪一个平面——1：yOz；2：xOz；3,：xOy
        private int lineCount;               //直线的个数
        private int arcCount;                //圆弧的个数
        private int circleCount;
        //public int ellipseCount;                //椭圆的个数
        //public int parabolicCount;              //抛物线的个数
        private int splineCount;                 //样条的个数

        public double RightUpX { get => rightUpX; set => rightUpX = value; }
        public double RightUpY { get => rightUpY; set => rightUpY = value; }
        public double LeftDownX { get => leftDownX; set => leftDownX = value; }
        public double LeftDownY { get => leftDownY; set => leftDownY = value; }
        public double LengthAxisX { get => lengthAxisX; set => lengthAxisX = value; }
        public double LengthAxisY { get => lengthAxisY; set => lengthAxisY = value; }
        public int Plane { get => plane; set => plane = value; }
        public int LineCount { get => lineCount; set => lineCount = value; }
        public int ArcCount { get => arcCount; set => arcCount = value; }
        public int CircleCount { get => circleCount; set => circleCount = value; }
        public int SplineCount { get => splineCount; set => splineCount = value; }
    }

    //*************************************************************************************************

    public class NodeFeature
    {
        //Sketch内物理属性        
        private int lineCount;               //直线的个数
        private int arcCount;                //圆弧的个数        
        private int bezierCount;                 //样条的个数
        private int boxCount;                 //包围盒数量
        //Feature内属性
        private int featureDataFlag;         //特征接口编号
        private int IsPostiveFlag;          //是否 为正特征
        private int data0;    //
        private double data1;    //
        private double data2;    //
        private double data3;    //
        private double data4;    //
        private double data5;    //
        private double data6;    //
        private double data7;    //
        private double data8;    //
        private double data9;    //
        private double data10;   //
        private double data11;   //
        private double data12;   //
        private int data13;   //
        private int data14;   //
        private int data15;   //
        private int data16;   //
        private int data17;   //
        private int data18;   // 

        public NodeFeature(MyFeatureVertexNode feaAttribute, MySketchMatrix skeAttribute)
        {
            this.LineCount = skeAttribute.lineCount;
            this.ArcCount = skeAttribute.circleCount;
            this.BezierCount = skeAttribute.bezierCount;
            this.BoxCount = skeAttribute.loopCount;

            this.FeatureDataFlag = feaAttribute.featureDataFlag;
            this.IsPostiveFlag1 = feaAttribute.IsPostiveFlag;
            this.Data0 = feaAttribute.data0;
            this.Data1 = feaAttribute.data1;
            this.Data2 = feaAttribute.data2;
            this.Data3 = feaAttribute.data3;
            this.Data4 = feaAttribute.data4;
            this.Data5 = feaAttribute.data5;
            this.Data6 = feaAttribute.data6;
            this.Data7 = feaAttribute.data7;
            this.Data8 = feaAttribute.data8;
            this.Data9 = feaAttribute.data9;
            this.Data10 = feaAttribute.data10;
            this.Data11 = feaAttribute.data11;
            this.Data12 = feaAttribute.data12;
            this.Data13 = feaAttribute.data13;
            this.Data14 = feaAttribute.data14;
            this.Data15 = feaAttribute.data15;
            this.Data16 = feaAttribute.data16;
            this.Data17 = feaAttribute.data17;
            this.Data18 = feaAttribute.data18;
        }

        public int LineCount { get => lineCount; set => lineCount = value; }
        public int ArcCount { get => arcCount; set => arcCount = value; }
        public int BezierCount { get => bezierCount; set => bezierCount = value; }
        public int BoxCount { get => boxCount; set => boxCount = value; }
        public int FeatureDataFlag { get => featureDataFlag; set => featureDataFlag = value; }
        public int IsPostiveFlag1 { get => IsPostiveFlag; set => IsPostiveFlag = value; }
        public int Data0 { get => data0; set => data0 = value; }
        public double Data1 { get => data1; set => data1 = value; }
        public double Data2 { get => data2; set => data2 = value; }
        public double Data3 { get => data3; set => data3 = value; }
        public double Data4 { get => data4; set => data4 = value; }
        public double Data5 { get => data5; set => data5 = value; }
        public double Data6 { get => data6; set => data6 = value; }
        public double Data7 { get => data7; set => data7 = value; }
        public double Data8 { get => data8; set => data8 = value; }
        public double Data9 { get => data9; set => data9 = value; }
        public double Data10 { get => data10; set => data10 = value; }
        public double Data11 { get => data11; set => data11 = value; }
        public double Data12 { get => data12; set => data12 = value; }
        public int Data13 { get => data13; set => data13 = value; }
        public int Data14 { get => data14; set => data14 = value; }
        public int Data15 { get => data15; set => data15 = value; }
        public int Data16 { get => data16; set => data16 = value; }
        public int Data17 { get => data17; set => data17 = value; }
        public int Data18 { get => data18; set => data18 = value; }
    }

    class AllNodeFeatures
    {
        private List<NodeFeature> allFile_NodeFeatures;


        //对单个零件文件数据进行存储,并进行json输出——不定长存储
        public void SavInIndefinite(OutputOneFileClass oneFile)
        {
            ReturnJson1 RJ = new ReturnJson1();
            foreach (MySketchMatrix SM in oneFile.curSketchList)
            {
                outSketchNode1 outSM = new outSketchNode1();
                foreach (LoopClass lc in SM.loopList)
                {
                    Box boundingBox = new Box()
                    {
                        RightUpX = lc.rightUpX,
                        RightUpY = lc.rightUpY,
                        LeftDownX = lc.leftDownX,
                        LeftDownY = lc.leftDownY,
                        LengthAxisX = lc.lengthAxisX,
                        LengthAxisY = lc.lengthAxisY,
                        Plane = lc.plane,
                        LineCount = lc.lineCount,
                        ArcCount = lc.arcCount,
                        CircleCount = lc.circleCount,
                        SplineCount = lc.splineCount
                    };
                    outSM.Boxes.Add(boundingBox);
                }
                RJ.ReturnSketchList.Add(outSM);
            }

            string json = JsonConvert.SerializeObject(RJ, Formatting.Indented);

            //outputText(json);
        }
        //对单个零件文件数据进行存储,并进行输出——定长存储
        public void SaveInFixed(OutputOneFileClass oneFile)
        {
            int index;
            
            foreach (MyFeatureVertexNode curFeaNode in oneFile.curFeatureGraph.AdjList)
            {
                index = curFeaNode.sketchsIndex[0];
                NodeFeature nF = new NodeFeature(curFeaNode, oneFile.curSketchList[index]);
                AllFile_NodeFeatures.Add(nF);
            }
        }

        /// <summary>
        /// 检查是否存在文件夹
        /// </summary>
        private void change()
        {
            //string path = Application.StartupPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\TextMessage.txt";
            string directory = "E:\\Users\\Cao-silver";
            string path = "E:\\Users\\Cao-silver\\OutputFile.txt";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            if (!File.Exists(path))
            {
                FileStream fs = File.Create(path);
                fs.Close();
            }
            else
            {
                FileInfo fInfo = new FileInfo(path);
                fInfo.Delete();
                FileStream fs = File.Create(path);
                fs.Close();
            }
        }
        /// <summary>
        /// 写入文本bai文件
        /// </summary>
        /// <param name="value"></param>
        private void outputText(string value)
        {
            change();
            //string path = Application.StartupPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\TextMessage.txt";
            string path = "E:\\Users\\Cao-silver\\OutputFile.txt";
            FileStream f = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(f);
            sw.WriteLine(value);
            sw.Flush();
            sw.Close();
            f.Close();
        }        
        

        public List<NodeFeature> AllFile_NodeFeatures { get => allFile_NodeFeatures; set => allFile_NodeFeatures = value; }
    }

}
