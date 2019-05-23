using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ChannelList
    {
        public string ChannelIndex { get; set; }
        public string ChannelName { get; set; }
        public string ChannelTitle { get; set; }
        public string ChannelAdmin { get; set; }
        public string ChannelLoginCount { get; set; }
        public string ChannelMember { get; set; }
        public string ChannelIsPw { get; set; }
        public string ChannelPw { get; set; }

        // 채널 리스트 컬렉션 저장용 
        public ChannelList(string index, string name, string title, string admin, string lcount, string member, string ispw, string pw)
        {
            ChannelIndex = index;
            ChannelName = name; // 채널이름
            ChannelAdmin = admin; // 관리자이름
            ChannelMember = member; // 최대멤버
            ChannelIsPw = ispw; // 비밀번호 여부
            ChannelTitle = title;
            ChannelLoginCount = lcount;
            ChannelPw = pw;
        }

        // 채널 목록(리스트뷰) 띄우기 용
        public ChannelList(string name, string admin, string member, string ispw)
        {
            ChannelName = name; // 채널이름
            ChannelAdmin = admin; // 관리자이름
            ChannelMember = member; // 최대멤버
            ChannelIsPw = ispw; // 비밀번호 여부 
        }
    }
}
