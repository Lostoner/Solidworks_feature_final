using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFeatures.FeatureAttributes
{
    public class CPattern
    {
        public int data0;               //PatternBodyCount
        public int data1;               //PatternFaceCount
        public int data2;               //PatternFeatureCount

        public double data3;            //D1相关数据
        public double data4;
        public double data5;
        public int data6;
        public int data7;
        public double data8;
        public int data9;

        public int data10;               //D2相关数据
        public double data11;
        public int data12;

        public int data13;              //varySketch
        public int data14;              //symmetric（对称）

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

        public double[] getAxisParm2(ICircularPatternFeatureData swCircularPatternFD, int axisType, ModelDoc2 swModel)
        {
            double[] DAndNAxis = new double[3];             //存放旋转轴-方向向量-和-原点的法向量-的数组
            double[] vParam;        //存放轴两端的三轴坐标
            double deX;         //存放三轴差
            double deY;
            double deZ;
            double dis;         //存放轴长

            bool boolstatus = swCircularPatternFD.AccessSelections(swModel, null);

            object CPaxis;
            CPaxis = swCircularPatternFD.Axis;

            switch (axisType)
            {
                case 0:             //轴为Axis
                    RefAxis axisObj = (RefAxis)CPaxis;
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
                    Edge edgeObj = (Edge)CPaxis;
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
                    Dimension dimenobj = (Dimension)CPaxis;
                    MathVector dire = dimenobj.DimensionLineDirection;
                    DAndNAxis = dire.ArrayData;

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

            swCircularPatternFD.ReleaseSelectionAccess();
            return DAndNAxis;
        }

        public CPattern(ICircularPatternFeatureData swCircularPatternFD, ModelDoc2 swModel)
        {
            this.data0 = isZeroI(swCircularPatternFD.GetPatternBodyCount());
            this.data1 = isZeroI(swCircularPatternFD.GetPatternFaceCount());
            this.data2 = isZeroI(swCircularPatternFD.GetPatternFeatureCount());

            int AxisType = swCircularPatternFD.GetAxisType();
            /*
            double[] KNaxis = getAxisParm2(swCircularPatternFD, AxisType, swModel);

            this.data3 = KNaxis[0];
            if (this.data3 == 0)
                this.data3 = 0.0001;
            this.data4 = KNaxis[1];
            if (this.data4 == 0)
                this.data4 = 0.0001;
            this.data5 = KNaxis[2];
            if (this.data5 == 0)
                this.data5 = 0.0001;
            */

            this.data3 = 114514;
            this.data4 = 114514;
            this.data5 = 114514;

            this.data6 = isZeroI(swCircularPatternFD.TotalInstances);
            if (swCircularPatternFD.ReverseDirection)
            {
                this.data7 = 1;
            }
            else
            {
                this.data7 = -1;
            }
            this.data8 = isZero(swCircularPatternFD.Spacing);
            if (swCircularPatternFD.EqualSpacing)
            {
                this.data9 = 1;
            }
            else
            {
                this.data9 = -1;
            }

            if (swCircularPatternFD.Direction2)
            {
                this.data10 = isZeroI(swCircularPatternFD.TotalInstances2);
                this.data11 = isZero(swCircularPatternFD.Spacing2);
                if (swCircularPatternFD.EqualSpacing2)
                {
                    this.data12 = 1;
                }
                else
                {
                    this.data12 = -1;
                }
            }
            else
            {
                this.data10 = 0;            //D2
                this.data11 = 0;
                this.data12 = 0;
            }
            if (swCircularPatternFD.VarySketch)
            {
                this.data13 = 1;
            }
            else
            {
                this.data13 = -1;
            }
            if(swCircularPatternFD.Symmetric)
            {
                this.data14 = 1;
            }
            else
            {
                this.data14 = -1;
            }
        }

        public CPattern()
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
        }
    }
}
