using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Control
    {
        #region 싱글톤
        static Control instance = null;
        static readonly object padlock = new Object();
        public static Control Instance
        {
            get
            {
                lock(padlock)
                {
                    if(instance == null)
                    {
                        instance = new Control();
                    }
                    return instance;
                }
            }
        }
        public Control()
        {
            instance = this;
        }
        #endregion

        public ChannelDialog GetAddChannel(string cname)
        {
            for(int i = 0; i< Data.Instance.MyChannelList.Count; i++)
            {
                if(Data.Instance.MyChannelList[i].ChannelName == cname)
                {
                    return Data.Instance.MyChannelList[i];
                }
            }
            return null;
        }
        public void RequestMyChannelList()
        {
            string packet = "CHANNEL_INIT" + "$"; // 해당 아이디로 접속되어있는 채널들을 받기위해 패킷 전송
            packet += Data.Instance.myId;

            MyClient.Instance.SendDataOne(packet);
        }
        public void RefreshJoinChannelList()
        {
            string packet = "CHANNEL_JOINLIST" + "$";
            packet += Data.Instance.exam_cname + "#";
            packet += Data.Instance.myId;

            MyClient.Instance.SendDataOne(packet);
        }
        public void ExitRequestList()
        {
            string packet = "CHANNEL_EXITLIST" + "$";
            packet += Data.Instance.mybeforeChannel + "#";
            packet += Data.Instance.myId;

            MyClient.Instance.SendDataOne(packet);
        }

        public void Init_JoinChannel()
        {
            // initstartindex의 초기값은 0
            if (Data.Instance.Init_Header < Data.Instance.Init_IndexList.Length-1) // 총 접속된 채널의 개수만큼 조인 이벤트를 발생
            {
                string packet = "CHANNEL_INITJOIN" + "$";
                packet += Data.Instance.SearchChannelName(Data.Instance.Init_IndexList[Data.Instance.Init_Header]) + "#";
                packet += Data.Instance.myId;

                MyClient.Instance.SendDataOne(packet);

                Data.Instance.Init_Header++;
            }
            else
            {
                Data.Instance.Init_Check = false;
                Data.Instance.Init_IndexList = null;
                Data.Instance.Init_Header = 0;
            }
        }

        public void ChannelAllList()
        {
            string packet = "CHANNEL_ALLLIST" + "$";
            MyClient.Instance.SendDataOne(packet);
        }

    }

}
