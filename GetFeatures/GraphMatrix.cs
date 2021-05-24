using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GetFeatures.FeatureAttributes;

namespace GetFeatures
{
    ////顶点表结点类
    public class MyFeatureVertexNode
    {
        class LinearPatternTypeError : Exception
        {
            public LinearPatternTypeError(string message)
            {
            }
        }

        static void LinearThrow(string type)
        {
            if (type == "LocalLPattern")
                throw new LinearPatternTypeError("线性特征类型不支持。");
        }

        class CircularPatternTypeError : Exception
        {
            public CircularPatternTypeError(string message)
            {
            }
        }

        static void CircularThrow(string type)
        {
            if (type == "LocalCirPattern")
                throw new CircularPatternTypeError("圆周特征类型不支持。");
        }

        class MirrorPatternTypeError : Exception
        {
            public MirrorPatternTypeError(string message)
            {
            }
        }

        static void MirrorThrow(string type)
        {
            if(type == "MirrorStock")
            throw new MirrorPatternTypeError("镜像特征类型不支持。");
        }

        class FilletTypeError : Exception
        {
            public FilletTypeError(string message)
            {
            }
        }

        static void FilletThrow(string type)
        {
            if(type == "VarFillet")
            throw new FilletTypeError("圆角特征类型不支持。");
        }

        public Feature fea;
        public string feaType;

        public int featureDataFlag;
        public int IsPostiveFlag;

        public int sketchsIndex;
        public int selfIndex;
        public List<int> fathersIndex;
        public List<int> sonsIndex;
        public List<int> treeFatherIndex;

        public string[] strs;

        public ExtrudeV extrudeV;
        public BoundaryBossV boundaryBossV;
        public LoftV loftV;
        public SweepV sweepV;
        public RevolveV revolveV;
        public LPattern LPatternV;
        public CPattern CPatternV;
        public MPattern MPatternV;
        public SFillet SFilletV;
        public Chamfer ChamferV;


        public MyFeatureVertexNode(Feature fea)
        {
            this.fea = fea;
            this.feaType = fea.GetTypeName();

            this.featureDataFlag = -1;
            this.IsPostiveFlag = 1;
           

            sketchsIndex = -1;
            fathersIndex = new List<int>();
            sonsIndex = new List<int>();

            strs = new string[71];

            this.extrudeV = new ExtrudeV();
            this.boundaryBossV = new BoundaryBossV();
            this.loftV = new LoftV();
            this.sweepV = new SweepV();
            this.revolveV = new RevolveV();
            this.LPatternV = new LPattern();
            this.CPatternV = new CPattern();
            this.MPatternV = new MPattern();
            this.SFilletV = new SFillet();
            this.ChamferV = new Chamfer();
        }  
  
        //对特征接口进行判断，调用相应提取函数
        public void JudgeAndToGet_FeatureDefinition(ModelDoc2 swModel)
        {
            ExtrudeFeatureData2 swExtrudeFeatureData;
            BoundaryBossFeatureData swBoundaryBossFeatureData;
            LoftFeatureData swLoftFeatureData;
            SweepFeatureData swSweepFeatureData;
            RevolveFeatureData2 swRevolveFeatureData;
            LinearPatternFeatureData swLinearPatternFeatureData;
            CircularPatternFeatureData swCircularPatternFeatureData;
            IMirrorPatternFeatureData swMirrorPatternFeatureData;
            ISimpleFilletFeatureData2 swFilletFeatureData;
            IChamferFeatureData2 swChamferFeatureData;

            //SimpleFilletFeatureData2 swSimpleFilletFeatureData = default(SimpleFilletFeatureData2);
            //ChamferFeatureData2 swChamferFeatureData = default(ChamferFeatureData2);

            if (feaType == "BaseBody" || feaType == "Boss" || feaType == "BossThin" ||
                feaType == "Cut" || feaType == "CutThin" || feaType == "Extrusion")
            {
                swExtrudeFeatureData = (ExtrudeFeatureData2)this.fea.GetDefinition();
                this.featureDataFlag = 1;
                
                this.extrudeV = new ExtrudeV(swExtrudeFeatureData);                

                if (feaType == "Cut" || feaType == "CutThin")
                    this.IsPostiveFlag = -1;
            }
            else if (feaType == "NetBlend")
            {
                swBoundaryBossFeatureData = (BoundaryBossFeatureData)this.fea.GetDefinition();
                this.featureDataFlag = 2;
                
                this.boundaryBossV = new BoundaryBossV(swBoundaryBossFeatureData);                
            }
            else if (feaType == "Blend" || feaType == "BlendCut")
            {
                swLoftFeatureData = (LoftFeatureData)this.fea.GetDefinition();
                this.featureDataFlag = 3;
                
                this.loftV = new LoftV(swLoftFeatureData);
                
                if (feaType == "BlendCut")
                    this.IsPostiveFlag = -1;
            }
            else if (feaType == "Sweep" || feaType == "SweepCut")
            {
                swSweepFeatureData = (SweepFeatureData)this.fea.GetDefinition();
                this.featureDataFlag = 4;
                this.sweepV = new SweepV(swSweepFeatureData);                
                if (feaType == "SweepCut")
                    this.IsPostiveFlag = -1;
            }
            else if (feaType == "RevCut" || feaType == "Revolution" || feaType == "RevolutionThin")
            {
                swRevolveFeatureData = (RevolveFeatureData2)this.fea.GetDefinition();
                this.featureDataFlag = 5;
                this.revolveV = new RevolveV(swRevolveFeatureData, swModel);                
                if (feaType == "RevCut")
                    this.IsPostiveFlag = -1;
            }
            else if(feaType == "LPattern" || feaType == "LocalLPattern")
            {
                try
                {
                    LinearThrow(feaType);
                }
                catch(LinearPatternTypeError LE)
                {
                    System.Console.WriteLine(LE.ToString());
                    MessageBox.Show(LE.ToString());
                    return;
                }
                swLinearPatternFeatureData = (LinearPatternFeatureData)this.fea.GetDefinition();
                this.featureDataFlag = 6;
                this.LPatternV = new LPattern(swLinearPatternFeatureData, swModel);
            }
            else if(feaType == "CirPattern" || feaType == "LocalCirPattern")
            {
                try
                {
                    CircularThrow(feaType);
                }
                catch (CircularPatternTypeError CE)
                {
                    System.Console.WriteLine(CE.ToString());
                    MessageBox.Show(CE.ToString());
                    return;
                }
                swCircularPatternFeatureData = (CircularPatternFeatureData)this.fea.GetDefinition();
                this.featureDataFlag = 7;
                this.CPatternV = new CPattern(swCircularPatternFeatureData, swModel);
            }
            else if(feaType == "MirrorPattern" || feaType == "MirrorSolid" || feaType == "MirrorStock")
            {
                try
                {
                    MirrorThrow(feaType);
                }
                catch (MirrorPatternTypeError ME)
                {
                    System.Console.WriteLine(ME.ToString());
                    MessageBox.Show(ME.ToString());
                    return;
                }
                //swMirrorPatternFeatureData = (MirrorPatternFeatureData)this.fea.GetDefinition();
                
                if(this.fea.GetTypeName() == "MirrorSolid")
                {
                    this.MPatternV = new MPattern((IMirrorSolidFeatureData)this.fea.GetDefinition(), swModel, feaType);
                }
                else if(this.fea.GetTypeName() == "MirrorPattern")
                {
                    this.MPatternV = new MPattern((IMirrorPatternFeatureData)this.fea.GetDefinition(), swModel, feaType);
                }
                this.featureDataFlag = 8;
            }
            else if(feaType == "Round fillet corner" || feaType == "Fillet" || feaType == "VarFillet")
            {
                /*
                try
                {
                    FilletThrow(feaType);
                }
                catch (FilletTypeError FE)
                {
                    System.Console.WriteLine(FE.ToString());
                    MessageBox.Show(FE.ToString());
                    return;
                }
                */
                if(feaType == "VarFillet")
                {
                    this.SFilletV = new SFillet((IVariableFilletFeatureData2)this.fea.GetDefinition(), swModel);
                }
                else
                {
                    this.SFilletV = new SFillet((ISimpleFilletFeatureData2)this.fea.GetDefinition(), swModel);
                }
                this.featureDataFlag = 9;
            }
            else if(feaType == "Chamfer")
            {
                swChamferFeatureData = (IChamferFeatureData2)this.fea.GetDefinition();
                this.featureDataFlag = 10;
                this.ChamferV = new Chamfer(swChamferFeatureData, swModel);
            }
            /*else if (feaType == "LPattern")
            {
                swLinearPatternFeatureData = (LinearPatternFeatureData)this.fea.GetDefinition();
                this.featureDataFlag = 6;
                
            }
            else if (feaType == "CirPattern")
            {
                swCircularPatternFeatureData = (CircularPatternFeatureData)this.fea.GetDefinition();
                this.featureDataFlag = 7;
                
            }
            else if (feaType == "MirrorPattern")
            {
                swMirrorPatternFeatureData = (MirrorPatternFeatureData)this.fea.GetDefinition();
                this.featureDataFlag = 8;
                
            }
            else if(feaType == "Fillet" || feaType == "Round fillet corner")
            {
                swSimpleFilletFeatureData = (SimpleFilletFeatureData2)this.fea.GetDefinition();
                this.featureDataFlag = 9;
            }
            else if (feaType == "Chamfer")
            {
                swChamferFeatureData = (ChamferFeatureData2)this.fea.GetDefinition();
                this.featureDataFlag = 10;
            }  */
            else
                Debug.WriteLine("\n未设置该类特征的提取函数！！！\n");
        }
        
        //获取每个特征分类的属性字符串组
        private string[] getExtrudeV_StringArray()
        {
            string strData0 = this.extrudeV.data0.ToString();
            string strData1 = this.extrudeV.data1.ToString();
            string strData2 = this.extrudeV.data2.ToString();
            string strData3 = this.extrudeV.data3.ToString();
            string strData4 = this.extrudeV.data4.ToString();
            string strData5 = this.extrudeV.data5.ToString();
            string strData6 = this.extrudeV.data6.ToString();
            string strData7 = this.extrudeV.data7.ToString();
            string strData8 = this.extrudeV.data8.ToString();

            string[] extrudeArr = { strData0, strData1, strData2, strData3, strData4, strData5, strData6, strData7,
                strData8 };
            return extrudeArr;
        }
        private string[] getBoundaryBossV_StringArray()
        {
            string strData0 = this.boundaryBossV.data0.ToString();
            string strData1 = this.boundaryBossV.data1.ToString();
            string strData2 = this.boundaryBossV.data2.ToString();
            string strData3 = this.boundaryBossV.data3.ToString();
            string strData4 = this.boundaryBossV.data4.ToString();
            string strData5 = this.boundaryBossV.data5.ToString();
            string strData6 = this.boundaryBossV.data6.ToString();
            string strData7 = this.boundaryBossV.data7.ToString();
            string strData8 = this.boundaryBossV.data8.ToString();
            string strData9 = this.boundaryBossV.data9.ToString();
            string strData10 = this.boundaryBossV.data10.ToString();
            string strData11 = this.boundaryBossV.data11.ToString();
            string strData12 = this.boundaryBossV.data12.ToString();
            string strData13 = this.boundaryBossV.data13.ToString();
            string strData14 = this.boundaryBossV.data14.ToString();

            string[] boundaryBossArr = { strData0, strData1, strData2, strData3, strData4, strData5, strData6, strData7,
                strData8, strData9, strData10, strData11, strData12, strData13, strData14};
            return boundaryBossArr;
        }
        private string[] getLoftV_StringArray()
        {
            string strData0 = this.loftV.data0.ToString();
            string strData1 = this.loftV.data1.ToString();
            string strData2 = this.loftV.data2.ToString();
            string strData3 = this.loftV.data3.ToString();
            string strData4 = this.loftV.data4.ToString();
            string strData5 = this.loftV.data5.ToString();
            string strData6 = this.loftV.data6.ToString();
            string strData7 = this.loftV.data7.ToString();
            string strData8 = this.loftV.data8.ToString();
            string strData9 = this.loftV.data9.ToString();
            string strData10 = this.loftV.data10.ToString();
            string strData11 = this.loftV.data11.ToString();
            string strData12 = this.loftV.data12.ToString();

            string[] loftArr = { strData0, strData1, strData2, strData3, strData4, strData5, strData6, strData7,
                strData8, strData9, strData10, strData11, strData12 };
            return loftArr;
        }
        private string[] getSweepV_StringArray()
        {
            string strData0 = this.sweepV.data0.ToString();
            string strData1 = this.sweepV.data1.ToString();
            string strData2 = this.sweepV.data2.ToString();
            string strData3 = this.sweepV.data3.ToString();
            string strData4 = this.sweepV.data4.ToString();
            string strData5 = this.sweepV.data5.ToString();
            string strData6 = this.sweepV.data6.ToString();
            string strData7 = this.sweepV.data7.ToString();
            string strData8 = this.sweepV.data8.ToString();
            string strData9 = this.sweepV.data9.ToString();
            string strData10 = this.sweepV.data10.ToString();
            string strData11 = this.sweepV.data11.ToString();
            string strData12 = this.sweepV.data12.ToString();
            string strData13 = this.sweepV.data13.ToString();
            string strData14 = this.sweepV.data14.ToString();
            string strData15 = this.sweepV.data15.ToString();
            string strData16 = this.sweepV.data16.ToString();
            string strData17 = this.sweepV.data17.ToString();
            string strData18 = this.sweepV.data18.ToString();

            string[] sweepArr = { strData0, strData1, strData2, strData3, strData4, strData5, strData6, strData7,
                strData8, strData9, strData10, strData11, strData12, strData13, strData14, strData15,
                strData16, strData17, strData18 };
            return sweepArr;
        }
        private string[] getRevolveV_StringArray()
        {
            string strData0 = this.revolveV.data0.ToString();
            string strData1 = this.revolveV.data1.ToString();
            string strData2 = this.revolveV.data2.ToString();
            string strData3 = this.revolveV.data3.ToString();
            string strData4 = this.revolveV.data4.ToString();
            string strData5 = this.revolveV.data5.ToString();
            string strData6 = this.revolveV.data6.ToString();
            string strData7 = this.revolveV.data7.ToString();
            string strData8 = this.revolveV.data8.ToString();
            string strData9 = this.revolveV.data9.ToString();
            string strData10 = this.revolveV.data10.ToString();

            string[] revolveArr = { strData0, strData1, strData2, strData3, strData4, strData5, strData6, strData7, 
                strData8, strData9, strData10 };
            return revolveArr;
        }

        //赋值属性字符串组，方便测试 表格输出
        //对 strs 赋值
        public void setAttribute_StringArrays()
        {
            string strFeatureDataFlag = this.featureDataFlag.ToString();
            string strIsPostiveFlag = this.IsPostiveFlag.ToString();            

            string[] sameAttr = { strFeatureDataFlag, strIsPostiveFlag };

            string[] extrudeStr = getExtrudeV_StringArray();
            string[] boundaryBossStr = getBoundaryBossV_StringArray();
            string[] LoftStr = getLoftV_StringArray();
            string[] SweepStr = getSweepV_StringArray();
            string[] RevolveStr = getRevolveV_StringArray();

            this.strs = sameAttr.Concat(extrudeStr).ToArray();
            this.strs = this.strs.Concat(boundaryBossStr).ToArray();
            this.strs = this.strs.Concat(LoftStr).ToArray();
            this.strs = this.strs.Concat(SweepStr).ToArray();
            this.strs = this.strs.Concat(RevolveStr).ToArray();
        }        

    }

    

    //图类
    public class ALGraph
    {
        public List<MyFeatureVertexNode> AdjList;//特征节点表
        public int VertexNodeCount;//顶点数
        public int BordeCount;//边数

        public int[] ColumnLens;//描述特征图表格每一维度的最大长度，用于Debug打印

        public ALGraph()
        {
            VertexNodeCount = 0;
            BordeCount = 0;
            AdjList = new List<MyFeatureVertexNode>();
            ColumnLens = null;
        }

        public void initColumnLens()
        {
            this.ColumnLens = new int[AdjList[0].strs.Length];            
        }        
        // 定义 特征图表格 每一条目的最大长度——对 ColumnLens 进行赋值        
        public void setColumnLens()
        {
            initColumnLens();
            foreach (MyFeatureVertexNode feaNode in AdjList)
            {
                for (int i = 0; i < feaNode.strs.Length; i++)
                {
                    if (feaNode.strs[i].Length < 5)
                    {

                    }
                    else if (feaNode.strs[i].Length < 10)
                    {
                        if (ColumnLens[i] < 1)
                            ColumnLens[i] = 1;
                    }
                    else if (feaNode.strs[i].Length < 21)
                    {
                        if (ColumnLens[i] < 2)
                            ColumnLens[i] = 2;
                    }
                    else
                    {
                        if (ColumnLens[i] < 3)
                            ColumnLens[i] = 3;
                    }
                }
            }

        }

        //有序-输出所有特征图节点的特征属性
        public void testGraph_NodeFeatures()
        {
            for (int i = 0; i < AdjList.Count; i++)            
                AdjList[i].setAttribute_StringArrays();
            
            setColumnLens();
            for (int i = 0; i < AdjList.Count; i++)
            {             
                int k = 3;                
                string[] strs = AdjList[i].strs;
                string name = AdjList[i].fea.Name.PadRight(17, ' ');
                Debug.Write("[" + Form2.graph_ID + "] ");
                Debug.Write(name);
                for(int j =0; j < strs.Length; j++)
                {
                    if (ColumnLens[j] == 0)
                    {
                        string strpad = strs[j].PadLeft(6, ' ');
                        Debug.Write(strpad + " [" + (k++) + "]");
                    }
                    else if (ColumnLens[j] == 1)
                    {
                        string strpad = strs[j].PadLeft(10, ' ');
                        Debug.Write(strpad + " [" + (k++) + "]");
                    }
                    else if (ColumnLens[j] == 2)
                    {
                        string strpad = strs[j].PadLeft(24, ' ');
                        Debug.Write(strpad + " [" + (k++) + "]");
                    }
                    else
                    {
                        string strpad = strs[j].PadLeft(26, ' ');
                        Debug.Write(strpad + " [" + (k++) + "]");
                    }
                }                
                Debug.Write("\n");                  
            }
        }
    }
}

