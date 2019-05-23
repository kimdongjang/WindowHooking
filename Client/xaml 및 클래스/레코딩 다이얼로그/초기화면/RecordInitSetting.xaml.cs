using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace Client
{
    /// <summary>
    /// RecSet.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RecordInitSetting : Window
    {
        public RecordInitSetting()
        {
            InitializeComponent();
            Data.Instance.Rec_Starttime = System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
            GetWindowName();
        }
        public void GetWindowName()
        {
            const int nChars = 256;
            IntPtr handle;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = (IntPtr)ImportFunctions.GetForegroundWindow();

            if (ImportFunctions.GetWindowText((int)handle, Buff, nChars) > 0)
            {
                Content_textBox.Text = Buff.ToString();
                listView.Items.Add(new InitMemo(List_textBox.Text, Content_textBox.Text));
                listView.SelectedIndex = listView.Items.Count - 1;

                List_textBox.Text = "";
                Content_textBox.Text = "";
            }
        }
        public void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (listView.Items.IsEmpty == true)
            {

                if (MessageBox.Show("변경 사항이 없습니다. 이대로 진행하시겠습니까?", "경고", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    this.Hide();
                    System.Threading.Thread.Sleep(200);
                    ImageInitShot();
                    Save_InitMemo();
                    MainRecording.Instance.Record_Button();   // 녹화 시작 버튼                    
                    KeyboardHooking.Instance.is_startsetting = true;
                    Data.Instance.RecSetting_Ondialog = false;

                    InitImage();
                }
                else
                {
                    KeyboardHooking.Instance.recording_starting = false;
                }
            }
            else
            {
                if (MessageBox.Show("이대로 초기설정을 저장하고 녹화를 시작하겠습니까?", "경고", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    this.Hide();
                    System.Threading.Thread.Sleep(200);
                    ImageInitShot();

                    MainRecording.Instance.Record_Button();   // 녹화 시작 버튼
                    KeyboardHooking.Instance.is_startsetting = true;
                    Data.Instance.RecSetting_Ondialog = false;
                    InitImage();
                }
                else
                {
                    KeyboardHooking.Instance.recording_starting = false;
                }
            }
        }
        public void ImageInitShot()
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
                Init_Image.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }

        public void InitImage()
        {
            // 이미지파일을 jpg로 저장 
            // 캡쳐본 경로 
            Data.Instance.screenpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/" + "SLOG" + "/" + SaveFunction.Instance.g_filenameexceptxml
                + "_image[" + Data.Instance.screenIndex.ToString() + "].jpg";

            // 캡쳐본을 담은 Image Control을 Bitmap으로 변환 후 위에서 지정한 경로에 저장
            RenderTargetBitmap bitmap = InitConverterBitmapImage(Init_Image);
            InitImageSave(bitmap);

            // 이미지 xml으로 저장
            Data.Instance.Rec_list.Add("InitImg&" + Data.Instance.screenpath);
            //SaveFunction.Instance.XmlCreateRecordList();
            Data.Instance.screenIndex++;
        }

        public static RenderTargetBitmap InitConverterBitmapImage(FrameworkElement element)
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
        public void InitImageSave(BitmapSource source)
        {
            BitmapEncoder encoder = new JpegBitmapEncoder();
            // 파일 생성
            FileStream stream = new FileStream(Data.Instance.screenpath, FileMode.Create, FileAccess.Write);

            encoder.Frames.Add(BitmapFrame.Create(source));
            // 파일에 저장
            encoder.Save(stream);

            stream.Close();

        }
        private void Save_InitMemo()
        {
            try
            {
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            KeyboardHooking.Instance.recording_starting = false;
            Data.Instance.RecSetting_Ondialog = false;
            this.Hide();
            MainRecording.Instance.WindowStateChange("normal");
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (List_textBox.Text == "")
            {
                MessageBox.Show("항목을 입력하세요.");
                return;
            }
            if (Content_textBox.Text == "")
            {
                MessageBox.Show("내용을 입력하세요.");
                return;
            }

            listView.Items.Add(new InitMemo(List_textBox.Text, Content_textBox.Text));
            listView.SelectedIndex = listView.Items.Count - 1;

            List_textBox.Text = "";
            Content_textBox.Text = "";
            List_textBox.Focus();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (listView.Items.Count == 0)
                return;
            int selectedIndex = listView.SelectedIndex;
            listView.Items.Remove(listView.SelectedItem);
            Data.Instance.initmemo.RemoveAt(selectedIndex);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void List_textBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int sindex = List_textBox.SelectedIndex;
            if (sindex == 4)
            {
                List_textBox.Focus();
                List_textBox.Text = "";
                List_textBox.IsReadOnly = false;
            }
            else if (sindex != 4)
            {
                List_textBox.IsReadOnly = true;
            }
        }
    }

    public class InitMemo
    {
        public string Col1 { get; set; }
        public string Col2 { get; set; }
        public InitMemo(string s1, string s2)
        {
            Col1 = s1;
            Col2 = s2;
            Data.Instance.initmemo.Add(this);
        }
    }
}