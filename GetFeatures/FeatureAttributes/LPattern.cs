using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFeatures
{
    public class LPattern
    {
        public int data0;              //PatternBodyCount
        public int data1;              //PatternFaceCount
        public int data2;              //PatternFeatureCount

        public double data3;            //D1相关数据
        public double data4;
        public double data5;
        public int data6;
        public int data7;
        public double data8;

        public double data9;            //D2相关数据
        public double data10;
        public double data11;
        public int data12;
        public int data13;
        public double data14;

        public int data15;              //varySketch
        public int data16;              //GeometryPattern

        public double abs(double x)
        {
            return x > 0 ? x : -x;
        }

        public double isZero(double x)
        {
            return abs(x) < 1e-7 ? 0.0001 : x;
        }

        public int isZeroI(int x)
        {
            return x == 0 ? -1 : x;
        }

        public double[] getAxisParm2(ILinearPatternFeatureData swLPatternFD, int axisType, bool dir, ModelDoc2 swModel)
        {
            double[] DAndNAxis = new double[3];             //存放旋转轴-方向向量-和-原点的法向量-的数组
            double[] vParam;        //存放轴两端的三轴坐标
            double deX;         //存放三轴差
            double deY;
            double deZ;
            double dis;         //存放轴长

            bool boolstatus = swLPatternFD.AccessSelections(swModel, null);

            object LPaxis;
            if (dir)
            {
                LPaxis = swLPatternFD.D1Axis;
            }
            else
            {
                LPaxis = swLPatternFD.D2Axis;
            }
            switch (axisType)
            {
                case 0:             //轴为Axis
                    RefAxis axisObj = (RefAxis)LPaxis;
                    if (axisObj == null)
                    {
                        DAndNAxis[0] = 0;
                        DAndNAxis[1] = 0;
                        DAndNAxis[2] = 1;
                        break;
                    }
                    vParam = axisObj.GetRefAxisParams();

                    deX = vParam[3] - vParam[0];
                    deY = vParam[4] - vParam[1];
                    deZ = vParam[5] - vParam[2];

                    dis = Math.Sqrt(deX * deX + deY * deY + deZ * deZ);
                    DAndNAxis[0] = deX / dis;
                    DAndNAxis[1] = deY / dis;
                    DAndNAxis[2] = deZ / dis;
                    break;
                case 1:                      //轴为Edge
                    Edge edgeObj = (Edge)LPaxis;
                    CurveParamData swCurveParamData = edgeObj.GetCurveParams3();
                    double[] vStartPoint = (double[])swCurveParamData.StartPoint;
                    double[] vEndPoint = (double[])swCurveParamData.EndPoint;

                    deX = vEndPoint[0] - vStartPoint[0];
                    deY = vEndPoint[1] - vStartPoint[1];
                    deZ = vEndPoint[2] - vStartPoint[2];

                    dis = Math.Sqrt(deX * deX + deY * deY + deZ * deZ);
                    DAndNAxis[0] = deX / dis;
                    DAndNAxis[1] = deY / dis;
                    DAndNAxis[2] = deZ / dis;
                    break;
                case 2:         //*轴为dimension
                    Dimension dimenobj = (Dimension)LPaxis;
                    //IMathVector oriVector = dimenobj.DimensionLineDirection;
                    //string[] tem1 = (string[])swModel.GetConfigurationNames();
                    //object tem = dimenobj.GetValue3(2, "默认");
                    MathVector dire = dimenobj.DimensionLineDirection;
                    DAndNAxis = dire.ArrayData;

                    break;
                case 3:         //*轴为sketchSeg
                    SketchSegment skObj = (SketchSegment)LPaxis;
                    Curve curve = skObj.IGetCurve();
                    vParam = curve.LineParams;

                    DAndNAxis[0] = vParam[3];
                    DAndNAxis[1] = vParam[4];
                    DAndNAxis[2] = vParam[5];

                    break;
                default:
                    Debug.WriteLine("\n******\n关于旋转轴参数转化出错！！！\nGetAxisType()结果：" + axisType + "\n******\n");
                    for (int i = 0; i <= DAndNAxis.GetUpperBound(0); i++)
                        DAndNAxis[i] = 0;
                    break;
            }
            //由于反方向重复轴，将相反方向轴归一定义
            if (DAndNAxis[2] == 0)
            {
                if (DAndNAxis[0] == 0)
                    DAndNAxis[1] = Math.Abs(DAndNAxis[1]);
                else if (DAndNAxis[0] < 0)
                {
                    DAndNAxis[0] = -DAndNAxis[0];
                    DAndNAxis[1] = -DAndNAxis[1];
                }
            }
            else if (DAndNAxis[2] < 0)
            {
                DAndNAxis[0] = -DAndNAxis[0];
                DAndNAxis[1] = -DAndNAxis[1];
                DAndNAxis[2] = -DAndNAxis[2];
            }

            swLPatternFD.ReleaseSelectionAccess();
            return DAndNAxis;
        }

        public LPattern(ILinearPatternFeatureData swLinearPatternFD, ModelDoc2 swModel)
        {
            bool status = false;
            status = swLinearPatternFD.AccessSelections(swModel, null);
            swModel.ClearSelection2(true);

            this.data0 = isZeroI(swLinearPatternFD.GetPatternBodyCount());
            this.data1 = isZeroI(swLinearPatternFD.GetPatternFaceCount());
            this.data2 = isZeroI(swLinearPatternFD.GetPatternFeatureCount());

            int AxisType = swLinearPatternFD.GetD1AxisType();
            double[] KNaxis = getAxisParm2(swLinearPatternFD, AxisType, true, swModel);

            this.data3 = isZero(KNaxis[0]);
            this.data4 = isZero(KNaxis[1]);
            this.data5 = isZero(KNaxis[2]);
            this.data6 = isZeroI(swLinearPatternFD.D1TotalInstances);
            if(swLinearPatternFD.D1ReverseDirection)
            {
                this.data7 = 1;
            }
            else
            {
                this.data7 = -1;
            }
            this.data8 = isZero(swLinearPatternFD.D1Spacing);


            if (swLinearPatternFD.IsDirection2Specified())
            {
                KNaxis = getAxisParm2(swLinearPatternFD, AxisType, false, swModel);
                this.data9 = isZero(KNaxis[0]);
                this.data10 = isZero(KNaxis[1]);
                this.data11 = isZero(KNaxis[2]);
                this.data12 = isZeroI(swLinearPatternFD.D2TotalInstances);
                if (swLinearPatternFD.D2ReverseDirection)
                {
                    this.data13 = 1;
                }
                else
                {
                    this.data13 = -1;
                }
                this.data14 = isZero(swLinearPatternFD.D2Spacing);
            }
            else
            {
                this.data9 = 0;            //D2
                this.data10 = 0;
                this.data11 = 0;
                this.data12 = 0;
                this.data13 = 0;
                this.data14 = 0;
            }
            if (swLinearPatternFD.VarySketch)
            {
                this.data15 = 1;
            }
            else
            {
                this.data15 = -1;
            }
            if(swLinearPatternFD.GeometryPattern)
            {
                this.data16 = 1;
            }
            else
            {
                this.data16 = -1;
            }
            swLinearPatternFD.ReleaseSelectionAccess();
        }

        public LPattern()
        {
            this.data0 = 0;
            this.data1 = 0;
            this.data2 = 0;
            this.data3 = 0;
            this.data4 = 0;
            this.data5 = 0;
            this.data6 = 0;
            this.data7 = 0;
            this.data8 = 0;
            this.data9 = 0;
            this.data10 = 0;
            this.data11 = 0;
            this.data12 = 0;
            this.data13 = 0;
            this.data14 = 0;
            this.data15 = 0;
            this.data16 = 0;
        }
    }
}
