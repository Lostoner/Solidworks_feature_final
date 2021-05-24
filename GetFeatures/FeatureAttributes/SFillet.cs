using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFeatures.FeatureAttributes
{
    public class SFillet
    {
        public int data0;           //边个数
        public int data1;           //面个数/控制点个数（GetControlPointsCount）
        public int data2;           //环个数/逆转点个数

        public int data3;           //圆角类别
        public int data4;           //对称、弦宽度、包络控制线手动划分枚举/对称非对称
        public double data5;        //顶点1半径1
        public double data6;        //顶点1半径2

        public double data7;        //顶点2半径1
        public double data8;        //顶点2半径2

        public int data9;           //轮廓类型手动划分枚举
        public double data10;       //轮廓参数

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

        public SFillet(ISimpleFilletFeatureData2 swSFilletFD, ModelDoc2 swModel)
        {
            bool status = false;
            status = swSFilletFD.AccessSelections(swModel, null);
            swModel.ClearSelection2(true);

            this.data0 = isZeroI(swSFilletFD.GetEdgeCount());
            if(swSFilletFD.Type == 0)
            {
                this.data1 = isZeroI(swSFilletFD.GetFaceCount(0));
            }
            else if(swSFilletFD.Type == 2)
            {
                this.data1 = isZeroI(swSFilletFD.GetFaceCount(1) + swSFilletFD.GetFaceCount(2));
            }
            else if(swSFilletFD.Type == 3)
            {
                this.data1 = isZeroI(swSFilletFD.GetFaceCount(3) + swSFilletFD.GetFaceCount(4) + swSFilletFD.GetFaceCount(5));
            }
            this.data2 = isZeroI(swSFilletFD.GetLoopCount());

            this.data3 = swSFilletFD.Type + 1;
            if(swSFilletFD.ConstantWidth)
            {
                this.data4 = 2;     //弦宽度（对称）
            }
            else if(swSFilletFD.AsymmetricFillet)
            {
                this.data4 = 1;     //非对称
            }
            else if(swSFilletFD.GetHoldLineCount() > 0)
            {
                this.data4 = 3;     //控制线（对称）
            }
            else
            {
                this.data4 = -1;     //仅对称（对称）
            }

            this.data5 = isZero(swSFilletFD.DefaultRadius);
            this.data6 = isZero(swSFilletFD.DefaultDistance);

            this.data7 = 0;
            this.data8 = 0;

            this.data9 = swSFilletFD.ConicTypeForCrossSectionProfile + 1;
            this.data10 = isZero(swSFilletFD.DefaultConicRhoOrRadius);

            swSFilletFD.ReleaseSelectionAccess();
        }

        public SFillet(IVariableFilletFeatureData2 swSFilletFD, ModelDoc2 swModel)
        {
            bool status = false;
            status = swSFilletFD.AccessSelections(swModel, null);
            swModel.ClearSelection2(true);

            this.data0 = isZeroI(swSFilletFD.FilletEdgeCount);
            this.data1 = isZeroI(swSFilletFD.GetControlPointsCount());
            this.data2 = isZeroI(swSFilletFD.GetSetbackVerticesCount());

            this.data3 = 2;
            if(swSFilletFD.AsymmetricFillet)
            {
                this.data4 = 1;
            }
            else
            {
                this.data4 = -1;
            }

            IEdge temEdge = swSFilletFD.GetFilletEdgeAtIndex(0);
            this.data5 = isZero(swSFilletFD.GetRadius(temEdge.GetStartVertex()));
            this.data6 = isZero(swSFilletFD.GetDistance(temEdge.GetStartVertex()));
            if(this.data6 < 0)
            {
                this.data6 = this.data5;
            }
            this.data7 = isZero(swSFilletFD.GetRadius(temEdge.GetEndVertex()));
            this.data8 = isZero(swSFilletFD.GetDistance(temEdge.GetEndVertex()));
            if (this.data8 < 0)
            {
                this.data8 = this.data7;
            }

            this.data9 = swSFilletFD.ConicTypeForCrossSectionProfile + 1;
            //this.data10 = isZero(swSFilletFD.GetConicRhoOrRadius(swSFilletFD.GetFilletEdgeAtIndex(0)));
            if(swSFilletFD.DefaultConicRhoOrRadius != 0)
            {
                this.data10 = isZero(swSFilletFD.DefaultConicRhoOrRadius);
            }
            else if(swSFilletFD.GetConicRhoOrRadius(swSFilletFD.GetFilletEdgeAtIndex(0)) != 0)
            {
                this.data10 = isZero(swSFilletFD.GetConicRhoOrRadius(swSFilletFD.GetFilletEdgeAtIndex(0)));
            }
            else
            {
                this.data10 = 0.001;
            }

            /*
            Debug.Print("[圆角数据]: " + swSFilletFD.FilletEdgeCount);
            IEdge tem = swSFilletFD.GetFilletEdgeAtIndex(0);
            double val = swSFilletFD.GetRadius(tem.GetStartVertex());
            Debug.Print("[圆角数据]: " + val);
            //Debug.Print("[圆角数据]: " + swSFilletFD.GetDistance(tem.GetStartVertex()));
            val = swSFilletFD.GetDistance(tem.GetStartVertex());
            Debug.Print("[圆角数据]: " + val);
            //Debug.Print("[圆角数据]: " + swSFilletFD.GetRadius(tem.GetEndVertex()));
            val = swSFilletFD.GetRadius(tem.GetEndVertex());
            Debug.Print("[圆角数据]: " + val);
            //Debug.Print("[圆角数据]: " + swSFilletFD.GetDistance(tem.GetEndVertex()));
            val = swSFilletFD.GetDistance(tem.GetEndVertex());
            Debug.Print("[圆角数据]: " + val);
            */

            swSFilletFD.ReleaseSelectionAccess();
        }

        public SFillet()
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
        }
    }
}
