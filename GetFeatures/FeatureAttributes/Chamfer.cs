using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFeatures.FeatureAttributes
{
    public class Chamfer
    {
        public int data0;           //边个数
        public int data1;           //面个数
        public int data2;           //环个数

        public int data3;           //倒角类别

        public double data4;        //角度z
        public double data5;        //距离1
        public double data6;        //距离2

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

        public Chamfer(IChamferFeatureData2 swChamferFD, ModelDoc2 swModel)
        {
            bool status = false;
            status = swChamferFD.AccessSelections(swModel, null);
            swModel.ClearSelection2(true);

            this.data0 = isZeroI(swChamferFD.GetEdgeCount());
            this.data1 = isZeroI(swChamferFD.GetFaceCount());
            this.data2 = isZeroI(swChamferFD.LoopCount);
            
            if(swChamferFD.Type == 16)
            {
                this.data3 = 4;
            }
            else
            {
                this.data3 = swChamferFD.Type;
            }

            this.data4 = isZero(swChamferFD.EdgeChamferAngle);
            this.data5 = isZero(swChamferFD.GetEdgeChamferDistance(0));
            this.data6 = isZero(swChamferFD.GetEdgeChamferDistance(1));
            
            swChamferFD.ReleaseSelectionAccess();
        }

        public Chamfer()
        {
            this.data0 = 0;
            this.data1 = 0;
            this.data2 = 0;
            this.data3 = 0;
            this.data4 = 0;
            this.data5 = 0;
            this.data6 = 0;
        }
    }
}
