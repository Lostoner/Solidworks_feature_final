using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFeatures
{
    public class LoopClass
    {
        //protected int _loopID;                     //当前Loop的id
        //protected string _baseShapeName;             //所属的环的类型
        //protected int _toCompoundId;                 //当前Loop所属的特征Loop组id
        //protected Boundbox _box;                    //包围盒信息
        //protected List<LineClass> _lineList;           //线段列表
        protected List<PointClass> _pointList;           //点列表
        //protected TopologyRelation topologyRelation;       //环的拓扑关系信息
        //protected GeomeRelation geomeRelation;             //环的几何结构信息
        //protected bool _isDashLine;             //当前Loop是否为虚线环
        //public int _layerIndex;              //当前Loop所在草图的层数
        //public bool _isNegFeat;              //当前环是否为负特征
        //public bool _isInLocalRevolution;       //当前Loop是否处在局部回转区域

        //**包围盒物理属性
        public double rightUpX;          //右上角起点横坐标
        public double rightUpY;
        public double leftDownX;            //左下角起点的横坐标
        public double leftDownY;
        public double lengthAxisX;            //沿横轴走向
        public double lengthAxisY;
        public int plane;                     //描述草图位于哪一个平面——1：yOz；2：xOz；3,：xOy
        public int lineCount;               //直线的个数
        public int arcCount;                //圆弧的个数
        public int circleCount;
        //public int ellipseCount;                //椭圆的个数
        //public int parabolicCount;              //抛物线的个数
        public int splineCount;                 //样条的个数

        public LoopClass(List<int> ans,MySketchMatrix SM)
        {
            rightUpX = double.MinValue;
            rightUpY = double.MinValue;
            leftDownX = double.MaxValue;
            leftDownY = double.MaxValue;

            setPointList(ans, SM);              //构建环内点列表
            plane = whichPlane();               //标记环的平面

            //分类讨论，不同平面内包围盒的左下角和右上角坐标
            if(plane == 1)                                  //传统三轴的正视图
            {
                foreach(PointClass p in _pointList)
                {
                    rightUpX = Max(rightUpX, p.y);
                    rightUpY = Max(rightUpY, p.z);
                    leftDownX = Min(leftDownX, p.y);
                    leftDownY = Min(leftDownY, p.z);
                }
            }
            else if(plane == 2)                              //传统三轴的左视图
            {
                foreach (PointClass p in _pointList)
                {
                    rightUpX = Max(rightUpX, p.x);
                    rightUpY = Max(rightUpY, p.z);
                    leftDownX = Min(leftDownX, p.x);
                    leftDownY = Min(leftDownY, p.z);
                }
            }
            else if(plane == 3)                             //传统三轴的下视图
            {
                foreach (PointClass p in _pointList)
                {
                    rightUpX = Max(rightUpX, p.x);
                    rightUpY = Max(rightUpY, p.y);
                    leftDownX = Min(leftDownX, p.x);
                    leftDownY = Min(leftDownY, p.y);
                }
            }

            lengthAxisX = rightUpX - leftDownX;
            lengthAxisY = rightUpY - leftDownY;

            setSegmentCount();                  //设置环内各个Seg的数目
        }
        public void setPointList(List<int> ans,MySketchMatrix SM)
        {
            this._pointList = new List<PointClass>();
            foreach (int pointNum in ans)
            {
                Tuple<int, int> ID = SM.pointIndexToId[pointNum];
                PointClass p = SM.SPG.findPointClass(ID);
                this._pointList.Add(p);
            }
        }
        public void setSegmentCount()
        {
            int endPointNumbers = 0;
            int circleNumbers = 0;
            int arcNumbers = 0;            
            int splineNumbers = 0;
            foreach(PointClass p in this._pointList)
            {
                endPointNumbers += p.EndFlag;
                circleNumbers += p.circleFlag;
                arcNumbers += p.arcFlag;
                splineNumbers += p.assistFlag;
            }
            this.splineCount = splineNumbers / 2;
            this.arcCount = arcNumbers / 2;
            this.circleCount = circleNumbers / 4;
            this.lineCount = endPointNumbers - this.splineCount - this.arcCount;
        }
        public double getBoxArea()
        {
            return lengthAxisX * lengthAxisY;
        }        
        public List<PointClass> GetPointList()
        {
            return _pointList;
        }
        public int whichPlane()
        {
            if(_pointList == null)
            {
                Debug.WriteLine("\n环类下的平面判断函数：环类中环点集为空，函数无法进行");
                return 0;
            }
            double xContrast = _pointList[0].x;
            double yContrast = _pointList[0].y;
            double zContrast = _pointList[0].z;
            int xFlag = 1;
            int yFlag = 1;
            int zFlag = 1;
            foreach(PointClass p in _pointList)
            {
                if (xContrast != p.x)
                    xFlag = 0;
                if (yContrast != p.y)
                    yFlag = 0;
                if (zContrast != p.z)
                    zFlag = 0;
            }
            if (xFlag == 1 && yFlag == 0 && zFlag == 0)
                return 1;
            else if (xFlag == 0 && yFlag == 1 && zFlag == 0)
                return 2;
            else if (xFlag == 0 && yFlag == 0 && zFlag == 1)
                return 3;
            else
            {
                Debug.WriteLine("\n环类下的平面判断函数：三种平面皆未对应，程序出错，返回值为 0");
                return 0;
            }
        }
        public double Max(double a, double b)
        {
            return a > b ? a : b;
        }
        public double Min(double a, double b)
        {
            return a < b ? a : b;
        }
    }

    public class Relation
    {
        public int relationFlag;          //拓扑关系的标志，用于表示0:相邻、1:相离、2:包含、3:对称
        public int symmetricFlag;             //对称关系标志，用以表示0:旋转对称、1:镜像对称、2:平移对称
        public int adjacentFlag;            //相邻关系标志，用以表示0：内邻、1：外邻
        public Tuple<int, int> loopsNum;     //对应环表，即LC中的环编号，描述该关系是哪两个环的（描述包含关系时，a包含b）

        //对称关系数据结构
        public double firstX;       //第一个X：1、旋转对称：中心点X；2、镜像对称：对称轴矢量X；3、平移对称：位移矢量X
        public double firstY;       //第一个Y：1、旋转对称：中心点Y；2、镜像对称：对称轴矢量Y；3、平移对称：位移矢量Y
        public double secondX_OrNums;          //1、旋转对称：对称环个数；2、镜像对称：对称轴起点X；3、最后一个环中心点X
        public double secondY;      //1、旋转对称：——无——；2、镜像对称：对称轴起点Y；3、最后一个环中心点Y

        public Relation(int i, int j, int relationFlag)
        {
            this.relationFlag = relationFlag;
            loopsNum = new Tuple<int, int>(i, j);
        }
        public void setAdjacentFlag(int flag)
        {
            this.adjacentFlag = flag;
        }
    }
}
