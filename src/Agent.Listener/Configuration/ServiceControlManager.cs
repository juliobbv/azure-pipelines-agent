using System;
using System.Linq;
using Microsoft.VisualStudio.Services.Agent.Util;

namespace Microsoft.VisualStudio.Services.Agent.Listener.Configuration
{
#if OS_WINDOWS
    [ServiceLocator(Default = typeof(WindowsServiceControlManager))]
    public interface IWindowsServiceControlManager : IAgentService
    {
        void ConfigureService(AgentSettings settings, CommandSettings command);

        void UnconfigureService();
    }
#endif

#if !OS_WINDOWS

#if OS_LINUX
    [ServiceLocator(Default = typeof(SystemDControlManager))]
#elif OS_OSX
    [ServiceLocator(Default = typeof(OsxServiceControlManager))]
#endif
    public interface ILinuxServiceControlManager : IAgentService
    {
        void GenerateScripts(AgentSettings settings);
    }
#endif

    public class ServiceControlManager : AgentService
    {
        public void CalculateServiceName(AgentSettings settings, string serviceNamePattern, string serviceDisplayNamePattern, out string serviceName, out string serviceDisplayName)
        {
            Trace.Entering();
            serviceName = string.Empty;
            serviceDisplayName = string.Empty;

            Uri serverUrl = new Uri(url);	
            string accountName;
            
            if (string.Equals(serverUrl.Host, "dev.azure.com"))
            {
                // Account name is specified outside the host (e.g. https://dev.azure.com/myaccount)
                accountName = serverUrl.AbsolutePath.TrimStart(new[] { '/' }).Split('/').FirstOrDefault();
            }
            else
            {
                // Account name is specified within the host (e.g. https://myaccount.visualstudio.com, or http://onprem:8080/tfs)
                accountName = serverUrl.Host.Split('.').FirstOrDefault();
            }

            if (string.IsNullOrEmpty(accountName))
            {
                throw new InvalidOperationException(StringUtil.Loc("CannotFindHostName", settings.ServerUrl));
            }

            serviceName = StringUtil.Format(serviceNamePattern, accountName, settings.AgentName);
            serviceDisplayName = StringUtil.Format(serviceDisplayNamePattern, accountName, settings.AgentName);

            Trace.Info($"Service name '{serviceName}' display name '{serviceDisplayName}' will be used for service configuration.");
        }
    }
}