using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFeatures
{
    
    //草图操作类,用于对草图进行相关操作
    public class SketchOperation
    {
        private int[] visited;
        private int innerStep;     //用于判断环提取操作时，是不是反向线段导致的错误环
        private int isRecall;       //用于判断递归时，遍历到已访问的点时，进行回退操作
        private List<int> loop_stack;    //用于在进行环提取操作时，存放环节点
        private int heap;            //环的开头节点，用于提取环
        private PointClass[] tempPointSet;         //临时点集，用于环提取时，排除已经成环的点
        private SketchEndPointEdge[] tempEdgeSet;       //临时边集，用于环提取，排除已成环的边
        private int temp;              //作为环提取算法中，回退时用到的临时顶点标记
        private int flagFindALoop;      //标志，找到了一个环，用于找到一个环后脱出DFS递归
        public SketchOperation() { }

        public void CreateSketchSegment(MySketchMatrix SM)
        {

            SketchRelation swSkRel = default;
            //DisplayDimension dispDim = default;
            SketchPoint swSkPt = default;
            SketchSegment swSkSeg = default;
            SketchLine swSkLine = default;
            SelectData swSelData = default;

            object[] vSkRelArr = null;
            object[] vSkSegArr = null;
            int[] vEntTypeArr = null;
            object[] vEntArr = null;
            object[] vDefEntArr = null;

            int i = 0, j = 0, k = 0;
            int num = 0;
            int lc = 0;
            int bc = 0;
            int cc = 0;
            bool bRet = false;

            StraightLineClass line = new StraightLineClass();

            Feature swFeat = (Feature)SM.sketch;
            Sketch swSketch = (Sketch)swFeat.GetSpecificFeature2();
            SketchRelationManager swSkRelMgr = swSketch.RelationManager;

            //Debug.Print("  Feat = " + swFeat.Name);                  //输出草图名字

            vSkSegArr = (object[])swSketch.GetSketchSegments();

            foreach (SketchSegment Seg in vSkSegArr)
            {
                swSketchSegments_e segType = (swSketchSegments_e)Seg.GetType();
                switch (segType)
                {
                    case swSketchSegments_e.swSketchLINE:
                        lc++;
                        break;
                    case swSketchSegments_e.swSketchARC:
                        cc++;
                        break;
                    case swSketchSegments_e.swSketchELLIPSE:
                        break;
                    case swSketchSegments_e.swSketchSPLINE:
                        bc++;
                        break;
                    case swSketchSegments_e.swSketchPARABOLA:
                        bc++;
                        break;
                    default:
                        break;
                }
            }

            SM.setStraightList(lc);
            SM.setCircleList(cc);
            SM.setBezierList(bc);
            SM.setAllList();
            
            foreach (SketchSegment Seg in vSkSegArr)
            {
                swSketchSegments_e segType = (swSketchSegments_e)Seg.GetType();
                switch (segType)
                {
                    case swSketchSegments_e.swSketchLINE:
                        SketchLine skLine = (SketchLine)Seg;
                        StraightLineClass sL = new StraightLineClass();
                        SketchPoint ls = skLine.GetStartPoint2();
                        SketchPoint le = skLine.GetEndPoint2();
                        PointClass p1 = new PointClass
                        {
                            x = ls.X,
                            y = ls.Y,
                            z = ls.Z,
                        };
                        p1.setPointAndID(ls);
                        PointClass p2 = new PointClass
                        {
                            x = le.X,
                            y = le.Y,
                            z = le.Z
                        };
                        p2.setPointAndID(le);
                        sL.setPoint(p1, p2);
                        SM.straightList.Add(sL);

                        break;
                    case swSketchSegments_e.swSketchARC:
                        SketchArc skArc = (SketchArc)Seg;
                        CircleClass cir = new CircleClass();
                        SketchPoint cen = skArc.GetCenterPoint2();
                        PointClass pc = new PointClass
                        {
                            x = cen.X,
                            y = cen.Y,
                            z = cen.Z
                        };
                        pc.setPointAndID(cen);
                        double r = skArc.GetRadius();
                        int flag = skArc.IsCircle();                //判断是否是个完整的圆
                        cir.setIsComplete(flag);
                        if (flag == 1)
                        {
                            //对同心圆情况进行判断（完整圆通过对圆心ID进行改变来赋值，所以同心圆需增加赋值幅度）
                            foreach(CircleClass comCircle in SM.circleList)
                            {
                                if (comCircle == null)
                                    break;
                                PointClass comCen = comCircle.getPointCenter();
                                //Debug.WriteLine("赋值圆心ID："+ pc.getPointID() + " 和 遍历圆心ID：" + comCen.getPointID());
                                if (comCen.getPointID() == pc.getPointID()) 
                                {
                                    Debug.WriteLine("增幅增加了");
                                    pc.completeArcAmplitude++;
                                }
                            }
                            cir.setCompleteArc(pc, r);
                        }
                        else if (flag == 0)
                        {
                            SketchPoint c1 = skArc.GetStartPoint2();
                            SketchPoint c2 = skArc.GetEndPoint2();
                            PointClass ps = new PointClass()
                            {
                                x = c1.X,
                                y = c1.Y,
                                z = c1.Z
                            };
                            ps.setPointAndID(c1);
                            PointClass pe = new PointClass()
                            {
                                x = c2.X,
                                y = c2.Y,
                                z = c2.Z
                            };
                            pe.setPointAndID(c2);
                            cir.setNotCompleteArc(pc, r, ps, pe);
                        }
                        SM.circleList.Add(cir);

                        break;
                    case swSketchSegments_e.swSketchELLIPSE:
                        Debug.WriteLine("ellipse先放一放~~");

                        break;
                    case swSketchSegments_e.swSketchSPLINE:
                        SketchSpline skSpline = (SketchSpline)Seg;
                        int pCount = skSpline.GetPointCount();
                        SketchPoint[] pSpline = skSpline.GetPoints2();
                        BezierClass bezSpl = new BezierClass();
                        PointClass[] bezSplineArr = new PointClass[4];

                        int pSpan = (pCount - 4) / 3 + 1;
                        int[] fourPoint = { 0, pSpan, pCount - 1 - pSpan, pCount - 1 };
                        for (int pNum = 0; pNum < 4; pNum++)
                        {
                            PointClass p = new PointClass
                            {
                                x = pSpline[fourPoint[pNum]].X,
                                y = pSpline[fourPoint[pNum]].Y,
                                z = pSpline[fourPoint[pNum]].Z
                            };
                            p.setPointAndID(pSpline[fourPoint[pNum]]);
                            bezSplineArr[pNum] = p;
                        }
                        bezSpl.setBezierPoint(bezSplineArr[0], bezSplineArr[1], bezSplineArr[2], bezSplineArr[3]);
                        SM.bezierList.Add(bezSpl);

                        break;
                    case swSketchSegments_e.swSketchPARABOLA:
                        SketchParabola skParabola = (SketchParabola)Seg;
                        SketchPoint[] parArr = new SketchPoint[4];
                        SketchPoint psPar = skParabola.GetStartPoint2();
                        SketchPoint pePar = skParabola.GetEndPoint2();
                        SketchPoint paPar = skParabola.GetApexPoint2();
                        SketchPoint pfPar = skParabola.GetFocalPoint2();
                        BezierClass bezPar = new BezierClass();
                        parArr[0] = psPar;
                        parArr[1] = paPar;
                        parArr[2] = pfPar;
                        parArr[3] = pePar;
                        PointClass[] bezParabolaArr = new PointClass[4];
                        for (int pNum = 0; pNum < 4; pNum++)
                        {
                            PointClass p = new PointClass
                            {
                                x = parArr[pNum].X,
                                y = parArr[pNum].Y,
                                z = parArr[pNum].Z
                            };
                            p.setPointAndID(parArr[pNum]);
                            bezParabolaArr[pNum] = p;
                        }
                        bezPar.setBezierPoint(bezParabolaArr[0], bezParabolaArr[1], bezParabolaArr[2], bezParabolaArr[3]);
                        SM.bezierList.Add(bezPar);

                        break;
                    default:
                        break;
                }

                //Debug.WriteLine(num + "、" + Seg.GetName() + segType);             //输出草图seg名字和类型
                num++;
            }

            //此处原引用elseFun()中的程序块

        }

        public void elseFun()
        {
            /*
            vSkRelArr = (object[])swSkRelMgr.GetRelations((int)swSketchRelationFilterType_e.swAll);
            if (vSkRelArr == null)
            {
                Debug.WriteLine("我空了！");
                return;
            }

            foreach (SketchRelation vRel in vSkRelArr)
            {
                swSkRel = vRel;

                Debug.Print("    Relation(" + i + ")");
                Debug.Print("      Type         = " + swSkRel.GetRelationType());

                dispDim = (DisplayDimension)swSkRel.GetDisplayDimension();
                if (dispDim != null)
                {
                    Debug.Print("      Display dimension         = " + dispDim.GetNameForSelection());
                }

                vEntTypeArr = (int[])swSkRel.GetEntitiesType();
                vEntArr = (object[])swSkRel.GetEntities();

                vDefEntArr = (object[])swSkRel.GetDefinitionEntities2();
                if (vDefEntArr == null)
                {
                }
                else
                {
                    Debug.Print("    Number of definition entities in this relation: " + vDefEntArr.GetUpperBound(0));
                }

                if (vEntTypeArr != null & vEntArr != null)
                {
                    if (vEntTypeArr.GetUpperBound(0) == vEntArr.GetUpperBound(0))
                    {
                        j = 0;

                        foreach (swSketchRelationEntityTypes_e vType in vEntTypeArr)
                        {
                            Debug.Print("        EntType    = " + vType);

                            switch (vType)
                            {
                                case swSketchRelationEntityTypes_e.swSketchRelationEntityType_Unknown:
                                    Debug.Print("          Not known");

                                    break;
                                case swSketchRelationEntityTypes_e.swSketchRelationEntityType_SubSketch:
                                    Debug.Print("SubSketch");

                                    break;
                                case swSketchRelationEntityTypes_e.swSketchRelationEntityType_Point:
                                    swSkPt = (SketchPoint)vEntArr[j];
                                    Debug.Assert(swSkPt != null);

                                    Debug.Print("          SkPoint ID = [" + ((int[])swSkPt.GetID())[0] + ", " + ((int[])swSkPt.GetID())[1] + "]");

                                    bRet = swSkPt.Select4(true, swSelData);
                                    
                                    break;

                                case swSketchRelationEntityTypes_e.swSketchRelationEntityType_Line:
                                    swSkLine = (SketchLine)vEntArr[j];

                                    break;

                                case swSketchRelationEntityTypes_e.swSketchRelationEntityType_Arc:
                                case swSketchRelationEntityTypes_e.swSketchRelationEntityType_Ellipse:
                                case swSketchRelationEntityTypes_e.swSketchRelationEntityType_Parabola:
                                case swSketchRelationEntityTypes_e.swSketchRelationEntityType_Spline:
                                    swSkSeg = (SketchSegment)vEntArr[j];

                                    Debug.Print("          SkSeg   ID = [" + ((int[])swSkSeg.GetID())[0] + ", " + ((int[])swSkSeg.GetID())[1] + "]");

                                    bRet = swSkSeg.Select4(true, swSelData);

                                    break;
                                case swSketchRelationEntityTypes_e.swSketchRelationEntityType_Hatch:
                                    Debug.Print("Hatch");

                                    break;
                                case swSketchRelationEntityTypes_e.swSketchRelationEntityType_Text:
                                    Debug.Print("Text");

                                    break;
                                case swSketchRelationEntityTypes_e.swSketchRelationEntityType_Plane:
                                    Debug.Print("Plane");

                                    break;
                                case swSketchRelationEntityTypes_e.swSketchRelationEntityType_Cylinder:
                                    Debug.Print("Cylinder");

                                    break;
                                case swSketchRelationEntityTypes_e.swSketchRelationEntityType_Sphere:
                                    Debug.Print("Sphere");

                                    break;
                                case swSketchRelationEntityTypes_e.swSketchRelationEntityType_Surface:
                                    Debug.Print("Surface");

                                    break;
                                case swSketchRelationEntityTypes_e.swSketchRelationEntityType_Dimension:
                                    Debug.Print("Dimension");

                                    break;
                                default:
                                    Debug.Print("Something else");

                                    break;
                            }

                            j = j + 1;

                        }
                    }
                }
                i = i + 1;
                Debug.Write("\n");
            }
            */
        }

        public void testSketchSeg(MySketchMatrix SM)
        {
            Feature fea = (Feature)SM.sketch;
            Debug.WriteLine("草图名字：" + fea.Name);
            Debug.WriteLine("——草图内容：直线数-" + SM.lineCount +
                " | 贝塞尔数-" + SM.bezierCount + " | 圆与圆弧数-" + SM.circleCount);
            for (int i = 0; i < SM.lineCount; i++)
            {
                StraightLineClass teLine = SM.straightList[i];
                PointClass Ls = teLine.getPointStart();
                PointClass Le = teLine.getPointEnd();
                Debug.WriteLine("————线" + i + "号：[" + Ls.ID.Item1 + "," + Ls.ID.Item2 +
                    "] | [" + Le.ID.Item1 + "," + Le.ID.Item2 + "]");
            }
            for (int i = 0; i < SM.circleCount; i++)
            {
                CircleClass teCircle = SM.circleList[i];
                PointClass Cc = teCircle.getPointCenter();
                if(teCircle.getIsComplete() == 1)                    //对于圆的测试分为整圆和圆弧
                {
                    PointClass Cup = teCircle.getPointUp();
                    PointClass Cdown = teCircle.getPointDown();
                    PointClass Cleft = teCircle.getPointLeft();
                    PointClass Cright = teCircle.getPointRight();
                    Debug.WriteLine("————圆" + i + "号：[" + Cup.ID.Item1 + "," + Cup.ID.Item2 + "] | [" 
                        + Cleft.ID.Item1 + "," + Cleft.ID.Item2 + "] | [" 
                        + Cdown.ID.Item1 + "," + Cdown.ID.Item2 + "] | ["
                        + Cright.ID.Item1 + "," + Cright.ID.Item2 + "]");
                }
                else
                {
                    PointClass Cstart = teCircle.getPointStart();
                    PointClass Cend = teCircle.getPointEnd();
                    Debug.WriteLine("————圆" + i + "号：[" + Cstart.ID.Item1 + "," + Cstart.ID.Item2 + "] | ["
                        + Cend.ID.Item1 + "," + Cend.ID.Item2 + "]");
                }              
            }
            for (int i = 0; i < SM.bezierCount; i++)
            {
                BezierClass teBezier = SM.bezierList[i];
                PointClass Bc = teBezier.getPointStart();
                Debug.WriteLine("————贝塞尔" + i + "号：[" + Bc.ID.Item1 + "," + Bc.ID.Item2 + "]");
            }
        }

        /// <summary>
        /// 建立草图中点-ID到索引-的字典。同时，构建点的图结构，以描述草图状态——SPG
        /// </summary>
        /// <param name="SM"></param>
        public void buildPointIdToIndexAndPointGraph(MySketchMatrix SM)
        {
            int index = 0;
            //对直线组进行遍历建图、建字典
            for (int i = 0; i < SM.lineCount; i++)
            {
                PointClass pStart = SM.straightList[i].getPointStart();
                PointClass pEnd = SM.straightList[i].getPointEnd();
                pStart.setEndFlag();
                pEnd.setEndFlag();
                int flag1 = 0, flag2 = 0;
                if (!SM.pointIdToIndex.ContainsKey(pStart.ID))
                {
                    SM.pointIdToIndex.Add(pStart.ID, index++);
                    flag1++;
                }
                if (!SM.pointIdToIndex.ContainsKey(pEnd.ID))
                {
                    SM.pointIdToIndex.Add(pEnd.ID, index++);
                    flag2++;
                }
                //做判断，需要构建图结构的点是否已在图中，以此判断如何将此线段并入图中            
                if (flag1 == 1 && flag2 == 1)
                {
                    pStart.firseEdge.adjvex = SM.pointIdToIndex[pEnd.ID];
                    SM.SPG.setPointSet(pStart, SM);
                    pEnd.firseEdge.adjvex = SM.pointIdToIndex[pStart.ID];
                    SM.SPG.setPointSet(pEnd, SM);
                }
                else if (flag1 == 1 && flag2 == 0)
                {
                    pStart.firseEdge.adjvex = SM.pointIdToIndex[pEnd.ID];
                    SM.SPG.setPointSet(pStart, SM);
                    SketchEdgeNode pEdge = new SketchEdgeNode();
                    SketchEdgeNode lastEdge = SM.SPG.pointSet[SM.pointIdToIndex[pEnd.ID]].firseEdge;
                    while (lastEdge.next != null)
                    {
                        lastEdge = lastEdge.next;
                    }
                    pEdge.adjvex = SM.pointIdToIndex[pStart.ID];
                    lastEdge.next = pEdge;
                }
                else if (flag1 == 0 && flag2 == 1)
                {
                    pEnd.firseEdge.adjvex = SM.pointIdToIndex[pStart.ID];
                    SM.SPG.setPointSet(pEnd, SM);
                    SketchEdgeNode pEdge = new SketchEdgeNode();
                    SketchEdgeNode lastEdge = SM.SPG.pointSet[SM.pointIdToIndex[pStart.ID]].firseEdge;
                    while (lastEdge.next != null)
                    {
                        lastEdge = lastEdge.next;
                    }
                    pEdge.adjvex = SM.pointIdToIndex[pEnd.ID];
                    lastEdge.next = pEdge;
                }
                else if (flag1 == 0 && flag2 == 0)
                {
                    int flagReptition = 0;                //逻辑判断，为了排除重复线导致的边关系重复并入的问题               
                    SketchEdgeNode pEdge1 = new SketchEdgeNode();      //对PStart的边表进行遍历，将新的边插入
                    SketchEdgeNode lastEdge = SM.SPG.pointSet[SM.pointIdToIndex[pStart.ID]].firseEdge;
                    while (lastEdge.next != null)
                    {
                        if (lastEdge.adjvex == SM.pointIdToIndex[pEnd.ID])
                            flagReptition = 1;
                        lastEdge = lastEdge.next;
                    }
                    if (lastEdge.adjvex == SM.pointIdToIndex[pEnd.ID])
                        flagReptition = 1;
                    if (flagReptition == 0)
                    {
                        pEdge1.adjvex = SM.pointIdToIndex[pEnd.ID];
                        lastEdge.next = pEdge1;
                    }

                    flagReptition = 0;                     //逻辑判断，为了排除重复线导致的边关系重复并入的问题
                    SketchEdgeNode pEdge2 = new SketchEdgeNode();      // 对pEnd的边表进行遍历，将新的边插入
                    lastEdge = SM.SPG.pointSet[SM.pointIdToIndex[pEnd.ID]].firseEdge;
                    while (lastEdge.next != null)
                    {
                        if (lastEdge.adjvex == SM.pointIdToIndex[pStart.ID])
                            flagReptition = 1;
                        lastEdge = lastEdge.next;
                    }
                    if (lastEdge.adjvex == SM.pointIdToIndex[pStart.ID])
                        flagReptition = 1;
                    if (flagReptition == 0)
                    {
                        pEdge2.adjvex = SM.pointIdToIndex[pStart.ID];
                        lastEdge.next = pEdge2;
                    }
                }
                //做判断，判断该直线边是否在边集中，不在则将该边对应端点对并入边集       
                if (SM.SPG.isEdgeLineRepeat(pStart, pEnd, SM) == 0)
                {
                    SM.SPG.setLineInEdgeSet(pStart, pEnd, SM);
                }
            }
            //对圆或弧组进行遍历建图、建字典
            for (int i = 0; i < SM.circleCount; i++)
            {
                CircleClass cir = SM.circleList[i];
                if (cir.getIsComplete() == 1)        //若为整圆，
                {
                    SM.completeCircleNum.Add(i);
                    SM.pointIdToIndex.Add(cir.getPointUp().ID, index++);
                    SM.pointIdToIndex.Add(cir.getPointDown().ID, index++);
                    SM.pointIdToIndex.Add(cir.getPointLeft().ID, index++);
                    SM.pointIdToIndex.Add(cir.getPointRight().ID, index++);
                    PointClass pUp = cir.getPointUp();
                    PointClass pLeft = cir.getPointLeft();
                    PointClass pDown = cir.getPointDown();
                    PointClass pRight = cir.getPointRight();
                    pUp.setCircleFlag();                        //给构造整圆的点标上Flag，用于环内的Seg区分
                    pLeft.setCircleFlag();
                    pDown.setCircleFlag();
                    pRight.setCircleFlag();
                    SketchEdgeNode nextUp = new SketchEdgeNode();
                    SketchEdgeNode nextLeft = new SketchEdgeNode();
                    SketchEdgeNode nextDown = new SketchEdgeNode();
                    SketchEdgeNode nextRight = new SketchEdgeNode();
                    pUp.firseEdge.adjvex = SM.pointIdToIndex[pRight.ID];            //关于pUp的点图建立
                    nextUp.adjvex = SM.pointIdToIndex[pLeft.ID];
                    pUp.firseEdge.next = nextUp;
                    SM.SPG.setPointSet(pUp, SM);
                    pRight.firseEdge.adjvex = SM.pointIdToIndex[pDown.ID];          //关于pRight的点图建立
                    nextRight.adjvex = SM.pointIdToIndex[pUp.ID]; 
                    pRight.firseEdge.next = nextRight;
                    SM.SPG.setPointSet(pRight, SM);
                    pDown.firseEdge.adjvex = SM.pointIdToIndex[pLeft.ID];           //关于pDown的点图建立
                    nextDown.adjvex = SM.pointIdToIndex[pRight.ID];
                    pDown.firseEdge.next = nextDown;
                    SM.SPG.setPointSet(pDown, SM);
                    pLeft.firseEdge.adjvex = SM.pointIdToIndex[pUp.ID];             //关于pLeft的点图建立
                    nextLeft.adjvex = SM.pointIdToIndex[pDown.ID];
                    pLeft.firseEdge.next = nextLeft;                    
                    SM.SPG.setPointSet(pLeft, SM);

                    SM.SPG.setLineInEdgeSet(pUp, pRight, SM);
                    SM.SPG.setLineInEdgeSet(pRight, pDown, SM);
                    SM.SPG.setLineInEdgeSet(pDown, pLeft, SM);
                    SM.SPG.setLineInEdgeSet(pLeft, pUp, SM);
                }
                else if (cir.getIsComplete() == 0)      //若circle为圆弧，则按找直线方式进行处理
                {
                    PointClass pStart = cir.getPointStart();
                    PointClass pEnd = cir.getPointEnd();
                    pStart.setArcFlag();
                    pStart.setEndFlag();
                    pEnd.setArcFlag();
                    pEnd.setEndFlag();
                    int flag1 = 0, flag2 = 0;
                    if (!SM.pointIdToIndex.ContainsKey(pStart.ID))
                    {
                        SM.pointIdToIndex.Add(pStart.ID, index++);
                        flag1++;
                    }
                    if (!SM.pointIdToIndex.ContainsKey(pEnd.ID))
                    {
                        SM.pointIdToIndex.Add(pEnd.ID, index++);
                        flag2++;
                    }
                    //做判断，需要构建图结构的点是否已在图中，以此判断如何将此线段并入图中            
                    if (flag1 == 1 && flag2 == 1)
                    {
                        pStart.firseEdge.adjvex = SM.pointIdToIndex[pEnd.ID];
                        SM.SPG.setPointSet(pStart, SM);
                        pEnd.firseEdge.adjvex = SM.pointIdToIndex[pStart.ID];
                        SM.SPG.setPointSet(pEnd, SM);
                    }
                    else if (flag1 == 1 && flag2 == 0)
                    {
                        pStart.firseEdge.adjvex = SM.pointIdToIndex[pEnd.ID];
                        SM.SPG.setPointSet(pStart, SM);
                        SketchEdgeNode pEdge = new SketchEdgeNode();
                        SketchEdgeNode lastEdge = SM.SPG.pointSet[SM.pointIdToIndex[pEnd.ID]].firseEdge;
                        while (lastEdge.next != null)
                        {
                            lastEdge = lastEdge.next;
                        }
                        pEdge.adjvex = SM.pointIdToIndex[pStart.ID];
                        lastEdge.next = pEdge;
                    }
                    else if (flag1 == 0 && flag2 == 1)
                    {
                        pEnd.firseEdge.adjvex = SM.pointIdToIndex[pStart.ID];
                        SM.SPG.setPointSet(pEnd, SM);
                        SketchEdgeNode pEdge = new SketchEdgeNode();
                        SketchEdgeNode lastEdge = SM.SPG.pointSet[SM.pointIdToIndex[pStart.ID]].firseEdge;
                        while (lastEdge.next != null)
                        {
                            lastEdge = lastEdge.next;
                        }
                        pEdge.adjvex = SM.pointIdToIndex[pEnd.ID];
                        lastEdge.next = pEdge;
                    }
                    else if (flag1 == 0 && flag2 == 0)
                    {
                        int flagReptition = 0;                //逻辑判断，为了排除重复线导致的边关系重复并入的问题               
                        SketchEdgeNode pEdge1 = new SketchEdgeNode();      //对PStart的边表进行遍历，将新的边插入
                        SketchEdgeNode lastEdge = SM.SPG.pointSet[SM.pointIdToIndex[pStart.ID]].firseEdge;
                        while (lastEdge.next != null)
                        {
                            if (lastEdge.adjvex == SM.pointIdToIndex[pEnd.ID])
                                flagReptition = 1;
                            lastEdge = lastEdge.next;
                        }
                        if (lastEdge.adjvex == SM.pointIdToIndex[pEnd.ID])
                            flagReptition = 1;
                        if (flagReptition == 0)
                        {
                            pEdge1.adjvex = SM.pointIdToIndex[pEnd.ID];
                            lastEdge.next = pEdge1;
                        }

                        flagReptition = 0;                     //逻辑判断，为了排除重复线导致的边关系重复并入的问题
                        SketchEdgeNode pEdge2 = new SketchEdgeNode();      // 对pEnd的边表进行遍历，将新的边插入
                        lastEdge = SM.SPG.pointSet[SM.pointIdToIndex[pEnd.ID]].firseEdge;
                        while (lastEdge.next != null)
                        {
                            if (lastEdge.adjvex == SM.pointIdToIndex[pStart.ID])
                                flagReptition = 1;
                            lastEdge = lastEdge.next;
                        }
                        if (lastEdge.adjvex == SM.pointIdToIndex[pStart.ID])
                            flagReptition = 1;
                        if (flagReptition == 0)
                        {
                            pEdge2.adjvex = SM.pointIdToIndex[pStart.ID];
                            lastEdge.next = pEdge2;                            
                        }
                    }
                    //做判断，判断该直线边是否在边集中，不在则将该边对应端点对并入边集（由于按直线处理，调用Line的并入函数）      
                    if (SM.SPG.isEdgeLineRepeat(pStart, pEnd, SM) == 0)
                    {
                        SM.SPG.setLineInEdgeSet(pStart, pEnd, SM);
                    }
                }
            }
            //对贝塞尔组进行遍历建图、建字典   该块代码暂未考虑样条重复线的可能性
            for (int i = 0; i < SM.bezierCount; i++)
            {
                int flag1 = 0, flag2 = 0;
                PointClass pStart = SM.bezierList[i].getPointStart();
                pStart.setEndFlag();
                if (!SM.pointIdToIndex.ContainsKey(pStart.ID))
                {
                    SM.pointIdToIndex.Add(pStart.ID, index++);
                    flag1++;
                }

                PointClass pAssist1 = SM.bezierList[i].getPointAssist1();   //对辅助点1进行并入点集
                pAssist1.setAssistFlag();
                if (!SM.pointIdToIndex.ContainsKey(pAssist1.ID))
                    SM.pointIdToIndex.Add(pAssist1.ID, index++);
                SM.SPG.setPointSet(pAssist1, SM);

                PointClass pAssist2 = SM.bezierList[i].getPointAssist2();    //对辅助点2进行并入点集
                pAssist2.setAssistFlag();
                if (!SM.pointIdToIndex.ContainsKey(pAssist2.ID))
                    SM.pointIdToIndex.Add(pAssist2.ID, index++);
                SM.SPG.setPointSet(pAssist2, SM);

                PointClass pEnd = SM.bezierList[i].getPointEnd();
                pEnd.setEndFlag();
                if (!SM.pointIdToIndex.ContainsKey(pEnd.ID))
                {
                    SM.pointIdToIndex.Add(pEnd.ID, index++);
                    flag2++;
                }

                //做判断，需要构建图结构的点是否已在图中，以此判断如何将此线段并入图中            
                if (flag1 == 1 && flag2 == 1)         //两个点原本都不在点集中
                {
                    pStart.firseEdge.adjvex = SM.pointIdToIndex[pAssist1.ID];
                    SM.SPG.setPointSet(pStart, SM);

                    SketchEdgeNode EdgeA1ToA2 = new SketchEdgeNode();
                    pAssist1.firseEdge.adjvex = SM.pointIdToIndex[pStart.ID];
                    pAssist1.firseEdge.next = EdgeA1ToA2;
                    EdgeA1ToA2.adjvex = SM.pointIdToIndex[pAssist2.ID];
                    SketchEdgeNode EdgeA2ToA1 = new SketchEdgeNode();
                    pAssist2.firseEdge.adjvex = SM.pointIdToIndex[pAssist1.ID];
                    pAssist2.firseEdge.next = EdgeA2ToA1;
                    EdgeA2ToA1.adjvex = SM.pointIdToIndex[pEnd.ID];

                    pEnd.firseEdge.adjvex = SM.pointIdToIndex[pAssist2.ID];
                    SM.SPG.setPointSet(pEnd, SM);
                }
                else if (flag1 == 1 && flag2 == 0)     //pStart刚放入点集，pEnd原本就在点集
                {
                    pStart.firseEdge.adjvex = SM.pointIdToIndex[pAssist1.ID];
                    SM.SPG.setPointSet(pStart, SM);

                    SketchEdgeNode EdgeA1ToA2 = new SketchEdgeNode();
                    pAssist1.firseEdge.adjvex = SM.pointIdToIndex[pStart.ID];
                    pAssist1.firseEdge.next = EdgeA1ToA2;
                    EdgeA1ToA2.adjvex = SM.pointIdToIndex[pAssist2.ID];
                    SketchEdgeNode EdgeA2ToA1 = new SketchEdgeNode();
                    pAssist2.firseEdge.adjvex = SM.pointIdToIndex[pAssist1.ID];
                    pAssist2.firseEdge.next = EdgeA2ToA1;
                    EdgeA2ToA1.adjvex = SM.pointIdToIndex[pEnd.ID];

                    SketchEdgeNode pEdge = new SketchEdgeNode();
                    SketchEdgeNode lastEdge = SM.SPG.pointSet[SM.pointIdToIndex[pEnd.ID]].firseEdge;
                    while (lastEdge.next != null)
                    {
                        lastEdge = lastEdge.next;
                    }
                    pEdge.adjvex = SM.pointIdToIndex[pAssist2.ID];
                    lastEdge.next = pEdge;
                }
                else if (flag1 == 0 && flag2 == 1)       //pStart原本就在点集，pEnd刚放入点集
                {
                    pEnd.firseEdge.adjvex = SM.pointIdToIndex[pAssist2.ID];
                    SM.SPG.setPointSet(pEnd, SM);

                    SketchEdgeNode EdgeA1ToA2 = new SketchEdgeNode();
                    pAssist1.firseEdge.adjvex = SM.pointIdToIndex[pStart.ID];
                    pAssist1.firseEdge.next = EdgeA1ToA2;
                    EdgeA1ToA2.adjvex = SM.pointIdToIndex[pAssist2.ID];
                    SketchEdgeNode EdgeA2ToA1 = new SketchEdgeNode();
                    pAssist2.firseEdge.adjvex = SM.pointIdToIndex[pAssist1.ID];
                    pAssist2.firseEdge.next = EdgeA2ToA1;
                    EdgeA2ToA1.adjvex = SM.pointIdToIndex[pEnd.ID];

                    SketchEdgeNode pEdge = new SketchEdgeNode();
                    SketchEdgeNode lastEdge = SM.SPG.pointSet[SM.pointIdToIndex[pStart.ID]].firseEdge;
                    while (lastEdge.next != null)
                    {
                        lastEdge = lastEdge.next;
                    }
                    pEdge.adjvex = SM.pointIdToIndex[pAssist1.ID];
                    lastEdge.next = pEdge;
                }
                else if (flag1 == 0 && flag2 == 0)            //两个点原本就都在点集
                {
                    SketchEdgeNode pEdge1 = new SketchEdgeNode();      //对pStart的边表进行遍历，将新的边插入
                    SketchEdgeNode lastEdge = SM.SPG.pointSet[SM.pointIdToIndex[pStart.ID]].firseEdge;
                    while (lastEdge.next != null)
                    {
                        lastEdge = lastEdge.next;
                    }
                    pEdge1.adjvex = SM.pointIdToIndex[pAssist1.ID];
                    lastEdge.next = pEdge1;

                    SketchEdgeNode EdgeA1ToA2 = new SketchEdgeNode();
                    pAssist1.firseEdge.adjvex = SM.pointIdToIndex[pStart.ID];
                    pAssist1.firseEdge.next = EdgeA1ToA2;
                    EdgeA1ToA2.adjvex = SM.pointIdToIndex[pAssist2.ID];
                    SketchEdgeNode EdgeA2ToA1 = new SketchEdgeNode();
                    pAssist2.firseEdge.adjvex = SM.pointIdToIndex[pAssist1.ID];
                    pAssist2.firseEdge.next = EdgeA2ToA1;
                    EdgeA2ToA1.adjvex = SM.pointIdToIndex[pEnd.ID];

                    SketchEdgeNode pEdge2 = new SketchEdgeNode();      // 对pEnd的边表进行遍历，将新的边插入
                    lastEdge = SM.SPG.pointSet[SM.pointIdToIndex[pEnd.ID]].firseEdge;
                    while (lastEdge.next != null)
                    {
                        lastEdge = lastEdge.next;
                    }
                    pEdge2.adjvex = SM.pointIdToIndex[pAssist2.ID];
                    lastEdge.next = pEdge2;
                }
                //做判断，判断该贝塞尔边是否在边集中，不在则将该边对应端点对并入边集           
                if (SM.SPG.isEdgeBezierRepeat(pStart, pAssist1, pAssist2, pEnd, SM) == 0)
                {
                    SM.SPG.setBezierInEdgeSet(pStart, pAssist1, pAssist2, pEnd, SM);
                }
            }
        }

        /// <summary>
        /// 建立草图中点-ID到索引-的字典。同时，构建边的图结构，以描述草图状态——SEG
        /// </summary>
        /// <param name="SM"></param>
        public void buildPointIdToIndexAndEdgeGraph(MySketchMatrix SM)
        {
            int index = 0;
            //对直线组进行遍历建图、建字典
            for (int i = 0; i < SM.lineCount; i++)
            {
                PointClass pStart = SM.straightList[i].getPointStart();
                PointClass pEnd = SM.straightList[i].getPointEnd();
                if (!SM.pointIdToIndex.ContainsKey(pStart.ID))
                {
                    SM.pointIdToIndex.Add(pStart.ID, index++);
                }
                if (!SM.pointIdToIndex.ContainsKey(pEnd.ID))
                {
                    SM.pointIdToIndex.Add(pEnd.ID, index++);
                }
                //做判断，判断该直线边是否在边集中，不在则将该边对应端点对并入边集       
                if(SM.SEG.isEdgeLineRepeat(pStart,pEnd,SM) == 0)
                {
                    SM.SEG.setLineInEdgeSet(pStart, pEnd, SM);
                }
            }
            //对圆或弧组进行遍历建图、建字典
            for (int i = 0; i < SM.circleCount; i++)
            {
                CircleClass cir = SM.circleList[i];
                if (cir.getIsComplete() == 1)        //若为整圆，
                {
                    SM.completeCircleNum.Add(i);
                }
                else if (cir.getIsComplete() == 0)      //若circle为圆弧，则按找直线方式进行处理
                {
                    PointClass pStart = cir.getPointStart();
                    PointClass pEnd = cir.getPointEnd();
                    if (!SM.pointIdToIndex.ContainsKey(pStart.ID))
                    {
                        SM.pointIdToIndex.Add(pStart.ID, index++);

                    }
                    if (!SM.pointIdToIndex.ContainsKey(pEnd.ID))
                    {
                        SM.pointIdToIndex.Add(pEnd.ID, index++);

                    }
                    //做判断，判断该直线边是否在边集中，不在则将该边对应端点对并入边集（由于按直线处理，调用Line的并入函数）      
                    if (SM.SEG.isEdgeLineRepeat(pStart, pEnd, SM) == 0)
                    {
                        SM.SEG.setLineInEdgeSet(pStart, pEnd, SM);
                    }
                }
            }
            //对贝塞尔组进行遍历建图、建字典   该块代码暂未考虑样条重复线的可能性
            for (int i = 0; i < SM.bezierCount; i++)
            {
                PointClass pStart = SM.bezierList[i].getPointStart();
                if (!SM.pointIdToIndex.ContainsKey(pStart.ID))
                {
                    SM.pointIdToIndex.Add(pStart.ID, index++);
                }

                PointClass pAssist1 = SM.bezierList[i].getPointAssist1();   //对辅助点1进行并入点集
                if (!SM.pointIdToIndex.ContainsKey(pAssist1.ID))
                    SM.pointIdToIndex.Add(pAssist1.ID, index++);

                PointClass pAssist2 = SM.bezierList[i].getPointAssist2();    //对辅助点2进行并入点集
                if (!SM.pointIdToIndex.ContainsKey(pAssist2.ID))
                    SM.pointIdToIndex.Add(pAssist2.ID, index++);

                PointClass pEnd = SM.bezierList[i].getPointEnd();
                if (!SM.pointIdToIndex.ContainsKey(pEnd.ID))
                {
                    SM.pointIdToIndex.Add(pEnd.ID, index++);
                }
                //做判断，判断该贝塞尔边是否在边集中，不在则将该边对应端点对并入边集           
                if (SM.SEG.isEdgeBezierRepeat(pStart, pAssist1, pAssist2, pEnd, SM) == 0)
                {
                    SM.SEG.setBezierInEdgeSet(pStart, pAssist1, pAssist2, pEnd, SM);
                }
            }
        }

        //建立SPG点-索引到ID-的字典
        public void buildPointIndexToId(MySketchMatrix SM)
        {
            foreach (Tuple<int, int> a in SM.pointIdToIndex.Keys)
            {
                SM.pointIndexToId.Add(SM.pointIdToIndex[a], a);
            }
        }

        //取环方法，输出：环节点对应编号
        public void getPointLoop(MySketchMatrix SM)
        {
            initializeSequenceStack();
            initializeVisited(SM);
            tempPointSet = new PointClass[SM.SPG.pointSet.Length];
            int index = 0;
            foreach(PointClass p in SM.SPG.pointSet)
            {
                tempPointSet[index++] = p;
            }
            int allVisited = 0;                  //全部访问的标记
            int NumVisited;               //已访问的数目
            //int isTempNotNULL = 1;            

            while (allVisited == 0)
            {
                int k = 0;
                int i = 0;
                PointClass p = tempPointSet[i];
                while (p == null)
                {
                    i++;
                    p = tempPointSet[i];
                }
                heap = SM.pointIdToIndex[p.ID];
                flagFindALoop = 0;
                clear_loopStack();
                innerStep = 0;
                isRecall = 0;

                //Debug.WriteLine("heap: " + heap);
                //Debug.WriteLine("\nDFS之前,环数是：" + SM.loopCount);
                //SM.outLoopTable();

                DFS1(SM.SPG, heap);
                if (flagFindALoop == 1)
                {
                    //Debug.WriteLine("\n之前test<setLoopTable>:" + SM.loopCount);
                    //SM.outLoopTable();

                    SM.setLoopTable(loop_stack);

                    //Debug.WriteLine("\n之后test<setLoopTable>:" + SM.loopCount);
                    //SM.outLoopTable();
                }

                NumVisited = 0;
                foreach (PointClass a in tempPointSet)
                {
                    k++;
                    if (a == null)
                    {
                        NumVisited++;
                    }
                    if (NumVisited == SM.SPG.VertexNodeCount)
                    {
                        allVisited = 1;
                    }
                    //Debug.WriteLine(k + ": " + allVisited + " | " + NumVisited);
                }
                //Debug.WriteLine("\n一轮循环结束:" + SM.loopCount);
                //SM.outLoopTable();
            }
        }
        public void DFS1(SketchPointGraph SPG, int startVertax)
        {
            SketchEdgeNode e;
            visited[startVertax] = 1;
            e = SPG.pointSet[startVertax].firseEdge;
            int nextVertax = e.adjvex;

            if (e.next == null)
            {
                //如果成立，则startVertax只有一条边，甚至更少，这样的点不足以成环，从临时点集去除
                //————————此处在日后进行修改，可改为提取破损环的结构！！！！
                tempPointSet[startVertax] = null;
                return;
            }

            loop_stack.Add(startVertax);
            innerStep++;
            for (; ; )
            {
                //Debug.WriteLine("nextVertax: " + nextVertax);
                if (nextVertax != -1)
                {
                    if (visited[nextVertax] == 1 && nextVertax == heap && innerStep == 2)//从1到2，从2到1，是一个反向线段，不是环
                    {
                        if (e.next == null)
                        {
                            tempPointSet[startVertax] = null;       //由于可能出现只存在反向线段的图，所以无法提环，
                            break;                                  //所以将点从临时点集去除
                        }
                        else
                        {
                            nextVertax = e.next.adjvex;
                        }
                        continue;
                    }
                    else if (visited[nextVertax] == 1 && nextVertax == heap && innerStep != 2)     //找到了一个环
                    {
                        Debug.WriteLine(" ▷ loop length: " + innerStep);
                        print_loopStack();
                        foreach (int flagVertax in loop_stack)
                        {
                            tempPointSet[flagVertax] = null;
                        }
                        nextVertax = e.next.adjvex;
                        flagFindALoop = 1;
                        break;
                    }
                    else if (visited[nextVertax] == 0)      //进行递归
                    {
                        //Debug.WriteLine("DFS:" + startVertax + " > " + nextVertax);
                        DFS1(SPG, nextVertax);
                    }
                    if (isRecall == 1)       //进行回退
                    {
                        innerStep--;
                        temp = nextVertax;
                        nextVertax = e.next.adjvex;
                        pop_loopStack();
                        visited[temp] = 0;
                        isRecall = 0;
                        continue;
                    }
                    if (flagFindALoop == 1)
                    {
                        break;
                    }
                    else
                    {
                        nextVertax = e.next.adjvex;
                    }
                }
                else if (nextVertax == -1)
                {
                    isRecall = 1;
                    break;
                }
            }
        }
        public void getEdgeLoop(MySketchMatrix SM)
        {
            initializeSequenceStack();
            initializeVisited(SM);
            tempPointSet = SM.SPG.pointSet;
            int allVisited = 0;                  //全部访问的标记
            int NumVisited;               //已访问的数目
            //int isTempNotNULL = 1;
            while (allVisited == 0)
            {
                int k = 0;
                int i = 0;
                PointClass p = tempPointSet[i];
                while (p == null)
                {
                    i++;
                    p = tempPointSet[i];
                }
                heap = SM.pointIdToIndex[p.ID];
                flagFindALoop = 0;
                clear_loopStack();
                innerStep = 0;
                isRecall = 0;

                //Debug.WriteLine("heap: " + heap);
                DFS1(SM.SPG, heap);

                NumVisited = 0;
                foreach (PointClass a in tempPointSet)
                {
                    k++;
                    if (a == null)
                    {
                        NumVisited++;
                    }
                    if (NumVisited == SM.SPG.VertexNodeCount)
                    {
                        allVisited = 1;
                    }
                    //Debug.WriteLine(k + ": " + allVisited + " | " + NumVisited);
                }
            }
        }
        public void DFS2(SketchEdgeGraph SEG, int startVertax)
        {
            
        }
        
        /// <summary>
        /// 提取环与环之间的拓扑关系
        /// </summary>
        /// <param name="SM"></param>
        public void extraTOPO(MySketchMatrix SM)
        {            
            int index = 0;
            for (int i = 0; i < SM.loopCount - 1; i++)
            {
                for (int j = i + 1; j < SM.loopCount; j++)
                {
                    Debug.WriteLine("进入循环");
                    if (SM.loopList[i].getBoxArea() == SM.loopList[j].getBoxArea())
                    {
                        Debug.WriteLine(i + "和" + j +"判断对称-1");
                        if (SM.isEdgeListEqual(SM.loopList[i], SM.loopList[j]) == 1)
                        {
                            Debug.WriteLine(i + "和" + j + "判断对称-2");
                            Relation rel = new Relation(i, j, 3);
                            SM.Rels.Add(rel);
                            index++;
                        }
                    }
                    else
                    {
                        if(SM.isCenterInAnotherBox(SM.loopList[i],SM.loopList[j]) == 1)
                        {
                            if (SM.isHaveSameEdge(SM.loopList[i], SM.loopList[j]) == 1)
                            {
                                Debug.WriteLine(i + "和" + j + "判断内邻");
                                Relation rel = new Relation(i, j, 0);
                                rel.setAdjacentFlag(0);                                
                                SM.Rels.Add(rel);
                                index++;
                            }
                            else
                            {
                                Debug.WriteLine(i + "和" + j + "判断包含");
                                Relation rel = new Relation(j, i, 2);
                                SM.Rels.Add(rel);
                                index++;
                            }
                        }
                        else
                        {
                            if (SM.isHaveSameEdge(SM.loopList[i], SM.loopList[j]) == 1)
                            {
                                Debug.WriteLine(i + "和" + j + "判断外邻");
                                Relation rel = new Relation(i, j, 0);
                                rel.setAdjacentFlag(1);
                                SM.Rels.Add(rel);
                                index++;
                            }
                            else
                            {
                                Debug.WriteLine(i + "和" + j + "判断相离");
                                Relation rel = new Relation(i, j, 1);
                                SM.Rels.Add(rel);
                                index++;
                            }
                        }
                    }                                         
                }
            }
            //Debug.WriteLine(index + " " + SM.loopCount);
            SM.RelationCount = index;
        }

        //辅助方法
        public void initializeSequenceStack() => loop_stack = new List<int>();
        public void clear_loopStack() => loop_stack.Clear();

        /// <summary>
        /// 用于输出环
        /// </summary>
        public void print_loopStack()
        {
            Debug.Write("·");
            foreach (int i in loop_stack)
            {
                Debug.Write(i + "-->");
            }
            foreach (int i in loop_stack)
            {
                Debug.Write(i + "\n");
                break;
            }
        }
        public void pop_loopStack() => loop_stack.RemoveAt(loop_stack.Count - 1);
        public void initializeVisited(MySketchMatrix SM)
        {
            int vertax_size = SM.SPG.VertexNodeCount;
            visited = new int[vertax_size];
            for (int i = 0; i < vertax_size; i++)
            {
                visited[i] = 0;
            }
        }


        //测试字典的构建
        public void testPointIdToIndex(MySketchMatrix SM)
        {
            foreach (Tuple<int, int> a in SM.pointIdToIndex.Keys)
            {
                Debug.WriteLine("<" + a.Item1 + "、" + a.Item2 + ">: " + SM.pointIdToIndex[a]);
            }
            /*
            Feature fea = (Feature)SM.sketchFeature;
            Sketch ske = (Sketch)fea.GetSpecificFeature2();
            object[] pArr = ske.GetSketchPoints2();
            for(int i = 0; i <= pArr.Length-1; i++)
            {
                SketchPoint p = (SketchPoint)pArr[i];
                Debug.WriteLine("--<" + ((int[])p.GetID())[0] + "、" + ((int[])p.GetID())[1] + ">");
            }         
            int theNum = pArr.Length;
            Debug.WriteLine(theNum);
            */
        }
        //测试SketchPoint点图结构的构建
        public void testPointGraph(MySketchMatrix SM) => SM.SPG.printAllPoint();
        //测试 -端点对- 边图结构的构建
        public void testEdgeGraph(MySketchMatrix SM) => SM.SPG.printAllEdge();
        //测试环类，对环类内容进行输出
        public void testLoopsClass(MySketchMatrix SM) => SM.printAllLoopData();
        //测试拓扑，对拓扑组内容进行输出验证
        public void testExtraTOPO(MySketchMatrix SM) => SM.printAllTOPO();

        //深度测试——关于包围盒构建的问题
        public void testBox(MySketchMatrix SM)
        {
            Debug.WriteLine("环点坐标测试——");
            foreach(LoopClass loop in SM.loopList)
            {
                Debug.WriteLine("平面测试：" + loop.plane);
                Debug.WriteLine("环点集测试：");
                foreach(PointClass p in loop.GetPointList())
                {
                    Debug.WriteLine(p.x + " | " + p.y + " | " + p.z);
                }
            }
        }
    }
}
