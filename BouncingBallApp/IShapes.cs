using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SightLineApp
{

    [ComVisible(false)]
    public class Clsid		 
    { 
        // {63CA2EE3-6460-4098-9B74-079CE740E4B5}
        public static readonly Guid CLSID_CLiveSource = new Guid ( 0x63ca2ee3, 0x6460, 0x4098, 0x9b, 0x74, 0x7, 0x9c, 0xe7, 0x40, 0xe4, 0xb5  );

        // {68FA755B-C8C2-4f48-9E62-AE8C82E00A3E}
        public static readonly Guid IID_ISLA3000 = new Guid (0x68fa755b, 0xc8c2, 0x4f48, 0x9e, 0x62, 0xae, 0x8c, 0x82, 0xe0, 0xa, 0x3e );

        public static readonly Guid CLSID_TextOverlay = new Guid ( 0xe52beab4, 0x45fb, 0x4d5a, 0xbc, 0x9e, 0x23, 0x81, 0xe6, 0x1d, 0xcc, 0x47 );

    }

    public class Shapes
    {

        protected IShapes shapeIface;
         
        [ComVisible(true), ComImport,
  Guid("B6F36855-D861-4ADB-B76F-5F3CF52410AC"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShapes
        {
            
             [PreserveSig]
            int AddTextOverlay(string text, int id, 
                                            int left,
                                            int    top,
									        int    right,
									        int    bottom, 
                                            Color color, 
                                            float fontSize);

            [PreserveSig]
	        int Clear();

            [PreserveSig]
	        int Remove(int id);

            [PreserveSig]
            int AddLine(int id,
                        int x1,
                        int y1,
                        int x2,
                        int y2,
                        Color color,
                        int width);


        }
        public Shapes()
        {

        }
        public Shapes(IBaseFilter baseFilter)
        {

            Type comType = null;
          
            comType = Type.GetTypeFromCLSID(Clsid.CLSID_TextOverlay);
            if (comType == null)
                throw new NotImplementedException(@"DirectShow FilterGraph not installed/registered!");
             
            shapeIface = (IShapes)baseFilter;

            /*
            shapeIface.AddTextOverlay("Eli Arad is here", 
                                        0, 
                                        10,
									    10,
                                       200, 100,
                                        Color.Red, 
                                        14);
             
            */
            //shapeIface.AddLine(1, 100, 100, 150, 150, Color.Red, 4);
            //shapeIface.Remove(1);
            
        }

        public void AddLine(int id,
                        int x1,
                        int y1,
                        int x2,
                        int y2,
                        Color color,
                        int width)
        {
            shapeIface.AddLine(id, x1,  y1, x2, y2, color, width);           
        }
        public void Remove(int id)
        {
            shapeIface.Remove(id);
        }
        
        public virtual void Clear()
        {
            shapeIface.Clear();
        }

        public void AddText(string text, 
                            int id, 
                            int left,
                            int    top,
							int    right,
							int    bottom, 
                            Color color, 
                            float fontSize)
        {
            shapeIface.AddTextOverlay(text, id, left, top, right, bottom, color, fontSize);
        }
         
        public int GetVal()
        {
            int r = 1;
            return r;
        }
    }
}
