using System.ServiceProcess;

namespace HDMI_Power_Manager
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new HdmiPowerManager()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
