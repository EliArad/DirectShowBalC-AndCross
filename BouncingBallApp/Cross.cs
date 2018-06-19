using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SightLineApp
{
    public class Cross : Shapes
    {
        int[] m_id = { 0, 0 };
        bool draw = false;
        public Cross(int line1Id , int line2Id , IBaseFilter baseFilter) : base(baseFilter)
        {
            m_id[0] = line1Id;
            m_id[1] = line2Id;
        }
        public Cross()
        {

        }
        public void Draw(int x1, int y1, int height, Color color1, Color color2, int width1, int width2)
        {
            AddLine(m_id[0], x1 , y1, x1 , y1 - height, color1 , width1);
            AddLine(m_id[1], x1 - height / 2, y1 - height / 2, x1 + height / 2, y1 - height / 2, color2, width2);
            draw = true;
        }
        public void Draw(int x1, int y1, int height, Color color1, Color color2, int width)
        {
            AddLine(m_id[0], x1, y1, x1, y1 - height, color1, width);
            AddLine(m_id[1], x1 - height / 2, y1 - height / 2, x1 + height / 2, y1 - height / 2, color2, width);
            draw = true;
        }
        public void Draw(int x1, int y1, int height, Color color, int width)
        {
            AddLine(m_id[0], x1, y1, x1, y1 - height, color, width);
            AddLine(m_id[1], x1 - height / 2, y1 - height / 2, x1 + height / 2, y1 - height / 2, color, width);
            draw = true;
        }
        public override void Clear()
        {
            if (draw == false)
                return;
            Remove(m_id[0]);
            Remove(m_id[1]);
        }
    }
}
