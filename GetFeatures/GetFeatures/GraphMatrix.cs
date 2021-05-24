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

namespace GetFeatures
{
    ////顶点表结点类
    public class MyFeatureVertexNode
    {
        public Feature fea;
        public int featureDataFlag;
        public int IsPostiveFlag;
                                
        public int data0;    //
        public double data1;    //
        public double data2;    //
        public double data3;    //
        public double data4;    //
        public double data5;    //
        public double data6;    //
        public double data7;    //
        public double data8;    //
        public double data9;    //
        public double data10;   //
        public double data11;   //
        public double data12;   //
        public int data13;   //
        public int data14;   //
        public int data15;   //
        public int data16;   //
        public int data17;   //
        public int data18;   //        

        public List<int> sketchsIndex;
        public List<int> fathersIndex;
        public List<int> sonsIndex;

        public MyFeatureVertexNode(Feature fea)
        {
            this.fea = fea;
            this.featureDataFlag = -1;
            this.IsPostiveFlag = 1;
            this.data0 = -1;
            this.data1 = -1;
            this.data2 = -1;
            this.data3 = -1;
            this.data4 = -1;
            this.data5 = -1;
            this.data6 = -1;
            this.data7 = -1;
            this.data8 = -1;
            this.data9 = -1;
            this.data10 = -1;
            this.data11 = -1;
            this.data12 = -1;
            this.data13 = -1;
            this.data14 = -1;
            this.data15 = -1;
            this.data16 = -1;
            this.data17 = -1;
            this.data18 = -1;            

            sketchsIndex = new List<int>();
            fathersIndex = new List<int>();
            sonsIndex = new List<int>();            
        }

        private void getExtrude_FeatureDefinition(ExtrudeFeatureData2 swExtrudeFD)
        {
            this.data0 = swExtrudeFD.ThinWallType;
            this.data1 = swExtrudeFD.GetWallThickness(true);
            this.data2 = swExtrudeFD.GetWallThickness(false);
            this.data4 = swExtrudeFD.GetDepth(true) + swExtrudeFD.GetDepth(false);
            this.data3 = swExtrudeFD.CapThickness;
            if (swExtrudeFD.CapEnds == true)
                this.data5 = 1;
            else
                this.data5 = 0;
            this.data6 = swExtrudeFD.GetEndCondition(true);
            this.data6 = swExtrudeFD.GetEndCondition(false);
        }
        private void getBoundaryBoss_FeatureDefinition(BoundaryBossFeatureData swBoundaryBossFD)
        {
            this.data0 = swBoundaryBossFD.ThinFeatureType;
            this.data1 = swBoundaryBossFD.ThinFeatureThickness[true];
            this.data2 = swBoundaryBossFD.ThinFeatureThickness[false];
            this.data3 = swBoundaryBossFD.D1CurveInfluence;
            this.data4 = swBoundaryBossFD.D2CurveInfluence;

            this.data14 = swBoundaryBossFD.GetCurvesCount(0);
            this.data15 = swBoundaryBossFD.GetCurvesCount(1);

            this.data5 = swBoundaryBossFD.GetAlignmentType(0, 0);
            this.data6 = swBoundaryBossFD.GetAlignmentType(0, this.data14 - 1);            
            this.data7 = swBoundaryBossFD.GetGuideTangencyType(0, 0);
            this.data8 = swBoundaryBossFD.GetGuideTangencyType(0, this.data14 - 1);
            this.data9 = swBoundaryBossFD.GetTangentInfluence(0, 0);
            this.data10 = swBoundaryBossFD.GetTangentInfluence(0, this.data14 - 1);
            this.data11 = swBoundaryBossFD.GetTangentLength(0, 0);
            this.data12 = swBoundaryBossFD.GetTangentLength(0, this.data14 - 1);            
        }
        private void getLoft_FeatureDefinition(LoftFeatureData swLoftFD)
        {
            this.data0 = swLoftFD.ThinWallType;
            this.data1 = swLoftFD.GetWallThickness(true);
            this.data2 = swLoftFD.GetWallThickness(false);
            this.data3 = swLoftFD.StartTangentLength;
            this.data4 = swLoftFD.EndTangentLength;
            this.data5 = swLoftFD.StartTangencyType;
            this.data6 = swLoftFD.EndTangencyType;                  
            this.data7 = swLoftFD.GuideCurveInfluence;
            if (swLoftFD.MaintainTangency == true)
                this.data8 = 1;
            else
                this.data8 = 0;
            if (swLoftFD.ReverseStartTangentDirection == true)
                this.data9 = 1;
            else
                this.data9 = 0;
            if (swLoftFD.ReverseEndTangentDirection == true)
                this.data10 = 1;
            else
                this.data10 = 0;          
            this.data11 = swLoftFD.GetGuideCurvesCount();
            this.data12 = swLoftFD.NumberOfSections;          
        }
        private void getSweep_FeatureDefinition(SweepFeatureData swSweepFD)
        {
            this.data0 = swSweepFD.ThinWallType;
            this.data1 = swSweepFD.GetWallThickness(true);
            this.data2 = swSweepFD.GetWallThickness(false);
            this.data3 = swSweepFD.GetTwistAngle();
            this.data4 = swSweepFD.GetD2TwistAngle();
            if (swSweepFD.AlignWithEndFaces == true)
                this.data5 = 1;
            else
                this.data5 = 0;
            this.data6 = swSweepFD.TwistControlType;
            this.data7 = swSweepFD.CircularProfileDiameter;                        
            if (swSweepFD.D1ReverseTwistDir == true)
                this.data8 = 1;
            else
                this.data8 = 0;            
            if (swSweepFD.D2ReverseTwistDir == true)
                this.data9 = 1;
            else
                this.data9 = 0;
            this.data10 = swSweepFD.Direction;
            if (swSweepFD.Direction != 1)
            {
                this.data11 = swSweepFD.EndTangencyType;
                this.data12 = swSweepFD.StartTangencyType;
            }
            if (swSweepFD.Merge == true)            
                this.data13 = 1;            
            else
                this.data13 = 0;
            if (swSweepFD.MergeSmoothFaces == true)
                this.data14 = 1;
            else
                this.data14 = 0;            
            this.data15 = swSweepFD.PathAlignmentType;
            this.data16 = swSweepFD.SweepType;                       
            this.data17 = swSweepFD.GetCutSweepOption();            
            this.data18 = swSweepFD.GetGuideCurvesCount();         
        } 
        private void getRevolve_FeatureDefinition(RevolveFeatureData2 swRevolveFD)
        {
            this.data0 = swRevolveFD.ThinWallType;
            this.data1 = swRevolveFD.GetWallThickness(true);
            this.data2 = swRevolveFD.GetWallThickness(false);

            object axis = swRevolveFD.Axis;
            int axisType = swRevolveFD.GetAxisType();            
            double[] KNaxis = getAxisParm(axis, axisType);
            this.data3 = KNaxis[0];
            this.data4 = KNaxis[1];
            this.data5 = KNaxis[2];
            this.data6 = KNaxis[3];
            this.data7 = KNaxis[4];
            this.data8 = KNaxis[5];
            this.data9 = swRevolveFD.GetRevolutionAngle(true) + swRevolveFD.GetRevolutionAngle(false);            
            if (swRevolveFD.ReverseDirection == true)
                this.data10 = 1;
            else
                this.data10 = 0;          

            this.data11 = swRevolveFD.Type;
            this.data12 = swRevolveFD.GetContoursCount();           
            if (swRevolveFD.IsBossFeature() == true)
                this.data13 = 1;
            else
                this.data13 = 0;
        }
        //旋转特征-辅助函数，用于找到旋转轴可以比较的参数
        private double[] getAxisParm(object axis, int axisType) 
        {
            double[] DAndNAxis = new double[6];             //存放旋转轴-方向向量-和-原点的法向量-的数组
            double t;       //直线两点式的公倍数
            if (axisType == 5)           //轴为Axis
            {
                RefAxis refAxis = (RefAxis)axis;
                var vParam = refAxis.GetRefAxisParams();

                double[] nPt1 = new double[3];
                double[] nPt2 = new double[3];
                nPt1[0] = vParam[0]; nPt1[1] = vParam[1]; nPt1[2] = vParam[2];
                nPt2[0] = vParam[3]; nPt2[1] = vParam[4]; nPt2[2] = vParam[5];
                
                double deX = nPt2[0] - nPt1[0];
                double deY = nPt2[1] - nPt1[1];
                double deZ = nPt2[2] - nPt1[2];
                double Denominator = Math.Sqrt(deX * deX + deY * deY + deZ * deZ);
                DAndNAxis[0] = deX / Denominator; DAndNAxis[1] = deY / Denominator; DAndNAxis[2] = deZ / Denominator;
                
                t = (nPt1[0] * deX + nPt1[1] * deY + nPt1[2] * deZ) / (deX * deX + deY * deY + deZ * deZ);
                DAndNAxis[3] = t * deX + nPt1[0];
                DAndNAxis[4] = t * deY + nPt1[1];
                DAndNAxis[5] = t * deZ + nPt1[2];                
            }
            else if(axisType == 1)          //轴为Edge
            {
                Edge edge = (Edge)axis;
                CurveParamData swCurveParamData = edge.GetCurveParams3();
                double[] vStartPoint = (double[])swCurveParamData.StartPoint;
                double[] vEndPoint = (double[])swCurveParamData.EndPoint;
                double[] startPoint = new double[3];
                double[] endPoint = new double[3];
                for (int i = 0; i <= vStartPoint.GetUpperBound(0); i++)
                    startPoint[i] = vStartPoint[i];
                for (int i = 0; i <= vEndPoint.GetUpperBound(0); i++)
                    endPoint[i] = vEndPoint[i];

                double deX = endPoint[0] - startPoint[0];
                double deY = endPoint[1] - startPoint[1];
                double deZ = endPoint[2] - startPoint[2];
                double Denominator = Math.Sqrt(deX * deX + deY * deY + deZ * deZ);
                DAndNAxis[0] = deX / Denominator; DAndNAxis[1] = deY / Denominator; DAndNAxis[2] = deZ / Denominator;
                               
                t = (startPoint[0] * deX + startPoint[1] * deY + startPoint[2] * deZ) / (deX * deX + deY * deY + deZ * deZ);
                DAndNAxis[3] = t * deX + startPoint[0];
                DAndNAxis[4] = t * deY + startPoint[1];
                DAndNAxis[5] = t * deZ + startPoint[2];
            }
            else if(axisType == 34)         //轴为sketchSeg
            {
                SketchSegment sketchSegment = (SketchSegment)axis;
                Curve curve = (Curve)sketchSegment.GetCurve();
                var vParam = curve.LineParams;

                double[] nPt1 = new double[3];
                double[] nPt2 = new double[3];
                nPt1[0] = vParam[0] * 1000; nPt1[1] = vParam[1] * 1000; nPt1[2] = vParam[2] * 1000;
                nPt2[0] = nPt1[0] + vParam[3];
                nPt2[1] = nPt1[1] + vParam[4];
                nPt2[2] = nPt1[2] + vParam[5];

                double deX = vParam[3];
                double deY = vParam[4];
                double deZ = vParam[5];
                double Denominator = Math.Sqrt(deX * deX + deY * deY + deZ * deZ);
                DAndNAxis[0] = deX / Denominator; DAndNAxis[1] = deY / Denominator; DAndNAxis[2] = deZ / Denominator;

                t = (nPt1[0] * deX + nPt1[1] * deY + nPt1[2] * deZ) / (deX * deX + deY * deY + deZ * deZ);
                DAndNAxis[3] = t * deX + nPt1[0];
                DAndNAxis[4] = t * deY + nPt1[1];
                DAndNAxis[5] = t * deZ + nPt1[2];
            }
            else
            {
                Debug.WriteLine("\n******\n关于旋转轴参数转化出错！！！\n******\n");
                for (int i = 0; i <= DAndNAxis.GetUpperBound(0); i++)
                    DAndNAxis[i] = 0;
            }
            return DAndNAxis;
        }
        
        private void getLinearPattern_FeatureDefinition(LinearPatternFeatureData swLinearPatternFD)
        {

        }
        private void getCircularPattern_FeatureDefinition(CircularPatternFeatureData swCircularPatternFD)
        {

        }
        private void getMirrorPattern_FeatureDefinition(MirrorPartFeatureData mirrorPatternFD)
        {

        }


        public void judgeAndToGet_FeatureDefinition()
        {
            string feaType = fea.GetTypeName2();
            ExtrudeFeatureData2 swExtrudeFeatureData = default(ExtrudeFeatureData2);
            BoundaryBossFeatureData swBoundaryBossFeatureData = default(BoundaryBossFeatureData);
            LoftFeatureData swLoftFeatureData = default(LoftFeatureData);
            SweepFeatureData swSweepFeatureData = default(SweepFeatureData);
            RevolveFeatureData2 swRevolveFeatureData = default(RevolveFeatureData2);
            SimpleFilletFeatureData2 swSimpleFilletFeatureData = default(SimpleFilletFeatureData2);
            ChamferFeatureData2 swChamferFeatureData = default(ChamferFeatureData2);
            LinearPatternFeatureData swLinearPatternFeatureData = default(LinearPatternFeatureData);
            CircularPatternFeatureData swCircularPatternFeatureData = default(CircularPatternFeatureData);
            MirrorPatternFeatureData swMirrorPatternFeatureData = default(MirrorPatternFeatureData);

            if (feaType == "BaseBody" || feaType == "Boss" || feaType == "BossThin" ||
                feaType == "Cut" || feaType == "CutThin" || feaType == "Extrusion")
            {
                swExtrudeFeatureData = (ExtrudeFeatureData2)this.fea.GetDefinition();
                this.featureDataFlag = 0;
                if (feaType == "Cut" || feaType == "CutThin")
                    this.IsPostiveFlag = 0;               
            }
            else if (feaType == "NetBlend")
            {
                swBoundaryBossFeatureData = (BoundaryBossFeatureData)this.fea.GetDefinition();
                this.featureDataFlag = 1;                
            }
            else if (feaType == "Blend" || feaType == "BlendCut")
            {
                swLoftFeatureData = (LoftFeatureData)this.fea.GetDefinition();
                this.featureDataFlag = 2;
                if (feaType == "BlendCut")
                    this.IsPostiveFlag = 0;
            }
            else if (feaType == "Sweep" || feaType == "SweepCut")
            {
                swSweepFeatureData = (SweepFeatureData)this.fea.GetDefinition();
                this.featureDataFlag = 3;
                if (feaType == "SweepCut")
                    this.IsPostiveFlag = 0;
            } 
            else if (feaType == "RevCut" || feaType == "Revolution" || feaType == "RevolutionThin")
            {
                swRevolveFeatureData = (RevolveFeatureData2)this.fea.GetDefinition();
                this.featureDataFlag = 4;
                if(feaType == "RevCut")
                    this.IsPostiveFlag = 0;
            }
            else if (feaType == "LPattern")
            {
                swLinearPatternFeatureData = (LinearPatternFeatureData)this.fea.GetDefinition();
                this.featureDataFlag = 5;
            }
            else if (feaType == "CirPattern")
            {
                swCircularPatternFeatureData = (CircularPatternFeatureData)this.fea.GetDefinition();
                this.featureDataFlag = 6;
            }
            else if (feaType == "MirrorPattern")
            {
                swMirrorPatternFeatureData = (MirrorPatternFeatureData)this.fea.GetDefinition();
                this.featureDataFlag = 7;
            }
            else if(feaType == "Fillet" || feaType == "Round fillet corner")
            {
                swSimpleFilletFeatureData = (SimpleFilletFeatureData2)this.fea.GetDefinition();
                this.featureDataFlag = 8;
            }
            else if (feaType == "Chamfer")
            {
                swChamferFeatureData = (ChamferFeatureData2)this.fea.GetDefinition();
                this.featureDataFlag = 9;
            }           
            

            if (this.featureDataFlag == 0)
                getExtrude_FeatureDefinition(swExtrudeFeatureData);
            else if (this.featureDataFlag == 1)
                getBoundaryBoss_FeatureDefinition(swBoundaryBossFeatureData);
            else if (this.featureDataFlag == 2)
                getLoft_FeatureDefinition(swLoftFeatureData);
            else if (this.featureDataFlag == 3)
                getSweep_FeatureDefinition(swSweepFeatureData);
            else if (this.featureDataFlag == 4)
                getRevolve_FeatureDefinition(swRevolveFeatureData);            
            else if (this.featureDataFlag == 5)
                getLinearPattern_FeatureDefinition(swLinearPatternFeatureData);
            else if (this.featureDataFlag == 6)
                getCircularPattern_FeatureDefinition(swCircularPatternFeatureData);
            else if (this.featureDataFlag == 7)
                getCircularPattern_FeatureDefinition(swCircularPatternFeatureData);
            /*else if (this.featureDataFlag == 8)
                getSimpleFillet_FeatureDefinition(swSimpleFilletFeatureData);
            else if (this.featureDataFlag == 9)
                getChamfer_FeatureDefinition(swChamferFeatureData);*/
            else
                Debug.WriteLine("\n未设置该类特征的提取函数！！！\n");
        }

    }

    //图类
    public class ALGraph
    {
        public List<MyFeatureVertexNode> AdjList;//特征节点表
        public int VertexNodeCount;//顶点数
        public int BordeCount;//边数

        public ALGraph()
        {
            VertexNodeCount = 0;
            BordeCount = 0;
            AdjList = new List<MyFeatureVertexNode>();
        }       

    }

}

