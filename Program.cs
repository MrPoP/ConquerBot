using ConquerBot.NetWork;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConquerBot
{
    static class Program
    {
        #region CONSTANTS
        public static OOP.IClient Client;
        public static string RootPath = "";
        public static string CryptoGraphyKey = "";
        private static string IPAddress = null;
        public static Dictionary<uint, Structures.AuthInformation> AuthInformation = new Dictionary<uint, Structures.AuthInformation>();
        public static Dictionary<ushort, byte[]> ToClient = new Dictionary<ushort, byte[]>();
        public static Dictionary<ushort, byte[]> ToServer = new Dictionary<ushort, byte[]>();
        public static string RootIPAddress
        {
            set
            {
                IPAddress = value;
            }
            get
            {
                try
                {
                    if (IPAddress == null)
                    {
                        IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName().ToString());
                        IPAddress[] addr = ipEntry.AddressList;
                        return addr[addr.Length - 1].ToString();
                    }
                    else
                        return IPAddress;
                }
                catch { MessageBox.Show("Error 0x0001."); return null; }
            }
        }
        public static int LocalGamePort = int.Parse(ConfigurationManager.AppSettings["LocalGameServerPort"]), LocalAuthPort = int.Parse(ConfigurationManager.AppSettings["LocalAuthServerPort"]);
        public static string ServerAllocated = "";
        public static string AuthServerIP = ConfigurationManager.AppSettings["AuthServerIP"];
        public static int AuthServerPort = int.Parse(ConfigurationManager.AppSettings["AuthServerPort"]), GameServerPort = 0;
        public static string GameServerIP = "";
        #endregion
        public static event Action<Link> NewConnection, LostConnection;
        public static event Action<Link, byte[]> OnReceive;
        public static WinSocket Auth, Game;
        public const int SERVER_PAD_SIZE = 11, CLIENT_PAD_SIZE = 7;
        public static byte[] GameCryptographerKey = System.Text.ASCIIEncoding.ASCII.GetBytes(CryptoGraphyKey);
        public static byte[] TQClient = System.Text.ASCIIEncoding.ASCII.GetBytes("TQClient");
        public static byte[] TQServer = System.Text.ASCIIEncoding.ASCII.GetBytes("TQServer");
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
        public static bool Run()
        {
            NewConnection += new Action<Link>(Program_NewConnection);
            OnReceive += new Action<Link, byte[]>(Program_OnReceive);
            LostConnection += new Action<Link>(Program_LostConnection);

            Auth = new WinSocket(LocalAuthPort, NewConnection, LostConnection, OnReceive, LinkType.AuthClient);
            Game = new WinSocket(LocalGamePort, NewConnection, LostConnection, OnReceive, LinkType.GameClient);
            return true;
        }
        public static void Program_NewConnection(Link obj)
        {
            if (obj.Type == LinkType.AuthClient)
                obj.Owner = new Client.Auth(obj);
            else
                obj.Owner = new Client.Game(obj);
        }

        public static void Program_OnReceive(Link arg1, byte[] arg2)
        {
            switch (arg1.Type)
            {
                case LinkType.AuthClient:
                case LinkType.AuthServer: (arg1.Owner as Client.Auth).Handler.Handle(arg2, arg1.Type); break;

                case LinkType.GameClient:
                case LinkType.GameServer: (arg1.Owner as Client.Game).Handler.Handle(arg2, arg1.Type); break;
            }
        }

        public static void Program_LostConnection(Link obj)
        {
            if (obj.Owner != null)
            {
                if (obj.Owner is Client.Auth)
                    (obj.Owner as Client.Auth).Disconnect();
                else
                    (obj.Owner as Client.Game).Disconnect();
            }
        }
        
    }
}
