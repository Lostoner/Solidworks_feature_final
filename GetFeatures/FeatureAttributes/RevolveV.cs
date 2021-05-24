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
    public class RevolveV
    {
        public int data0;
        public double data1;
        public double data2;
        public double data3;
        public double data4;
        public double data5;
        public double data6;
        public double data7;
        public double data8;
        public double data9;
        public double data10;
              
        //旋转特征-辅助函数，用于找到旋转轴可以比较的参数
        //返回空间属性和方向属性
        private double[] getAxisParm1(RevolveFeatureData2 swRevolveFD, swSelectType_e axisType, ModelDoc2 swModel)
        {
            double[] DAndNAxis = new double[6];             //存放旋转轴-方向向量-和-原点的法向量-的数组
            double[] vParam;        //存放轴两端的三轴坐标
            double[] nPt1 = new double[3];
            double[] nPt2 = new double[3];
            double t;       //直线两点式的公倍数
            double deX, deY, deZ;       //三轴差值
            double Denominator;          //旋转轴长
            SketchSegment skObj;
            RefAxis axisObj;
            Edge edgeObj;
            bool boolstatus = swRevolveFD.AccessSelections(swModel, null);

            switch (axisType)
            {
                case swSelectType_e.swSelDATUMAXES:             //轴为Axis
                    axisObj = swRevolveFD.Axis;
                    vParam = axisObj.GetRefAxisParams();

                    nPt1[0] = vParam[0]; nPt1[1] = vParam[1]; nPt1[2] = vParam[2];
                    nPt2[0] = vParam[3]; nPt2[1] = vParam[4]; nPt2[2] = vParam[5];

                    deX = nPt2[0] - nPt1[0];
                    deY = nPt2[1] - nPt1[1];
                    deZ = nPt2[2] - nPt1[2];
                    Denominator = Math.Sqrt(deX * deX + deY * deY + deZ * deZ);
                    DAndNAxis[0] = deX / Denominator; DAndNAxis[1] = deY / Denominator; DAndNAxis[2] = deZ / Denominator;

                    t = (nPt1[0] * deX + nPt1[1] * deY + nPt1[2] * deZ) / (deX * deX + deY * deY + deZ * deZ);
                    DAndNAxis[3] = t * deX + nPt1[0];
                    DAndNAxis[4] = t * deY + nPt1[1];
                    DAndNAxis[5] = t * deZ + nPt1[2];
                    break;
                case swSelectType_e.swSelEDGES:                      //轴为Edge            
                    edgeObj = swRevolveFD.Axis;
                    CurveParamData swCurveParamData = edgeObj.GetCurveParams3();
                    double[] vStartPoint = (double[])swCurveParamData.StartPoint;
                    double[] vEndPoint = (double[])swCurveParamData.EndPoint;
                    double[] startPoint = new double[3];
                    double[] endPoint = new double[3];
                    for (int i = 0; i <= vStartPoint.GetUpperBound(0); i++)
                        startPoint[i] = vStartPoint[i];
                    for (int i = 0; i <= vEndPoint.GetUpperBound(0); i++)
                        endPoint[i] = vEndPoint[i];

                    deX = endPoint[0] - startPoint[0];
                    deY = endPoint[1] - startPoint[1];
                    deZ = endPoint[2] - startPoint[2];
                    Denominator = Math.Sqrt(deX * deX + deY * deY + deZ * deZ);
                    DAndNAxis[0] = deX / Denominator; DAndNAxis[1] = deY / Denominator; DAndNAxis[2] = deZ / Denominator;

                    t = (startPoint[0] * deX + startPoint[1] * deY + startPoint[2] * deZ) / (deX * deX + deY * deY + deZ * deZ);
                    DAndNAxis[3] = t * deX + startPoint[0];
                    DAndNAxis[4] = t * deY + startPoint[1];
                    DAndNAxis[5] = t * deZ + startPoint[2];
                    break;
                case swSelectType_e.swSelSKETCHSEGS:         //*轴为sketchSeg                    
                    skObj = swRevolveFD.Axis;
                    Curve curve = skObj.IGetCurve();
                    vParam = curve.LineParams;

                    nPt1 = new double[3];
                    nPt2 = new double[3];
                    nPt1[0] = vParam[0] * 1000; nPt1[1] = vParam[1] * 1000; nPt1[2] = vParam[2] * 1000;
                    nPt2[0] = nPt1[0] + vParam[3];
                    nPt2[1] = nPt1[1] + vParam[4];
                    nPt2[2] = nPt1[2] + vParam[5];

                    deX = vParam[3];
                    deY = vParam[4];
                    deZ = vParam[5];
                    Denominator = Math.Sqrt(deX * deX + deY * deY + deZ * deZ);
                    DAndNAxis[0] = deX / Denominator; DAndNAxis[1] = deY / Denominator; DAndNAxis[2] = deZ / Denominator;

                    t = (nPt1[0] * deX + nPt1[1] * deY + nPt1[2] * deZ) / (deX * deX + deY * deY + deZ * deZ);
                    DAndNAxis[3] = t * deX + nPt1[0];
                    DAndNAxis[4] = t * deY + nPt1[1];
                    DAndNAxis[5] = t * deZ + nPt1[2];

                    break;
                default:
                    Debug.WriteLine("\n******\n关于旋转轴参数转化出错！！！\nGetAxisType()结果：" + axisType + "\n******\n");
                    for (int i = 0; i <= DAndNAxis.GetUpperBound(0); i++)
                        DAndNAxis[i] = 0;
                    break;
            }

            swRevolveFD.ReleaseSelectionAccess();
            return DAndNAxis;
        }
        //仅返回单位方向属性，并为相反方向的同一轴，设计归一方法
        private double[] getAxisParm2(RevolveFeatureData2 swRevolveFD, swSelectType_e axisType, ModelDoc2 swModel)
        {
            double[] DAndNAxis = new double[3];             //存放旋转轴-方向向量-和-原点的法向量-的数组
            double[] vParam;        //存放轴两端的三轴坐标
            double deX;         //存放三轴差
            double deY;
            double deZ;
            double dis;         //存放轴长
            SketchSegment skObj;
            RefAxis axisObj;
            Edge edgeObj;
            bool boolstatus = swRevolveFD.AccessSelections(swModel, null);

            switch (axisType)
            {
                case swSelectType_e.swSelDATUMAXES:             //轴为Axis
                    axisObj = swRevolveFD.Axis;
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
                case swSelectType_e.swSelEDGES:                      //轴为Edge            
                    edgeObj = swRevolveFD.Axis;
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
                case swSelectType_e.swSelSKETCHSEGS:         //*轴为sketchSeg                    
                    skObj = swRevolveFD.Axis;
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

            swRevolveFD.ReleaseSelectionAccess();
            return DAndNAxis;
        }

        public RevolveV(RevolveFeatureData2 swRevolveFD, ModelDoc2 swModel)
        {
            swSelectType_e AxisType = (swSelectType_e)swRevolveFD.GetAxisType();
            double[] KNaxis = getAxisParm2(swRevolveFD, AxisType, swModel);
            double Angle_dir1, Angle_dir2, Angle_all;

            if (swRevolveFD.ThinWallType != -1)
            {
                this.data0 = swRevolveFD.ThinWallType + 1;
                this.data1 = swRevolveFD.GetWallThickness(true) * 1000;
                this.data2 = swRevolveFD.GetWallThickness(false) * 1000;
                if (this.data1 == 0)
                    this.data1 = -1;
                if (this.data2 == 0)
                    this.data2 = -1;
            }
            else
            {
                this.data0 = -1;
                this.data1 = -1;
                this.data2 = -1;
            }              
           
            this.data3 = KNaxis[0];
            if (this.data3 == 0)
                this.data3 = 0.0001;
            this.data4 = KNaxis[1];
            if (this.data4 == 0)
                this.data4 = 0.0001;
            this.data5 = KNaxis[2];
            if (this.data5 == 0)
                this.data5 = 0.0001;

            //考虑用角度制 还是 弧度制
            Angle_dir1 = swRevolveFD.GetRevolutionAngle(true) * 180 / Math.PI;
            Angle_dir2 = swRevolveFD.GetRevolutionAngle(false) * 180 / Math.PI;
            Angle_all = Angle_dir1 + Angle_dir2;
            if (Angle_all > 360)
                this.data6 = 360;
            else
                this.data6 = Angle_all;

            if (swRevolveFD.ReverseDirection == true)
                this.data7 = 1;
            else
                this.data7 = -1;

            this.data8 = swRevolveFD.Type + 1;
            if (swRevolveFD.GetContoursCount() != 0)
                this.data9 = swRevolveFD.GetContoursCount();
            else
                this.data9 = -1;

            if (swRevolveFD.IsBossFeature() == true)
                this.data10 = 1;
            else
                this.data10 = -1;
        }

        public RevolveV()
        {
            data0 = 0;
            data1 = 0;
            data2 = 0;
            data3 = 0;
            data4 = 0;
            data5 = 0;
            data6 = 0;
            data7 = 0;
            data8 = 0;
            data9 = 0;
            data10 = 0;
        }
    }
}
