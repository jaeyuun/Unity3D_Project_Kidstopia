using kcp2k;
using LitJson;
/* Network namespace */
using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum ServerType
{
    Empty = 0,
    Server,
    Client
}

public class ServerItem
{ // license file class
    public string license;
    public string serverIp;
    public string port;

    public ServerItem(string license, string ipValue, string port)
    {
        this.license = license;
        serverIp = ipValue;
        this.port = port;
    }
}

public class ServerChecker : MonoBehaviour
{
    public static ServerChecker instance = null;

    public ServerType type;

    public NetworkManager manager;
    private KcpTransport kcp;

    private string path = string.Empty;
    public string serverIp { get; private set; }
    public string serverPort { get; private set; }

    [Header("Input My IP")]
    public string myIp = string.Empty;
    public string stringType = string.Empty;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        FileSetting();

        manager = GetComponent<NetworkManager>();
        kcp = (KcpTransport)manager.transport;
    }
    #region FileSetting
    private void FileSetting()
    {
        if (path.Equals(string.Empty))
        {
            path = Application.persistentDataPath + "/License";
        }
        if (!File.Exists(path))
        { // folder 검사
            Directory.CreateDirectory(path);
        }
        if (!File.Exists(path + "/License.json"))
        { // file 검사
            DefaultData(path);
        }
    }

    private void DefaultData(string path)
    {
        List<ServerItem> items = new List<ServerItem>();
        items.Add(new ServerItem("2", "13.124.181.154", "7777"));
        JsonData data = JsonMapper.ToJson(items); // 반드시 자료구조로 넣어야 함
        File.WriteAllText(path + "/License.json", data.ToString());
    }
    #endregion
    private void Start()
    {
        type = LicenseType();
        if (type.Equals(ServerType.Server))
        {
            StartServer();
        }
    }

    private ServerType LicenseType()
    {
        ServerType type = ServerType.Empty;
        try
        {
            string jsonString = File.ReadAllText(path + "/License.json");
            JsonData itemData = JsonMapper.ToObject(jsonString);
            string stringType = this.stringType;
            // string stringType = itemData[0]["license"].ToString();
            string stringServerIp = itemData[0]["serverIp"].ToString();
            string stringPort = itemData[0]["port"].ToString();
            serverIp = myIp;
            // serverIp = stringServerIp;
            serverPort = stringPort;

            type = (ServerType)Enum.Parse(typeof(ServerType), stringType); // string to enum

            manager.networkAddress = serverIp;
            kcp.port = ushort.Parse(serverPort);
            return type;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return ServerType.Empty;
        }
    }

    public void StartServer()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("WebGL can not be Server");
        }
        else
        {
            Debug.Log($"{manager.networkAddress} StartServer...");
            manager.StartServer();

            NetworkServer.OnConnectedEvent += (NetworkConnectionToClient) =>
            {
                Debug.Log($"New Client Connect : {NetworkConnectionToClient.address}");
            };
            NetworkServer.OnDisconnectedEvent += (NetworkDisconnectionToClient) =>
            {
                Debug.Log($"Client Disconnect : {NetworkDisconnectionToClient.address}");
            };
        }
    }

    public void StartClient()
    {
        Debug.Log($"{manager.networkAddress} : StartClient...");
        manager.StartClient();
    }

    private void OnApplicationQuit()
    {
        if (NetworkClient.isConnected)
        {
            User_info info = SQLManager.instance.info;
            SQLManager.instance.UpdateUserInfo("connecting", 'F', info.User_Id);
            manager.StopClient();
        }
        if (NetworkServer.active)
        {
            SQLManager.instance.UpdateUserInfo_Server();
            manager.StopServer();
        }
    }
}