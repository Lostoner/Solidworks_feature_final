using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace GetFeatures
{
    //草图属性类
    public class MySketchMatrix
    {
        public Feature sketch;                  //
        public int lineCount;                   //
        public int bezierCount;                 //
        public int circleCount;                 //
        public int loopCount;                   //**
        public int RelationCount;
        public List<int> completeCircleNum;         //整圆Seg在circleList的编号
        public List<StraightLineClass> straightList;            //
        public List<BezierClass> bezierList;                //
        public List<CircleClass> circleList;                //
        public Dictionary<Tuple<int,int>, int> pointIdToIndex;
        public Dictionary<int, Tuple<int, int>> pointIndexToId;
        public SketchPointGraph SPG;            
        public SketchEdgeGraph SEG;
        public List<LoopClass> loopList;                  //**描述环（包围盒）的内容信息
        public List<List<int>> loopTable;    //环表，存储环中节点编号
        public List<int> loopsNodeNum;          //每个环的point数目，序号按环表依次
        public List<Relation> Rels;                 //**描述环的TOPO关系

        public MySketchMatrix(Feature ske)
        {
            lineCount = 0;
            bezierCount = 0;
            circleCount = 0;
            loopCount = 0;
            RelationCount = 0;
            completeCircleNum = new List<int>();
            this.sketch = ske;
            straightList = new List<StraightLineClass>();
            bezierList = new List<BezierClass>();
            circleList = new List<CircleClass>();
            pointIdToIndex = new Dictionary<Tuple<int, int>, int>();
            pointIndexToId = new Dictionary<int, Tuple<int, int>>();
            this.SPG = new SketchPointGraph(ske);
            this.SEG = new SketchEdgeGraph(ske);
            loopList = null;
            loopTable = new List<List<int>>();
            loopsNodeNum = new List<int>();
            Rels = new List<Relation>();
        }
        public void setStraightList(int n)
        {
            this.lineCount = n;
        }
        public void setBezierList(int n)
        {
            this.bezierCount = n;
        }
        public void setCircleList(int n)
        {
            this.circleCount = n;
        }
             
        public void setLoopTable(List<int> al)
        {
            List<int> loop = new List<int>(al);
            loopTable.Add(loop);
            loopCount++;                  
        }
        //*********************************************
        /// <summary>
        /// 提取拓扑相关方法
        /// </summary>
        /// <param name="loop1"></param>
        /// <param name="loop2"></param>
        /// <returns></returns>
        //判断两个环之间边集是否-有序-相等-
        public int isEdgeListEqual(LoopClass loop1, LoopClass loop2)
        {
            List<PointClass> tempPointList1, tempPointList2;
            tempPointList1 = new List<PointClass>();
            tempPointList2 = new List<PointClass>();
            tempPointList1 = loop1.GetPointList();
            tempPointList2 = loop2.GetPointList();

            if(tempPointList1.Count != tempPointList2.Count)
                return 0;

            PointClass pFirst = null;
            PointClass pNext = null;
            PointClass pBefore = null;
            pFirst = tempPointList1[0];
            pNext = tempPointList1[1];
            pBefore = tempPointList1[tempPointList1.Count - 1];
            PointClass pCycle = null;
            PointClass pCycBefore = null;
            PointClass pCycNext = null;            
            List<int> equals = new List<int>(tempPointList1.Count);
            for(int num = 0; num < 4; num++)
            {
                equals.Add(0);
            }         
            double disBefore, disNext, disCycBefore, disCycNext;
            int rotateFlag = -1, counterpointFlag = -1;

            disBefore = distanceBetweenPoints(pFirst, pBefore);
            disNext = distanceBetweenPoints(pFirst, pNext);
            for (int i = 0; i < tempPointList2.Count; i++)
            {
                pCycle = tempPointList2[i];
                if (i == 0)
                    pCycBefore = tempPointList2[tempPointList2.Count - 1];
                else
                    pCycBefore = tempPointList2[i - 1];

                if (i == tempPointList2.Count - 1)
                    pCycNext = tempPointList2[0];
                else
                    pCycNext = tempPointList2[i + 1];

                disCycBefore = distanceBetweenPoints(pCycle, pCycBefore);
                disCycNext = distanceBetweenPoints(pCycle, pCycNext);
                if(disCycNext == disNext && disCycBefore == disBefore)
                {
                    rotateFlag = 1;
                    counterpointFlag = i;
                    break;
                }
                else if(disCycNext == disBefore && disCycBefore == disNext)
                {
                    rotateFlag = 0;
                    counterpointFlag = i;
                    break;
                }
            }
            if (counterpointFlag == -1)
                return 0;
            else
            {              
                for(int i = 0;i < tempPointList1.Count; i++)
                {
                    pFirst = tempPointList1[i];
                    if (i == tempPointList1.Count - 1)
                        pNext = tempPointList1[0];
                    else
                        pNext = tempPointList1[i + 1];
                    disNext = distanceBetweenPoints(pFirst, pNext);
                    
                    if (rotateFlag == 1)
                    {
                        int cycPosition = (i + counterpointFlag) % tempPointList2.Count;
                        pCycle = tempPointList2[cycPosition];
                        if (cycPosition == tempPointList2.Count - 1)
                            pCycNext = tempPointList2[0];
                        else
                            pCycNext = tempPointList2[cycPosition + 1];
                    }
                    else if(rotateFlag == 0)
                    {
                        int cycPosition = (4 + counterpointFlag - i) % tempPointList2.Count;
                        pCycle = tempPointList2[cycPosition];
                        if (cycPosition == 0)
                            pCycNext = tempPointList2[tempPointList2.Count - 1];
                        else
                            pCycNext = tempPointList2[cycPosition - 1];
                    }
                    disCycNext = distanceBetweenPoints(pCycle, pCycNext);

                    if (disNext == disCycNext)
                        equals[i] = 1;
                }
                foreach (int equal in equals) 
                {
                    if (equal == 0)
                        return 0;
                }           
            }
            return 1;
        }
        //判断包围盒1中心点是否在包围盒2内
        public int isCenterInAnotherBox(LoopClass loop1, LoopClass loop2)
        {
            double centerX1 = (loop1.leftDownX + loop1.rightUpX) / 2;
            double centerY1 = (loop1.leftDownY + loop1.rightUpY) / 2;
            if(centerX1 < loop2.rightUpX && centerX1 > loop2.leftDownX 
                && centerY1 < loop2.rightUpY && centerY1 > loop2.leftDownY)
            {
                return 1;
            }
            else  
                return 0;
        }
        //返回两点之间的距离
        public double distanceBetweenPoints(PointClass p1,PointClass p2)
        {
            return Math.Sqrt(Math.Pow(p1.x - p2.x,2) + Math.Pow(p1.y - p2.y, 2) + Math.Pow(p1.z - p2.z, 2));
        }
        //判断两个环之间有没有共同边
        public int isHaveSameEdge(LoopClass loop1,LoopClass loop2) 
        {
            List<PointClass> tempPointList1, tempPointList2;
            tempPointList1 = loop1.GetPointList();
            tempPointList2 = loop2.GetPointList();
            int loop1EdgeCount = tempPointList1.Count;
            int loop2EdgeCount = tempPointList2.Count;
            List<double> KB1;           //k与b，y = kx + b,描述平面内一条直线
            List<double> KB2;
            double eps = 1e-8;

            for (int i = 0; i < loop1EdgeCount; i++)
            {
                for(int j = 0; j < loop2EdgeCount; j++)
                {
                    int iNext, jNext;
                    if (i == loop1EdgeCount - 1)
                        iNext = 0;
                    else
                        iNext = i + 1;
                    if (j == loop2EdgeCount - 1)
                        jNext = 0;
                    else
                        jNext = j + 1;

                    //计算直线KB的值，平面做参数，是为了方便知道该调用x、y、z中哪两个
                    KB1 = twoPointsToLine(tempPointList1[i], tempPointList1[iNext], loop1.plane);
                    KB2 = twoPointsToLine(tempPointList2[j], tempPointList2[jNext], loop2.plane);
                    if (Math.Abs(KB1[0] - KB2[1]) < eps && Math.Abs(KB1[0] - KB2[1]) < eps)
                        return 1;                    
                }
            }
            return 0;
        }
        //以两点构建二维平面的直线，返回斜率k、偏移b
        public List<double> twoPointsToLine(PointClass p1, PointClass p2, int plane)
        {
            List<double> KAndB = null;
            double k, b;
            if (plane == 1)
            {
                k = (p1.z - p2.z) / (p1.y - p2.y);
                b = p1.z - k * p1.y;
                KAndB = new List<double> { k, b };
            }
            else if (plane == 2)
            {
                k = (p1.z - p2.z) / (p1.x - p2.x);
                b = p1.z - k * p1.x;
                KAndB = new List<double> { k, b };
            }
            else if (plane == 3)
            {
                k = (p1.y - p2.y) / (p1.x - p2.x);
                b = p1.y - k * p1.x;
                KAndB = new List<double> { k, b };
            }
            return KAndB;
        }
        //*********************************************
        
        public void initializeLoopClass()
        {
            loopList = new List<LoopClass>(loopCount);
        }
        public void initializeRelations()
        {
            //int relationCount = (loopCount2 - loopCount) / 2;
            Rels = new List<Relation>();
        }
        public void printAllLoopData()
        {
            for(int i = 0; i < loopCount; i++)
            {
                Debug.WriteLine("> *** 第" + (i + 1) + "个 环/包围盒 信息 ****");
                LoopClass box = loopList[i];
                Debug.WriteLine("       左下角X：" + box.leftDownX + " | 左下角Y：" + box.leftDownY +
                    " | 右上角X：" + box.rightUpX + " | 右上角Y：" + box.rightUpY);
            }
        }
        public void printAllTOPO()
        {
            if (RelationCount == 0)
                Debug.WriteLine("无拓扑关系");
            else
            {
                for (int i = 0; i < RelationCount; i++)
                {
                    if (Rels[i].relationFlag == 0)
                    {
                        if (Rels[i].adjacentFlag == 0)
                            Debug.WriteLine("环" + Rels[i].loopsNum.Item1 + " 与 环" + Rels[i].loopsNum.Item2
                                + " 的拓扑关系：内邻");
                        else if (Rels[i].adjacentFlag == 1)
                            Debug.WriteLine("环" + Rels[i].loopsNum.Item1 + " 与 环" + Rels[i].loopsNum.Item2
                                + " 的拓扑关系：外邻");
                    }
                    else if (Rels[i].relationFlag == 1)
                        Debug.WriteLine("环" + Rels[i].loopsNum.Item1 + " 与 环" + Rels[i].loopsNum.Item2
                            + " 的拓扑关系：相离");
                    else if (Rels[i].relationFlag == 2)
                        Debug.WriteLine("环" + Rels[i].loopsNum.Item1 + " 对 环" + Rels[i].loopsNum.Item2
                            + " 的拓扑关系：包含");
                    else if (Rels[i].relationFlag == 3)
                    {
                        Debug.WriteLine("环" + Rels[i].loopsNum.Item1 + " 对 环" + Rels[i].loopsNum.Item2
                            + " 的拓扑关系：对称");
                    }
                }
            }
        }
        public void outLoopTable()
        {
            Debug.WriteLine("环表展示——");
            foreach (List<int> box in loopTable)
            {
                Debug.Write("环序列：");
                foreach (int a in box)
                {           
                    Debug.Write(a + "->");
                }
                Debug.Write("\n");
            }
            Debug.Write("\n");
        }
    }

    //————————————————————————————————————————————————————

    //草图的边表
    public class SketchEdgeNode
    {
        public int adjvex;  //指向顶点的编号
        public SketchEdgeNode next; //下一条边

        public SketchEdgeNode()
        {
            adjvex = -1;
            next = null;
        }
    }

    /// <summary>
    /// 草图的点图结构（其中在传统的邻接表上，加了边集）
    /// </summary>
    public class SketchPointGraph
    {
        public object sketchFeature;
        public List<PointClass> pointSet;//点集
        public List<SketchEndPointEdge> EdgeSet;   //边集
        public int VertexNodeCount;//顶点数
        public int BordeCount;//边数
        public SketchPointGraph(Feature sketchFeature)
        {
            this.sketchFeature = sketchFeature;                  

            pointSet = new List<PointClass>();            
            EdgeSet = new List<SketchEndPointEdge>();
            VertexNodeCount = 0; 
            BordeCount = 0;
        }
        //端点加入点集
        public void setPointSet(PointClass p) 
        {            
            this.pointSet.Add(p);
            this.VertexNodeCount++;
        }
        //端点对加入边集
        public void setLineInEdgeSet(PointClass p1, PointClass p2, MySketchMatrix SM)
        {            
            int a = SM.pointIdToIndex[p1.ID];
            int b = SM.pointIdToIndex[p2.ID];
            SketchEndPointEdge epEdge = new SketchEndPointEdge(a, b);
            EdgeSet.Add(epEdge);
            BordeCount++;
        }
        public void setBezierInEdgeSet(PointClass p1, PointClass p2, PointClass p3, PointClass p4, MySketchMatrix SM)
        {            
            int a = SM.pointIdToIndex[p1.ID];
            int b = SM.pointIdToIndex[p2.ID];
            int c = SM.pointIdToIndex[p3.ID];
            int d = SM.pointIdToIndex[p4.ID];
            SketchEndPointEdge epEdge1 = new SketchEndPointEdge(a, b);
            EdgeSet[BordeCount++] = epEdge1;
            SketchEndPointEdge epEdge2 = new SketchEndPointEdge(b, c);
            EdgeSet[BordeCount++] = epEdge2;
            SketchEndPointEdge epEdge3 = new SketchEndPointEdge(c, d);
            EdgeSet[BordeCount++] = epEdge3;
        }
        //判断Line是否在 -边集- 中重复
        public int isEdgeLineRepeat(PointClass p1, PointClass p2, MySketchMatrix SM)
        {
            int start = SM.pointIdToIndex[p1.ID];
            int end = SM.pointIdToIndex[p2.ID];
            int flag = 0;
            for (int i = 0; i < SM.SPG.BordeCount; i++)
            {
                if (EdgeSet[i].EndPointPair.Key == start && EdgeSet[i].EndPointPair.Value == end)
                {
                    flag = 1;
                }
                if (EdgeSet[i].EndPointPair.Key == end && EdgeSet[i].EndPointPair.Value == start)
                {
                    flag = 1;
                }
            }
            if (flag == 1)
            {
                return 1;
            }
            return 0;
        }
        //判断Bezier是否在 -边集- 中重复
        public int isEdgeBezierRepeat(PointClass p1, PointClass p2, PointClass p3, PointClass p4, MySketchMatrix SM)
        {
            int start = SM.pointIdToIndex[p1.ID];
            int assist1 = SM.pointIdToIndex[p2.ID];
            int assist2 = SM.pointIdToIndex[p3.ID];
            int end = SM.pointIdToIndex[p4.ID];
            int flag1 = 0, flag2 = 0;
            for (int i = 0; i < SM.SPG.BordeCount; i++)
            {
                if (EdgeSet[i].EndPointPair.Key == start && EdgeSet[i].EndPointPair.Value == assist1)
                {
                    flag1 = 1;
                }
                if (EdgeSet[i].EndPointPair.Key == assist1 && EdgeSet[i].EndPointPair.Value == start)
                {
                    flag1 = 1;
                }
                if (EdgeSet[i].EndPointPair.Key == assist2 && EdgeSet[i].EndPointPair.Value == end)
                {
                    flag2 = 1;
                }
                if (EdgeSet[i].EndPointPair.Key == end && EdgeSet[i].EndPointPair.Value == assist2)
                {
                    flag2 = 1;
                }
            }
            if (flag1 == 1 && flag2 == 1)
            {
                return 1;
            }
            return 0;
        }

        //测试边集的建立
        public void printAllEdge()
        {
            for (int i = 0; i < BordeCount; i++)
            {
                SketchEndPointEdge e = EdgeSet[i];
                Debug.Write(i + ": ");
                Debug.Write("<" + e.EndPointPair.Key + "," + e.EndPointPair.Value + ">");
                Debug.Write("\n");
            }
        }
        //测试点集及点图的建立
        public void printAllPoint()
        {
            for (int i = 0; i < VertexNodeCount; i++) 
            {
                PointClass p = pointSet[i];
                SketchEdgeNode e = p.firseEdge;
                Debug.Write(i);
                while(e.next != null)
                {
                    Debug.Write("-->" + e.adjvex);
                    e = e.next;
                }
                Debug.Write("-->" + e.adjvex);
                Debug.Write("\n");
            }
        }
       //利用ID，寻找点PointClass
        public PointClass findPointClass(Tuple<int,int> ID)
        {
            PointClass point = new PointClass();
            foreach(PointClass p in pointSet)
            {
                if(p.ID == ID)
                {
                    point = p;
                    break;
                }
            }
            return point;
        }
    }

    //————————————————————————————————————————————————————

    //草图的端点对类——用于边图结构，构建草图的图结构
    public class SketchEndPointEdge
    {
        public KeyValuePair<int, int> EndPointPair;
        public SketchEndPointEdge(int a, int b)
        {
            EndPointPair = new KeyValuePair<int, int>(a, b);
        }
    }

    //草图的边图结构
    public class SketchEdgeGraph
    {
        public object sketchFeature;       //草图特征
        public SketchEndPointEdge[] EdgeSet;
        public int VertexNodeCount;//顶点数
        public int BordeCount;//边数
        public SketchEdgeGraph(Feature sketchFeature)
        {
            this.sketchFeature = sketchFeature;
            Sketch ske = (Sketch)sketchFeature.GetSpecificFeature2();
            object[] vSkSegArr = (object[])ske.GetSketchSegments();
            int theNum = vSkSegArr.Length;
            EdgeSet = new SketchEndPointEdge[theNum];
            VertexNodeCount = 0;
            BordeCount = 0;     //由于需要判断Arc是圆还是弧，所以暂时不把cc并入边总数
        }
        public void setLineInEdgeSet(PointClass p1, PointClass p2, MySketchMatrix SM)
        {            
            int a = SM.pointIdToIndex[p1.ID];
            int b = SM.pointIdToIndex[p2.ID];
            SketchEndPointEdge epEdge = new SketchEndPointEdge(a, b);
            EdgeSet[BordeCount] = epEdge;
            BordeCount++;
        }
        public void setBezierInEdgeSet(PointClass p1, PointClass p2, PointClass p3, PointClass p4, MySketchMatrix SM)
        {            
            int a = SM.pointIdToIndex[p1.ID];
            int b = SM.pointIdToIndex[p2.ID];
            int c = SM.pointIdToIndex[p3.ID];
            int d = SM.pointIdToIndex[p4.ID];
            SketchEndPointEdge epEdge1 = new SketchEndPointEdge(a, b);
            EdgeSet[BordeCount++] = epEdge1;
            SketchEndPointEdge epEdge2 = new SketchEndPointEdge(b, c);
            EdgeSet[BordeCount++] = epEdge2;
            SketchEndPointEdge epEdge3 = new SketchEndPointEdge(c, d);
            EdgeSet[BordeCount++] = epEdge3;
        }
        //判断Line是否在边集中重复
        public int isEdgeLineRepeat(PointClass p1, PointClass p2, MySketchMatrix SM)
        {
            int start = SM.pointIdToIndex[p1.ID];
            int end = SM.pointIdToIndex[p2.ID];
            int flag = 0;
            for (int i = 0; i < SM.SEG.BordeCount; i++)
            {
                if( EdgeSet[i].EndPointPair.Key == start && EdgeSet[i].EndPointPair.Value == end )
                {
                    flag = 1;
                }
                if( EdgeSet[i].EndPointPair.Key == end && EdgeSet[i].EndPointPair.Value == start)
                {
                    flag = 1;
                }                
            }
            if(flag == 1)
            {
                return 1;
            }
            return 0;
        }
        //判断Bezier是否在边集中重复
        public int isEdgeBezierRepeat(PointClass p1,PointClass p2, PointClass p3, PointClass p4, MySketchMatrix SM)
        {
            int start = SM.pointIdToIndex[p1.ID];
            int assist1 = SM.pointIdToIndex[p2.ID];
            int assist2 = SM.pointIdToIndex[p3.ID];
            int end = SM.pointIdToIndex[p4.ID];
            int flag1 = 0, flag2 = 0;
            for (int i = 0; i < SM.SEG.BordeCount; i++)
            {
                if (EdgeSet[i].EndPointPair.Key == start && EdgeSet[i].EndPointPair.Value == assist1)
                {
                    flag1 = 1;
                }
                if (EdgeSet[i].EndPointPair.Key == assist1 && EdgeSet[i].EndPointPair.Value == start)
                {
                    flag1 = 1;
                }
                if (EdgeSet[i].EndPointPair.Key == assist2 && EdgeSet[i].EndPointPair.Value == end)
                {
                    flag2 = 1;
                }
                if (EdgeSet[i].EndPointPair.Key == end && EdgeSet[i].EndPointPair.Value == assist2)
                {
                    flag2 = 1;
                }
            }
            if(flag1 == 1 && flag2 == 1)
            {
                return 1;
            }
            return 0;
        }

        //测试边图的建立
        public void printAllEdge()
        {
            for (int i = 0; i < BordeCount; i++)
            {
                SketchEndPointEdge e = EdgeSet[i];                
                Debug.Write(i + ": ");
                Debug.Write("<" + e.EndPointPair.Key + "," + e.EndPointPair.Value + ">");                
                Debug.Write("\n");
            }
        }
    }
}
