using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace UMonitorWebAPI.Utility
{
    public class Utility
    {/// <summary>
     /// 取得IIS Server IP
     /// </summary>
     /// <returns></returns>
        public static string GetServerIPAddress()
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                // 只檢查啟用狀態的網絡介面
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        // 選擇 IPv4 地址
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            // 判斷介面類型
                            //if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                            //{
                            //    ethernetIP = ip.Address.ToString();
                            //}
                            if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            return "";
        }

        /// <summary>
        ///取得Docker Container 的IP
        /// </summary>
        /// <returns></returns>
        public static string GetDockerContainerBridgeIPAddress()
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                // 只檢查名稱為 eth0 的接口
                if (networkInterface.Name == "eth0" &&
                    networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
            return "";
        }

        /// <summary>
         ///取得Docker Container 的 Gateway
         /// </summary>
         /// <returns></returns>
        public static string GetDockerContainerBridgeGatewayAddress()
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Check for the network interface named "eth0"
                if (networkInterface.Name == "eth0" &&
                    networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    // Find the first IPv4 gateway address
                    foreach (GatewayIPAddressInformation gateway in networkInterface.GetIPProperties().GatewayAddresses)
                    {
                        if (gateway.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return gateway.Address.ToString();
                        }
                    }
                }
            }
            return "";
        }

        public static string GetLocalIPv4()
        {
            // 預設為空字串，表示未找到符合的IP
            string ipv4 = "";

            try
            {
                // 取得所有網路介面
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface inf in interfaces)
                {
                    // 過濾出狀態為"Up"且類型為無線網卡的介面
                    if (inf.OperationalStatus == OperationalStatus.Up &&
                        inf.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    {
                        // 取得介面的IP屬性
                        var props = inf.GetIPProperties();

                        // 找出IPv4地址
                        var ipv4Address = props.UnicastAddresses
                            .FirstOrDefault(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork);

                        if (ipv4Address != null)
                        {
                            ipv4 = ipv4Address.Address.ToString();
                            Debug.WriteLine($"Wireless IPv4 found: {ipv4}");
                            break; // 已找到無線網卡IP，退出迴圈
                        }
                    }
                }

                if (string.IsNullOrEmpty(ipv4))
                {
                    Debug.WriteLine("No Wireless IPv4 found.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving wireless IPv4: {ex.Message}");
            }

            return ipv4;
        }
    }
}
