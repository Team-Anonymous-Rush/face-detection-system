using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.Structure;


namespace RUSH_PROJECT
{
    public partial class Form1 : Form
    {
        SqlConnection conn = new SqlConnection("Data Source=INDIKA;Initial Catalog=RUSH_PROJECT;Integrated Security=True");
        Byte[] b = null;
        FilterInfoCollection filter;
        VideoCaptureDevice device;
        string path;
        static readonly CascadeClassifier casecadeClassifier = new CascadeClassifier("haarcascade_frontalface_alt_tree.xml");
        DateTime now = DateTime.Now;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            filter = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in filter)
            {
                cbDevice.Items.Add(device.Name);
            }
            cbDevice.SelectedIndex = 0;
            device = new VideoCaptureDevice();
        }

        private void BtnStartCam_Click(object sender, EventArgs e)
        {
            Detection();
        }

        private void Detection()
        {
            device = new VideoCaptureDevice(filter[cbDevice.SelectedIndex].MonikerString);
            device.NewFrame += Device_NewFrame;
            device.Start();
        }


        private void Device_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            Image<Bgr, byte> grayImg = new Image<Bgr, byte>(bitmap);
            Rectangle[] rectangles = casecadeClassifier.DetectMultiScale(grayImg, 1.2, 1);
            foreach(Rectangle rectangle in rectangles)
            {
                using (Graphics graphic = Graphics.FromImage(bitmap))
                {
                    using (Pen pen = new Pen(Color.Crimson, 4))
                    {
                        graphic.DrawRectangle(pen, rectangle);
                        ScreenShot();
                    }
                }
            }
            pb.Image = bitmap;

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(device.IsRunning)
            {
                device.Stop();
            }
        }

        private void ScreenShot()
        {
            Bitmap bm = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(bm);
            g.CopyFromScreen(0,0,0,0,bm.Size);
            pb2.Image = bm;
            InsertInfo();

        }  

        private void Save()
        {
            var myUniqueFileName = $@"{DateTime.Now.Ticks}.txt";
            pb2.Image.Save("C:\\xampp\\htdocs\\RUSH_DATA\\'"+ myUniqueFileName + "'.jpg");
            path = "C:\\xampp\\htdocs\\RUSH_DATA\\'" + myUniqueFileName + "'.jpg";
        }

        private void Convereter()
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(stream);
            b = br.ReadBytes((int)stream.Length);
        }

        private void InsertInfo()
        {
            try
            {
                Save();
                Convereter();
                conn.Open();

                string query = "INSERT INTO table_SS(SCREENS) values(@images)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add(new SqlParameter("@images", b));

                cmd.ExecuteNonQuery();

                conn.Close();
            }
            catch(Exception error)
            {
                MessageBox.Show(error.ToString(),"This is an error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void BtnSERVER_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://localhost/RUSH_DATA/");
        }

        private void BtnFile_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("C:\\xampp\\htdocs\\RUSH_DATA");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
