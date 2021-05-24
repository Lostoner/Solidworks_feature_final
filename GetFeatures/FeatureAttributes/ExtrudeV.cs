using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFeatures
{
    public class ExtrudeV
    {
        public int data0;
        public double data1;
        public double data2;
        public double data3;
        public double data4;
        public double data5;
        public double data6;
        public double data7;
        public int data8;

        public ExtrudeV(ExtrudeFeatureData2 swExtrudeFD)
        {
            if (swExtrudeFD.ThinWallType != -1)
            {
                this.data0 = swExtrudeFD.ThinWallType + 1;
                this.data1 = swExtrudeFD.GetWallThickness(true) * 1000;
                this.data2 = swExtrudeFD.GetWallThickness(false) * 1000;
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
            if (swExtrudeFD.CapEnds == true)
                this.data3 = swExtrudeFD.CapThickness * 1000;
            else
                this.data3 = -1;
            
            this.data4 = swExtrudeFD.GetDepth(true) * 1000 + swExtrudeFD.GetDepth(false) * 1000;
            if (this.data4 == 0)
                this.data4 = -1;

            if (swExtrudeFD.CapEnds == true)
                this.data5 = 1;
            else
                this.data5 = -1;
            this.data6 = swExtrudeFD.GetEndCondition(true) + 1;
            this.data7 = swExtrudeFD.GetEndCondition(false) + 1;
            this.data8 = swExtrudeFD.FromType + 1;
        }

        public ExtrudeV()
        {
            this.data0 = 0;
            this.data1 = 0;
            this.data2 = 0;
            this.data4 = 0;
            this.data3 = 0;
            this.data5 = 0;
            this.data6 = 0;
            this.data7 = 0;
            this.data8 = 0;
        }
    }
}
