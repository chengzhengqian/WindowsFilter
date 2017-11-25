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
            set_full_screen();

        }
        public void set_full_screen()
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            update_windows();
            update_UI();

        }           
        private void update_windows(Boolean is_clean=false)
        {
            enumwindows.getProcess(is_clean);
            List<String> s = enumwindows.getContents();
            listView1.Items.Clear();
            foreach (string s_ in s)
            {
                listView1.Items.Add(s_);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            update_windows();
        }        
        private void update_image()
        {
            if (selected_index>=0 && selected_index<enumwindows.getSize())
            {
               
                Bitmap win_bitmap = WindowsBitmap.PrintWindow(enumwindows.getHandle(selected_index));
                win_bitmap = WindowsBitmap.unsafe_expand(win_bitmap,BinaryThreshold,false,dx,dx);
                pictureBox1.Image = win_bitmap;
            }
        }
        private int selected_index = -1;


        private void update_parameters()
        {


            WindowsBitmap.ratio_x = WindowsBitmap.ratio_x0 * print_bitmap_ration;
            WindowsBitmap.ratio_y = WindowsBitmap.ratio_y0 * print_bitmap_ration;
            update_UI();
        }
        private void update_labels()
        {
            if (pictureBox1.Image != null)
                label1.Text = String.Format("({0},{1})->({2},{3})", this.Width, this.Height, pictureBox1.Image.Width, pictureBox1.Image.Height);
            else
                label1.Text = String.Format("({0},{1}) ", this.Width, this.Height);
            label2.Text = String.Format("{0}", BinaryThreshold);
            label3.Text = String.Format("{0}", print_bitmap_ration);
            label5.Text = String.Format("Index:{0}", selected_index);
            label6.Text = String.Format("{0}", dx);

            label8.Text = String.Format("{0}", screen_dx);
            label9.Text = String.Format("{0}", screen_dy);
        }
        private void update_UI()
        {

            update_image();
            update_labels();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                int index = listView1.SelectedIndices[0];
                enumwindows.sendMessage(index, "prior");
                update_image();
                //update_image_fully();
                //enumwindows.setToFront(Process.GetCurrentProcess().MainWindowHandle);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                int index = listView1.SelectedIndices[0];
                enumwindows.sendMessage(index, "next");
                update_image();
               
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            BinaryThreshold = BinaryThreshold + 10;
            update_parameters();
        }
        private void button6_Click(object sender, EventArgs e)
        {
            BinaryThreshold = BinaryThreshold - 10;
            update_parameters();
        }

        public int BinaryThreshold = 170;
        public double print_bitmap_ration = 1.0;


        private void button8_Click(object sender, EventArgs e)
        {
            print_bitmap_ration = print_bitmap_ration + 0.01;
            update_parameters();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            print_bitmap_ration = print_bitmap_ration - 0.01;
            update_parameters();
        }
        private void toggle_visibility(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c!=pictureBox1&&c!=panel1)
                {
                    c.Visible = !c.Visible;
                }
                else
                {
                    toggle_visibility(c);
                }
            }
        }
        private void toggle_visibility()
        {
            toggle_visibility(this);
            if (listView1.Visible)
                update_windows();
            
        }
        private void toggle_other_controls()
        {
            toggle_visibility();   
            update_UI();

        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if(!IsCursor_show)
            Cursor.Show();
            IsCursor_show = true;
            timer1.Enabled = true;
            toggle_other_controls();
           

        }
        private void handle_keymap(KeyEventArgs e)
        {
            if (selected_index >= 0 && selected_index < enumwindows.getSize())
            {

                enumwindows.sendKeyPress(selected_index, e);
                label4.Text = String.Format("Key:{0}", e.KeyCode.ToString());
                update_image();

            }

        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            handle_keymap(e);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            print_bitmap_ration = print_bitmap_ration + 0.1;
            update_parameters();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            print_bitmap_ration = print_bitmap_ration - 0.1;
            update_parameters();
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            handle_keymap(e);
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                selected_index = listView1.SelectedIndices[0];
                this.Text = String.Format("!{0}", listView1.Items[selected_index].Text);
            }
            update_image();
            update_labels();

        }

        private int dx = 2;
        private int dx_max = 6;
        private void button11_Click(object sender, EventArgs e)
        {
            if (dx >0)
                dx = dx - 1;
            update_UI();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (dx < dx_max)
                dx = dx + 1;
            update_UI();
        }
        bool IsCursor_show = true;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if(IsCursor_show)
            Cursor.Hide();
            IsCursor_show = false;
            timer1.Enabled = false;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (selected_index >= 0 && selected_index < enumwindows.getSize())
            {

                enumwindows.setFullscreen(selected_index,screen_dx,screen_dy);
                update_image();
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
       

        private void button16_Click(object sender, EventArgs e)
        {
            if (screen_dx <100)
                screen_dx = screen_dx + 1;
            update_labels();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            if (screen_dx > 0)
                screen_dx = screen_dx - 1;
            update_labels();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (screen_dy <100)
                screen_dy = screen_dy + 1;
            update_labels();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            if (screen_dy> 0)
                screen_dy= screen_dy - 1;
            update_labels();
        }

        private void button19_Click(object sender, EventArgs e)
        {
            update_windows(true);
        }
    }
}
