using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFilter
{
    public partial class MainWindow : Form
    {
        EnumWindows enumwindows = new EnumWindows();
        public MainWindow()
        {
            InitializeComponent();
            setFullScreen();

        }
        /// <summary>
        /// Set main window full screen and update UI
        /// </summary>
        public void setFullScreen()
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            updateWindowsList();
            updateUI();

        }           
        /// <summary>
        /// 
        /// </summary>
        /// <param name="is_clean">whether clear the previous result. This is useful when open several pdf files together while EnumWindows can only return the main window (last one with focus) </param>
        private void updateWindowsList(Boolean is_clean=false)
        {
            enumwindows.getProcess(is_clean);
            List<String> s = enumwindows.getContents();
            listView1.Items.Clear();
            foreach (string s_ in s)
            {
                listView1.Items.Add(s_);
            }
        }
        private void refreshBtnClick(object sender, EventArgs e)
        {
            updateWindowsList();
        }        
        /// <summary>
        /// show the image of selected windows to imageBox
        /// </summary>
        private void showTargetImage()
        {
            if (selectedIndex>=0 && selectedIndex<enumwindows.getSize())
            {
               
                Bitmap win_bitmap = WindowsBitmap.grabWindowBitmap(enumwindows.getHandle(selectedIndex));
                win_bitmap = WindowsBitmap.unsafeExpand(win_bitmap,binaryThreshold,false,dx,dx);
                pictureBox1.Image = win_bitmap;
            }
        }
        private int selectedIndex = -1;


        private void updateRatio()
        {


            WindowsBitmap.ratio_x = WindowsBitmap.ratio_x0 * printBitmapRatio;
            WindowsBitmap.ratio_y = WindowsBitmap.ratio_y0 * printBitmapRatio;
            updateUI();
        }
        private void updateLabels()
        {
            if (pictureBox1.Image != null)
                label1.Text = String.Format("({0},{1})->({2},{3})", this.Width, this.Height, pictureBox1.Image.Width, pictureBox1.Image.Height);
            else
                label1.Text = String.Format("({0},{1}) ", this.Width, this.Height);
            label2.Text = String.Format("{0}", binaryThreshold);
            label3.Text = String.Format("{0}", printBitmapRatio);
            label5.Text = String.Format("Index:{0}", selectedIndex);
            label6.Text = String.Format("{0}", dx);

            label8.Text = String.Format("{0}", screen_dx);
            label9.Text = String.Format("{0}", screen_dy);
            label15.Text = String.Format("{0}", screen_pos_dx);
            label16.Text = String.Format("{0}", screen_pos_dy);
        }
        private void updateUI()
        {

            showTargetImage();
            updateLabels();

        }

        private void previousPageClicked(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                int index = listView1.SelectedIndices[0];
                enumwindows.sendMessage(index, "prior");
                showTargetImage();
                //update_image_fully();
                //enumwindows.setToFront(Process.GetCurrentProcess().MainWindowHandle);
            }
        }

        private void nextPageClicked(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                int index = listView1.SelectedIndices[0];
                enumwindows.sendMessage(index, "next");
                showTargetImage();
               
            }
        }
        private void addThreshold(object sender, EventArgs e)
        {
            binaryThreshold = binaryThreshold + 10;
            updateRatio();
        }
        private void decreaseThreshold(object sender, EventArgs e)
        {
            binaryThreshold = binaryThreshold - 10;
            updateRatio();
        }

        public int binaryThreshold = 170;
        public double printBitmapRatio = 1.0;


        private void addPrintRatio(object sender, EventArgs e)
        {
            printBitmapRatio = printBitmapRatio + 0.01;
            updateRatio();
        }

        private void decreasePrintRatio(object sender, EventArgs e)
        {
            printBitmapRatio = printBitmapRatio - 0.01;
            updateRatio();
        }
        private void toggleVisibility(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c!=pictureBox1&&c!=panel1)
                {
                    c.Visible = !c.Visible;
                }
                else
                {
                    toggleVisibility(c);
                }
            }
        }
        /// <summary>
        /// change Visibility for auxillary GUI
        /// </summary>
        private void toggleVisibility()
        {
            toggleVisibility(this);
            if (listView1.Visible)
                updateWindowsList();
            
        }
        private void toggleOtherControls()
        {
            toggleVisibility();   
            updateUI();

        }
        private void pictureBoxClick(object sender, EventArgs e)
        {
            if(!IsCursor_show)
            Cursor.Show();
            IsCursor_show = true;
            timer1.Enabled = true;
            toggleOtherControls();
           

        }
        private void dispatchKeyMsg(KeyEventArgs e)
        {
            if (selectedIndex >= 0 && selectedIndex < enumwindows.getSize())
            {

                enumwindows.sendKeyPress(selectedIndex, e);
                label4.Text = String.Format("Key:{0}", e.KeyCode.ToString());
                showTargetImage();

            }

        }
        private void mainWindownKeyDown(object sender, KeyEventArgs e)
        {
            dispatchKeyMsg(e);
        }

        private void largeAddRatio(object sender, EventArgs e)
        {
            printBitmapRatio = printBitmapRatio + 0.1;
            updateRatio();
        }

        private void largeDecreaseRatio(object sender, EventArgs e)
        {
            printBitmapRatio = printBitmapRatio - 0.1;
            updateRatio();
        }

        private void listViewKeyDown(object sender, KeyEventArgs e)
        {
            dispatchKeyMsg(e);
        }

        private void listViewClick(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                selectedIndex = listView1.SelectedIndices[0];
                this.Text = String.Format("!{0}", listView1.Items[selectedIndex].Text);
            }
            showTargetImage();
            updateLabels();

        }

        private int dx = 2;
        private int dx_max = 6;
        private void decreaseBoldness(object sender, EventArgs e)
        {
            if (dx >0)
                dx = dx - 1;
            updateUI();
        }

        private void increaseBoldness(object sender, EventArgs e)
        {
            if (dx < dx_max)
                dx = dx + 1;
            updateUI();
        }
        bool IsCursor_show = true;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if(IsCursor_show)
            Cursor.Hide();
            IsCursor_show = false;
            timer1.Enabled = false;
        }

        private void setFullScreen(object sender, EventArgs e)
        {
            if (selectedIndex >= 0 && selectedIndex < enumwindows.getSize())
            {

                enumwindows.setFullscreen(selectedIndex,screen_dx,screen_dy,screen_pos_dx,screen_pos_dy);
                showTargetImage();
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            for (int i=0;i<enumwindows.getSize();i++)
            {
                enumwindows.setFullscreen(i,screen_dx,screen_dy);
            }
        }

        int screen_dx = 0;
        int screen_dy = 0;
        int screen_pos_dx = 0;
        int screen_pos_dy = 0;

        private void addScreenDx(object sender, EventArgs e)
        {
            if (screen_dx <100)
                screen_dx = screen_dx + 1;
            updateLabels();
        }

        private void decreaseScreenDx(object sender, EventArgs e)
        {
            if (screen_dx > 0)
                screen_dx = screen_dx - 1;
            updateLabels();
        }

        private void addScreenDy(object sender, EventArgs e)
        {
            if (screen_dy <100)
                screen_dy = screen_dy + 1;
            updateLabels();
        }

        private void decreaseScreenDy(object sender, EventArgs e)
        {
            if (screen_dy> 0)
                screen_dy= screen_dy - 1;
            updateLabels();
        }

        private void clearWindowsList(object sender, EventArgs e)
        {
            updateWindowsList(true);
        }

        private void button20_Click(object sender, EventArgs e)
        {
            if (screen_pos_dx > 0)
                screen_pos_dx = screen_pos_dx - 1;
            updateLabels();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (screen_pos_dx < 100)
                screen_pos_dx = screen_pos_dx + 1;
            updateLabels();
        }

        private void button14_Click_1(object sender, EventArgs e)
        {
              if (screen_pos_dy> 0)
                screen_pos_dy= screen_pos_dy - 1;
            updateLabels();
        
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (screen_pos_dy < 100)
                screen_pos_dy = screen_pos_dy + 1;
            updateLabels();
        }
    }
}
