using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1209_team_study
{
    class Data
    {
        #region 싱글톤
        static Data single_control;
        public static Data Instance { get { return single_control; } }
        static Data() { single_control = new Data(); }
        private Data()
        {

        }
        #endregion

        #region 전역 변수

        public int channel_index;
        public string playinit_filename = "";
        public string Today;
        public string ClientID;
        public string UseProgram = "입력값 없음";
        public string Comment;
        public string FileName;
        #endregion
    }
}
