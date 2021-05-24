using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFeatures
{
    public class PointClass
    {
        private SketchPoint skePoint;
        public Tuple<int, int> ID;
        public double x;
        public double y;
        public double z;
        public int completeArcAmplitude;
        public SketchEdgeNode firseEdge;

        public int assistFlag;
        public int arcFlag;
        public int circleFlag;
        public int EndFlag;

        public PointClass()
        {
            skePoint = null;
            ID = null;
            x = 0.00;
            y = 0.00;
            z = 0.00;
            completeArcAmplitude = 1;
            firseEdge = new SketchEdgeNode();
            assistFlag = 0;
            arcFlag = 0;
            circleFlag = 0;
            EndFlag = 0;
        }
        //将SketchPoint的特征和点ID输入点类中
        public void setPointAndID(SketchPoint p)
        {
            this.skePoint = p;
            this.ID = new Tuple<int, int>(((int[])p.GetID())[0], ((int[])p.GetID())[1]);
        }
        //设置各类点的Seg的性质，即描述这个点是哪种Seg中的
        public void setAssistFlag()
        {
            this.assistFlag = 1;
        }
        public void setArcFlag()
        {
            this.arcFlag = 1;
        }
        public void setCircleFlag()
        {
            this.circleFlag = 1;
        }
        public void setEndFlag()
        {
            this.EndFlag = 1;
        }

        public string getPointID()
        {
            return " [" + ID.Item1 + "," + ID.Item2 + "]"; 
        }
    }

    public class StraightLineClass : LineClass
    {
        protected PointClass _pointStart;
        protected PointClass _pointEnd;
        public StraightLineClass()
        {
            _pointStart = null;
            _pointEnd = null;
        }
        public void setPoint(PointClass ps, PointClass pe)
        {
            this._pointStart = ps;
            this._pointEnd = pe;
        }
        public PointClass getPointStart()
        {
            return this._pointStart;
        }
        public PointClass getPointEnd()
        {
            return this._pointEnd;
        }
    }

    public class BezierClass : LineClass
    {
        protected PointClass _p0;
        protected PointClass _p1;
        protected PointClass _p2;
        protected PointClass _p3;
        public BezierClass()
        {
            _p0 = null;
            _p1 = null;
            _p2 = null;
            _p3 = null;
        }
        public void setBezierPoint(PointClass ps, PointClass p1, PointClass p2, PointClass pe)
        {
            this._p0 = ps;
            this._p1 = p1;
            this._p2 = p2;
            this._p3 = pe;
        }
        public PointClass getPointStart()
        {
            return this._p0;
        }
        public PointClass getPointEnd()
        {
            return this._p3;
        }
        public PointClass getPointAssist1()
        {
            return this._p1;
        }
        public PointClass getPointAssist2()
        {
            return this._p2;
        }
    }

    public class CircleClass : LineClass
    {
        protected PointClass _center;
        protected PointClass _start;
        protected PointClass _end;
        protected int _isComplete;
        protected double _radius;

        protected PointClass _completePointUp;
        protected PointClass _completePointDown;
        protected PointClass _completePointLeft;
        protected PointClass _completePointRight;
        public CircleClass()
        {
            _center = null;
            _start = null;
            _end = null;
            _completePointUp = null;
            _completePointDown = null;
            _completePointLeft = null;
            _completePointRight = null;
            _isComplete = -1;
            _radius = 0;
        }
        public PointClass getPointCenter()
        {
            return this._center;
        }
        public PointClass getPointStart()
        {
            return this._start;
        }
        public PointClass getPointEnd()
        {
            return this._end;
        }
        public PointClass getPointUp()
        {
            return _completePointUp;
        }
        public PointClass getPointDown()
        {
            return _completePointDown;
        }
        public PointClass getPointLeft()
        {
            return _completePointLeft;
        }
        public PointClass getPointRight()
        {
            return _completePointRight;
        }
        public int getIsComplete()
        {
            return this._isComplete;
        }

        //对完整圆的结构赋值
        public void setCompleteArc(PointClass pc, double r)
        {
            this._center = pc;
            this._radius = r;
            int a = _center.ID.Item1;
            int b = _center.ID.Item2;
            int Amplitude = pc.completeArcAmplitude;        //同心圆改写赋值增幅
            _completePointUp = new PointClass 
            {
                x = pc.x,
                y = pc.y + r,
                z = pc.z,
                ID = new Tuple<int, int>(a, 5 * b + Amplitude)
            };
            _completePointDown = new PointClass
            {
                x = pc.x,
                y = pc.y - r,
                z = pc.z,
                ID = new Tuple<int, int>(a, 5 * b - Amplitude)
            };
            _completePointLeft = new PointClass
            {
                x = pc.x - r,
                y = pc.y,
                z = pc.z,
                ID = new Tuple<int, int>(5 * a - Amplitude, b)
            };
            _completePointRight = new PointClass
            {
                x = pc.x + r,
                y = pc.y,
                z = pc.z,
                ID = new Tuple<int, int>(5 * a + Amplitude, b)
            };
        }
        public void setNotCompleteArc(PointClass pc, double r, PointClass ps, PointClass pe)
        {
            this._center = pc;
            this._radius = r;
            this._start = ps;
            this._end = pe;
        }
        public void setIsComplete(int cOrNot)
        {
            this._isComplete = cOrNot;
        }
    }

    public class LineClass
    {
        protected int _lineId;
        protected int _LineStyles;
        public virtual string getLineType { get; set; }
    }
}
