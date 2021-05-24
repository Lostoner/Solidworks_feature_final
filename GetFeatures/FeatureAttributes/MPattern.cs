using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFeatures.FeatureAttributes
{
    public class MPattern
    {
        public int data0;               //ScopeBodiesCount
        public int data1;               //MirrorFaceCount
        public int data2;               //PatternFeatureCount

        public double data3;            //法向量
        public double data4;
        public double data5;
        public double data6;            //原点的投影
        public double data7;
        public double data8;

        public int data9;               //GeometryPattern
        public int data10;               //PropagateVisualProperty

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

        public MPattern(IMirrorPatternFeatureData swMirrorPatternFD, ModelDoc2 swModel, string feaType)
        {
            bool status = false;
            status = swMirrorPatternFD.AccessSelections(swModel, null);
            swModel.ClearSelection2(true);

            this.data0 = isZeroI(swMirrorPatternFD.GetFeatureScopeBodiesCount());
            this.data1 = isZeroI(swMirrorPatternFD.GetMirrorFaceCount());
            this.data2 = isZeroI(swMirrorPatternFD.GetPatternFeatureCount());

            if (swMirrorPatternFD.GetMirrorPlaneType() == 1)
            {
                Feature ftem = (Feature)swMirrorPatternFD.Plane;
                IRefPlane fea = (IRefPlane)swMirrorPatternFD.Plane;
                object[] points = (object[])fea.CornerPoints;
                double[] a = ((IMathPoint)points[0]).ArrayData;
                double[] b = ((IMathPoint)points[1]).ArrayData;
                double[] c = ((IMathPoint)points[2]).ArrayData;
                double va = (b[1] - a[1]) * (c[2] - a[2]) - (c[1] - a[1]) * (b[2] - a[2]);
                double vb = (b[2] - a[2]) * (c[0] - a[0]) - (c[2] - a[2]) * (b[0] - a[0]);
                double vc = (b[0] - a[0]) * (c[1] - a[1]) - (c[0] - a[0]) * (b[1] - a[1]);
                double tem = Math.Sqrt(va * va + vb * vb + vc * vc);
                va = va / tem;
                vb = vb / tem;
                vc = vc / tem;

                double t = (va * a[0] + vb * b[0] + vc * c[0]) / (va * va + vb * vb + vc * vc);


                this.data3 = isZero(va);
                this.data4 = isZero(vb);
                this.data5 = isZero(vc);

                this.data6 = isZero(va * t);
                this.data7 = isZero(vb * t);
                this.data8 = isZero(vc * t);

            }
            else if(swMirrorPatternFD.GetMirrorPlaneType() == 0)
            {
                IFace2 face = (IFace2)swMirrorPatternFD.Plane;
                double[] normal = face.Normal;
                double[] point = face.GetClosestPointOn(0, 0, 0);
                this.data3 = isZero(normal[0]);
                this.data4 = isZero(normal[1]);
                this.data5 = isZero(normal[2]);
                this.data6 = isZero(point[0]);
                this.data7 = isZero(point[1]);
                this.data8 = isZero(point[2]);
            }

            if (swMirrorPatternFD.GeometryPattern)
            {
                this.data9 = 1;
            }
            else
            {
                this.data9 = -1;
            }
            if (swMirrorPatternFD.PropagateVisualProperty)
            {
                this.data10 = 1;
            }
            else
            {
                this.data10 = -1;
            }

            swMirrorPatternFD.ReleaseSelectionAccess();
        }

        public MPattern(IMirrorSolidFeatureData swMirrorPatternFD, ModelDoc2 swModel, string feaType)
        {
            bool status = false;
            status = swMirrorPatternFD.AccessSelections(swModel, null);
            swModel.ClearSelection2(true);

            this.data0 = isZeroI(swMirrorPatternFD.GetPatternBodyCount());
            //this.data1 = swMirrorPatternFD.GetMirrorFaceCount();
            //this.data2 = swMirrorPatternFD.GetPatternFeatureCount();
            this.data1 = 0;
            this.data2 = 0;

            Feature tem = (Feature)swMirrorPatternFD.Face;
            Debug.Print("[你想要看的数据]: " + tem.GetTypeName());

            /*
            IFace2 face = (IFace2)swMirrorPatternFD.Face;
            double[] normal = face.Normal;
            double[] point = face.GetClosestPointOn(0, 0, 0);
            this.data3 = isZero(normal[0]);
            this.data4 = isZero(normal[1]);
            this.data5 = isZero(normal[2]);
            this.data6 = isZero(point[0]);
            this.data7 = isZero(point[1]);
            this.data8 = isZero(point[2]);

            this.data9 = 0;
            this.data10 = 0;
            */

            swMirrorPatternFD.ReleaseSelectionAccess();
        }

        public MPattern()
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
