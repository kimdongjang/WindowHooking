using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public class MouseHooking
    {
        #region 싱글톤
        static MouseHooking instance = null;
        static readonly object padlock = new Object();
        public static MouseHooking Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MouseHooking();
                    }
                    return instance;
                }
            }
        }
        public MouseHooking()
        {
            instance = this;
        }
        #endregion

        private Data dt_class = Data.Instance;

        #region 레코딩 함수
        void HookManager_MouseMove(object sender, MouseEventArgs e)
        {
            if (KeyboardHooking.Instance.module_StartOrStop == false)   // 기존 Recording 녹화
            {
                MainRecording.Instance.LogXamlList.Items.Add("X좌표 : " + e.X + " Y좌표 : " + e.Y);
                MainRecording.Instance.LogXamlList.ScrollIntoView(MainRecording.Instance.LogXamlList.Items[MainRecording.Instance.LogXamlList.Items.Count - 1]);

                // 레코딩 ShowList 이벤트 기록
                MainRecording.Instance.ShowList_Dlg.S_List.Items.Add("X좌표 : " + e.X + " Y좌표 : " + e.Y);
                MainRecording.Instance.ShowList_Dlg.S_List.ScrollIntoView(MainRecording.Instance.ShowList_Dlg.S_List.Items[MainRecording.Instance.ShowList_Dlg.S_List.Items.Count - 1]);
                dt_class.Rec_list.Add(e.X.ToString() + "§" + e.Y.ToString()); // 녹화 리스트에 좌표 저장 
            }
            else if (KeyboardHooking.Instance.module_StartOrStop)   // 모듈 Recording 녹화
            {
                // 레코딩 SampleModule 이벤트 기록
                MainRecording.Instance.SampleModule_Dlg.M_List.Items.Add("X좌표 : " + e.X + " Y좌표 : " + e.Y);
                MainRecording.Instance.SampleModule_Dlg.M_List.ScrollIntoView(MainRecording.Instance.SampleModule_Dlg.M_List.Items[MainRecording.Instance.SampleModule_Dlg.M_List.Items.Count - 1]);
                dt_class.ModRec_list.Add(e.X.ToString() + "§" + e.Y.ToString()); // 모듈녹화 리스트에 좌표 저장 
            }
        }
        void HookManager_MouseDown(object sender, MouseEventArgs e)
        {
            if (KeyboardHooking.Instance.module_StartOrStop == false)   // 기존 Recording 녹화
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    MainRecording.Instance.LogXamlList.Items.Add(e.Button + "버튼 상태 : " + e.Button + " Down ");
                    MainRecording.Instance.LogXamlList.ScrollIntoView(MainRecording.Instance.LogXamlList.Items[MainRecording.Instance.LogXamlList.Items.Count - 1]);

                    // 레코딩 ShowList 이벤트 기록
                    MainRecording.Instance.ShowList_Dlg.S_List.Items.Add(e.Button + "버튼 상태 : " + e.Button + " Down ");
                    MainRecording.Instance.ShowList_Dlg.S_List.ScrollIntoView(MainRecording.Instance.ShowList_Dlg.S_List.Items[MainRecording.Instance.ShowList_Dlg.S_List.Items.Count - 1]);
                }
                else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    MainRecording.Instance.LogXamlList.Items.Add(e.Button + "버튼 상태 : " + e.Button + " Down ");
                    MainRecording.Instance.LogXamlList.ScrollIntoView(MainRecording.Instance.LogXamlList.Items[MainRecording.Instance.LogXamlList.Items.Count - 1]);

                    // 레코딩 ShowList 이벤트 기록
                    MainRecording.Instance.ShowList_Dlg.S_List.Items.Add(e.Button + "버튼 상태 : " + e.Button + " Down ");
                    MainRecording.Instance.ShowList_Dlg.S_List.ScrollIntoView(MainRecording.Instance.ShowList_Dlg.S_List.Items[MainRecording.Instance.ShowList_Dlg.S_List.Items.Count - 1]);
                }
                dt_class.Rec_list.Add(e.Button.ToString() + "Down");
            }
            else if (KeyboardHooking.Instance.module_StartOrStop)   // 모듈 Recording 녹화
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    // 레코딩 SampleModule 이벤트 기록
                    MainRecording.Instance.SampleModule_Dlg.M_List.Items.Add(e.Button + "버튼 상태 : " + e.Button + " Down ");
                    MainRecording.Instance.SampleModule_Dlg.M_List.ScrollIntoView(MainRecording.Instance.SampleModule_Dlg.M_List.Items[MainRecording.Instance.SampleModule_Dlg.M_List.Items.Count - 1]);
                }
                else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    // 레코딩 SampleModule 이벤트 기록
                    MainRecording.Instance.SampleModule_Dlg.M_List.Items.Add(e.Button + "버튼 상태 : " + e.Button + " Down ");
                    MainRecording.Instance.SampleModule_Dlg.M_List.ScrollIntoView(MainRecording.Instance.SampleModule_Dlg.M_List.Items[MainRecording.Instance.SampleModule_Dlg.M_List.Items.Count - 1]);
                }
                dt_class.ModRec_list.Add(e.Button.ToString() + "Down");
            }
        }
        void HookManager_MouseUp(object sender, MouseEventArgs e)
        {
            if (KeyboardHooking.Instance.module_StartOrStop == false)   // 기존 Recording 녹화
            {
                MainRecording.Instance.LogXamlList.Items.Add(e.Button + "버튼 상태 : " + e.Button + " Up ");
                MainRecording.Instance.LogXamlList.ScrollIntoView(MainRecording.Instance.LogXamlList.Items[MainRecording.Instance.LogXamlList.Items.Count - 1]);

                // 레코딩 ShowList 이벤트 기록
                MainRecording.Instance.ShowList_Dlg.S_List.Items.Add(e.Button + "버튼 상태 : " + e.Button + " Up ");
                MainRecording.Instance.ShowList_Dlg.S_List.ScrollIntoView(MainRecording.Instance.ShowList_Dlg.S_List.Items[MainRecording.Instance.ShowList_Dlg.S_List.Items.Count - 1]);

                dt_class.Rec_list.Add(e.Button.ToString() + "Up");
            }
            else if (KeyboardHooking.Instance.module_StartOrStop)   // 모듈 Recording 녹화
            {
                // 레코딩 SampleModule 이벤트 기록
                MainRecording.Instance.SampleModule_Dlg.M_List.Items.Add(e.Button + "버튼 상태 : " + e.Button + " Up ");
                MainRecording.Instance.SampleModule_Dlg.M_List.ScrollIntoView(MainRecording.Instance.SampleModule_Dlg.M_List.Items[MainRecording.Instance.SampleModule_Dlg.M_List.Items.Count - 1]);

                dt_class.ModRec_list.Add(e.Button.ToString() + "Up");
            }
        }
        void HookManager_MouseWheel(object sender, MouseEventArgs e)
        {
            if (KeyboardHooking.Instance.module_StartOrStop == false)   // 기존 Recording 녹화
            {
                MainRecording.Instance.LogXamlList.Items.Add("휠 상태 : " + e.Delta);
                MainRecording.Instance.LogXamlList.ScrollIntoView(MainRecording.Instance.LogXamlList.Items[MainRecording.Instance.LogXamlList.Items.Count - 1]);

                // 레코딩 ShowList 이벤트 기록
                MainRecording.Instance.ShowList_Dlg.S_List.Items.Add("휠 상태 : " + e.Delta);
                MainRecording.Instance.ShowList_Dlg.S_List.ScrollIntoView(MainRecording.Instance.ShowList_Dlg.S_List.Items[MainRecording.Instance.ShowList_Dlg.S_List.Items.Count - 1]);

                dt_class.Rec_list.Add("※" + e.Delta.ToString());
            }

            else if (KeyboardHooking.Instance.module_StartOrStop)   // 모듈 Recording 녹화
            {
                // 레코딩 SampleModule 이벤트 기록
                MainRecording.Instance.SampleModule_Dlg.M_List.Items.Add("휠 상태 : " + e.Delta);
                MainRecording.Instance.SampleModule_Dlg.M_List.ScrollIntoView(MainRecording.Instance.SampleModule_Dlg.M_List.Items[MainRecording.Instance.SampleModule_Dlg.M_List.Items.Count - 1]);

                dt_class.ModRec_list.Add("※" + e.Delta.ToString());
            }

        }
        //}
        //void HookManager_MouseDoubleClick(object sender, MouseEventArgs e)
        //{
        //    if (KeyboardHooking.Instance.module_StartOrStop == false)   // 기존 Recording 녹화
        //    {
        //        MainRecording.Instance.LogXamlList.Items.Add(" 더블클릭 " + e.Clicks);
        //        MainRecording.Instance.LogXamlList.ScrollIntoView(MainRecording.Instance.LogXamlList.Items[MainRecording.Instance.LogXamlList.Items.Count - 1]);

        //        // 레코딩 ShowList 이벤트 기록
        //        MainRecording.Instance.ShowList_Dlg.S_List.Items.Add(" 더블클릭 " + e.Clicks);
        //        MainRecording.Instance.ShowList_Dlg.S_List.ScrollIntoView(MainRecording.Instance.ShowList_Dlg.S_List.Items[MainRecording.Instance.ShowList_Dlg.S_List.Items.Count - 1]);

        //        dt_class.Rec_list.Add("Double");
        //    }

        //    else if (KeyboardHooking.Instance.module_StartOrStop)   // 모듈 Recording 녹화
        //    {
        //        // 레코딩 SampleModule 이벤트 기록
        //        MainRecording.Instance.SampleModule_Dlg.M_List.Items.Add(" 더블클릭 " + e.Clicks);
        //        MainRecording.Instance.SampleModule_Dlg.M_List.ScrollIntoView(MainRecording.Instance.SampleModule_Dlg.M_List.Items[MainRecording.Instance.SampleModule_Dlg.M_List.Items.Count - 1]);

        //        dt_class.ModRec_list.Add("Double");
        //    }
        //}

        //타이머 이벤트 ======================================================
        //private void timer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    move_time++;
        //}
        #endregion

        public void MouseStart()
        {
            MouseHooker.MouseMove += HookManager_MouseMove;
            MouseHooker.MouseUp += HookManager_MouseUp;
            MouseHooker.MouseDown += HookManager_MouseDown;
            MouseHooker.MouseWheel += HookManager_MouseWheel;
            //MouseHooker.MouseDoubleClick += HookManager_MouseDoubleClick;
        }
        public void MouseStop()
        {
            MouseHooker.MouseMove -= HookManager_MouseMove;
            MouseHooker.MouseUp -= HookManager_MouseUp;
            MouseHooker.MouseDown -= HookManager_MouseDown;
            MouseHooker.MouseWheel -= HookManager_MouseWheel;
            //MouseHooker.MouseDoubleClick -= HookManager_MouseDoubleClick;
        }
    }
}
