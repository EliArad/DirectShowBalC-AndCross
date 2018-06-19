using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SightLineApp
{
    public class CrossMx : Cross
    {
        Cross [] m_cross;
        IBaseFilter m_baseFilter;
        static int index = 0;
        struct XY
        {
            public int x;
            public int y;
        }
        int m_size = 0;
        XY[] m_xy;
        int wix = 0;

        public CrossMx(int line1Id, int line2Id, IBaseFilter baseFilter) : base(line1Id, line2Id, baseFilter)
        {
            m_baseFilter = baseFilter;
        }
        public void Init(int size)
        {           
            m_size = size;
            m_xy = new XY[size];

            m_cross = new Cross[size];
            int j = 0;
            for (int i = 0; i < size * 2; i+=2)
            {
                m_cross[j++] = new CrossMx(index, index + 1, m_baseFilter);
                index += 2;
            }
        }
         
        public void Push(int x , int y)
        {
            if (wix == m_size)
                return;
            m_xy[wix].x = x;
            m_xy[wix].y = y;
            wix = wix + 1;
        }
        public void Draw(int  height, Color color, int width)
        {
            for (int i = 0 ; i < wix ; i++)
            {
                m_cross[i].Draw(m_xy[i].x, m_xy[i].y, height, color, width);
            }
            wix = 0;
        }

        public void DrawFirst(int height, Color color, int width)
        {
            m_cross[0].Draw(m_xy[0].x, m_xy[0].y, height, color, width);
            wix = 0;
        }
    }
}
