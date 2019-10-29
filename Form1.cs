using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace NumberRiddle
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region 设定

        public int m = 3, temp = 1, index, number = 0;     //默认难度为“简单”，当点击“中等”或者“困难”之后，m值改为4或者5
        public Image[,] place = new Image[10, 10];         //二维数组，用来存放[i,j]位置放置的是哪张图片
        public Color[,] place1;
        public int[,] num = new int[9, 9];                 //num[i,j]=1,该位置有字母；num[i,j]=0，该位置没有字母
        public int[,] calculate = new int[10, 10];         //用于计算及判断各个结果是否正确
        public string[,] calculate1 = new string[10, 10];
        public Image[] eg1 = { Resource1.bing, Resource1.che, Resource1.ma, Resource1.pao, Resource1.zu };//例题1图片
        public Image[] eg2 = { Resource1.A, Resource1.B, Resource1.C, Resource1.D, Resource1.E, Resource1.F, Resource1.n7 };//
        public Image[] character = { Resource1.A, Resource1.B, Resource1.C, Resource1.D, Resource1.E, Resource1.F, Resource1.G, Resource1.H, Resource1.I, Resource1.J, Resource1.K, Resource1.L, Resource1.M, Resource1.N };//所有可能的图片选择，将其存入character数组
        public Image[] fuhao = { Resource1.jiahao, Resource1.jianhao, Resource1.chenghao, Resource1.chuhao, Resource1.kuohao };//将符号存入fuhao数组，便于界面显示
        public Char[] letter = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', };
        public bool[] s = new bool[14];         //用户答案部分，与letter[]对应，若相应的字母出现在题目中，则布尔值为true；否则为false
        public int[] NumFlag = new int[10];		//NumFlag[j]=1表示该数字已经分配给字母，NumFlag[j]=0表示未分配给字母
        public int[] NumTaken = new int[11];	//记录字母的数字分配方案（元素0不用，题目最多用到10种字母，对应0~9的数字）
        public int change_num;                  //用于记录每次改变的随机数是什么
        public Image backgroud = Resource1.background;

        #endregion


        #region 出题区域方格设定

        /// <summary>
        /// 在出题区域画出方格线函数
        /// </summary>
        /// <param name="n">n*n的方格</param>
        private void drawLine(int n)
        {
            Graphics g = panel1.CreateGraphics();
            for (int i = 0; i < n; i++)
            {
                g.DrawLine(new Pen(Color.AliceBlue), 0, panel1.Height / n * i, panel1.Width, panel1.Height / n * i);
                g.DrawLine(new Pen(Color.AliceBlue), panel1.Width / n * i, 0, panel1.Width / n * i, panel1.Height);
            }
        }

        #endregion


        #region 设定图片及符号显示

        //所有图片放入程序的资源文件

        /// <summary>
        /// 出题区域载入字母图片
        /// </summary>
        /// <param name="i">行数</param>
        /// <param name="j">列数</param>
        /// <param name="character">要载入的字母图片</param>
        /// <param name="dx">指定字母图片比方格小的像素个数</param>
        private void write_character(int i, int j, Image character, int dx)
        {
            Graphics g = panel1.CreateGraphics();
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;     //高质量显示图片
            int x = panel1.Width / 5, y = panel1.Height / 5;    //x，y分别为单个方块宽和高
            string temp = character.ToString().Substring(7).Replace("]", "");
            g.DrawImage(character, x * j + dx, y * i + dx, x - dx - dx, y - dx - dx);   //将图片画在出题区域
            place[i, j] = character;//更新place数组
            num[i, j] = 1;//更新num数组，使得有图片的位置为1；
        }
        private void write_character(int i, int j, Image character)//重载上面的函数，将dx默认为4
        {
            write_character(i, j, character, 4);
        }


        /// <summary>
        /// 在出题区域载入运算符
        /// </summary>
        /// <param name="i">行数</param>
        /// <param name="j">列数</param>
        /// <param name="fuhao">要载入的运算符</param>
        ///  <param name="dx">指定字母图片比方格小的像素个数</param>
        private void write_fuhao(int i, int j, Image fuhao, int dx)
        {
            Graphics g = panel1.CreateGraphics();
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;     //高质量显示图片
            int x = panel1.Width / 5, y = panel1.Height / 5;    //x，y分别为单个方块宽和高
            string temp = character.ToString().Substring(7).Replace("]", "");   //color的颜色值转换为字符串后形如：color[Red]，本句代码执行后的结果为Red
            g.DrawImage(fuhao, x * j + dx, y * i + dx, x - dx - dx, y - dx - dx);   //将图片画在出题区
            place[i, j] = fuhao;//更新place数组
                                //符号位置num仍为0（num[i, j] = 0），不需要更改；
        }
        private void write_fuhao(int i, int j, Image fuhao)//重载上面的函数，将dx默认为4
        {
            write_fuhao(i, j, fuhao, 4);
        }


        /// <summary>
        /// 除式专用载入图片、符号
        /// </summary>
        /// <param name="i">行数</param>
        /// <param name="j">列数</param>
        /// <param name="character">要载入的字母图片</param>
        /// <param name="fuhao">要载入的运算符</param>
        /// <param name="dx">指定字母图片比方格小的像素个数</param>
        private void write_character1(int i, int j, Image character, int dx)
        {
            Graphics g = panel1.CreateGraphics();
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;     //高质量显示图片
            int x = panel1.Width / 6, y = panel1.Height / 6;    //x，y分别为单个方块宽和高
            string temp = character.ToString().Substring(7).Replace("]", "");   //color的颜色值转换为字符串后形如：color[Red]，本句代码执行后的结果为Red
            g.DrawImage(character, x * j + dx, y * i + dx, x - dx - dx, y - dx - dx);   //将图片画在出题区
            place[i, j] = character;//同时更新place数组
            num[i, j] = 1;//更新num数组，数字位置改为1；
        }

        private void write_character1(int i, int j, Image character)//重载上面的函数，将dx默认为4
        {
            write_character1(i, j, character, 4);
        }

        private void write_fuhao1(int i, int j, Image fuhao, int dx)
        {
            Graphics g = panel1.CreateGraphics();
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;     //高质量显示图片
            int x = panel1.Width / 6, y = panel1.Height / 6;    //x，y分别为单个方块宽和高
            string temp = character.ToString().Substring(7).Replace("]", "");   //color的颜色值转换为字符串后形如：color[Red]，本句代码执行后的结果为Red
            g.DrawImage(fuhao, x * j + dx, y * i + dx, x - dx - dx, y - dx - dx);   //将图片画在游戏区
            place[i, j] = fuhao;//同时更新place数组
                                //符号位置num仍为0（num[i, j] = 0），不需要更改；
        }
        private void write_fuhao1(int i, int j, Image fuhao)//重载上面的函数，将dx默认为4
        {
            write_fuhao1(i, j, fuhao, 1);
        }

        /// <summary>
        /// 清空，用背景色填充特定方格
        /// </summary>
        /// <param name="i">行</param>
        /// <param name="j">列</param>
        private void clear_character(int i, int j)
        {
            Graphics g = panel1.CreateGraphics();
            int x = panel1.Width / m, y = panel1.Height / m;
            g.FillRectangle(new SolidBrush(panel1.BackColor), x * j + 2, y * i + 2, x - 4, y - 4);
            place1[i, j] = panel1.BackColor;
        }

        #endregion


        #region 选择模式
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            label2.Text = "简单";
            m = 3;
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            label2.Text = "中等";
            m = 4;
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            label2.Text = "困难";
            m = 5;
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            label3.Text = "加法";
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            label3.Text = "减法";
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            label3.Text = "乘法";
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            label3.Text = "除法";
        }

        private void 例题1ToolStripMenuItem_Click(object sender, EventArgs e) //选择例题1直接出题
        {
            button2.Enabled = true;     //选择例题1后，提交按钮可用
            button3.Enabled = true;     //选择例题1后，查看答案按钮可用
            button4.Enabled = true;     //选择例题1后，我要作答按钮可用
            textBox1.Text = "";         //清空文本框内容
            textBox2.Text = "";         //清空文本框内容
            label3.Text = "例题1";
            label2.Text = "困难";
            label2.Visible = false;     //选择例题时，不显示难度
            m = 5;
            this.dataGridView1.Rows.Clear();    //首先清空datagridview中的值
            temp = 1;
            index = 0;

            for (int i1 = 0; i1 < 5; i1++)
                for (int j1 = 0; j1 < 5; j1++)
                {
                    calculate[i1, j1] = 0;
                    num[i1, j1] = 0;
                    place[i1, j1] = null;
                }
            string[,] calculate1 = new string[10, 10];
            Graphics g = panel1.CreateGraphics();
            g.Clear(panel1.BackColor);  //用背景色清空
            drawLine(5);                //出题区划为5x5的方格
            write_fuhao(2, 0, fuhao[0]);        //相应地方写上运算符号
            g.DrawLine(new Pen(Color.Black), 0, panel1.Height / 5 * 3, panel1.Width, panel1.Height / 5 * 3);//算式求解线画成黑色
            //依次载入各个位置的图片
            write_character(1, 1, eg1[0]);
            write_character(1, 2, eg1[3]);
            write_character(1, 3, eg1[2]);
            write_character(1, 4, eg1[4]);

            write_character(2, 1, eg1[0]);
            write_character(2, 2, eg1[3]);
            write_character(2, 3, eg1[1]);
            write_character(2, 4, eg1[4]);

            write_character(3, 0, eg1[1]);
            write_character(3, 1, eg1[4]);
            write_character(3, 2, eg1[2]);
            write_character(3, 3, eg1[0]);
            write_character(3, 4, eg1[4]);
        }

        private void 例题2ToolStripMenuItem_Click(object sender, EventArgs e)  //选择例题2直接出题
        {
            button2.Enabled = true;     //选择例题2后，提交按钮可用
            button3.Enabled = true;     //选择例题2后，查看答案按钮可用
            button4.Enabled = true;     //选择例题2后，我要作答按钮可用
            textBox1.Text = "";         //清空文本框内容
            textBox2.Text = "";         //清空文本框内容
            label3.Text = "例题2";
            label2.Text = "困难";
            label2.Visible = false;     //选择例题时，不显示难度
            m = 5;
            this.dataGridView1.Rows.Clear();    //首先清空datagridview中的值
            temp = 1;
            index = 0;

            for (int i1 = 0; i1 < 5; i1++)
                for (int j1 = 0; j1 < 5; j1++)
                {
                    calculate[i1, j1] = 0;
                    num[i1, j1] = 0;
                    place[i1, j1] = null;
                }
            string[,] calculate1 = new string[10, 10];
            Graphics g = panel1.CreateGraphics();
            g.Clear(panel1.BackColor);  //用背景色清空
            drawLine(5);                //出题区划为5x5的方格
            write_fuhao(1, 0, fuhao[2]);        //相应地方写上运算符号（多位数乘法符号写在第1行）
            g.DrawLine(new Pen(Color.Black), 0, panel1.Height / 5 * 2, panel1.Width, panel1.Height / 5 * 2);//算式求解线画为黑色
            g.DrawLine(new Pen(Color.Black), 0, panel1.Height / 5 * 4, panel1.Width, panel1.Height / 5 * 4);//算式求解线画为黑色

            //依次载入各个位置的图片
            write_character(0, 2, eg2[0]);
            write_character(0, 3, eg2[1]);
            write_character(0, 4, eg2[2]);

            write_character(1, 3, eg2[3]);
            write_character(1, 4, eg2[2]);

            write_character(2, 1, eg2[3]);
            write_character(2, 2, eg2[4]);
            write_character(2, 3, eg2[0]);
            write_character(2, 4, eg2[2]);

            write_character(3, 1, eg2[6]);
            write_character(3, 2, eg2[4]);
            write_character(3, 3, eg2[3]);

            write_character(4, 1, eg2[5]);
            write_character(4, 2, eg2[3]);
            write_character(4, 3, eg2[1]);
            write_character(4, 4, eg2[2]);
        }



        #endregion


        private void Form1_Load(object sender, EventArgs e)     //游戏初始化
        {
            this.BackgroundImage = backgroud;//上载背景图片
            button2.Enabled = false;    //提交按钮不可用
            button3.Enabled = false;    //查看答案按钮不可用
            button4.Enabled = false;    //我要作答按钮不可用
            label2.Text = "简单";       //默认选择
            label3.Text = "加法";       //默认选择
            panel1.BackColor = Color.AliceBlue; //首次运行，给出题区填充AliceBlue背景色
            Graphics g = panel1.CreateGraphics();
            g.Clear(panel1.BackColor);  //用背景色清空
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            drawLine(5);
        }


        #region 出题

        private void button1_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;     //点击出题按钮后，提交按钮可用
            button3.Enabled = true;     //点击出题按钮后，查看答案按钮可用
            button4.Enabled = true;     //点击出题按钮后，我要作答按钮可用
            textBox1.Text = "";         //清空文本框内容
            textBox2.Text = "";         //清空文本框内容
            if (label3.Text == "例题1" || label3.Text == "例题2")
            {
                label2.Visible = false;
            }
            else
            {
                label2.Visible = true;
            }

            switch (label3.Text)
            {
                case "加法":
                    ADD();
                    break;
                case "减法":
                    SUB();
                    break;
                case "乘法":
                    MUL();
                    break;
                case "除法":
                    DIV();
                    break;
            }
            
        }


        #region 加法题

        /// <summary>
        /// 出加法题
        /// </summary>
        private void ADD()
        {
            this.dataGridView1.Rows.Clear();    //首先清空datagridview中的值
            temp = 1;
            index = 0;
            for (int i1 = 0; i1 < 5; i1++)
                for (int j1 = 0; j1 < 5; j1++)
                {
                    calculate[i1, j1] = 0;
                    num[i1, j1] = 0;
                    place[i1, j1] = null;
                }
            string[,] calculate1 = new string[10, 10];
            Graphics g = panel1.CreateGraphics();
            g.Clear(panel1.BackColor);  //用背景色清空
            drawLine(5);                //出题区划为5x5的方格
            write_fuhao(2, 0, fuhao[0]);        //  相应地方写上运算符号
            g.DrawLine(new Pen(Color.Black), 0, panel1.Height / 5 * 3, panel1.Width, panel1.Height / 5 * 3);//算式求解线画成黑色
            Random ran = new Random();
            if (m == 5)
            {
                int x1 = ran.Next(1000, 9999);//产生一个4位数的随机数，作为第一个加数
                int x2 = ran.Next(1000, 9999);//产生第二个4位数随机数，作为第二个加数
                int change = ran.Next(0, 4);
                int x3 = x1 + x2;//定义第三个数，为以上两个加数的和
                //依次载入各行数字的各个位置的字母图片，包括个位、十位、百位、千位
                //character[0]对应“A”character[1]对应“B”……character[13]对应“N”，见设定处定义
                write_character(1, 4, character[x1 % 10 + change]);
                write_character(1, 3, character[x1 % 100 / 10 + change]);
                write_character(1, 2, character[x1 % 1000 / 100 + change]);
                write_character(1, 1, character[x1 / 1000 + change]);

                write_character(2, 4, character[x2 % 10 + change]);
                write_character(2, 3, character[x2 % 100 / 10 + change]);
                write_character(2, 2, character[x2 % 1000 / 100 + change]);
                write_character(2, 1, character[x2 / 1000 + change]);

                write_character(3, 4, character[x3 % 10 + change]);
                write_character(3, 3, character[x3 % 100 / 10 + change]);
                write_character(3, 2, character[x3 % 1000 / 100 + change]);
                write_character(3, 1, character[x3 % 10000 / 1000 + change]);
                if (x3 > 9999)
                    write_character(3, 0, character[x3 / 10000 + change]);
            }
            if (m == 4)
            {
                int x1 = ran.Next(100, 999);//产生一个3位数的随机数，作为第一个加数
                int x2 = ran.Next(100, 999);//产生第二个3位数随机数，作为第二个加数
                int change = ran.Next(0, 4);
                int x3 = x1 + x2;//定义第三个数，为以上两个加数的和
                //依次载入各行数字的各个位置的字母图片，包括个位、十位、百位
                //character[0]对应“A”character[1]对应“B”……character[13]对应“N”，见设定处定义
                write_character(1, 4, character[x1 % 10 + change]);
                write_character(1, 3, character[x1 % 100 / 10 + change]);
                write_character(1, 2, character[x1 % 1000 / 100 + change]);

                write_character(2, 4, character[x2 % 10 + change]);
                write_character(2, 3, character[x2 % 100 / 10 + change]);
                write_character(2, 2, character[x2 % 1000 / 100 + change]);

                write_character(3, 4, character[x3 % 10 + change]);
                write_character(3, 3, character[x3 % 100 / 10 + change]);
                write_character(3, 2, character[x3 % 1000 / 100 + change]);

                if (x3 > 999)
                    write_character(3, 1, character[x3 / 1000 + change]);
            }
            if (m == 3)
            {
                int x1 = ran.Next(10, 99);//产生一个2位数的随机数，作为第一个加数
                int x2 = ran.Next(10, 99);//产生第二个2位数随机数，作为第二个加数
                int change = ran.Next(0, 4);
                int x3 = x1 + x2;//定义第三个数，为以上两个加数的和
                //依次载入各行数字的各个位置的字母图片，包括个位、十位
                //character[0]对应“A”character[1]对应“B”……character[13]对应“N”，见设定处定义
                write_character(1, 4, character[x1 % 10 + change]);
                write_character(1, 3, character[(x1 % 100) / 10 + change]); ;

                write_character(2, 4, character[x2 % 10 + change]);
                write_character(2, 3, character[(x2 % 100) / 10 + change]);

                write_character(3, 4, character[x3 % 10 + change]);
                write_character(3, 3, character[(x3 % 100) / 10 + change]);

                if (x3 > 99)
                { write_character(3, 2, character[x3 / 100 + change]); }

            }


        }
        #endregion

        #region 减法题

        /// <summary>
        /// 出减法题
        /// </summary>
        private void SUB()
        {
            this.dataGridView1.Rows.Clear();    //首先清空datagridview中的值
            temp = 1;
            index = 0;
            for (int i1 = 0; i1 < 5; i1++)
                for (int j1 = 0; j1 < 5; j1++)
                {
                    calculate[i1, j1] = 0;
                    num[i1, j1] = 0;
                    place[i1, j1] = null;
                }
            string[,] calculate1 = new string[10, 10];
            Graphics g = panel1.CreateGraphics();
            g.Clear(panel1.BackColor);  //用背景色清空
            drawLine(5);                //出题区划为5x5的方格
            write_fuhao(2, 0, fuhao[1]);       //  相应地方写上运算符号
            g.DrawLine(new Pen(Color.Black), 0, panel1.Height / 5 * 3, panel1.Width, panel1.Height / 5 * 3);//算式求解线画为黑色
            Random ran = new Random();
            if (m == 5)
            {
                int x1 = ran.Next(1000, 9999);//产生一个4位数的随机数，作为第一个减数
                int x2 = ran.Next(1000, 9999);//产生第二个4位数随机数，作为第二个减数
                int change = ran.Next(0, 4);
                //调换使得x1>x2
                if (x1 < x2)
                {
                    int a = x1;
                    x1 = x2;
                    x2 = a;
                }
                int x3 = x1 - x2;               //定义第三个数，为以上两个数的差
                //character[0]对应“A”character[1]对应“B”……character[13]对应“N”，见设定处定义
                write_character(1, 4, character[x1 % 10 + change]);
                write_character(1, 3, character[x1 % 100 / 10 + change]);
                write_character(1, 2, character[x1 % 1000 / 100 + change]);
                write_character(1, 1, character[x1 / 1000 + change]);

                write_character(2, 4, character[x2 % 10 + change]);
                write_character(2, 3, character[x2 % 100 / 10 + change]);
                write_character(2, 2, character[x2 % 1000 / 100 + change]);
                write_character(2, 1, character[x2 / 1000 + change]);

                write_character(3, 4, character[x3 % 10 + change]);
                if (x3 > 9)
                    write_character(3, 3, character[x3 % 100 / 10 + change]);
                if (x3 > 99)
                    write_character(3, 2, character[x3 % 1000 / 100 + change]);
                if (x3 > 999)
                    write_character(3, 1, character[x3 % 10000 / 1000 + change]);
            }
            if (m == 4)
            {
                int x1 = ran.Next(100, 999);//产生一个3位数的随机数，作为第一个减数
                int x2 = ran.Next(100, 999);//产生第二个3位数随机数，作为第二个减数
                int change = ran.Next(0, 4);
                //调换使得x1>x2
                if (x1 < x2)
                {
                    int a = x1;
                    x1 = x2;
                    x2 = a;
                }
                int x3 = x1 - x2;               //定义第三个数，为以上两个数的差
                //character[0]对应“A”character[1]对应“B”……character[13]对应“N”，见设定处定义
                write_character(1, 4, character[x1 % 10 + change]);
                write_character(1, 3, character[x1 % 100 / 10 + change]);
                write_character(1, 2, character[x1 % 1000 / 100 + change]);

                write_character(2, 4, character[x2 % 10 + change]);
                write_character(2, 3, character[x2 % 100 / 10 + change]);
                write_character(2, 2, character[x2 % 1000 / 100 + change]);

                write_character(3, 4, character[x3 % 10 + change]);
                if (x3 > 9)
                    write_character(3, 3, character[x3 % 100 / 10 + change]);
                if (x3 > 99)
                    write_character(3, 2, character[x3 % 1000 / 100 + change]);
            }
            if (m == 3)
            {
                int x1 = ran.Next(10, 99);//产生一个2位数的随机数，作为第一个减数
                int x2 = ran.Next(10, 99);//产生第二个2位数随机数，作为第二个减数
                int change = ran.Next(0, 4);
                //调换使得x1>x2
                if (x1 < x2)
                {
                    int a = x1;
                    x1 = x2;
                    x2 = a;
                }
                int x3 = x1 - x2;               //定义第三个数，为以上两个数的差
                //character[0]对应“A”character[1]对应“B”……character[13]对应“N”，见设定处定义
                write_character(1, 4, character[x1 % 10 + change]);
                write_character(1, 3, character[x1 % 100 / 10 + change]);

                write_character(2, 4, character[x2 % 10 + change]);
                write_character(2, 3, character[x2 % 100 / 10 + change]);

                write_character(3, 4, character[x3 % 10 + change]);
                if (x3 > 9)
                    write_character(3, 3, character[x3 % 100 / 10 + change]);
                if (x3 > 99)
                    write_character(3, 2, character[x3 % 1000 / 100 + change]);
            }
        }

        #endregion

        #region 乘法题

        /// <summary>
        /// 出乘法题
        /// </summary>
        private void MUL()
        {
            this.dataGridView1.Rows.Clear();    //首先清空datagridview中的值
            temp = 1;
            index = 0;
            for (int i1 = 0; i1 < 6; i1++)
                for (int j1 = 0; j1 < 6; j1++)
                {
                    calculate[i1, j1] = 0;
                    num[i1, j1] = 0;
                    place[i1, j1] = null;
                }
            string[,] calculate1 = new string[10, 10];
            Graphics g = panel1.CreateGraphics();
            g.Clear(panel1.BackColor);  //用背景色清空
            drawLine(5);                //出题区划为5x5的方格
            if (m == 4 || m == 5)
            {
                g.DrawLine(new Pen(Color.Black), 0, panel1.Height / 5 * 2, panel1.Width, panel1.Height / 5 * 2);//算式求解线画为黑色
                g.DrawLine(new Pen(Color.Black), 0, panel1.Height / 5 * 4, panel1.Width, panel1.Height / 5 * 4);//算式求解线画为黑色
                write_fuhao(1, 0, fuhao[2]);        //多位数乘法符号写在第1行
            }
            if (m == 3)
            {
                g.DrawLine(new Pen(Color.Black), 0, panel1.Height / 5 * 3, panel1.Width, panel1.Height / 5 * 3);//算式求解线画为黑色
                write_fuhao(2, 0, fuhao[2]);        //两位数乘以一位数写在第2行
            }
            Random ran = new Random();
            if (m == 5)
            {
                int x1 = ran.Next(100, 999);    //产生一个3位数的随机数，作为第一个乘数
                int x2 = ran.Next(10, 99);      //产生第二个2位数随机数，作为第二个乘数
                int change = ran.Next(0, 4);
                int t1 = x1 * (x2 % 10);        //定义t1，为x1乘以x2的个位的结果
                int t2 = x1 * (x2 % 100 / 10);  //定义t2，为x1乘以x2的十位的结果
                int x3 = x1 * x2;               //定义x3，为x1与x2的乘积
                //character[0]对应“A”character[1]对应“B”……character[13]对应“N”，见设定处定义
                write_character(0, 4, character[x1 % 10 + change]);
                write_character(0, 3, character[x1 % 100 / 10 + change]);
                write_character(0, 2, character[x1 % 1000 / 100 + change]);

                write_character(1, 4, character[x2 % 10 + change]);
                write_character(1, 3, character[x2 % 100 / 10 + change]);

                write_character(2, 4, character[t1 % 10 + change]);
                write_character(2, 3, character[t1 % 100 / 10 + change]);
                write_character(2, 2, character[t1 % 1000 / 100 + change]);
                if (t1 > 999)
                    write_character(2, 1, character[t1 % 10000 / 1000 + change]);

                write_character(3, 3, character[t2 % 10 + change]);
                write_character(3, 2, character[t2 % 100 / 10 + change]);
                write_character(3, 1, character[t2 % 1000 / 100 + change]);
                if (t2 > 999)
                    write_character(3, 0, character[t2 % 10000 / 1000 + change]);

                write_character(4, 4, character[x3 % 10 + change]);
                write_character(4, 3, character[x3 % 100 / 10 + change]);
                write_character(4, 2, character[x3 % 1000 / 100 + change]);
                write_character(4, 1, character[x3 % 10000 / 1000 + change]);
                if (x3 > 9999)
                    write_character(4, 0, character[x3 / 10000 + change]);
                change_num = change;
            }
            if (m == 4)
            {
                int x1 = ran.Next(10, 99);      //产生一个2位数的随机数，作为第一个乘数
                int x2 = ran.Next(10, 99);      //产生第二个2位数随机数，作为第二个乘数
                int change = ran.Next(0, 4);
                int t1 = x1 * (x2 % 10);        //定义t1，为x1乘以x2的个位的结果
                int t2 = x1 * (x2 % 100 / 10);  //定义t2，为x1乘以x2的十位的结果
                int x3 = x1 * x2;               //定义x3，为x1与x2的乘积
                //character[0]对应“A”character[1]对应“B”……character[13]对应“N”，见设定处定义
                write_character(0, 4, character[x1 % 10 + change]);
                write_character(0, 3, character[x1 % 100 / 10 + change]);

                write_character(1, 4, character[x2 % 10 + change]);
                write_character(1, 3, character[x2 % 100 / 10 + change]);

                write_character(2, 4, character[t1 % 10 + change]);
                write_character(2, 3, character[t1 % 100 / 10 + change]);
                if (t1 > 99)
                    write_character(2, 2, character[t1 % 1000 / 100 + change]);

                write_character(3, 3, character[t2 % 10 + change]);
                write_character(3, 2, character[t2 % 100 / 10 + change]);
                if (t2 > 99)
                    write_character(3, 1, character[t2 % 1000 / 100 + change]);

                write_character(4, 4, character[x3 % 10 + change]);
                write_character(4, 3, character[x3 % 100 / 10 + change]);
                write_character(4, 2, character[x3 % 1000 / 100 + change]);
                if (x3 > 999)
                    write_character(4, 1, character[x3 % 10000 / 1000 + change]);
            }
            if (m == 3)
            {
                int x1 = ran.Next(10, 99);  //产生一个2位数的随机数，作为第一个乘数
                int x2 = ran.Next(1, 9);    //产生第二个1位数随机数，作为第二个乘数
                int change = ran.Next(0, 4); 
                int x3 = x1 * x2;           //定义x3，为x1与x2的乘积
                //character[0]对应“A”character[1]对应“B”……character[13]对应“N”，见设定处定义
                write_character(1, 4, character[x1 % 10 + change]);
                write_character(1, 3, character[x1 % 100 / 10 + change]);

                write_character(2, 4, character[x2 + change]);

                write_character(3, 4, character[x3 % 10 + change]);
                write_character(3, 3, character[x3 % 100 / 10 + change]);
                if (x3 > 99)
                    write_character(3, 2, character[x3 % 1000 / 100 + change]);
            }
        }

        #endregion

        #region 除法题

        /// <summary>
        /// 出除法题
        /// </summary>
        private void DIV()
        {
            this.dataGridView1.Rows.Clear();    //首先清空datagridview中的值
            temp = 1;
            index = 0;
            for (int i1 = 0; i1 < 6; i1++)
                for (int j1 = 0; j1 < 6; j1++)
                {
                    calculate[i1, j1] = 0;
                    num[i1, j1] = 0;
                    place[i1, j1] = null;
                }
            string[,] calculate1 = new string[10, 10];
            Graphics g = panel1.CreateGraphics();
            g.Clear(panel1.BackColor);//用背景色清空
            drawLine(6);
            //从三个格子之后开始画线，每个格子为50；
            g.DrawLine(new Pen(Color.Black), 150, panel1.Height / 6 * 3, panel1.Width, panel1.Height / 6 * 3);//算式求解线画为黑色
            g.DrawLine(new Pen(Color.Black), 150, panel1.Height / 6 * 1, panel1.Width, panel1.Height / 6 * 1);//算式求解线画为黑色
            write_fuhao1(1, 2, fuhao[4]);        //除式写在第1行第2列
            Random ran = new Random();

            if (m == 5)     //三位数除以两位数
            {
                int x1 = ran.Next(100, 999);    //产生一个3位数的随机数，作为被除数
                int x2 = ran.Next(10, 99);      //产生第二个2位数随机数，作为除数
                int change = ran.Next(0, 4);
                int x3 = x1 / x2;               //定义x3为x1整除x2的结果，作为最上层的商
                for (int j = 0; j < 20; j++)    //确保复杂度，维持难度为困难
                {
                    if (x3 > 9) break;
                    else
                    {
                        x1 = ran.Next(100, 999);
                        x2 = ran.Next(10, 99);
                        x3 = x1 / x2;
                    }
                }

                write_character1(1, 5, character[x1 % 10 + change]);
                write_character1(1, 4, character[x1 % 100 / 10 + change]);
                write_character1(1, 3, character[x1 % 1000 / 100 + change]);

                write_character1(1, 1, character[x2 % 10 + change]);
                write_character1(1, 0, character[x2 % 100 / 10 + change]);

                int x4, x5, x6, x7, x8, x9;

                if ((x3 > 9) && (x3 % 10 == 0))
                {
                    x4 = (x3 / 10) * x2;
                    x5 = x1 % x2;
                    write_character1(0, 5, character[x3 % 10 + change]);
                    write_character1(0, 4, character[x3 % 100 / 10 + change]);
                    write_character1(2, 4, character[x4 % 10 + change]);
                    write_character1(2, 3, character[x4 % 100 / 10 + change]);
                    write_character1(3, 5, character[x5 % 10 + change]);
                    if (x5 > 9)
                        write_character1(3, 4, character[x5 / 10 + change]);
                }

                else
                {
                    g.DrawLine(new Pen(Color.Black), 150, panel1.Height / 6 * 5, panel1.Width, panel1.Height / 6 * 5);//算式求解线画为黑色
                    x6 = (x3 / 10) * x2;
                    x7 = x1 - (x6 * 10);
                    x8 = (x3 % 10) * x2;
                    x9 = x1 % x2;
                    write_character1(0, 5, character[x3 % 10 + change]);
                    write_character1(0, 4, character[x3 % 100 / 10 + change]);
                    write_character1(2, 4, character[x6 % 10 + change]);
                    write_character1(2, 3, character[x6 % 100 / 10 + change]);
                    write_character1(3, 5, character[x7 % 10 + change]);
                    if ((x7 > 9))
                        write_character1(3, 4, character[x7 % 100 / 10 + change]);
                    if (x7 > 99)
                        write_character1(3, 3, character[x7 / 100 + change]);
                    write_character1(4, 5, character[x8 % 10 + change]);
                    write_character1(4, 4, character[x8 % 100 / 10 + change]);
                    if (x8 > 99)
                        write_character1(4, 3, character[x8 / 100 + change]);

                    write_character1(5, 5, character[x9 % 10 + change]);
                    if (x9 > 9)
                        write_character1(5, 4, character[x9 % 100 / 10 + change]);
                }
            }

            if (m == 4)     //两位数除以一位数
            {
                int x1 = ran.Next(10, 99);//产生一个2位数的随机数，作为被除数
                int x2 = ran.Next(1, 9);//产生第二个1位数随机数，作为除数
                int change = ran.Next(0, 4);
                int x3 = x1 / x2;//x3为商
                int x4, x5, x6, x7, x8, x9;
                write_character1(1, 5, character[x1 % 10 + change]);
                write_character1(1, 4, character[x1 % 100 / 10 + change]);
                write_character1(1, 1, character[x2 % 10 + change]);

                if (x3 > 9)
                {
                    g.DrawLine(new Pen(Color.Black), 150, panel1.Height / 6 * 5, panel1.Width, panel1.Height / 6 * 5);//算式求解线画为黑色
                    x4 = (x3 / 10) * x2;
                    x5 = x1 - 10 * x4;
                    x6 = x2 * (x3 % 10);
                    x7 = x1 % x2;
                    write_character1(0, 5, character[x3 % 10 + change]);
                    write_character1(0, 4, character[x3 / 10 + change]);
                    write_character1(2, 4, character[x4 + change]);

                    write_character1(3, 5, character[x5 % 10 + change]);
                    if (x5 > 9)
                        write_character1(3, 4, character[x5 / 10 + change]);
                    write_character1(4, 5, character[x6 % 10 + change]);
                    if (x6 > 9)
                        write_character1(4, 4, character[x6 / 10 + change]);
                    write_character1(5, 5, character[x7 + change]);

                }
                else
                {
                    x8 = x3 * x2;
                    x9 = x1 % x2;
                    write_character1(0, 5, character[x3 % 10 + change]);
                    write_character1(2, 5, character[x8 % 10 + change]);
                    write_character1(2, 4, character[x8 % 100 / 10 + change]);
                    write_character1(3, 5, character[x9 % 10 + change]);
                    if (x9 > 9)
                        write_character1(3, 4, character[x9 / 10 + change]);
                }

            }

            if (m == 3)     //两位数除以两位数
            {
                int x1 = ran.Next(10, 99);  //产生一个2位数的随机数，作为被除数
                int x2 = ran.Next(10, 99);  //产生第二个2位数随机数，作为除数
                int change = ran.Next(0, 4);
                if (x1 < x2)                //保证被除数大于除数
                {
                    int a2 = x1;
                    x1 = x2;
                    x2 = a2;
                }

                int x3 = x1 / x2;//定义x3为x1整除x2的结果，作为最上层的商
                int x4 = x3 * x2;//商与除数的乘积
                int x5 = x1 % x2;//余数
                write_character1(1, 5, character[x1 % 10 + change]);
                write_character1(1, 4, character[x1 % 100 / 10 + change]);
                write_character1(1, 1, character[x2 % 10 + change]);
                write_character1(1, 0, character[x2 % 100 / 10 + change]);
                write_character1(0, 5, character[x3 + change]);
                write_character1(2, 5, character[x4 % 10 + change]);
                write_character1(2, 4, character[x4 % 100 / 10 + change]);
                write_character1(3, 5, character[x5 % 10 + change]);
                if (x5 > 9)
                    write_character1(3, 4, character[x5 / 10 + change]);
            }

        }

        #endregion

        #endregion


        #region 用户答案


        #region 显示题目中出现的字母

        private void button4_Click(object sender, EventArgs e)      //点击“我要作答”按钮，文本框显示题目中出现的字母
        {

            for (int i = 0; i < 14; i++)  //布尔数组的元素置为false（避免受之前显示的影响）
            {
                s[i] = false;
            }

            if (label3.Text == "例题1")
            {
                textBox1.Text = "兵车马炮卒";
                for (int i = 0; i < 5; i++)      //例题1出现5种棋子
                {
                    s[i] = true;
                }
            }

            else if (label3.Text == "例题2")
            {
                textBox1.Text = "ABCDEF";
                for (int i = 0; i < 6; i++)  //例题2出现6种字母
                {
                    s[i] = true;
                }
            }

            else     //出题状态
            {

                char[] letterselection = new char[14]; //存放题目中出现的字母

                //character[t]字母图片；place[i,j]出题区位置放置的字母图片；letter[]A~N`的字符数组
                for (int t = 0; t < 14; t++)        //若题目中出现某字母，使布尔数组s中相应的值为true
                {
                    for (int i = 0; i < 6; i++)
                        for (int j = 0; j < 6; j++)
                        {
                            if (character[t] == place[i, j] && s[t] == false)   //比较character[t]的字母图片与出题区[i,j]位置的图片是否一致
                            {
                                s[t] = true;
                            }
                        }
                }

                int k = 0;
                for (int t = 0; t < 14; t++)
                {
                    if (s[t] == true)           //布尔值为true，该字母在题目中出现
                    {
                        letterselection[k] = letter[t];   //将题目出现的字母放入数组letterselection
                        k = k + 1;
                    }

                }

                textBox1.Text = string.Join("", letterselection); //将题目中出现的字母显示在文本框中

            }
        }

        #endregion

        #region 用户输入答案并检查正误

        private void button2_Click(object sender, EventArgs e)
        {
            int k = 0, t = 0;
            char[] answernum = new char[14];   //存放用户输入的代表字母的数值字符
            int[,] x = new int[6, 6];          // 6*6的整型矩阵，对应出题区
            int x0, x1, x2, x3, x4, x5, x6;    //xi对应x矩阵中第i行的数，除法中第1行有2个数，x1记录被除数，x6记录除数

            for (int i = 0; i < 14; i++)       //题目中出现的字母种类数（布尔数组s中true的个数）
            {
                if (s[i] == true)
                {
                    k = k + 1;
                }
            }

            string answer = textBox2.Text;      //读入用户输入的答案
            char[] tempanswer = answer.ToCharArray();   //将字符串转为字符数组

            int d = 0;
            for (int i = 0; i < answer.Length; i++)     //检查用户输入的数字是否有重复的
            {
                for (int j = i + 1; j < answer.Length; j++)
                {
                    if (tempanswer[i] == tempanswer[j])
                    {
                        d = 1;
                        break;
                    }
                }
                if (d == 1)
                {
                    break;
                }
            }

            if (textBox1.Text == "")            //用户未点击我要作答按钮，直接点击提交按钮
            {
                MessageBox.Show("请点击“我要作答”按钮，按顺序输入答案！");
                textBox2.Text = "";             //清空文本框内容
            }

            else if (textBox2.Text == "")       //用户未输入答案，点击提交按钮
            {
                MessageBox.Show("请输入答案");
            }

            else if (answer.Length != k)        //用户答案个数与题目字母个数不匹配，点击提交按钮
            {
                MessageBox.Show("答案个数有误，请检查重新输入");
            }
            else if (d == 1)                    //用户答案数字有重复
            {
                MessageBox.Show("答案错误");
            }
            else
            {
                for (int i = 0; i < 14; i++)    //将用户输入的代表字母的数值字符存入answernum数组的对应位置
                {
                    if (s[i] == true)
                    {
                        answernum[i] = tempanswer[t];
                        t = t + 1;
                    }
                }

                for (int l = 0; l < 14; l++)        //在矩阵x中写入字母位置对应的数值
                {
                    for (int i = 0; i < 6; i++)
                    {
                        for (int j = 0; j < 6; j++)
                        {
                            if (character[l] == place[i, j])
                            {
                                x[i, j] = answernum[l] - 48;
                            }
                        }
                    }
                }
            
                switch (label3.Text)
                {
                    case "例题1":
                        x1 = (answernum[0] - 48) * 1000 + (answernum[3] - 48) * 100 + (answernum[2] - 48) * 10 + (answernum[4] - 48);
                        x2 = (answernum[0] - 48) * 1000 + (answernum[3] - 48) * 100 + (answernum[1] - 48) * 10 + (answernum[4] - 48);
                        x3 = (answernum[1] - 48) * 10000 + (answernum[4] - 48) * 1000 + (answernum[2] - 48) * 100 + (answernum[0] - 48) * 10 + (answernum[4] - 48);
                        if ((x1 + x2) == x3)
                        {
                            MessageBox.Show("恭喜你，答对了！");
                        }
                        else
                        {
                            MessageBox.Show("答案错误");
                        }
                        break;

                    case "例题2":
                        x0 = (answernum[0] - 48) * 100 + (answernum[1] - 48) * 10 + (answernum[2] - 48);
                        x1 = (answernum[3] - 48) * 10 + (answernum[2] - 48);
                        x2 = (answernum[3] - 48) * 1000 + (answernum[4] - 48) * 100 + (answernum[0] - 48) * 10 + (answernum[2] - 48);
                        x3 = 7 * 100 + (answernum[4] - 48) * 10 + (answernum[3] - 48);
                        x4 = (answernum[5] - 48) * 1000 + (answernum[3] - 48) * 100 + (answernum[1] - 48) * 10 + (answernum[2] - 48);
                        if ((x0 * x1) == x4 && (x0 * (answernum[2] - 48)) == x2 && (x0 * (answernum[3] - 48)) == x3)
                        {
                            MessageBox.Show("恭喜你，答对了！");
                        }
                        else
                        {
                            MessageBox.Show("答案错误");
                        }
                        break;

                    case "加法":
                        x1 = x[1, 1] * 1000 + x[1, 2] * 100 + x[1, 3] * 10 + x[1, 4];
                        x2 = x[2, 1] * 1000 + x[2, 2] * 100 + x[2, 3] * 10 + x[2, 4];
                        x3 = x[3, 0] * 10000 + x[3, 1] * 1000 + x[3, 2] * 100 + x[3, 3] * 10 + x[3, 4];
                        //要确保被加数、加数的首位不为0
                        if (((x1 + x2) == x3) && ((m == 3 && x[1, 3] != 0 && x[2, 3] != 0) || (m == 4 && x[1, 2] != 0 && x[2, 2] != 0) || (m == 5 && x[1, 1] != 0 && x[2, 1] != 0))) 
                        {
                            MessageBox.Show("恭喜你，答对了！");
                        }
                        else
                        {
                            MessageBox.Show("答案错误");
                        }
                        break;

                    case "减法":
                        x1 = x[1, 1] * 1000 + x[1, 2] * 100 + x[1, 3] * 10 + x[1, 4];
                        x2 = x[2, 1] * 1000 + x[2, 2] * 100 + x[2, 3] * 10 + x[2, 4];
                        x3 = x[3, 1] * 1000 + x[3, 2] * 100 + x[3, 3] * 10 + x[3, 4];
                        //要确保被减数、减数的首位不为0
                        if (((x1 - x2) == x3) && ((m == 3 && x[1, 3] != 0 && x[2, 3] != 0) || (m == 4 && x[1, 2] != 0 && x[2, 2] != 0) || (m == 5 && x[1, 1] != 0 && x[2, 1] != 0)))
                        {
                            MessageBox.Show("恭喜你，答对了！");
                        }
                        else
                        {
                            MessageBox.Show("答案错误");
                        }
                        break;

                    case "乘法":
                        if (m == 3)     //难度为简单，2位数乘以1位数
                        {
                            x1 = x[1, 3] * 10 + x[1, 4];
                            x2 = x[2, 4];
                            x3 = x[3, 2] * 100 + x[3, 3] * 10 + x[3, 4];
                            if ((x1 * x2) == x3 && x[1, 3] != 0 ) //确保被乘数首位不为0（乘数为1位）
                            {
                                MessageBox.Show("恭喜你，答对了！");
                            }
                            else
                            {
                                MessageBox.Show("答案错误");
                            }
                        }
                        else            //难度为中等，2位数乘以2位数；难度为困难，3位数乘以2位数
                        {
                            x0 = x[0, 2] * 100 + x[0, 3] * 10 + x[0, 4];
                            x1 = x[1, 3] * 10 + x[1, 4];
                            x2 = x[2, 1] * 1000 + x[2, 2] * 100 + x[2, 3] * 10 + x[2, 4];
                            x3 = x[3, 0] * 1000 + x[3, 1] * 100 + x[3, 2] * 10 + x[3, 3];
                            x4 = x[4, 0] * 10000 + x[4, 1] * 1000 + x[4, 2] * 100 + x[4, 3] * 10 + x[4, 4];
                            //确保被乘数、乘数首位不为0
                            if ((x0 * x1) == x4 && (x0 * x[1, 4]) == x2 && (x0 * x[1, 3]) == x3 && x[1, 3] != 0 && ((m == 4 && x[0, 3] != 0) || (m == 5 && x[0, 2] != 0)))
                            {
                                MessageBox.Show("恭喜你，答对了！");
                            }
                            else
                            {
                                MessageBox.Show("答案错误");
                            }
                        }
                        break;

                    case "除法":        //除法中第1行有2个数，x1记录除数，x6记录被除数
                        x0 = x[0, 4] * 10 + x[0, 5];                    //商
                        x1 = x[1, 0] * 10 + x[1, 1];                    //除数
                        x6 = x[1, 3] * 100 + x[1, 4] * 10 + x[1, 5];    //被除数
                        x2 = x[2, 3] * 100 + x[2, 4] * 10 + x[2, 5];
                        x3 = x[3, 3] * 100 + x[3, 4] * 10 + x[3, 5];
                        x4 = x[4, 3] * 100 + x[4, 4] * 10 + x[4, 5];
                        x5 = x[5, 4] * 10 + x[5, 5];
                        if (x1 == 0)
                        {
                            MessageBox.Show("答案错误");        //除数不能为0
                        }
                        else
                        {
                            if (x4 == 0 && x5 == 0)     //算式未占用第4、5行
                            {
                                if ((x0 * x1 + x3) == x6 && (x0 * x1) == x2 &&
                                    ((m == 3 && x[1, 0] != 0 && x[1, 4] != 0) || (m == 4 && x[1, 4] != 0) || (m == 5 && x[1, 0] != 0 && x[1, 3] != 0)))
                                {
                                    MessageBox.Show("恭喜你，答对了！");
                                }
                                else
                                {
                                    MessageBox.Show("答案错误");
                                }
                            }
                            else        //算式占用第4、5行
                            {
                                if ((x0 * x1 + x5) == x6 && (x6 - x2) == x3 && (x3 - x4) == x5 &&
                                   ((m == 4 && x[1, 4] != 0) || (m == 5 && x[1, 0] != 0 && x[1, 3] != 0))) //m=3只占用0~3行
                                {
                                    MessageBox.Show("恭喜你，答对了！");
                                }
                                else
                                {
                                    MessageBox.Show("答案错误");
                                }
                            }
                        }
                        
                        break;
                }
            }
        }

        #endregion

        #endregion


        #region 查看答案

        private void button3_Click(object sender, EventArgs e)
        {
            //新建dataGridView1 7列，结果存放在其中
            for (int k = 0; k < 7; k++) //新建7列，输出结果
            {
                dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
                dataGridView1.Columns[k].Width = 35;    //将宽度调整为存放数字的合适的宽度
            }

            switch (label3.Text)
            {
                case "例题1":
                    EG1_key();
                    break;
                case "例题2":
                    EG2_key();
                    break;
                case "加法":
                    ADD_key();
                    break;
                case "减法":
                    SUB_key();
                    break;
                case "乘法":
                    MUL_key();
                    break;
                case "除法":
                    DIV_key();
                    break;
            }

        }

        #region 加减法字母及种类数、配置分配方案

        /// <summary>
        /// 题目出现的字母及其种类数、初始化分配方案
        /// </summary>
        /// <returns>返回字母的种类数</returns>
        private int letterselection()
        {
            for (int i = 0; i < 14; i++)  //布尔数组的元素置为false（避免受之前显示的影响）
            {
                s[i] = false;
            }

            for (int i = 0; i < 10; i++)  //NumFlag数组的元素置为0，数字未分配（避免受之前显示的影响）
            {
                NumFlag[i] = 0;
            }

            for (int i = 0; i < 11; i++)  //NumNum数组的元素置为-1，字母未分配数字（避免受之前显示的影响）
            {
                NumTaken[i] = -1;
            }

            for (int i = 0; i < 6; i++)     //calculate数组置0
            {
                for (int j = 0; j < 6; j++)
                {
                    calculate[i, j] = 0;
                }
            }

            //character[t]字母图片；place[i,j]出题区位置放置的字母图片；letter[]A~N`的字符数组
            int k = 0;
            for (int t = 0; t < 14; t++)        //若题目中出现某字母，使布尔数组s中相应的值为true
            {
                for (int i = 0; i < 6; i++)
                    for (int j = 0; j < 6; j++)
                    {
                        if (character[t] == place[i, j] && s[t] == false)   //比较character[t]的字母图片与出题区[i,j]位置的图片是否一致
                        {
                            s[t] = true;
                            k = k + 1;							//k记录题目中出现的字母的种类数
                        }
                    }
            }
            return (k);
        }


        /// <summary>
        /// 将搜索到的数字分配方案配置到calculate数组
        /// </summary>
        private void allocation()
        {
            int t = 1;
            int[] allocatenum = new int[14];   //存放搜索到的代表字母的数字，位置与布尔数组对应
            for (int i = 0; i < 14; i++)       //将搜索到的代表字母的数字存入answernum数组的对应位置
            {
                if (s[i] == true)
                {
                    allocatenum[i] = NumTaken[t];
                    t = t + 1;
                }
            }
            for (int l = 0; l < 14; l++)        //在矩阵calculate中写入字母位置对应的数值
            {
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        if (character[l] == place[i, j])    //[i,j]位置为图片l时，calculate[i,j]赋值
                        {
                            calculate[i, j] = allocatenum[l];
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 例题1搜索到的数字分配方案配置到calculate数组
        /// </summary>
        private void EG1_allocation()
        {
            int t = 1;
            int[] allocatenum = new int[14];   //存放搜索到的代表棋子的数字，位置与布尔数组对应
            for (int i = 0; i < 14; i++)       //将搜索到的代表字母的数字存入answernum数组的对应位置
            {
                if (s[i] == true)
                {
                    allocatenum[i] = NumTaken[t];
                    t = t + 1;
                }
            }
            for (int l = 0; l < 5; l++)        //在矩阵calculate中写入字母位置对应的数值
            {
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        if (eg1[l] == place[i, j])    //[i,j]位置为图片l时，calculate[i,j]赋值（用例题1的图片比对）
                        {
                            calculate[i, j] = allocatenum[l];
                        }
                    }
                }
            }
        }


        #endregion

        #region 乘除法设定

        public int[] mm = new int[10];

        private void NUM(int i, int j)      //找出题目中与[i,j]处具有相同字母的位置
        {
            for (int p = 0; p < 6; p++)
            {
                for (int q = 0; q < 6; q++)
                {
                    if ((num[p, q] == 1) && (place[p, q] == place[i, j]))//[p,q]处的字母与[i,j]的字母相同
                    {
                        num[p, q] = num[p, q] + temp + 10;//标记
                    }
                }
            }
            num[i, j] = num[i, j] - 10;//[i,j]处标记修订，修订后比其他位置有相同字母的num值小10
        }

        private void CAL(int i, int j)   //题目中与[i,j]处具有相同字母的位置对应的calculate值相同
        {
            for (int p = 0; p < 6; p++)
                for (int q = 0; q < 6; q++)
                {
                    if ((place[p, q] == place[i, j]) && (num[p, q] != 0) && (num[p, q] < 25))
                    {
                        calculate[p, q] = calculate[i, j];//字母相同，其位置对应的calculate值相同
                    }
                }
        }

        #endregion

        #region 例题1解答

        /// <summary>
        /// 例题1解答
        /// </summary>

        private void EG1_key()
        {
            for (int i = 0; i < 14; i++)    //例题1出现5种棋子,其他元素置为false
            {
                if (i < 5)
                {
                    s[i] = true;
                }
                else
                {
                    s[i] = false;
                }
            }
            int EG1_n = 1;
            EG1_search(5, EG1_n);           //例题1出现5种棋子
        }


        #region 例题1的搜索

        /// <summary>
        /// 例题1搜索
        /// </summary>
        /// <param name="k">题目中出现k种棋子</param>
        /// <param name="n">第n个棋子</param>
        /// NumFlag[j]=1表示该数字已经分配给棋子，NumFlag[j]=0表示未分配给棋子；NumTaken[n]记录第n个棋子分配的数字
        private void EG1_search(int k, int n)	// k表示题目中出现了k种棋子，n表示尝试给第n个棋子分配数字，j代表0~9的数字
        {
            int j;
            for (j = 0; j <= 9; j++)	    //尝试把数字0~9分配给第n个棋子
            {
                if (NumFlag[j] != 0)         //数字j已经分配给其他棋子，此次分配失败
                {
                    continue;			    //跳到下次循环（直接到j++)
                }
                NumTaken[n] = j;            //把数字j分配给第n个棋子
                NumFlag[j] = 1;             //数字j已分配
                if (n == k)				    //已找到一种数字分配方案（NumTaken数组是一种方案）
                {
                    EG1_allocation();       //给calculate矩阵按数字分配方案赋值
                    ADD_out();              //将符合要求的方案输出
                }
                else
                {
                    EG1_search(k, n + 1);	// 给第n+1个棋子分配数字
                }
                NumTaken[n] = -1;		    //回溯，把这一次分得的数字退回，棋子未分配
                NumFlag[j] = 0;             //数字标记为未分配
            }
        }

        #endregion

        //例题1的输出同加法输出

        #endregion

        #region 例题2解答

        /// <summary>
        /// 例题2解答
        /// </summary>

        private void EG2_key()
        {
            for (int c1 = 0; c1 < 10; c1++)
            {
                mm[c1] = -1;
            }
            num[3, 1] = 0;
            int x = 0;          //搜索中用到，标记从0开始搜索
            int i = 0, j = 0;
            for (i = 0; i < 5; i++)
                for (j = 0; j < 5; j++)
                {
                    if (num[i, j] == 1)         //成立，则表示该字母为首次搜索到
                    {
                        NUM(i, j);
                        temp = temp + 1;
                    }
                }
            temp = temp - 1;    //统计该题目中有多少种字母
            eg2_MUL_search(temp, x);
        }


        #region 例题2搜索

        /// <summary>
        /// 例题2搜索
        /// </summary>
        /// <param name="k"></param>
        /// <param name="x"></param>
        private void eg2_MUL_search(int k, int x)
        {
            int i, j, h, z = 1;
            if (k > x)          //第一层x=0，表示不同的数字多于0个
            {
                for (i = 0; i < 5; i++)    //加法只有第1层到第三层有数字
                {
                    for (j = 0; j < 5; j++)
                    {
                        if (num[i, j] != 0 && z == 1)         //每一行数的第一个数不为0，若为第一个不为0的数，则从1开始置数，否则从0开始
                        { h = 1; z = 0; }
                        else if (num[i, j] != 0 && j == (5 - m))
                            h = 1;
                        else h = 0;

                        if (num[i, j] == (x + 2))     //因为标记是否有图及图片是否相同的数组是“0”代表没有图或者图片是符号，2开始标记，此处加上2
                        {
                            for (int q = h; q < 10; q++)    //根据是否是第一列来从0~9或者从1~9搜索
                            {
                                //保证q被赋值为与已经赋值的calculate数组均不同的数字
                                for (int c2 = 0; c2 < x; c2++)
                                {
                                    if (q == mm[c2])
                                    { q++; c2 = -1; }
                                }
                                if ((q > -1) && (q < 10))
                                {
                                    calculate[i, j] = q;
                                    mm[x] = q;
                                    CAL(i, j);

                                    if ((x == (k - 1)) && (((m == 5) && (calculate[0, 2] != 0) && (calculate[1, 3] != 0)) || ((m == 4) && (calculate[0, 3] != 0) && (calculate[1, 3] != 0)) || ((m == 3) && (calculate[1, 3] != 0) && (calculate[2, 4] != 0))))
                                    {
                                        eg2_MUL_out();

                                    }
                                    eg2_MUL_search(k, x + 1);

                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region 例题2输出

        /// <summary>
        /// 例题2输出
        /// </summary>
        private void eg2_MUL_out()
        {
            calculate[3, 1] = 7;
            if ((((calculate[0, 2] * 100 + calculate[0, 3] * 10 + calculate[0, 4]) * calculate[1, 4]) == (calculate[2, 1] * 1000 + calculate[2, 2] * 100 + calculate[2, 3] * 10 + calculate[2, 4]))
                    && (((calculate[0, 2] * 100 + calculate[0, 3] * 10 + calculate[0, 4]) * calculate[1, 3]) == (calculate[3, 0] * 1000 + calculate[3, 1] * 100 + calculate[3, 2] * 10 + calculate[3, 3]))
                    && ((calculate[4, 0] * 10000 + calculate[4, 1] * 1000 + calculate[4, 2] * 100 + calculate[4, 3] * 10 + calculate[4, 4]) != 0)
                    && ((calculate[3, 0] * 1000 + calculate[3, 1] * 100 + calculate[3, 2] * 10 + calculate[3, 3]) != 0)
                    && ((calculate[4, 0] * 10000 + calculate[4, 1] * 1000 + calculate[4, 2] * 100 + calculate[4, 3] * 10 + calculate[4, 4]) == ((calculate[0, 2] * 100 + calculate[0, 3] * 10 + calculate[0, 4]) * (calculate[1, 3] * 10 + calculate[1, 4]))))
            {
                if (((m == 5) && (num[2, 1] != 0) && (calculate[2, 1] != 0)) || ((m == 5) && (num[2, 1] == 0)))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        dataGridView1.Rows.Add(1);
                        for (int j = 0; j < 5; j++)
                        {
                            calculate1[i, j] = calculate[i, j] + "";
                            if (num[i, j] != 0)
                            {
                                dataGridView1.Rows[i + index].Cells[j].Value = calculate1[i, j].ToString();
                            }
                            if (i == 1 && j == 0)
                            {
                                dataGridView1.Rows[i + index].Cells[0].Value = "x";
                                number = number + 1;
                            }
                            if (i == 3 && j == 1)
                            {
                                dataGridView1.Rows[i + index].Cells[1].Value = "7";
                            }
                        }
                    }
                    index = index + 6;
                }
            }
        }

        #endregion

        #endregion

        #region 加法解答

        /// <summary>
        /// 加法解答
        /// </summary>
        private void ADD_key()
        {
            int Add_k;
            Add_k = letterselection();
            int Add_n = 1;
            Add_search(Add_k, Add_n);
        }

        #region 加法搜索

        /// <summary>
        /// 加法搜索
        /// </summary>
        /// <param name="k">题目中出现k种字母</param>
        /// <param name="n">第n个字母</param>
        /// NumFlag[j]=1表示该数字已经分配给字母，NumFlag[j]=0表示未分配给字母；NumTaken[n]记录第n个字母分配的数字
        private void Add_search(int k, int n)	// k表示题目中出现了k种字母，n表示尝试给第n个字母分配数字，j代表0~9的数字
        {
            int j;
            for (j = 0; j <= 9; j++)	    //尝试把数字0~9分配给第n个字母
            {
                if (NumFlag[j] != 0)         //数字j已经分配给其他字母，此次分配失败
                {
                    continue;			    //跳到下次循环（直接到j++)
                }
                NumTaken[n] = j;            //把数字j分配给第n个字母
                NumFlag[j] = 1;             //数字j已分配
                if (n == k)				    //已找到一种数字分配方案（NumTaken数组是一种方案）
                {
                    allocation();           //给calculate矩阵按数字分配方案赋值
                    ADD_out();              //将符合要求的方案输出
                }
                else
                {
                    Add_search(k, n + 1);	// 给第n+1个字母分配数字
                }
                NumTaken[n] = -1;		    //回溯，把这一次分得的数字退回，字母未分配
                NumFlag[j] = 0;             //数字标记为未分配
            }
        }

        #endregion

        #region 加法输出
        /// <summary>
        /// 加法输出
        /// </summary>
        private void ADD_out()            //加法的输出，判断若所有的结果位均不为0，且加数相加的结果等于最终和则输出
        {
            if (m == 5)
            {
                if (num[3, 0] == 0)
                {
                    calculate[3, 0] = 0;
                }

                if (((calculate[1, 1] * 1000 + calculate[1, 2] * 100 + calculate[1, 3] * 10 + calculate[1, 4]
                    + calculate[2, 1] * 1000 + calculate[2, 2] * 100 + calculate[2, 3] * 10 + calculate[2, 4])
                    == (calculate[3, 0] * 10000 + calculate[3, 1] * 1000 + calculate[3, 2] * 100 + calculate[3, 3] * 10 + calculate[3, 4])))
                {
                    if (((calculate[3, 5 - m] != 0) && (num[3, 5 - m] != 0)) || (num[3, 5 - m] == 0))
                    {
                        //  写数字到datagridview1
                        for (int i = 0; i < 4; i++)
                        {
                            dataGridView1.Rows.Add(1);
                            for (int j = 5 - m; j < 5; j++)
                            {
                                calculate1[i, j] = calculate[i, j] + "";
                                if (num[i, j] != 0)
                                {
                                    dataGridView1.Rows[i + index].Cells[j].Value = calculate1[i, j].ToString();
                                }
                                //第二行写入符号
                                if (i == 2 && j == 4)
                                {
                                    dataGridView1.Rows[2 + index].Cells[0].Value = "+";
                                    number = number + 1;
                                }

                            }
                        } index = index + 4;   //每隔四行需要写一个符号，index标记+4  
                    }
                }
                if (num[3, 0] == 0)
                    calculate[3, 0] = -1;
            }
            else if (m == 4)
            {
                if (num[3, 1] == 0)
                {
                    calculate[3, 1] = 0;
                }
                if (((calculate[1, 2] * 100 + calculate[1, 3] * 10 + calculate[1, 4] + calculate[2, 2] * 100 + calculate[2, 3] * 10 + calculate[2, 4]) == (calculate[3, 1] * 1000 + calculate[3, 2] * 100 + calculate[3, 3] * 10 + calculate[3, 4])))
                {
                    if (((calculate[3, 5 - m] != 0) && (num[3, 5 - m] != 0)) || (num[3, 5 - m] == 0))
                    {
                        //  写数字到datagridview1
                        for (int i = 0; i < 4; i++)
                        {

                            dataGridView1.Rows.Add(1);
                            for (int j = 5 - m; j < 5; j++)
                            {
                                calculate1[i, j] = calculate[i, j] + "";
                                if (num[i, j] != 0)
                                {
                                    dataGridView1.Rows[i + index].Cells[j].Value = calculate1[i, j].ToString();
                                }
                                //第二行写入符号
                                if (i == 2 && j == 4)
                                {
                                    dataGridView1.Rows[2 + index].Cells[0].Value = "+";
                                    number = number + 1;
                                }

                            }
                        } index = index + 4;   //每隔四行需要写一个符号，index标记+4  

                    }
                }
                if (num[3, 1] == 0)
                {
                    calculate[3, 1] = -1;
                }
            }
            else if (m == 3)
            {
                if (num[3, 2] == 0)
                {
                    calculate[3, 2] = calculate[3, 2] + 1;
                }
                if (((calculate[1, 3] * 10 + calculate[1, 4] + calculate[2, 3] * 10 + calculate[2, 4])
                    == (calculate[3, 2] * 100 + calculate[3, 3] * 10 + calculate[3, 4])))
                {
                    if (((calculate[3, 5 - m] != 0) && (num[3, 5 - m] != 0)) || (num[3, 5 - m] == 0))
                    {
                        //  写数字到datagridview1
                        for (int i = 0; i < 4; i++)
                        {

                            dataGridView1.Rows.Add(1);
                            for (int j = 5 - m; j < 5; j++)
                            {
                                calculate1[i, j] = calculate[i, j] + "";
                                if (num[i, j] != 0)
                                {
                                    dataGridView1.Rows[i + index].Cells[j].Value = calculate1[i, j].ToString();
                                }
                                //第二行写入符号
                                if (i == 2 && j == 4)
                                {
                                    dataGridView1.Rows[2 + index].Cells[0].Value = "+";
                                    number = number + 1;
                                }

                            }
                        } index = index + 4;   //每隔四行需要写一个符号，index标记+4  

                    }
                }
                if (num[3, 2] == 0)
                {
                    calculate[3, 2] = -1;
                }
            }
        }

        #endregion

        #endregion

        #region 减法解答

        /// <summary>
        /// 减法解答
        /// </summary>
        private void SUB_key()
        {
            int Sub_k;
            Sub_k = letterselection();
            int Sub_n = 1;
            Sub_search(Sub_k, Sub_n);
        }

        #region 减法搜索

        /// <summary>
        /// 减法搜索
        /// </summary>
        /// <param name="k">题目中出现k种字母</param>
        /// <param name="n">第n个字母</param>
        /// NumFlag[j]=1表示该数字已经分配给字母，NumFlag[j]=0表示未分配给字母；NumTaken[n]记录第n个字母分配的数字
        private void Sub_search(int k, int n)	// k表示题目中出现了k种字母，n表示尝试给第n个字母分配数字，j代表0~9的数字
        {
            int j;
            for (j = 0; j <= 9; j++)	    //尝试把数字0~9分配给第n个字母
            {
                if (NumFlag[j] != 0)         //数字j已经分配给其他字母，此次分配失败
                {
                    continue;			    //跳到下次循环（直接到j++)
                }
                NumTaken[n] = j;            //把数字j分配给第n个字母
                NumFlag[j] = 1;             //数字j已分配
                if (n == k)				    //已找到一种数字分配方案（NumTaken数组是一种方案）
                {
                    allocation();           //给calculate矩阵按数字分配方案赋值
                    SUB_out();              //将符合要求的方案输出
                }
                else
                {
                    Sub_search(k, n + 1);	// 给第n+1个字母分配数字
                }
                NumTaken[n] = -1;		    //回溯，把这一次分得的数字退回，字母未分配
                NumFlag[j] = 0;             //数字标记为未分配
            }
        }

        #endregion

        #region 减法输出

        /// <summary>
        /// 减法输出
        /// </summary>
        private void SUB_out()
        {
            int a1 = 0;

            if ((((calculate[1, 1] * 1000 + calculate[1, 2] * 100 + calculate[1, 3] * 10 + calculate[1, 4]) - (calculate[2, 1] * 1000 + calculate[2, 2] * 100 + calculate[2, 3] * 10 + calculate[2, 4])) == (calculate[3, 1] * 1000 + calculate[3, 2] * 100 + calculate[3, 3] * 10 + calculate[3, 4])))
            {

                for (int j1 = 5 - m; j1 < 5; j1++)
                {
                    if (num[3, j1] != 0)
                        a1++; //a1用于标记结果行写入的数字数目
                }
                if (a1 == 1)   //若只有一个数字，则可以为0，采用以下的输出方式
                {
                    //这里实现加数及符号的写入，即前两行的输出
                    for (int i = 0; i < 4; i++)
                    {

                        dataGridView1.Rows.Add(1);
                        for (int j = 5 - m; j < 5; j++)
                        {
                            calculate1[i, j] = calculate[i, j] + "";//将calculate整型数组装换为calculate1字符串数组，便于输出

                            if ((num[i, j] != 0))       //在有图的地方都输出相应的数字
                            {
                                dataGridView1.Rows[i + index].Cells[j].Value = calculate1[i, j].ToString();

                            }
                            //第二行时，在最前面所在位输出符号
                            if (i == 2 && j == 4)
                            {
                                dataGridView1.Rows[2 + index].Cells[0].Value = "-";
                                number = number + 1;
                            }

                        }
                    }
                    //此处输出第三行结果行
                    index = index + 4;
                }
                else if (num[3, 5 - a1] != 0 && calculate[3, 5 - a1] != 0)  //若不止有一个数字，则需要保证不是个位的那个不能是0
                {
                    for (int i = 0; i < 4; i++)
                    {

                        dataGridView1.Rows.Add(1);  //  新建datagridview的行
                        for (int j = 5 - m; j < 5; j++)
                        {
                            calculate1[i, j] = calculate[i, j] + "";

                            if ((num[i, j] != 0))
                            {
                                dataGridView1.Rows[i + index].Cells[j].Value = calculate1[i, j].ToString();

                            }
                            if (i == 2 && j == 4)
                            {
                                dataGridView1.Rows[2 + index].Cells[0].Value = "-";
                                number = number + 1;
                            }

                        }
                    }
                    index = index + 4;
                }
            }

        }

        #endregion

        #endregion

        #region 乘法解答

        /// <summary>
        /// 乘法解答
        /// </summary>
        private void MUL_key()
        {
            for (int c1 = 0; c1 < 10; c1++)
            {
                mm[c1] = -1;
            }
            int x = 0;          //搜索中用到，标记从0开始搜索
            int i = 0, j = 0;
            for (i = 0; i < 5; i++)
                for (j = 0; j < 5; j++)
                {
                    if (num[i, j] == 1)         //成立，则表示该字母为首次搜索到
                    {
                        NUM(i, j);
                        temp = temp + 1;
                    }
                }
            temp = temp - 1;    //统计该题目中有多少种字母
            MUL_search(temp, x);
        }


        #region 乘法搜索

        /// <summary>
        /// 乘法搜索
        /// </summary>
        /// <param name="k"></param>
        /// <param name="x"></param>
        private void MUL_search(int k, int x)
        {
            int i, j, h, z = 1;
            if (k > x)          //第一层x=0，表示不同的数字多于0个
            {
                for (i = 0; i < 5; i++)    
                {
                    for (j = 0; j < 5; j++)
                    {
                        if (num[i, j] != 0 && z == 1)         //每一行数的第一个数不为0，若为第一个不为0的数，则从1开始置数，否则从0开始
                        { h = 1; z = 0; }
                        else if (num[i, j] != 0 && j == (5 - m))
                            h = 1;
                        else h = 0;

                        if (num[i, j] == (x + 2))     //因为标记是否有图及图片是否相同的数组是“0”代表没有图或者图片是符号，2开始标记，此处加上2
                        {
                            for (int q = h; q < 10; q++)    //根据是否是第一列来从0~9或者从1~9搜索
                            {
                                //保证q被赋值为与已经赋值的calculate数组均不同的数字
                                for (int c2 = 0; c2 < x; c2++)
                                {
                                    if (q == mm[c2])
                                    { q++; c2 = -1; }
                                }
                                if ((q > -1) && (q < 10))
                                {
                                    calculate[i, j] = q;
                                    mm[x] = q;
                                    CAL(i, j);

                                    if ((x == (k - 1)) && (((m == 5) && (calculate[0, 2] != 0) && (calculate[1, 3] != 0)) || ((m == 4) && (calculate[0, 3] != 0) && (calculate[1, 3] != 0)) || ((m == 3) && (calculate[1, 3] != 0) && (calculate[2, 4] != 0))))
                                    {
                                        MUL_out();

                                    }
                                    MUL_search(k, x + 1);

                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion


        #region 乘法输出

        /// <summary>
        /// 乘法输出
        /// </summary>
        private void MUL_out()
        {

            //m=4或者m=5时，乘法部分遍布5行，所以为一种输出
            if (m == 4 || m == 5)
            {

                if ((((calculate[0, 2] * 100 + calculate[0, 3] * 10 + calculate[0, 4]) * calculate[1, 4]) == (calculate[2, 1] * 1000 + calculate[2, 2] * 100 + calculate[2, 3] * 10 + calculate[2, 4]))
                    && (((calculate[0, 2] * 100 + calculate[0, 3] * 10 + calculate[0, 4]) * calculate[1, 3]) == (calculate[3, 0] * 1000 + calculate[3, 1] * 100 + calculate[3, 2] * 10 + calculate[3, 3]))
                    && ((calculate[4, 0] * 10000 + calculate[4, 1] * 1000 + calculate[4, 2] * 100 + calculate[4, 3] * 10 + calculate[4, 4]) != 0)
                    && ((calculate[3, 0] * 1000 + calculate[3, 1] * 100 + calculate[3, 2] * 10 + calculate[3, 3]) != 0)
                    && ((calculate[4, 0] * 10000 + calculate[4, 1] * 1000 + calculate[4, 2] * 100 + calculate[4, 3] * 10 + calculate[4, 4]) == ((calculate[0, 2] * 100 + calculate[0, 3] * 10 + calculate[0, 4]) * (calculate[1, 3] * 10 + calculate[1, 4]))))
                {
                    if (((m == 4) && (num[2, 2] != 0) && (calculate[2, 2] != 0))
                        || ((m == 4) && (num[2, 2] == 0))
                        || ((m == 5) && (num[2, 1] != 0) && (calculate[2, 1] != 0)) || ((m == 5) && (num[2, 1] == 0)))
                    {
                        for (int i = 0; i < 6; i++)
                        {

                            dataGridView1.Rows.Add(1);
                            for (int j = 0; j < 5; j++)
                            {
                                calculate1[i, j] = calculate[i, j] + "";
                                if (num[i, j] != 0)
                                {
                                    dataGridView1.Rows[i + index].Cells[j].Value = calculate1[i, j].ToString();
                                }
                                if (i == 1 && j == 0)
                                {
                                    dataGridView1.Rows[i + index].Cells[0].Value = "x";
                                    number = number + 1;
                                }
                            }
                        }
                        index = index + 6;

                    }
                }
            }
            //m=3时由于是两位数与一位数相乘，故只占据中间三行，采用另一种输出方式
            else if (m == 3)
            {
                if ((((calculate[1, 3] * 10 + calculate[1, 4]) * (calculate[2, 4])) == (calculate[3, 2] * 100 + calculate[3, 3] * 10 + calculate[3, 4])))// && (num[3,2]!=0)
                {
                    if (((num[3, 2] != 0) && (calculate[3, 2] != 0)) || (num[3, 2] == 0))
                    {
                        //  写数字到datagridview1
                        for (int i = 0; i < 4; i++)
                        {

                            dataGridView1.Rows.Add(1);
                            for (int j = 0; j < 5; j++)
                            {
                                calculate1[i, j] = calculate[i, j] + "";
                                if (num[i, j] != 0)
                                {
                                    dataGridView1.Rows[i + index].Cells[j].Value = calculate1[i, j].ToString();
                                }
                                if (i == 2 && j == 0)
                                {
                                    dataGridView1.Rows[2 + index].Cells[0].Value = "x";
                                    number = number + 1;
                                }


                            }
                        }
                        index = index + 4;
                    }
                }
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #endregion

        #region 除法解答

        /// <summary>
        /// 除法解答
        /// </summary>
        private void DIV_key()
        {
            for (int c1 = 0; c1 < 10; c1++)
            {
                mm[c1] = -1;
            }
            int x = 0;          //搜索中用到，标记从0开始搜索
            int i = 0, j = 0;
            if (m == 4 || m == 5)
            {
                for (i = 0; i < 6; i++)
                    for (j = 0; j < 6; j++)
                    {
                        if (num[i, j] == 1)         //成立，则表示该字母为首次搜索到
                        {
                            NUM(i, j);
                            temp = temp + 1;
                        }
                    }
                temp = temp - 1;    //统计该题目中有多少种字母

                DIV_search(temp, x);
            }
            else if (m == 3)
            {
                for (i = 0; i < 6; i++)
                    for (j = 0; j < 6; j++)
                    {
                        if (num[i, j] == 1)         //成立，则表示该字母为首次搜索到
                        {
                            NUM(i, j);
                            temp = temp + 1;
                        }
                    }
                temp = temp - 1;    //统计该题目中有多少种字母 
                DIV_search(temp, x);
            }
        }

        #region 除法搜索

        /// <summary>
        /// 除法搜索
        /// </summary>
        /// <param name="k"></param>
        /// <param name="x"></param>
        private void DIV_search(int k, int x)
        {

            int i, j;
            if (k > x)          //第一层x=0，表示不同的数字多于0个
            {
                for (i = 0; i < 6; i++)      //加法只有第1层到第三层有数字
                {
                    for (j = 0; j < 6; j++)
                    {
                        if (num[i, j] == (x + 2))     //因为标记是否有图及图片是否相同的数组是“0”代表没有图或者图片是符号，2开始标记，此处加上2
                        {
                            for (int q = 0; q < 10; q++)    //根据是否是第一列来从0~9或者从1~9搜索
                            {
                                for (int c2 = 0; c2 < x; c2++)
                                {
                                    if (q == mm[c2])
                                    { q++; c2 = -1; }
                                }
                                if ((q > -1) && (q < 10))
                                {
                                    calculate[i, j] = q;
                                    mm[x] = q;
                                    CAL(i, j);
                                    {
                                        if (m == 5)
                                        {
                                            if ((x == k - 1) && (calculate[1, 3] != 0) && (calculate[1, 0] != 0))
                                            {
                                                DIV_out();
                                            }
                                        }
                                        else if (m == 4)
                                        {
                                            if ((x == k - 1) && (calculate[1, 4] != 0) && (calculate[1, 1] != 0))
                                            {
                                                DIV_out();
                                            }
                                        }
                                        else if (m == 3)
                                        {
                                            if ((x == k - 1) && (calculate[1, 4] != 0) && (calculate[1, 0] != 0))//&&(calculate[0,5]!=0)
                                            {

                                                DIV_out();
                                            }
                                        }
                                    }
                                    DIV_search(k, x + 1);
                                }
                            }
                        }
                    }
                }
            }

        }

        #endregion

        #region 除法输出

        /// <summary>
        /// 除法输出
        /// </summary>
        private void DIV_out()
        {
            if (calculate[4, 3] == 0 && calculate[4, 4] == 0 && calculate[4, 5] == 0 && calculate[5, 4] == 0 && calculate[5, 5] == 0) //除式只占用0~3行
            {
                if (((calculate[0, 4] * 10 + calculate[0, 5])) * (calculate[1, 0] * 10 + calculate[1, 1]) + (calculate[3, 4] * 10 + calculate[3, 5]) == (calculate[1, 3] * 100 + calculate[1, 4] * 10 + calculate[1, 5])
                      && ((calculate[1, 3] * 100 + calculate[1, 4] * 10 + calculate[1, 5]) - (calculate[2, 3] * 100 + calculate[2, 4] * 10 + calculate[2, 5])) == (calculate[3, 4] * 10 + calculate[3, 5]))
                {
                    //m=3，两位数/两位数；m=4，两位数/一位数；m=5，三位数/两位数
                    if ((m == 3 && calculate[1, 0] != 0 && calculate[1, 4] != 0) || (m == 4 && calculate[1, 1] != 0 && calculate[1, 4] != 0) || (m == 5 && calculate[1, 0] != 0 && calculate[1, 3] != 0))
                    {
                        //  写数字到datagridview1
                        for (int i = 0; i < 6; i++)
                        {

                            dataGridView1.Rows.Add(1);
                            for (int j = 0; j < 6; j++)
                            {
                                calculate1[i, j] = calculate[i, j] + "";
                                if ((num[i, j] != 0))
                                {
                                    dataGridView1.Rows[i + index].Cells[j].Value = calculate1[i, j].ToString();
                                }
                                //第1行写入符号
                                if (i == 1 && j == 2)
                                {
                                    dataGridView1.Rows[1 + index].Cells[2].Value = "╱";
                                    number = number + 1;
                                }
                            }
                        }
                        index = index + 5;   //每隔五行需要写一个符号，index标记+5 
                    }
                }
            }
            else        //除式占用0~5行
            {
                if (((calculate[0, 4] * 10 + calculate[0, 5])) * (calculate[1, 0] * 10 + calculate[1, 1]) + (calculate[5, 4] * 10 + calculate[5, 5]) == (calculate[1, 3] * 100 + calculate[1, 4] * 10 + calculate[1, 5])
                     && ((calculate[1, 3] * 100 + calculate[1, 4] * 10 + calculate[1, 5]) - (calculate[2, 3] * 100 + calculate[2, 4] * 10 + calculate[2, 5])) == (calculate[3, 3] * 100 + calculate[3, 4] * 10 + calculate[3, 5])
                     && (calculate[3, 3] * 100 + calculate[3, 4] * 10 + calculate[3, 5]) - (calculate[4, 3] * 100 + calculate[4, 4] * 10 + calculate[4, 5]) == (calculate[5, 4] * 10 + calculate[5, 5]))
                {
                    //m=4，两位数/一位数；m=5，三位数/两位数
                    if ((m == 4 && calculate[1, 1] != 0 && calculate[1, 4] != 0) || (m == 5 && calculate[1, 0] != 0 && calculate[1, 3] != 0))
                    {
                        //  写数字到datagridview1
                        for (int i = 0; i < 6; i++)
                        {

                            dataGridView1.Rows.Add(1);
                            for (int j = 0; j < 6; j++)
                            {
                                calculate1[i, j] = calculate[i, j] + "";
                                if ((num[i, j] != 0))
                                {
                                    dataGridView1.Rows[i + index].Cells[j].Value = calculate1[i, j].ToString();
                                }
                                //第1行写入符号
                                if (i == 1 && j == 2)
                                {
                                    dataGridView1.Rows[1 + index].Cells[2].Value = "╱";
                                    number = number + 1;
                                }

                            }
                        }
                        index = index + 6;   //每隔六行需要写一个符号，index标记+6
                    }
                }
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
