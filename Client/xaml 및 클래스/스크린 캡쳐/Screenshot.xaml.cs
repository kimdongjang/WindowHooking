using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace Client
{
    /// <summary>
    /// Screenshot.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Screenshot : Window
    {
        #region 편집, 저장 관련 전역 변수

        #endregion

        public Screenshot()
        {
            InitializeComponent();
        }

        #region Capture Function

        // Screen Capture :: 전체화면
        private void CaptureScreenButton_Click(object sender, RoutedEventArgs e)
        {
            //CapturedImage.Source = ScreenCapture.CaptureFullScreen(true);   // 이미지에 해당 캡쳐이미지 씌우기 

            int width = (int)SystemParameters.PrimaryScreenWidth;
            int height = (int)SystemParameters.PrimaryScreenHeight;

            using (Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                // Bitmap 이미지 변경을 위해 Graphics 객체 생성
                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    // 화면을 그대로 카피해서 Bitmap 메모리에 저장
                    gr.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                }
                xaml_CapturedImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }

        // Drag Capture :: 드래그 영역
        private void CaptureDragButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            // 투명한 윈도우를 바탕화면 전 영역에 덮어씌움
            DragCaptureWindow invWin = new DragCaptureWindow();
            invWin.ShowDialog();
            // 드래그 영역을 지정할 객체 생성
            // 마우스 다운 -> 마우스 업이 일어나면 객체를 닫고 아래 알고리즘 실행
            Bitmap bmap = new System.Drawing.Bitmap((int)invWin.SelRegon.Width, (int)invWin.SelRegon.Height); // Select한 Rect객체를 Bitmap으로 생성

            Graphics g = System.Drawing.Graphics.FromImage(bmap); // bitmap을 Graphics객체로 형변환

            g.CopyFromScreen(new System.Drawing.Point((int)invWin.SelRegon.Left, (int)invWin.SelRegon.Top),
                             new System.Drawing.Point(0, 0), new System.Drawing.Size((int)invWin.SelRegon.Width, (int)invWin.SelRegon.Height));
            xaml_CapturedImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            this.Show();
        }
        #endregion

        #region Save, Edit Function

        // 캡쳐본 저장
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // 이미지파일을 jpg로 저장 
            // 캡쳐본 경로 
            Data.Instance.screenpath = SaveFunction.Instance.g_filepath + SaveFunction.Instance.g_filenameexceptxml
                + "_image[" + Data.Instance.screenIndex.ToString() + "].jpg";

            // 캡쳐본을 담은 Image Control을 Bitmap으로 변환 후 위에서 지정한 경로에 저장
            RenderTargetBitmap bitmap = ConverterBitmapImage(xaml_CapturedImage);
            ImageSave(bitmap);

            // 이미지 xml으로 저장
            Data.Instance.Rec_list.Add("Image&" + Data.Instance.screenpath);
            //SaveFunction.Instance.XmlCreateRecordList(SaveFunction.Instance.realtime_folderpath + SaveFunction.Instance.realtime_savename);
            Data.Instance.screenIndex++;
            if (MessageBox.Show("해당 이미지가 저장되었습니다. 스크린 샷을 종료하시겠습니까?", "경고", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                this.Close(); 
                PauseEvent.Instance.opened_screenshot = false;
            }
            PauseEvent.Instance.Show();
        }

        // 해당 객체를 이미지로 변환
        private static RenderTargetBitmap ConverterBitmapImage(FrameworkElement element)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            // 해당 객체의 그래픽요소로 사각형의 그림을 그립니다.
            drawingContext.DrawRectangle(new VisualBrush(element), null, new Rect(new System.Windows.Point(0, 0), new System.Windows.Point(1920, 1080)));
            drawingContext.Close();

            // 비트맵으로 변환합니다.
            RenderTargetBitmap target = new RenderTargetBitmap(int.Parse(MainRecording.Instance.Window_Width.ToString()), int.Parse(MainRecording.Instance.Window_Height.ToString()),
                96, 96, System.Windows.Media.PixelFormats.Pbgra32);

            target.Render(drawingVisual);
            return target;
        }

        // 해당 이미지 .jpg로 바로 저장
        private void ImageSave(BitmapSource source)
        {
            BitmapEncoder encoder = new JpegBitmapEncoder();
            // 파일 생성
            FileStream stream = new FileStream(Data.Instance.screenpath, FileMode.Create, FileAccess.Write);

            encoder.Frames.Add(BitmapFrame.Create(source));
            // 파일에 저장
            encoder.Save(stream);

            stream.Close();

        }

        #endregion

        private void Window_Closed(object sender, EventArgs e)
        {
            PauseEvent.Instance.opened_screenshot = false;
            PauseEvent.Instance.Show();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }


    }
}