using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Client
{
    public class CpuControl
    {
        #region CPU 점유율
        public PerformanceCounter CPU = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        public bool ExitCpu = false;
        public int iCPU = 0; // 점유율 표기
        public Thread checkThread;
        #endregion

        #region 싱글톤
        static CpuControl instance = null;
        static readonly object padlock = new Object();
        public static CpuControl Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new CpuControl();
                    }
                    return instance;
                }
            }
        }
        public CpuControl()
        {
            // ===========================================================
            // CPU 점유율을 확인하기 위한 스레드 시작
            instance = this;
        }
        #endregion

        #region CPU 측정
        public void StartCpu() // CPU의 점유율을 확인하기 위한 클래스
        {
            checkThread = new Thread(getCPU_Info);
            checkThread.IsBackground = true;
            checkThread.Start();
        }

        public void getCPU_Info()
        {
            // CPU 측정 스레드
            while (!ExitCpu)
            {
                iCPU = (int)CPU.NextValue();
                // 델리게이트를 이용한 Recording클래스의 text 객체 제어
                MainRecording.Instance.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,(Action)delegate() { getCpu_InfoText(); });
                if (KeyboardHooking.Instance.recording_starting == true)
                {
                    MainRecording.Instance.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { MainRecording.Instance.ShowWindowDialog_Pause(); });
                }
                Thread.Sleep(1000);
            }
        }
        public void getCpu_InfoText()
        {
            MainRecording.Instance.this_Cpu.Text = iCPU.ToString() + "%";
        }

        #endregion
    }
}