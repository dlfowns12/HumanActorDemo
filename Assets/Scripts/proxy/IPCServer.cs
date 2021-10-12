using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

public class IPCServer
{
#if UNITY_LINUX_RECORDER || UNITY_EDITOR

    private Socket mListenSocket = null;
    private const int mMaxBufferSize = 20000;
    private byte[] mDataBuffer = null;
    private bool mInterrupt = false;
    private ArrayList mReadList = new ArrayList();
    public delegate void CommandListener(string data);
    private CommandListener mAvatarCommand;
    private Thread mWorker = null;
    private Dictionary<Socket, ClientState> mClientList = new Dictionary<Socket, ClientState>();

    private class ClientState
    {
        public Socket socket;
        public byte[] readBuff = new byte[1024];
    }

    public IPCServer(string ipaddr, int port)
    {
        mDataBuffer = new byte[mMaxBufferSize];
        IPAddress host = IPAddress.Parse(ipaddr);
        IPEndPoint hostAddr = new IPEndPoint(host, port);
        mListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        mListenSocket.Bind(hostAddr);
        mListenSocket.Listen(1);
        mListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        mReadList.Add(mListenSocket);
        Debug.Log("IPCServer Construct ...");
    }

    public void setCommandListener(CommandListener l)
    {
        mAvatarCommand = l;
    }

    public void run()
    {
        mWorker = new Thread(new ThreadStart(Work));
        mWorker.Start();
    }

    private void Work()
    {
        Debug.Log("IPCServer Work ...mSocketList size:" + mReadList.Count);
        while (!mInterrupt)
        {
            mReadList.Clear();
            mReadList.Add(mListenSocket);
            
            foreach (var client in mClientList.Values)
            {
                mReadList.Add(client.socket);
            }

            Socket.Select(mReadList, null, null, 2000);
            foreach (Socket sock in mReadList)
            {
                if (sock == mListenSocket)
                {
                    ReadListenfd(sock);
                }
                else
                {
                    ReadClientfd(sock);
                }
            }
/*
            if (sock == mListenSocket)
                {
                    Debug.Log("IPCServer listen accept ...");
                    Socket acceptSock = mListenSocket.Accept();
                    mSocketList.Add(acceptSock);
                }
                else
                {
                    Debug.Log("IPCServer Receive data ...");
                    int retLen = sock.Receive(mDataBuffer);
                    if (retLen <= 0)
                    {
                        break;
                    }
                    string jsonData = Encoding.ASCII.GetString(mDataBuffer, 0, retLen);
                    if (mAvatarCommand != null)
                    {
                        mAvatarCommand(jsonData);
                    }
                }
*/            

        }

        foreach (Socket sock in mReadList)
        {
            sock.Close();
        }
    }

    private ArrayList SplitBuffer(byte[] buffer, int len)
    {
        ArrayList dataList = new ArrayList();
        int dataLen = 0;
        int offset = 0;
        Debug.Log("SplitBuffer buffer len:" + len);
        for (int i = 0; i < len; i++)
        {
            dataLen++;
            if (buffer[i] == '\0')
            {
                byte[] dataBuffer = new byte[dataLen];
                Array.Copy(buffer, offset, dataBuffer, 0, dataLen);
                offset += dataLen;
                Debug.Log("SplitBuffer dataLen:" + dataLen);
                Debug.Log("SplitBuffer dataBuffer:" + dataBuffer);
                dataLen = 0;
                dataList.Add(dataBuffer);
                continue;
            }
        }
        return dataList;
    }

    public void ReadListenfd(Socket listenfd)
    {
        Debug.Log("ReadListenfd Accept ...");
        Socket clientfd = listenfd.Accept();
        ClientState state = new ClientState();
        state.socket = clientfd;
        mClientList.Add(clientfd, state);
    }

    public bool ReadClientfd(Socket clientfd)
    {
        ClientState state = mClientList[clientfd];
        int count = 0;
        Debug.Log("ReadClientfd ...");
        try
        {
            count = clientfd.Receive(state.readBuff);
            Debug.Log("ReadClientfd count:" + count);
        }
        catch (SocketException ex)
        {
            clientfd.Close();
            mClientList.Remove(clientfd);
            Debug.LogError($"Receive Socket Exception {ex.ToString()}");
            return false;
        }
        if (count == 0)
        {
            clientfd.Close();
            mClientList.Remove(clientfd);
            Debug.LogWarning("Socket close");
            return false;
        }

        //string jsonData = Encoding.ASCII.GetString(state.readBuff, 0, count);
        ArrayList dataList = SplitBuffer(state.readBuff, count);
        foreach (byte[] dat in dataList)
        {
            string jsonData = Encoding.ASCII.GetString(dat, 0, dat.Length);
            if (mAvatarCommand != null)
            {
                mAvatarCommand(jsonData);
            }
        }
        
        /*
                string recvStr = System.Text.Encoding.Default.GetString(state.readBuff, 0, count);
                Debug.Log($"Rec {recvStr}");
                string strFromClientWithTime = DateTime.Now.ToString("hh:mm") + recvStr;
                byte[] sendBytes = System.Text.Encoding.Default.GetBytes(strFromClientWithTime);
                foreach (ClientState cs in mClientList.Values)
                {
                    cs.socket.Send(sendBytes);
                }
        */
        return true;
    }

    public void exit()
    {
        mInterrupt = true;
    }
#endif
}
