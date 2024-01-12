using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

/**
 *
            /// 安装服务 方法
            //string server = "WMSService";
            //bool isExists = InstallerHelp.ServiceIsExisted(server);
            //if (!isExists)
            //{
            //    string path = Server.MapPath("~/") + "bin\\MST.Server.exe";
            //    InstallerHelp.UnInstallService(path, server);   //卸载
            //    InstallerHelp.InsertService(path, ref server);  //安装
            //    InstallerHelp.ChangeServiceStartType(2, server);//配置为自动启动
            //    InstallerHelp.StartService(server);             //启动
            //}
 *
 */

namespace MST.Code
{
    public class InstallerHelp
    {
        /// <summary>
        /// 安装服务
        /// </summary>
        /// <param name="filepath"></param>
        public static void InstallService(string filepath,string serviceName)
        {
            try
            {
                IDictionary stateSaver = new Hashtable();
                System.ServiceProcess.ServiceController service = new System.ServiceProcess.ServiceController("ServiceName");
                if (!ServiceIsExisted(serviceName))
                {
                    AssemblyInstaller myAssemblyInstaller = new AssemblyInstaller();
                    myAssemblyInstaller.UseNewContext = true;
                    myAssemblyInstaller.Path = filepath;
                    myAssemblyInstaller.Install(stateSaver);
                    myAssemblyInstaller.Commit(stateSaver);
                    myAssemblyInstaller.Dispose();
                    service.Start();
                }
                else
                {
                    if (service.Status != System.ServiceProcess.ServiceControllerStatus.Running && service.Status != System.ServiceProcess.ServiceControllerStatus.StartPending)
                    {
                        service.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("installServiceError/n" + ex.Message);
            }
        }

        /// <summary>
        ///或者 安装服务
        /// </summary>
        /// <param name="p_Path">指定服务文件路径</param>
        /// <param name="p_ServiceName">返回安装完成后的服务名</param>
        /// <returns>安装信息 正确安装返回""</returns>
        public static string InsertService(string p_Path, ref string p_ServiceName)
        {
            if (!File.Exists(p_Path)) return "文件不存在！";
            p_ServiceName = "";
            FileInfo _InsertFile = new FileInfo(p_Path);
            IDictionary _SavedState = new Hashtable();
            try
            {
                //加载一个程序集，并运行其中的所有安装程序。
                AssemblyInstaller _AssemblyInstaller = new AssemblyInstaller(p_Path, new string[] { "/LogFile=" + _InsertFile.DirectoryName + "//" + _InsertFile.Name.Substring(0, _InsertFile.Name.Length - _InsertFile.Extension.Length) + ".log" });
                _AssemblyInstaller.UseNewContext = true;
                _AssemblyInstaller.Install(_SavedState);
                _AssemblyInstaller.Commit(_SavedState);
                Type[] _TypeList = _AssemblyInstaller.Assembly.GetTypes();//获取安装程序集类型集合
                for (int i = 0; i != _TypeList.Length; i++)
                {
                    if (_TypeList[i].BaseType.FullName == "System.Configuration.Install.Installer")
                    {
                        //找到System.Configuration.Install.Installer 类型
                        object _InsertObject = System.Activator.CreateInstance(_TypeList[i]);//创建类型实列
                        FieldInfo[] _FieldList = _TypeList[i].GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                        for (int z = 0; z != _FieldList.Length; z++)
                        {
                            if (_FieldList[z].FieldType.FullName == "System.ServiceProcess.ServiceInstaller")
                            {
                                object _ServiceInsert = _FieldList[z].GetValue(_InsertObject);
                                if (_ServiceInsert != null)
                                {
                                    p_ServiceName = ((ServiceInstaller)_ServiceInsert).ServiceName;
                                    return p_ServiceName;
                                }
                            }
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 卸载windows服务
        /// </summary>
        /// <param name="filepath"></param>
        public static void UnInstallService(string filepath,string serviceName)
        {
            try
            {
                if (ServiceIsExisted(serviceName))
                {
                    AssemblyInstaller myAssemblyInstaller = new AssemblyInstaller();
                    myAssemblyInstaller.UseNewContext = true;
                    myAssemblyInstaller.Path = filepath;
                    myAssemblyInstaller.Uninstall(null);
                    myAssemblyInstaller.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("unInstallServiceError/n" + ex.Message);
            }
        }


        /// <summary>
        /// 判断window服务是否存在
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool ServiceIsExisted(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services)
            {
                if (s.ServiceName == serviceName)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="serviceName"></param>
        public static void StartService(string serviceName)
        {
            if (ServiceIsExisted(serviceName))
            {
                System.ServiceProcess.ServiceController service = new System.ServiceProcess.ServiceController(serviceName);
                if (service.Status != System.ServiceProcess.ServiceControllerStatus.Running && service.Status != System.ServiceProcess.ServiceControllerStatus.StartPending)
                {
                    service.Start();
                    for (int i = 0; i < 60; i++)
                    {
                        service.Refresh();
                        System.Threading.Thread.Sleep(1000);
                        if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                        {
                            break;
                        }
                        if (i == 59)
                        {
                            throw new Exception(serviceName + ":启动失败");
                        }
                    }
                }
            }
        }


        //另外方法
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool ServiceStart(string serviceName)
        {
            try
            {
                ServiceController service = new ServiceController(serviceName);
                if (service.Status == ServiceControllerStatus.Running)
                {
                    return true;
                }
                else
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(1000 * 10);
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }
            }
            catch
            {
                return false;
            }
            return true;

        }


        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="serviceName"></param>
        public static void StopService(string serviceName)
        {
            if (ServiceIsExisted(serviceName))
            {
                System.ServiceProcess.ServiceController service = new System.ServiceProcess.ServiceController(serviceName);
                if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                {
                    service.Stop();
                    for (int i = 0; i < 60; i++)
                    {
                        service.Refresh();
                        System.Threading.Thread.Sleep(1000);
                        if (service.Status == System.ServiceProcess.ServiceControllerStatus.Stopped)
                        {
                            break;
                        }

                        if (i == 59)
                        {
                            throw new Exception(serviceName + ":停止失败");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="serviseName"></param>
        /// <returns></returns>
        public static bool ServiceStop(string serviseName)
        {
            try
            {
                ServiceController service = new ServiceController(serviseName);
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    return true;
                }
                else
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(1000 * 10);
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 修改服务的启动项 2为自动,3为手动
        /// </summary>
        /// <param name="startType"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool ChangeServiceStartType(int startType, string serviceName)
        {
            try
            {
                RegistryKey regist = Registry.LocalMachine;
                RegistryKey sysReg = regist.OpenSubKey("SYSTEM");
                RegistryKey currentControlSet = sysReg.OpenSubKey("CurrentControlSet");
                RegistryKey services = currentControlSet.OpenSubKey("Services");
                RegistryKey servicesName = services.OpenSubKey(serviceName, true);
                servicesName.SetValue("Start", startType);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取服务启动类型 2为自动 3为手动 4 为禁用
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static string GetServiceStartType(string serviceName)
        {
            try
            {
                RegistryKey regist = Registry.LocalMachine;
                RegistryKey sysReg = regist.OpenSubKey("SYSTEM");
                RegistryKey currentControlSet = sysReg.OpenSubKey("CurrentControlSet");
                RegistryKey services = currentControlSet.OpenSubKey("Services");
                RegistryKey servicesName = services.OpenSubKey(serviceName, true);
                return servicesName.GetValue("Start").ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 验证服务是否启动 服务是否存在
        /// </summary>
        /// <returns></returns>
        public static bool ServiceIsRunning(string serviceName)
        {
            ServiceController service = new ServiceController(serviceName);
            if (service.Status == ServiceControllerStatus.Running)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
