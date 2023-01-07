using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Net.Mail;

namespace JpoApi
{
    public class NetworkState : IDisposable
    {
        private bool disposedValue;
        public List<PhysicalAddress> m_macaddress { get; set; }
        public List<IPAddress> m_ipv4address { get; set; }
        public List<IPAddress> m_ipv6address { get; set; }

        public Int64 m_i64macaddress { get; set; }

        public NetworkState()
        {
            GetPhysicalAddress();
            GetLocalIPAddress();
            if(m_macaddress.Count > 0)
            {
                m_i64macaddress = BitConverter.ToUInt16(m_macaddress[0].GetAddressBytes(), 4);
                m_i64macaddress <<= 32;
                m_i64macaddress |= BitConverter.ToUInt32(m_macaddress[0].GetAddressBytes(), 0);
            } else
            {
                m_i64macaddress = 0;
            }
        }
        private void GetPhysicalAddress()
        {
            m_macaddress = new List<PhysicalAddress>();
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var adapter in interfaces)
            {
                if (OperationalStatus.Up == adapter.OperationalStatus)
                {
                    if ((NetworkInterfaceType.Unknown != adapter.NetworkInterfaceType) &&
                        (NetworkInterfaceType.Loopback != adapter.NetworkInterfaceType))
                    {
                        m_macaddress.Add(adapter.GetPhysicalAddress());
                    }
                }
            }
        }
        private void GetLocalIPAddress()
        {
            m_ipv4address = new List<IPAddress>();
            m_ipv6address = new List<IPAddress>();
            // 物理インターフェース情報をすべて取得
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            // 各インターフェースごとの情報を調べる
            foreach (var adapter in interfaces)
            {
                // 有効なインターフェースのみを対象とする
                if (adapter.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                // インターフェースに設定されたIPアドレス情報を取得
                var properties = adapter.GetIPProperties();

                // 設定されているすべてのユニキャストアドレスについて
                foreach (var unicast in properties.UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        // IPv4アドレス
                        m_ipv4address.Add(unicast.Address);
                    }
                    else if (unicast.Address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        // IPv6アドレス
                        m_ipv6address.Add(unicast.Address);
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~NetworkState()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
