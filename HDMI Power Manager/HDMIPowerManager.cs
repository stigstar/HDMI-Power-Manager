using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace HDMI_Power_Manager
{
    public partial class HdmiPowerManager : ServiceBase
    {
        private readonly EventLog _eventLog;
        private IntPtr _hConsoleDisplayState;
        private ServiceControlHandlerEx _myCallback;
        private const int SERVICE_CONTROL_STOP = 1;
        private const int SERVICE_CONTROL_SHUTDOWN = 5;
        Guid GUID_CONSOLE_DISPLAY_STATE = new Guid("6fe69556-704a-47a0-8f24-c28d936fda47");
        private const int DEVICE_NOTIFY_SERVICE_HANDLE = 0x00000001;
        private const int WM_POWERBROADCAST = 0x0218;

        public HdmiPowerManager()
        {
            InitializeComponent();
            _eventLog = new EventLog();

            if (!EventLog.SourceExists("HDMIPowerManagerSource"))
            {
                EventLog.CreateEventSource("HDMIPowerManagerSource", "HDMIPowerManagerLog");
            }
            _eventLog.Source = "HDMIPowerManagerSource";
            _eventLog.Log = "HDMIPowerManagerLog";
        }

       protected override void OnStart(string[] args)
        {
            _eventLog.WriteEntry("HDMI Power Manager starting...");
            // Update the service state to Start Pending.
           var serviceStatus = new ServiceStatus
           {
               dwCurrentState = ServiceState.SERVICE_START_PENDING,
               dwWaitHint = 100000
           };

           SetServiceStatus(ServiceHandle, ref serviceStatus);

            RegisterForPowerNotifications();

            _myCallback = new ServiceControlHandlerEx(ServiceControlHandler);
            
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            _eventLog.WriteEntry("HDMI Power Manager starting...");
            UnregisterForPowerNotifications();
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr RegisterServiceCtrlHandlerEx(string lpServiceName, ServiceControlHandlerEx cbex, IntPtr context);

        [DllImport(@"User32", EntryPoint = "RegisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid powerSettingGuid, int flags);

        [DllImport(@"User32", EntryPoint = "UnregisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnregisterPowerSettingNotification(IntPtr handle);

        public delegate int ServiceControlHandlerEx(int control, int eventType, IntPtr eventData, IntPtr context);

        private void RegisterForPowerNotifications()
        {
            RegisterServiceCtrlHandlerEx(ServiceName, _myCallback, IntPtr.Zero);
            _hConsoleDisplayState = RegisterPowerSettingNotification(ServiceHandle, ref GUID_CONSOLE_DISPLAY_STATE, DEVICE_NOTIFY_SERVICE_HANDLE);
        }

        private void UnregisterForPowerNotifications()
        {
            UnregisterPowerSettingNotification(_hConsoleDisplayState);
        }

        private int ServiceControlHandler(int control, int eventType, IntPtr eventData, IntPtr context)
        {
            if (control == SERVICE_CONTROL_STOP || control == SERVICE_CONTROL_SHUTDOWN)
            {
                UnregisterForPowerNotifications();
                Stop();
            }
            else if (control == WM_POWERBROADCAST)
            {
                switch (eventType)
                {
                    case WM_POWERBROADCAST:
                        _eventLog.WriteEntry("WM_POWERBROADCAST HAPPENED!");
                        break;
                }
            }

            return 0;
        }
    }
}
