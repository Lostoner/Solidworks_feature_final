using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;

namespace GetFeatures
{
    public class OutputOneFileClass
    {
        public ALGraph curFeatureGraph;
        public ISldWorks swApp;
        public string fileName;
        public List<MySketchMatrix> curSketchList;

        public OutputOneFileClass(string fil)
        {
            curFeatureGraph = new ALGraph();
            curSketchList = new List<MySketchMatrix>();
            this.fileName = fil;
            this.swApp.OpenDoc(fil, (int)swDocumentTypes_e.swDocPART);
            swApp = ConnectToSolidWorks();                         
        }

        //创建特征图关系 PS:对倒角和圆角特征做了剔除
        private void CreateFeatureGraph(object[] featureList)
        {
            //n为序号
            int flag = 0, n = 0, indexSketch = -1, indexFather = -1, indexSon = -1, flagProfile = 0;            
            object[] faFeatureList = null;
            object[] sonFeatureList = null;
            string TypeName, FaTypeName, SonTypeName;

            Debug.WriteLine("特征设计数树数目：" + featureList.Length);      //做特征树数目输出测试

            //获取特征树中有意义的特征，并且完善父特征索引、子特征索引、对应草图索引，特征图对应草图
            foreach (Feature theNowFea in featureList)
            {
                TypeName = theNowFea.GetTypeName2();
                if (theNowFea == null)
                    break;

                Debug.WriteLine((n++) + "、" + theNowFea.Name + "——" + theNowFea.GetTypeName2());  //输出当前遍历特征                

                if (flag == 1)
                {
                    //此判断剔除了倒角和圆角
                    if (TypeName == "ProfileFeature" ||
                        TypeName == "RefPlane" ||
                        TypeName == "RefAxis" ||
                        TypeName == "OriginProfileFeature" ||
                        TypeName == "3DProfileFeature"
                        || TypeName == "Chamfer"
                        || TypeName == "Fillet"
                        || TypeName == "Round fillet corner")
                    {
                        continue;
                    }
                    else
                    {
                        MyFeatureVertexNode newRealFeature = new MyFeatureVertexNode(theNowFea);
                        
                        //遍历真实特征的父特征，以获取对应草图 及 真实父特征                                            
                        faFeatureList = theNowFea.GetParents();
                        foreach(Feature theFaFea in faFeatureList)      
                        {
                            FaTypeName = theFaFea.GetTypeName2();

                            if (FaTypeName == "ProfileFeature" && flagProfile == 0)
                            {
                                MySketchMatrix newRealSketch = new MySketchMatrix(theFaFea);
                                curSketchList.Add(newRealSketch);
                                //提取真实草图的索引，用于特征节点获取自己的对应草图
                                indexSketch = curSketchList.FindIndex(s => s.sketch.Name.Equals(newRealSketch.sketch.Name));
                                newRealFeature.sketchsIndex.Add(indexSketch);
                                flagProfile = 1;    //置1时表示已找到该特征的唯一对应草图，之后不再获取其草图
                            }                            
                            if (FaTypeName == "ProfileFeature" ||
                                FaTypeName == "RefPlane" ||
                                FaTypeName == "RefAxis" ||
                                FaTypeName == "OriginProfileFeature"||
                                FaTypeName == "3DProfileFeature"
                                || FaTypeName == "Chamfer"
                                || FaTypeName == "Fillet"
                                || FaTypeName == "Round fillet corner") 
                            {
                                continue;
                            }
                            else
                            {
                                //获取父特征的索引
                                indexFather = curFeatureGraph.AdjList.FindIndex(s => s.fea.Name.Equals(theFaFea.Name));
                                if(indexFather != -1)
                                    newRealFeature.fathersIndex.Add(indexFather);
                            }                           
                        }
                        flagProfile = 0;
                                             
                        curFeatureGraph.AdjList.Add(newRealFeature);
                        curFeatureGraph.VertexNodeCount++;
                    }
                }
                if (TypeName == "OriginProfileFeature")
                    flag = 1;                
            }

            //遍历特征图的特征节点列表，添加特征节点的子特征的索引列表
            for(int i = 0; i < curFeatureGraph.VertexNodeCount; i++)
            {
                Feature curFea = curFeatureGraph.AdjList[i].fea;
                //遍历真实特征的子特征，以获取相应真实子特征
                sonFeatureList = curFea.GetChildren();               
                if (sonFeatureList == null)
                    continue;    
                foreach (Feature theSonFea in sonFeatureList)
                {
                    SonTypeName = theSonFea.GetTypeName2(); 
                    if (SonTypeName == "ProfileFeature" ||
                        SonTypeName == "RefPlane" ||
                        SonTypeName == "RefAxis" ||
                        SonTypeName == "OriginProfileFeature" ||
                        SonTypeName == "3DProfileFeature"
                        || SonTypeName == "Chamfer"
                        || SonTypeName == "Fillet"
                        || SonTypeName == "Round fillet corner")
                    {
                        continue;
                    }
                    else
                    {
                        curFeatureGraph.BordeCount++;       //一个子特征的出现，表示特征图多了一条边关系
                        indexSon = curFeatureGraph.AdjList.FindIndex(s => s.fea.Name.Equals(theSonFea.Name));
                        if (indexSon != -1)
                            curFeatureGraph.AdjList[i].sonsIndex.Add(indexSon);
                    }
                }

            }
            Debug.Write("\n");
        }
        //输出特征草图对应关系
        private void OutFeatureGraphTest()
        {
            int n = 0;
            Debug.WriteLine("\n——输出特征图——");
            Debug.WriteLine("节点数目：" + curFeatureGraph.VertexNodeCount);
            Debug.WriteLine("节点关系数目" + curFeatureGraph.BordeCount);
            Debug.Write("\n");
            //草图节点的名称输出
            Debug.WriteLine("草图节点列表：");
            foreach(MySketchMatrix skeNode in curSketchList)
            {
                Debug.WriteLine((n++) + "、" + skeNode.sketch.Name);
            }
            Debug.Write("\n");
            //特征节点的名称类型输出
            n = 0;
            Debug.WriteLine("特征节点列表：");
            foreach(MyFeatureVertexNode feaNode in curFeatureGraph.AdjList)
            {
                Debug.WriteLine((n++) + "、" + feaNode.fea.Name + "——" + feaNode.fea.GetTypeName2());
            }
            Debug.Write("\n");
            //所有索引的分类输出
            n = 0;            
            foreach (MyFeatureVertexNode feaNode in curFeatureGraph.AdjList)
            {                
                Debug.WriteLine((n++) + "、" + feaNode.fea.Name);
                Debug.Write("    父节点关系：");
                foreach (int faIndex in feaNode.fathersIndex)
                {
                    Debug.Write(faIndex + " | ");
                }
                Debug.Write("\n");
                Debug.Write("    子节点关系：");
                foreach (int sonIndex in feaNode.sonsIndex)
                {
                    Debug.Write(sonIndex + " | ");
                }
                Debug.Write("\n");
                Debug.Write("    草图节点关系：");
                foreach (int skeIndex in feaNode.sketchsIndex)
                {
                    Debug.Write(skeIndex + " | ");
                }
                Debug.Write("\n");
            }    

        }               
        
        //草图操作类——草图数据相关操作
        private void ExtractionSketchDefinition()
        {
            //草图操作类，构建对象（SketchMatrix = SM）
            SketchOperation so1 = new SketchOperation();

            //构建循环,并且进行 测试循环1号
            Debug.WriteLine("\n【测试1号——关于Seg、Point索引、领接表的构建测试】");
            int i = 1;
            foreach (MySketchMatrix sketchNode in curSketchList)
            {
                Debug.WriteLine("————————初始化" + (i++) + "号————————");

                so1.CreateSketchSegment(sketchNode);
                Debug.WriteLine("**** 草图线段测试 ****");
                so1.testSketchSeg(sketchNode);

                so1.buildPointIdToIndexAndPointGraph(sketchNode);  //建立字典同时构建草图的点图结构（含边集）             
                //so1.buildPointIdToIndexAndEdgeGraph(sketchNode);    //建立字典同时构建草图的边图结构
                so1.buildPointIndexToId(sketchNode);     //建立反向字典
                Debug.WriteLine("**** 草图点的字典测试 ****");
                so1.testPointIdToIndex(sketchNode);

                Debug.WriteLine("**** 草图点图邻接表测试 ****");
                so1.testPointGraph(sketchNode);
                Debug.WriteLine("**** 草图边图边集测试 ****");
                so1.testEdgeGraph(sketchNode);

                sketchNode.initializeLoopTable1();      //初始化环表
            }
            Debug.Write("\n");

            //获取环及其拓扑结构
            Debug.WriteLine("\n【测试2号——关于环与TOPO的构建测试】");
            i = 1;
            foreach (MySketchMatrix sketchNode in curSketchList)
            {
                //取环，同时输出获取的环结构
                Debug.WriteLine(">>———————第" + (i++) + "个草图的环———————");
                Debug.WriteLine("**** 草图环内容细节输出测试 ****");
                so1.getPointLoop(sketchNode);
                //(new SketchOperation()).getEdgeLoop(curGraph.AdjList[i].sketch);

                //遍历环表，将环表的编号信息录入到SM下的环组中
                sketchNode.initializeLoopClass();       //初始化环组
                MySketchMatrix SM;
                SM = sketchNode;
                SM.loopList = new List<LoopClass>(SM.loopCount);
                for (int j = 0; j < sketchNode.loopCount; j++)
                {
                    List<int> numsInLoop = SM.loopTable[j];
                    LoopClass loop = new LoopClass(numsInLoop, SM);
                    SM.loopList.Add(loop);                                   //-loopList- 中的位置与 -环表- 中的位置对应

                }
                //提取拓扑结构
                so1.extraTOPO(sketchNode);

                Debug.WriteLine("**** 草图环类信息输出测试 ****");
                so1.testLoopsClass(sketchNode);
                Debug.WriteLine("**** 草图TOPO输出测试 ****");
                so1.testExtraTOPO(sketchNode);
                //so1.testBox(sketchNode);
            }
            Debug.Write("\n\n");
        }
        //对特征节点的属性进行提取
        private void ExtractionFeatureDefinition()
        {
            foreach(MyFeatureVertexNode curFea in curFeatureGraph.AdjList)
            {
                curFea.judgeAndToGet_FeatureDefinition();
            }
            
        }


        public void TraverseFeatureGraph(bool TopLevel)
        {
            ModelDoc2 swModel = (ModelDoc2)swApp.ActiveDoc;         

            FeatureManager featureManager = swModel.FeatureManager;
            object[] featureList = featureManager.GetFeatures(TopLevel);

            //构建特征图，并进行测试构建结果的测试
            CreateFeatureGraph(featureList);
            OutFeatureGraphTest();
            Debug.Write("\n");

            //进行草图操作
            ExtractionSketchDefinition();            
            //进行特征的属性提取
            ExtractionFeatureDefinition();                   

        }
        

        public static ISldWorks SwApp { get; private set; }

        public static ISldWorks ConnectToSolidWorks()
        {
            if (SwApp != null)
            {
                return SwApp;
            }
            Debug.Print("Connect to solidworks...");
            try
            {
                SwApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
            }
            catch (COMException)
            {
                try
                {
                    SwApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application.23");//2015
                }
                catch (COMException)
                {
                    try
                    {
                        SwApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application.26");//2018
                    }
                    catch (COMException)
                    {
                        MessageBox.Show("Could not connect to SolidWorks.", "SolidWorks", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        SwApp = null;
                    }
                }
            }
            if (SwApp != null)
            {
                Debug.Print("Connection succeed.");
                return SwApp;
            }
            else
            {
                Debug.Print("Connection failed.");
                return null;
            }
        }

        //目前被注释
        public void print()
        {/*
            string path = @"D:\F\三维模型库\Parts_WithFeature";
            //StreamWriter sw = new StreamWriter(fileName.Substring(path.Length, fileName.Length - 7) + ".txt");
            //Debug.Print(fileName.Substring(0, fileName.Length - 7) + ".txt");
            FileStream fs = new FileStream(fileName.Substring(0, fileName.Length - 7) + ".txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            string FeaNum = "Number of Feature: " + feas.Count + "\n";
            byte[] FeaN = Encoding.UTF8.GetBytes(FeaNum);
            fs.Write(FeaN, 0, FeaN.Length);
            for (int i = 0; i < feas.Count; i++)
            {
                string FeaSum = "Feature " + i + feas[i].sketchs.Count + " " + feas[i].sons.Count + "\n";
                byte[] FeaS = Encoding.UTF8.GetBytes(FeaSum);
                fs.Write(FeaS, 0, FeaS.Length);

                string sketchInf = "";
                for (int j = 0; j < feas[i].sketchs.Count; j++)
                {
                    sketchInf += " ";
                    sketchInf += feas[i].sketchs[j];
                }
                sketchInf += "\n";
                byte[] si = Encoding.UTF8.GetBytes(sketchInf);
                fs.Write(si, 0, si.Length);

                string sonInf = "";
                for (int j = 0; j < feas[i].sons.Count; j++)
                {
                    sonInf += " ";
                    sonInf += feas[i].sons[j];
                }
                sonInf += "\n";
                byte[] sin = Encoding.UTF8.GetBytes(sonInf);
                fs.Write(sin, 0, sin.Length);
            }

            for (int i = 0; i < skets.Count; i++)
            {
                //sw.WriteLine("Sketch " + i + ", " + skets[i].loops.Count);
                string SketchNum = "Sketch " + i + ", " + skets[i].loops.Count + "\n";
                byte[] SketchN = Encoding.UTF8.GetBytes(SketchNum);
                fs.Write(SketchN, 0, SketchN.Length);

                for (int j = 0; j < skets[i].loops.Count; j++)
                {
                    string LoopNum = "Loop " + j + "\n";
                    byte[] LoopN = Encoding.UTF8.GetBytes(LoopNum);
                    fs.Write(LoopN, 0, LoopN.Length);

                    //string outString = "";
                    for (int k = 0; k < skets[i].loops[j].Count; k++)
                    {
                        string outString = "";
                        outString += skets[i].loopSegs[skets[i].loops[j][k]].type;
                        outString += " ";
                        outString += skets[i].loopSegs[skets[i].loops[j][k]].start;
                        outString += " ";
                        outString += skets[i].loopSegs[skets[i].loops[j][k]].end;

                        outString += "\n";
                        byte[] OutS = Encoding.UTF8.GetBytes(outString);
                        fs.Write(OutS, 0, OutS.Length);
                    }
                }

                //sw.WriteLine("Points " + skets[i].pois.Count);
                string PointNum = "Points " + skets[i].pois.Count + "\n";
                byte[] PointN = Encoding.UTF8.GetBytes(PointNum);
                fs.Write(PointN, 0, PointN.Length);

                //string outString2 = "";
                for (int j = 0; j < skets[i].pois.Count; j++)
                {
                    string outString2 = "";
                    outString2 += skets[i].pois[j].ox;
                    outString2 += " ";
                    outString2 += skets[i].pois[j].oy;
                    outString2 += " ";
                    outString2 += skets[i].pois[j].oz;

                    outString2 += "\n";
                    byte[] OutS2 = Encoding.UTF8.GetBytes(outString2);
                    fs.Write(OutS2, 0, OutS2.Length);
                }
                //sw.WriteLine(outString2);
            }*/
        }

        public void close()
        {
            swApp.CloseDoc(fileName);
        }

    }
}
