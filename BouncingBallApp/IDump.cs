using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SightLineApp
{
     

    public class DumpCom
    {

        IFileSinkFilter dumpIface;
         

        [ComVisible(true), ComImport,      
         Guid("a2104830-7c70-11cf-8bce-00aa00a3f1a6"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IFileSinkFilter
        {
            [PreserveSig]
            int SetFileName([In] string fileName, IntPtr p);
             
        }

        public DumpCom(IBaseFilter baseFilter)
        {
            Type comType = null;      
            comType = Type.GetTypeFromCLSID(Clsid.CLSID_CLiveSource);
            if (comType == null)
                throw new NotImplementedException(@"DirectShow FilterGraph not installed/registered!");

            dumpIface = (IFileSinkFilter)baseFilter;
        }
        public int SetFileName(string fileName)
        {
            return dumpIface.SetFileName(fileName, System.IntPtr.Zero);
        }
         
 
    }
}
