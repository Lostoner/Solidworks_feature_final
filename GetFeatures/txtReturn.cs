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
using GetFeatures.FeatureAttributes;

namespace GetFeatures
{   
    public class NodeAttributes
    {
        //Sketch内物理属性        
        public int lineCount;               //直线的个数
        public int arcCount;                //圆弧的个数        
        public int bezierCount;                 //样条的个数        

        //Feature内属性
        public int featureDataFlag;         //特征接口编号
        public int isPostiveFlag;          //是否 为正特征        

        public ExtrudeV extrudeV;
        public BoundaryBossV boundaryBossV;
        public LoftV loftV;
        public SweepV sweepV;
        public RevolveV revolveV;
        public LPattern lPatternV;
        public CPattern cPatternV;
        public MPattern mPatternV;
        public SFillet sFilletV;
        public Chamfer chamferV;

        public NodeAttributes(MyFeatureVertexNode feaAttribute, MySketchMatrix skeAttribute)
        {
            this.lineCount = skeAttribute.lineCount;
            this.arcCount = skeAttribute.circleCount;
            this.bezierCount = skeAttribute.bezierCount;            

            this.featureDataFlag = feaAttribute.featureDataFlag;
            this.isPostiveFlag = feaAttribute.IsPostiveFlag;            

            this.extrudeV = feaAttribute.extrudeV;
            this.boundaryBossV = feaAttribute.boundaryBossV;
            this.loftV = feaAttribute.loftV;
            this.sweepV = feaAttribute.sweepV;
            this.revolveV = feaAttribute.revolveV;
            this.lPatternV = feaAttribute.LPatternV;
            this.cPatternV = feaAttribute.CPatternV;
            this.mPatternV = feaAttribute.MPatternV;
            this.sFilletV = feaAttribute.SFilletV;
            this.chamferV = feaAttribute.ChamferV;
        }
        public NodeAttributes(MyFeatureVertexNode feaAttribute)
        {
            this.lineCount = 0;
            this.arcCount = 0;
            this.bezierCount = 0;

            this.featureDataFlag = feaAttribute.featureDataFlag;
            this.isPostiveFlag = feaAttribute.IsPostiveFlag;            

            this.extrudeV = feaAttribute.extrudeV;
            this.boundaryBossV = feaAttribute.boundaryBossV;
            this.loftV = feaAttribute.loftV;
            this.sweepV = feaAttribute.sweepV;
            this.revolveV = feaAttribute.revolveV;
            this.lPatternV = feaAttribute.LPatternV;
            this.cPatternV = feaAttribute.CPatternV;
            this.mPatternV = feaAttribute.MPatternV;
            this.sFilletV = feaAttribute.SFilletV;
            this.chamferV = feaAttribute.ChamferV;
        }        
    }

    class DataFiles
    {
        private string directory;
        private string path_node_attributes;
        public List<NodeAttributes> allFiles_Node_Attributes;

        public DataFiles(string dir)
        {
            this.directory = dir;
            this.path_node_attributes = dir + "\\node_attributes.txt";
            this.allFiles_Node_Attributes = new List<NodeAttributes>();
        }        
                
        //对单个零件文件数据进行存储,并进行输出——定长存储
        public void Node_Features_SaveOutputFixed(OutputOneFileClass oneFile)
        {
            int index;
            Debug.WriteLine("\n正在准备存储节点属性并输出——");
            foreach (MyFeatureVertexNode curFeaNode in oneFile.curFeatureGraph.AdjList)
            {
                int i = 1;
                NodeAttributes nA = null;
                index = curFeaNode.sketchsIndex;
                if (index != -1)
                    nA = new NodeAttributes(curFeaNode, oneFile.curSketchList[index]);
                else
                    nA = new NodeAttributes(curFeaNode);
                allFiles_Node_Attributes.Add(nA);

                Debug.Write("[ " + i++ + " ] ");
                //writeToTxt_Node_Attributes(path_node_attributes, nA);
                writeToText_Node_Attributes2(path_node_attributes, nA);
            }
        }

        /// <summary>
        /// 节点属性 写入文本文件 !!! 1号暂时废弃，之后修改!!!
        /// </summary>
        /// <param name="path">文件绝对路径</param>
        /// <param name="na">节点属性存储类</param>
        private void writeToTxt_Node_Attributes(string path, NodeAttributes na)
        {
            string PATH = path;
            string content = "";
/*
            content = na.lineCount + ",  " + na.arcCount + ",  " + na.bezierCount + ",  " + na.featureDataFlag + ",  " + 
                na.IsPostiveFlag + ",  " + na.data0 + ",  " + na.data1 + ",  " + na.data2 + ",  " + na.data3 + ",  " + na.data4 + ",  " + 
                na.data5 + ",  " + na.data6 + ",  " + na.data7 + ",  " + na.data8 + ",  " + na.data9 + ",  " + na.data10 + ",  " + 
                na.data11 + ",  " + na.data12 + ",  " + na.data13 + ",  " + na.data14 + ",  " + na.data15 + ",  " + na.data16 + ",  " + 
                na.data17 + ",  " + na.data18;
*/
            //File.AppendAllText(PATH, content);
                                    
            FileStream fs = new FileStream(PATH, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(content);
            sw.Flush();
            sw.Close();
            fs.Close();
        }     
        
        public void writeToText_Node_Attributes2(string path, NodeAttributes na)
        {
            string PATH = path;
            Debug.Print("[path]: " + PATH);
            string content = na.lineCount + ",  " +
                na.arcCount + ",  " +
                na.bezierCount + ",  " +
                na.featureDataFlag + ",  " +
                na.isPostiveFlag + ",  " +
                na.extrudeV.data0 + ",  " +
                na.extrudeV.data1 + ",  " +
                na.extrudeV.data2 + ",  " +
                na.extrudeV.data3 + ",  " +
                na.extrudeV.data4 + ",  " +
                na.extrudeV.data5 + ",  " +
                na.extrudeV.data6 + ",  " +
                na.extrudeV.data7 + ",  " +
                na.extrudeV.data8 + ",  " +
                na.boundaryBossV.data0 + ",  " +
                na.boundaryBossV.data1 + ",  " +
                na.boundaryBossV.data2 + ",  " +
                na.boundaryBossV.data3 + ",  " +
                na.boundaryBossV.data4 + ",  " +
                na.boundaryBossV.data5 + ",  " +
                na.boundaryBossV.data6 + ",  " +
                na.boundaryBossV.data7 + ",  " +
                na.boundaryBossV.data8 + ",  " +
                na.boundaryBossV.data9 + ",  " +
                na.boundaryBossV.data10 + ",  " +
                na.boundaryBossV.data11 + ",  " +
                na.boundaryBossV.data12 + ",  " +
                na.boundaryBossV.data13 + ",  " +
                na.boundaryBossV.data14 + ",  " +
                na.loftV.data0 + ",  " +
                na.loftV.data1 + ",  " +
                na.loftV.data2 + ",  " +
                na.loftV.data3 + ",  " +
                na.loftV.data4 + ",  " +
                na.loftV.data5 + ",  " +
                na.loftV.data6 + ",  " +
                na.loftV.data7 + ",  " +
                na.loftV.data8 + ",  " +
                na.loftV.data9 + ",  " +
                na.loftV.data10 + ",  " +
                na.loftV.data11 + ",  " +
                na.loftV.data12 + ",  " +
                na.sweepV.data0 + ",  " +
                na.sweepV.data1 + ",  " +
                na.sweepV.data2 + ",  " +
                na.sweepV.data3 + ",  " +
                na.sweepV.data4 + ",  " +
                na.sweepV.data5 + ",  " +
                na.sweepV.data6 + ",  " +
                na.sweepV.data7 + ",  " +
                na.sweepV.data8 + ",  " +
                na.sweepV.data9 + ",  " +
                na.sweepV.data10 + ",  " +
                na.sweepV.data11 + ",  " +
                na.sweepV.data12 + ",  " +
                na.sweepV.data13 + ",  " +
                na.sweepV.data14 + ",  " +
                na.sweepV.data15 + ",  " +
                na.sweepV.data16 + ",  " +
                na.sweepV.data17 + ",  " +
                na.sweepV.data18 + ",  " +
                na.revolveV.data0 + ",  " +
                na.revolveV.data1 + ",  " +
                na.revolveV.data2 + ",  " +
                na.revolveV.data3 + ",  " +
                na.revolveV.data4 + ",  " +
                na.revolveV.data5 + ",  " +
                na.revolveV.data6 + ",  " +
                na.revolveV.data7 + ",  " +
                na.revolveV.data8 + ",  " +
                na.revolveV.data9 + ",  " +
                na.revolveV.data10 + ",  " +
                na.lPatternV.data0 + ",  " +
                na.lPatternV.data1 + ",  " +
                na.lPatternV.data2 + ",  " +
                na.lPatternV.data3 + ",  " +
                na.lPatternV.data4 + ",  " +
                na.lPatternV.data5 + ",  " +
                na.lPatternV.data6 + ",  " +
                na.lPatternV.data7 + ",  " +
                na.lPatternV.data8 + ",  " +
                na.lPatternV.data9 + ",  " +
                na.lPatternV.data10 + ",  " +
                na.lPatternV.data11 + ",  " +
                na.lPatternV.data12 + ",  " +
                na.lPatternV.data13 + ",  " +
                na.lPatternV.data14 + ",  " +
                na.lPatternV.data15 + ",  " +
                na.lPatternV.data16 + ",  " +
                na.cPatternV.data0 + ",  " +
                na.cPatternV.data1 + ",  " +
                na.cPatternV.data2 + ",  " +
                na.cPatternV.data3 + ",  " +
                na.cPatternV.data4 + ",  " +
                na.cPatternV.data5 + ",  " +
                na.cPatternV.data6 + ",  " +
                na.cPatternV.data7 + ",  " +
                na.cPatternV.data8 + ",  " +
                na.cPatternV.data9 + ",  " +
                na.cPatternV.data10 + ",  " +
                na.cPatternV.data11 + ",  " +
                na.cPatternV.data12 + ",  " +
                na.cPatternV.data13 + ",  " +
                na.cPatternV.data14 + ",  " +
                na.mPatternV.data0 + ",  " +
                na.mPatternV.data1 + ",  " +
                na.mPatternV.data2 + ",  " +
                na.mPatternV.data3 + ",  " +
                na.mPatternV.data4 + ",  " +
                na.mPatternV.data5 + ",  " +
                na.mPatternV.data6 + ",  " +
                na.mPatternV.data7 + ",  " +
                na.mPatternV.data8 + ",  " +
                na.mPatternV.data9 + ",  " +
                na.mPatternV.data10 + ",  " +
                na.sFilletV.data0 + ",  " +
                na.sFilletV.data1 + ",  " +
                na.sFilletV.data2 + ",  " +
                na.sFilletV.data3 + ",  " +
                na.sFilletV.data4 + ",  " +
                na.sFilletV.data5 + ",  " +
                na.sFilletV.data6 + ",  " +
                na.sFilletV.data7 + ",  " +
                na.sFilletV.data8 + ",  " +
                na.sFilletV.data9 + ",  " + 
                na.sFilletV.data10 + ",  " +
                na.chamferV.data0 + ",  " +
                na.chamferV.data1 + ",  " +
                na.chamferV.data2 + ",  " +
                na.chamferV.data3 + ",  " +
                na.chamferV.data4 + ",  " +
                na.chamferV.data5 + ",  " +
                na.chamferV.data6;

            FileStream fs = new FileStream(PATH, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(content);
            sw.Flush();
            sw.Close();
            fs.Close();
        }
             
    }

}
