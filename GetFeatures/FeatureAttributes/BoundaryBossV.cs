using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFeatures
{
    public class BoundaryBossV
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


        public BoundaryBossV(BoundaryBossFeatureData swBoundaryBossFD)
        {
            if(swBoundaryBossFD.ThinFeatureType != -1)
            {
                this.data0 = swBoundaryBossFD.ThinFeatureType + 1;
                this.data1 = swBoundaryBossFD.ThinFeatureThickness[true] * 1000;
                this.data2 = swBoundaryBossFD.ThinFeatureThickness[false] * 1000;
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

            if (swBoundaryBossFD.D1CurveInfluence != -1)
                this.data3 = swBoundaryBossFD.D1CurveInfluence + 1;
            else
                this.data3 = -1;
            if (swBoundaryBossFD.D2CurveInfluence != -1)
                this.data4 = swBoundaryBossFD.D2CurveInfluence + 1;
            else
                this.data4 = -1;

            //枚举类型判断，避开“0”值
            if (swBoundaryBossFD.GetAlignmentType(0, 0) != -1)
                this.data5 = swBoundaryBossFD.GetAlignmentType(0, 0) + 1;
            else
                this.data5 = -1;
            if (swBoundaryBossFD.GetAlignmentType(0, this.data13 - 1) != -1)
                this.data6 = swBoundaryBossFD.GetAlignmentType(0, this.data13 - 1) + 1;
            else
                this.data6 = -1;
            if (swBoundaryBossFD.GetGuideTangencyType(0, 0) != -1)
                this.data7 = swBoundaryBossFD.GetGuideTangencyType(0, 0) + 1;
            else
                this.data7 = -1;
            if (swBoundaryBossFD.GetGuideTangencyType(0, this.data13 - 1) != -1)
                this.data8 = swBoundaryBossFD.GetGuideTangencyType(0, this.data13 - 1) + 1;
            else
                this.data8 = -1;

            this.data9 = swBoundaryBossFD.GetTangentInfluence(0, 0);    //其值在0.0 - 1.0之间
            if (this.data9 == 0)
                this.data9 = 0.000001;
            this.data10 = swBoundaryBossFD.GetTangentInfluence(0, this.data13 - 1);     //其值在0.0 - 1.0之间
            if (this.data10 == 0)
                this.data10 = 0.000001;

            this.data11 = swBoundaryBossFD.GetTangentLength(0, 0) * 1000;
            if (this.data11 == 0)
                this.data11 = -1;
            this.data12 = swBoundaryBossFD.GetTangentLength(0, this.data13 - 1) * 1000;
            if (this.data12 == 0)
                this.data12 = -1;

            this.data13 = swBoundaryBossFD.GetCurvesCount(0);
            if (this.data13 == 0)
                this.data13 = -1;
            this.data14 = swBoundaryBossFD.GetCurvesCount(1);
            if(this.data14 == 0)
                this.data14 = -1;
        }

        public BoundaryBossV()
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
