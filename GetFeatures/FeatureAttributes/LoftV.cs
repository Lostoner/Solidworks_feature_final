using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFeatures
{
    public class LoftV
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
        public double data11;
        public double data12;

        public LoftV(LoftFeatureData swLoftFD)
        {
            if (swLoftFD.ThinWallType != -1)
            {
                this.data0 = swLoftFD.ThinWallType + 1;
                this.data1 = swLoftFD.GetWallThickness(true) * 1000;
                this.data2 = swLoftFD.GetWallThickness(false) * 1000;
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

            this.data3 = swLoftFD.StartTangentLength * 1000;
            this.data4 = swLoftFD.EndTangentLength * 1000;

            this.data5 = swLoftFD.StartTangencyType + 1;
            this.data6 = swLoftFD.EndTangencyType + 1;
            this.data7 = swLoftFD.GuideCurveInfluence + 1;

            if (swLoftFD.MaintainTangency == true)
                this.data8 = 1;
            else
                this.data8 = -1;
            if (swLoftFD.ReverseStartTangentDirection == true)
                this.data9 = 1;
            else
                this.data9 = -1;
            if (swLoftFD.ReverseEndTangentDirection == true)
                this.data10 = 1;
            else
                this.data10 = -1;

            this.data11 = swLoftFD.GetGuideCurvesCount();
            if (this.data11 == 0)
                this.data11 = -1;
            this.data12 = swLoftFD.NumberOfSections;
            if (this.data12 == 0)
                this.data12 = -1;
        }

        public LoftV()
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
            data11 = 0;
            data12 = 0;
    }
    }
}
