using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFeatures
{
    public class SweepV
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
        public int data13;
        public int data14;
        public int data15;
        public int data16;
        public int data17;
        public int data18;

        public SweepV(SweepFeatureData swSweepFD)
        {
            if(swSweepFD.ThinWallType != -1)
            {
                this.data0 = swSweepFD.ThinWallType + 1;
                this.data1 = swSweepFD.GetWallThickness(true) * 1000;
                this.data2 = swSweepFD.GetWallThickness(false) * 1000;
                if (this.data1 == 0)
                    this.data1 = -1;
                if(this.data2 == 0)
                    this.data2 = -1;
            }
            else
            {
                this.data0 = -1;
                this.data1 = -1;
                this.data2 = -1;
            }
            
            //角度
            this.data3 = swSweepFD.GetTwistAngle();
            if (this.data3 == 0)
                this.data3 = -1;
            this.data4 = swSweepFD.GetD2TwistAngle();
            if (this.data4 == 0)
                this.data4 = -1;

            if (swSweepFD.AlignWithEndFaces == true)
                this.data5 = 1;
            else
                this.data5 = -1;

            this.data6 = swSweepFD.TwistControlType + 1;

            this.data7 = swSweepFD.CircularProfileDiameter * 1000;
            if (this.data7 == 0)
                this.data7 = -1;

            if (swSweepFD.D1ReverseTwistDir == true)
                this.data8 = 1;
            else
                this.data8 = -1;
            if (swSweepFD.D2ReverseTwistDir == true)
                this.data9 = 1;
            else
                this.data9 = -1;
            
            if (swSweepFD.Direction != -1)            
                this.data10 = swSweepFD.Direction + 1;            
            else            
                this.data10 = -1;
                
            if(swSweepFD.Direction == 1)
            {
                this.data11 = swSweepFD.EndTangencyType + 1;
                this.data12 = swSweepFD.StartTangencyType + 1;
            }
            else
            {
                this.data11 = -1;
                this.data12 = -1;
            }                       

            if (swSweepFD.Merge == true)
                this.data13 = 1;
            else
                this.data13 = -1;
            if (swSweepFD.MergeSmoothFaces == true)
                this.data14 = 1;
            else
                this.data14 = -1;

            this.data15 = swSweepFD.PathAlignmentType + 1;
            this.data16 = swSweepFD.SweepType + 1;
            this.data17 = swSweepFD.GetCutSweepOption();        //API Hepler 定义只有1、2两种
            
            // 数量
            this.data18 = swSweepFD.GetGuideCurvesCount();
            if (data18 == 0)
                this.data18 = -1;
        }

        public SweepV()
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
            data13 = 0;
            data14 = 0;
            data15 = 0;
            data16 = 0;
            data17 = 0;
            data18 = 0;

        }
    }
}
