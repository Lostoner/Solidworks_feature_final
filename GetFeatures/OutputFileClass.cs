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
        public ModelDoc2 swModel;
        public string fileName;
        public List<MySketchMatrix> curSketchList;

        public OutputOneFileClass(string fil)
        {
            int errors = 0;
            int warnings = 0;
            this.swApp = null;
            this.swModel = null;

            curFeatureGraph = new ALGraph();
            curSketchList = new List<MySketchMatrix>();
            this.fileName = fil;
            this.swApp = new SldWorks();
            this.swModel = (ModelDoc2)this.swApp.OpenDoc6(fil, (int)swDocumentTypes_e.swDocPART, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", ref errors, ref warnings);                          
        }
        public OutputOneFileClass()
        {
            curFeatureGraph = new ALGraph();
            curSketchList = new List<MySketchMatrix>();
            this.swApp = null;
            this.swModel = null;

            swApp = ConnectToSolidWorks();
            swModel = (ModelDoc2)swApp.ActiveDoc;
        }

        //创建特征图关系——1号：只构建特征图，属性提取由其他方法分开进行；2号：集中构建提取特征图数据
        private void CreateFeatureGraph1(object[] featureList)
        {            
            int flagbegin = 0, flagProfile = 0;     //循环体开始标志；找到唯一对应草图的标志
            int n = 0, indexSketch = -1, indexFather = -1, indexSon = -1;         //n为序号                                                                                              
            object[] faFeatureList = null;
            object[] sonFeatureList = null;
            string TypeName, FaTypeName, SonTypeName;

            Debug.WriteLine("特征设计数树数目：" + featureList.Length);      //做特征树数目输出测试

            //获取特征树中有意义的特征，并且完善父特征索引、子特征索引、对应草图索引，特征图对应草图
            foreach (Feature theNowFea in featureList)
            {
                TypeName = theNowFea.GetTypeName();
                if (theNowFea == null)
                    break;

                Debug.WriteLine((n++) + "、" + theNowFea.Name + "——" + TypeName);  //输出当前遍历特征                

                if (flagbegin == 1)
                {
                    //此判断直接对需要的类型进行操作
                    if (TypeName == "BaseBody" || TypeName == "Boss" || TypeName == "BossThin" || TypeName == "Cut" || TypeName == "CutThin" ||
                        TypeName == "Extrusion" ||
                        TypeName == "NetBlend" || 
                        TypeName == "Blend" || TypeName == "BlendCut" ||
                        TypeName == "Sweep" || TypeName == "SweepCut" ||
                        TypeName == "RevCut" || TypeName == "Revolution" || TypeName == "RevolutionThin")                    
                    {
                        MyFeatureVertexNode newRealFeature = new MyFeatureVertexNode(theNowFea);
                        
                        //遍历真实特征的父特征，以获取对应草图 及 真实父特征                                            
                        faFeatureList = theNowFea.GetParents();
                        foreach(Feature theFaFea in faFeatureList)      
                        {
                            FaTypeName = theFaFea.GetTypeName();

                            //添加特征节点唯一对应草图
                            if (FaTypeName == "ProfileFeature" && flagProfile == 0)
                            {
                                MySketchMatrix newRealSketch = new MySketchMatrix(theFaFea);
                                
                                curSketchList.Add(newRealSketch);
                                //提取真实草图的索引，用于特征节点获取自己的对应草图
                                indexSketch = curSketchList.FindIndex(s => s.sketch.Name.Equals(newRealSketch.sketch.Name));                                
                                newRealFeature.sketchsIndex = indexSketch;
                                flagProfile = 1;                //置1时表示已找到该特征的唯一对应草图，之后不再获取其草图
                            }
                            if (FaTypeName == "BaseBody" || FaTypeName == "Boss" || FaTypeName == "BossThin" || FaTypeName == "Cut" || FaTypeName == "CutThin" ||
                                FaTypeName == "Extrusion" ||
                                FaTypeName == "NetBlend" ||
                                FaTypeName == "Blend" || FaTypeName == "BlendCut" ||
                                FaTypeName == "Sweep" || FaTypeName == "SweepCut" ||
                                FaTypeName == "RevCut" || FaTypeName == "Revolution" || FaTypeName == "RevolutionThin")                            
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
                    flagbegin = 1;                
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
                    SonTypeName = theSonFea.GetTypeName();
                    if (SonTypeName == "BaseBody" || SonTypeName == "Boss" || SonTypeName == "BossThin" || SonTypeName == "Cut" || SonTypeName == "CutThin" ||
                        SonTypeName == "Extrusion" ||
                        SonTypeName == "NetBlend" ||
                        SonTypeName == "Blend" || SonTypeName == "BlendCut" ||
                        SonTypeName == "Sweep" || SonTypeName == "SweepCut" ||
                        SonTypeName == "RevCut" || SonTypeName == "Revolution" || SonTypeName == "RevolutionThin")
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
        private void CreateFeatureGraph2(object[] featureList)
        {
            int flagbegin = 0, flagProfile = 0;     //循环体开始标志；找到唯一对应草图的标志
            int n = 0, indexSketch = -1, indexFather = -1, indexSon = -1;         //n为序号
            int serial = 0;          //用于print特征图内草图序号                                                                                  
            object[] faFeatureList = null;
            object[] sonFeatureList = null;
            string TypeName, FaTypeName, SonTypeName;

            Debug.WriteLine("特征设计数树数目：" + featureList.Length);      //做特征树数目输出测试

            //获取特征树中有意义的特征，并且完善父特征索引、子特征索引、对应草图索引，特征图对应草图
            foreach (Feature theNowFea in featureList)
            {
                TypeName = theNowFea.GetTypeName();
                if (theNowFea == null)
                    break;

                //Debug.WriteLine((n++) + "、" + theNowFea.Name + "——" + TypeName);  //输出当前遍历特征                

                if (flagbegin == 1)
                {
                    //此判断直接对需要的类型进行操作
                    if (TypeName == "BaseBody" || TypeName == "Boss" || TypeName == "BossThin" || TypeName == "Cut" || TypeName == "CutThin" ||
                        TypeName == "Extrusion" ||
                        TypeName == "NetBlend" ||
                        TypeName == "Blend" || TypeName == "BlendCut" ||
                        TypeName == "Sweep" || TypeName == "SweepCut" ||
                        TypeName == "RevCut" || TypeName == "Revolution" || TypeName == "RevolutionThin" ||
                        TypeName == "LPattern" || TypeName == "LocalLPattern" ||
                        TypeName == "CirPattern" || TypeName == "LocalCirPattern" ||
                        TypeName == "MirrorPattern" || TypeName == "MirrorSolid" || TypeName == "MirrorStock" ||
                        TypeName == "Fillet" || TypeName == "Round fillet corner" || TypeName == "VarFillet" ||
                        TypeName == "Chamfer")
                    {
                        MyFeatureVertexNode newRealFeature = new MyFeatureVertexNode(theNowFea);
                        Debug.WriteLine((n++) + "、" + theNowFea.Name + "——" + TypeName);

                        //遍历真实特征的父特征，以获取对应草图 及 真实父特征                                            
                        faFeatureList = theNowFea.GetParents();
                        foreach (Feature theFaFea in faFeatureList)
                        {
                            FaTypeName = theFaFea.GetTypeName();

                            //添加特征节点唯一对应草图
                            if (FaTypeName == "ProfileFeature" && flagProfile == 0)
                            {
                                MySketchMatrix newRealSketch = new MySketchMatrix(theFaFea);

                                //对特征节点的唯一对应草图进行属性提取
                                Extractuion_aSketchDefinition(newRealSketch, ref serial);

                                curSketchList.Add(newRealSketch);

                                //提取真实草图的索引，用于特征节点获取自己的对应草图
                                indexSketch = curSketchList.FindIndex(s => s.sketch.Name.Equals(newRealSketch.sketch.Name));
                                newRealFeature.sketchsIndex = indexSketch;
                                flagProfile = 1;                //置1时表示已找到该特征的唯一对应草图，之后不再获取其草图
                            }
                            if (FaTypeName == "BaseBody" || FaTypeName == "Boss" || FaTypeName == "BossThin" || FaTypeName == "Cut" || FaTypeName == "CutThin" ||
                                FaTypeName == "Extrusion" ||
                                FaTypeName == "NetBlend" ||
                                FaTypeName == "Blend" || FaTypeName == "BlendCut" ||
                                FaTypeName == "Sweep" || FaTypeName == "SweepCut" ||
                                FaTypeName == "RevCut" || FaTypeName == "Revolution" || FaTypeName == "RevolutionThin")
                            {
                                //获取父特征的索引
                                indexFather = curFeatureGraph.AdjList.FindIndex(s => s.fea.Name.Equals(theFaFea.Name));
                                if (indexFather != -1)
                                    newRealFeature.fathersIndex.Add(indexFather);
                            }
                        }
                        flagProfile = 0;

                        //对特征节点的特征进行属性提取
                        Debug.WriteLine("   关于本特征图，" + (n-1) + "号特征 " + newRealFeature.fea.Name + " 的必要信息提取——");
                        newRealFeature.JudgeAndToGet_FeatureDefinition(swModel);
                        Debug.WriteLine("       提取完成！\n");

                        curFeatureGraph.AdjList.Add(newRealFeature);
                        newRealFeature.selfIndex = curFeatureGraph.AdjList.Count() - 1;
                        curFeatureGraph.VertexNodeCount++;
                    }
                }
                if (TypeName == "OriginProfileFeature")
                    flagbegin = 1;
            }

            //遍历特征图的特征节点列表，添加特征节点的子特征的索引列表
            for (int i = 0; i < curFeatureGraph.VertexNodeCount; i++)
            {
                Feature curFea = curFeatureGraph.AdjList[i].fea;
                //遍历真实特征的子特征，以获取相应真实子特征
                sonFeatureList = curFea.GetChildren();
                if (sonFeatureList == null)
                    continue;
                foreach (Feature theSonFea in sonFeatureList)
                {
                    SonTypeName = theSonFea.GetTypeName();
                    if (SonTypeName == "BaseBody" || SonTypeName == "Boss" || SonTypeName == "BossThin" || SonTypeName == "Cut" || SonTypeName == "CutThin" ||
                        SonTypeName == "Extrusion" ||
                        SonTypeName == "NetBlend" ||
                        SonTypeName == "Blend" || SonTypeName == "BlendCut" ||
                        SonTypeName == "Sweep" || SonTypeName == "SweepCut" ||
                        SonTypeName == "RevCut" || SonTypeName == "Revolution" || SonTypeName == "RevolutionThin")
                    {
                        curFeatureGraph.BordeCount++;       //一个子特征的出现，表示特征图多了一条边关系
                        indexSon = curFeatureGraph.AdjList.FindIndex(s => s.fea.Name.Equals(theSonFea.Name));
                        if (indexSon != -1)
                            curFeatureGraph.AdjList[i].sonsIndex.Add(indexSon);
                    }
                }

            }

            //输出
            OutOfFile(1);
            OutOfFile(2);
            Form2.node_ID += curFeatureGraph.AdjList.Count();
            Debug.Write("\n");
        }

        public void saveTree(object[] featureList)
        {
            int flagbegin = 0, flagProfile = 0;     //循环体开始标志；找到唯一对应草图的标志
            int n = 0, indexSketch = -1, indexFather = -1, indexSon = -1;         //n为序号
            int serial = 0;          //用于print特征图内草图序号                                                                                  
            object[] faFeatureList = null;
            object[] sonFeatureList = null;
            string TypeName, FaTypeName, SonTypeName;

            Debug.WriteLine("特征设计数树数目：" + featureList.Length);      //做特征树数目输出测试

            //获取特征树中有意义的特征，并且完善父特征索引、子特征索引、对应草图索引，特征图对应草图
            foreach (Feature theNowFea in featureList)
            {
                TypeName = theNowFea.GetTypeName();
                if (theNowFea == null)
                    break;

                bool oneFather = false;

                //Debug.WriteLine((n++) + "、" + theNowFea.Name + "——" + TypeName);  //输出当前遍历特征                

                if (flagbegin == 1)
                {
                    //此判断直接对需要的类型进行操作
                    if (TypeName == "BaseBody" || TypeName == "Boss" || TypeName == "BossThin" || TypeName == "Cut" || TypeName == "CutThin" ||
                        TypeName == "Extrusion" ||
                        TypeName == "NetBlend" ||
                        TypeName == "Blend" || TypeName == "BlendCut" ||
                        TypeName == "Sweep" || TypeName == "SweepCut" ||
                        TypeName == "RevCut" || TypeName == "Revolution" || TypeName == "RevolutionThin" ||
                        TypeName == "LPattern" || TypeName == "LocalLPattern" ||
                        TypeName == "CirPattern" || TypeName == "LocalCirPattern" ||
                        TypeName == "MirrorPattern" || TypeName == "MirrorSolid" || TypeName == "MirrorStock" ||
                        TypeName == "Fillet" || TypeName == "Round fillet corner" || TypeName == "VarFillet" ||
                        TypeName == "Chamfer")
                    {
                        MyFeatureVertexNode newRealFeature = new MyFeatureVertexNode(theNowFea);
                        Debug.WriteLine((n++) + "、" + theNowFea.Name + "——" + TypeName);

                        //遍历真实特征的父特征，以获取对应草图 及 真实父特征                                            
                        faFeatureList = theNowFea.GetParents();
                        foreach (Feature theFaFea in faFeatureList)
                        {
                            FaTypeName = theFaFea.GetTypeName();

                            //添加特征节点唯一对应草图
                            if (FaTypeName == "ProfileFeature" && flagProfile == 0)
                            {
                                MySketchMatrix newRealSketch = new MySketchMatrix(theFaFea);

                                //对特征节点的唯一对应草图进行属性提取
                                Extractuion_aSketchDefinition(newRealSketch, ref serial);

                                curSketchList.Add(newRealSketch);

                                //提取真实草图的索引，用于特征节点获取自己的对应草图
                                indexSketch = curSketchList.FindIndex(s => s.sketch.Name.Equals(newRealSketch.sketch.Name));
                                newRealFeature.sketchsIndex = indexSketch;
                                flagProfile = 1;                //置1时表示已找到该特征的唯一对应草图，之后不再获取其草图
                            }
                            if (FaTypeName == "BaseBody" || FaTypeName == "Boss" || FaTypeName == "BossThin" || FaTypeName == "Cut" || FaTypeName == "CutThin" ||
                                FaTypeName == "Extrusion" ||
                                FaTypeName == "NetBlend" ||
                                FaTypeName == "Blend" || FaTypeName == "BlendCut" ||
                                FaTypeName == "Sweep" || FaTypeName == "SweepCut" ||
                                FaTypeName == "RevCut" || FaTypeName == "Revolution" || FaTypeName == "RevolutionThin" ||
                                FaTypeName == "LPattern" || FaTypeName == "LocalLPattern" ||
                                FaTypeName == "CirPattern" || FaTypeName == "LocalCirPattern" ||
                                FaTypeName == "MirrorPattern" || FaTypeName == "MirrorSolid" || FaTypeName == "MirrorStock" ||
                                FaTypeName == "Fillet" || FaTypeName == "Round fillet corner" || FaTypeName == "VarFillet" ||
                                FaTypeName == "Chamfer")
                            {
                                //获取父特征的索引
                                indexFather = curFeatureGraph.AdjList.FindIndex(s => s.fea.Name.Equals(theFaFea.Name));
                                if (indexFather != -1)
                                {
                                    if (!oneFather)
                                    {
                                        newRealFeature.fathersIndex.Add(indexFather);
                                        oneFather = true;
                                    }
                                }
                            }
                        }
                        flagProfile = 0;

                        //对特征节点的特征进行属性提取
                        Debug.WriteLine("   关于本特征图，" + (n - 1) + "号特征 " + newRealFeature.fea.Name + " 的必要信息提取——");
                        newRealFeature.JudgeAndToGet_FeatureDefinition(swModel);
                        Debug.WriteLine("       提取完成！\n");

                        curFeatureGraph.AdjList.Add(newRealFeature);
                        newRealFeature.selfIndex = curFeatureGraph.AdjList.Count() - 1;
                        curFeatureGraph.VertexNodeCount++;
                    }
                }
                if (TypeName == "OriginProfileFeature")
                    flagbegin = 1;
            }

            //遍历特征图的特征节点列表，添加特征节点的子特征的索引列表
            for (int i = 0; i < curFeatureGraph.VertexNodeCount; i++)
            {
                Feature curFea = curFeatureGraph.AdjList[i].fea;
                //遍历真实特征的子特征，以获取相应真实子特征
                sonFeatureList = curFea.GetChildren();
                if (sonFeatureList == null)
                    continue;
                foreach (Feature theSonFea in sonFeatureList)
                {
                    SonTypeName = theSonFea.GetTypeName();
                    if (SonTypeName == "BaseBody" || SonTypeName == "Boss" || SonTypeName == "BossThin" || SonTypeName == "Cut" || SonTypeName == "CutThin" ||
                        SonTypeName == "Extrusion" ||
                        SonTypeName == "NetBlend" ||
                        SonTypeName == "Blend" || SonTypeName == "BlendCut" ||
                        SonTypeName == "Sweep" || SonTypeName == "SweepCut" ||
                        SonTypeName == "RevCut" || SonTypeName == "Revolution" || SonTypeName == "RevolutionThin" ||
                        SonTypeName == "LPattern" || SonTypeName == "LocalLPattern" ||
                        SonTypeName == "CirPattern" || SonTypeName == "LocalCirPattern" ||
                        SonTypeName == "MirrorPattern" || SonTypeName == "MirrorSolid" || SonTypeName == "MirrorStock" ||
                        SonTypeName == "Fillet" || SonTypeName == "Round fillet corner" || SonTypeName == "VarFillet" ||
                        SonTypeName == "Chamfer")
                    {
                        indexSon = curFeatureGraph.AdjList.FindIndex(s => s.fea.Name.Equals(theSonFea.Name));
                        if (indexSon != -1)
                        {
                            if(curFeatureGraph.AdjList[indexSon].fathersIndex[0] == i)
                            {
                                curFeatureGraph.AdjList[i].sonsIndex.Add(indexSon);
                                curFeatureGraph.BordeCount++;       //一个子特征的出现，表示特征图多了一条边关系
                            }
                        }
                            
                    }
                }

            }

            //输出
            OutOfFile(1);
            OutOfFile(2);
            Form2.node_ID += curFeatureGraph.AdjList.Count();
            Debug.Write("\n");
        }

        //测试输出特征图——
        //1号：只对特征图结构进行测试；
        //2号：对特征图相关属性做必要测试；
        //3号：只对节点属性进行 表格输出，并标记图索引
        private void OutFeatureGraphTest_1()
        {
            int n = 0;

            Debug.WriteLine("\n——输出特征图——");
            Debug.WriteLine("节点数目：" + curFeatureGraph.VertexNodeCount);
            Debug.WriteLine("节点关系数目" + curFeatureGraph.BordeCount);

            //特征节点的特征与唯一对应草图的名字输出
            Debug.WriteLine("草图节点列表：");
            foreach (MyFeatureVertexNode feaNode in curFeatureGraph.AdjList)
            {
                if (feaNode.sketchsIndex != -1)
                    Debug.WriteLine(" [" + (n++) + "] 【" + feaNode.fea.Name + "】 —— [" +
                        feaNode.sketchsIndex + "] 【" + curSketchList[feaNode.sketchsIndex].sketch.Name + "】");
                else
                    Debug.WriteLine(" [" + (n++) + "] 【" + feaNode.fea.Name + "】 —— [" +
                        feaNode.sketchsIndex + "] 【无】");

                Debug.Write("    ·父节点关系：");
                foreach (int faIndex in feaNode.fathersIndex)
                {
                    Debug.Write(faIndex + " | ");
                }                

                Debug.Write("    ·子节点关系：");
                foreach (int sonIndex in feaNode.sonsIndex)
                {
                    Debug.Write(sonIndex + " | ");
                }
                Debug.Write("\n");

                Debug.Write("    ·草图节点关系：");
                Debug.WriteLine(feaNode.sketchsIndex);
            }

        }
        private void OutFeatureGraphTest_2()
        {
            int n = 0;

            Debug.WriteLine("\n——输出特征图——");
            Debug.WriteLine("节点数目：" + curFeatureGraph.VertexNodeCount);
            Debug.WriteLine("节点关系数目" + curFeatureGraph.BordeCount);

            //特征节点的特征与唯一对应草图的名字输出
            Debug.WriteLine("特征节点列表：");
            foreach (MyFeatureVertexNode feaNode in curFeatureGraph.AdjList)
            {
                if(feaNode.sketchsIndex != -1)
                    Debug.WriteLine(" [" + (n++) + "] 【" + feaNode.fea.Name + "】 —— [" +
                        feaNode.sketchsIndex + "] 【" + curSketchList[feaNode.sketchsIndex].sketch.Name + "】");
                else
                    Debug.WriteLine(" [" + (n++) + "] 【" + feaNode.fea.Name + "】 —— [" +
                        feaNode.sketchsIndex + "] 【无】");

                Debug.Write("    ·父节点关系：");
                foreach (int faIndex in feaNode.fathersIndex)
                {
                    Debug.Write(faIndex + " | ");
                }

                Debug.Write("    ·子节点关系：");
                foreach (int sonIndex in feaNode.sonsIndex)
                {
                    Debug.Write(sonIndex + " | ");
                }
                Debug.Write("\n");

                Debug.Write("    ·草图节点关系：");
                Debug.WriteLine(feaNode.sketchsIndex);
            }

            Debug.WriteLine("\n特征图的草图属性输出：");
            n = 0;
            foreach(MySketchMatrix skeNode in curSketchList)
            {
                Debug.WriteLine("【————————草图各项信息" + (n++) + "号————————】");
                Debug.WriteLine("草图名字：" + skeNode.sketch.Name);
                Debug.WriteLine("   ** 直线数：" + skeNode.lineCount + " ** 圆与圆弧数：" + skeNode.circleCount + 
                    " ** 贝塞尔数：" + skeNode.bezierCount);
                if(skeNode.loopTable != null)
                    Debug.WriteLine("   有路径的环；");
                int k = 0;
                foreach (List<int> loop_stack in skeNode.loopTable)
                {
                    Debug.WriteLine("    ▷ loop length：" + skeNode.loopsNodeNum[k++]);
                    Debug.Write("   ·");
                    foreach (int i in loop_stack)
                    {
                        Debug.Write(i + "-->");
                    }                    
                    Debug.WriteLine(loop_stack[0]);
                }
                Debug.WriteLine("       环数：" + skeNode.loopCount);
                Debug.WriteLine("测试完成！");
            }

            Debug.WriteLine("\n特征节点的特征属性输出：");
            curFeatureGraph.testGraph_NodeFeatures();
        }
        private void OutFeatureGraphTest_3()
        {
            int n = 0;
            Debug.WriteLine("\n输出特征图——节点数目：" + curFeatureGraph.VertexNodeCount);        

            curFeatureGraph.testGraph_NodeFeatures();
        }

        //对特征节点的草图属性进行-遍历提取
        private void Extraction_SketchsDefinition()
        {
            //草图操作类，构建对象（SketchMatrix = SM）
            SketchOperation so1 = new SketchOperation();

            //构建循环,并且进行测试            
            int i = 1;
            foreach (MySketchMatrix sketchNode in curSketchList)
            {
                Debug.WriteLine("【————————草图各项信息" + (i++) + "号————————】");                
                Debug.WriteLine("草图名字：" + sketchNode.sketch.Name);
                so1.CreateSketchSegment1(sketchNode);              
                //so1.buildPointIdToIndexAndPointGraph(sketchNode);  //建立字典同时构建草图的点图结构（含边集）             
                //so1.buildPointIdToIndexAndEdgeGraph(sketchNode);    //建立字典同时构建草图的边图结构
                //so1.buildPointIndexToId(sketchNode);     //建立反向字典
               
                //测试代码
                if (false)
                {
                    Debug.WriteLine("**** 草图Seg测试 ****");
                    so1.testSketchSeg(sketchNode);
                    Debug.WriteLine("**** 草图Point字典测试 ****");
                    so1.testPointIdToIndex(sketchNode);
                    Debug.WriteLine("**** 草图点图邻接表测试 ****");
                    so1.testPointGraph(sketchNode);
                    Debug.WriteLine("**** 草图边图边集测试 ****");
                    so1.testEdgeGraph(sketchNode); 
                }
               /*                
                //获取环及其拓扑结构
                //取环，同时输出获取的环结构
                Debug.WriteLine("**** 草图环内容细节输出测试 ****");                
                if (sketchNode.SPG.pointSet.Count >= 3)
                    so1.getPointLoop(sketchNode);
                else
                    Debug.WriteLine("——点集不足，无法显示有路径的环~");
                Debug.WriteLine("————环数：" + sketchNode.loopCount);
                Debug.WriteLine("完成！");
                
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
                so1.testBox(sketchNode);
                */
            }
            Debug.Write("\n\n");            
        }
        //对特征节点的草图属性进行-单个提取
        private void Extractuion_aSketchDefinition(MySketchMatrix sketchNode,ref int i)
        {
            SketchOperation so1 = new SketchOperation();

            Debug.WriteLine("   关于本特征图，"+ (i++) +"号草图 " + sketchNode.sketch.Name + " 的必要信息提取——");            
            so1.CreateSketchSegment2(sketchNode);
            
            /*
            //建立字典同时构建草图的点图结构（含边集）  ps:面对整圆时，无需构建点集，直接计算环数
            so1.buildPointIdToIndexAndPointGraph(sketchNode);                        
            //取环，同时输出获取的环结构                      
            if (sketchNode.SPG.pointSet.Count >= 3)
                so1.getPointLoop(sketchNode);            
            */

            Debug.WriteLine("       提取完成！");
        }
        //对特征节点的特征属性进行-遍历提取
        private void Extraction_FeatureDefinition()
        {
            foreach(MyFeatureVertexNode curFea in curFeatureGraph.AdjList)
            {
                curFea.JudgeAndToGet_FeatureDefinition(this.swModel);
            }            
        }


        //输出 A, graph_indicator, 父子关系
        public void OutOfFile(int type)
        {
            //type = 1, graph_indicator
            if(type == 1)
            {
                string content = "";
                string path_graph_indicator = Form2.path_graph_indicator;

                for (int i = 0; i < curFeatureGraph.AdjList.Count(); i++) content += Form2.graph_ID.ToString() + "\n";
                File.AppendAllText(path_graph_indicator, content);
            }
            //type = 2, 边关系
            else if(type == 2)
            {
                string content = "";
                string path_A = Form2.path_A;

                for (int i = 0; i < curFeatureGraph.AdjList.Count(); i ++)
                {
                    string selfIndex = (curFeatureGraph.AdjList[i].selfIndex + Form2.node_ID).ToString();
                    for (int j = 0; j < curFeatureGraph.AdjList[i].fathersIndex.Count(); j ++)
                    {
                        content += selfIndex + ", " + (curFeatureGraph.AdjList[i].fathersIndex[j] + Form2.node_ID).ToString() + "\n";
                    }
                    for (int j = 0; j < curFeatureGraph.AdjList[i].sonsIndex.Count(); j++)
                    {
                        content += selfIndex + ", " + (curFeatureGraph.AdjList[i].sonsIndex[j] + Form2.node_ID).ToString() + "\n";
                    }
                }
             
                File.AppendAllText(path_A, content);
            }
            else if(type == 3)
            {
                string content = "";
                string path_Tree = Form2.path_Tree;

                for(int i = 0; i < curFeatureGraph.AdjList.Count(); i++)
                {
                    string selfIndex = (curFeatureGraph.AdjList[i].selfIndex + Form2.node_ID).ToString();
                    content += selfIndex + ", " + (curFeatureGraph.AdjList[i].treeFatherIndex[0] + Form2.node_ID).ToString() + "\n";
                    for(int j = 0; j < curFeatureGraph.AdjList[i].sonsIndex.Count(); j++)
                    {
                        content += selfIndex + ", " + (curFeatureGraph.AdjList[i].sonsIndex[j] + Form2.node_ID).ToString() + "\n";
                    }
                }

                File.AppendAllText(path_Tree, content);
            }
        }

        //输出 graph_labels
        public void writeToTxt_GraphLabel(string path_graph_labels)
        {            
            string[] resAry = this.fileName.Split(new string[] { "\\" }, StringSplitOptions.None);
            string label_str = resAry[resAry.Length - 2];

            FileStream tar = new FileStream(path_graph_labels, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            
            Debug.Print("\n正在为图打标签——[Label: ]" + label_str);
            string str = "";
            switch (label_str)
            {
                case "20_Gear":
                    str += "1";
                    break;
                case "21_Washer":
                    str += "2";
                    break;
                case "22_Ball":
                    str += "3";
                    break;
                case "23_Nut":
                    str += "4";
                    break;
                case "24_Screw":
                    str += "5";
                    break;
                case "25_Spring":
                    str += "6";
                    break;
                case "26_Wheel":
                    str += "7";
                    break;
                case "27_Flange":
                    str += "8";
                    break;
                case "28_Carrlane_Block_Cylinder":
                    str += "9";
                    break;
                default:
                    break;
            }
            StreamWriter sw = new StreamWriter(tar);
            sw.WriteLine(str);
            sw.Flush();
            sw.Close();
            tar.Close();
        }


        public void TraverseFeatureGraph(bool TopLevel, bool testOrget)
        {           
            FeatureManager featureManager = this.swModel.FeatureManager;
            object[] featureList = featureManager.GetFeatures(TopLevel);

            //testOrget 为true则进行单零件测试，为false则进行批量提取
            if (testOrget)                   
            {
                //构建特征图，并进行测试构建结果的测试
                CreateFeatureGraph1(featureList);
                OutFeatureGraphTest_1();
                Debug.Write("\n");

                //进行草图操作
                Extraction_SketchsDefinition();
                //进行特征的属性提取
                Extraction_FeatureDefinition();
                //对提取 特征图 做 有序表格输出
                curFeatureGraph.testGraph_NodeFeatures();

                Debug.WriteLine("\n单个零件提取完成！！！\n");
            }
            else
            {
                //构建特征图，写入txt文件，不测试
                CreateFeatureGraph2(featureList);

                saveTree(featureList);

                OutFeatureGraphTest_2();
                //OutFeatureGraphTest_3();        //做节点属性的 表格打印

                Debug.WriteLine("\n一个零件提取完成！！！\n");                
            }
            
        }

        public void hasImported(bool TopLevel)
        {
            FeatureManager featureManager = this.swModel.FeatureManager;
            object[] featureList = featureManager.GetFeatures(TopLevel);

            foreach (Feature f in featureList)
            {
                if (f.Name == "输入3" || f.Name == "Imported1")
                {
                    File.AppendAllText("C:\\Users\\Qin\\Desktop\\input3.txt", this.fileName + "\n");
                }
            }
        }
        public void hasImported2(bool TopLevel)
        {
            FeatureManager featureManager = this.swModel.FeatureManager;
            object[] featureList = featureManager.GetFeatures(TopLevel);

            foreach (Feature f in featureList)
            {
                if (f.IGetChildCount() == 0 && f.IGetParentCount() == 0 && f.GetTypeName() == "BaseBody")
                {
                    Debug.Print("噫，好，我中了: " + this.fileName);               //定义存在该情况的处理
                }
            }
            return;
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

        public void close()
        {
            swApp.CloseDoc(fileName);
        }

    }
}
