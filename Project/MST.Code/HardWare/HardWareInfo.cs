using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace MST.Code.HardWare
{
    public class HardWareInfo
    {
        public static string GetCPUSerialNum()
        {
            string cpuInfo = "";//cpu序列号 
            ManagementClass cimobject = new ManagementClass("Win32_Processor");
            ManagementObjectCollection moc = cimobject.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                mo.Dispose();
            }
            moc.Dispose();
            cimobject.Dispose();
            //获取硬盘ID 
            return cpuInfo;


        }

        public static string GetHDidSerialNum()
        {
            string HDid = null;
            ManagementClass cimobject1 = new ManagementClass("Win32_DiskDrive");
            ManagementObjectCollection moc1 = cimobject1.GetInstances();
            foreach (ManagementObject mo in moc1)
            {
                HDid = (string)mo.Properties["Model"].Value;
                mo.Dispose();
            }
            cimobject1.Dispose();
            moc1.Dispose();
            return HDid;
        }

        public static string GetNetworkSerialNum()
        {
            //获取网卡硬件地址 
            string network = null;
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc2 = mc.GetInstances();
            foreach (ManagementObject mo in moc2)
            {
                if ((bool)mo["IPEnabled"] == true)
                {
                    network = mo["MacAddress"].ToString();
                }
                mo.Dispose();
            }
            mc.Dispose();
            moc2.Dispose();
            return network;
        }
    }
}
