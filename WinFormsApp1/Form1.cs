using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        //флаги остановки задачи
        CancellationTokenSource cancelTokenSource;
        CancellationToken token;

        Graphics graphics;
        Task<int> task;

        //мишень
        Rectangle target;


        double width;
        double height;
        public Form1()
        {
            InitializeComponent();
            graphics = this.CreateGraphics();
            graphics.PageUnit = GraphicsUnit.Pixel;
            width = this.Width;
            height = this.Height;
            Random random = new Random();

            target = new Rectangle(new Point(random.Next(0, (int)width - 20),  (int)height - 100), new Size(25, 25));
        }

        public void DrawExplosion(Point point) 
        {
            Graphics graphics1 = this.CreateGraphics();
            graphics.PageUnit = GraphicsUnit.Pixel;
            graphics1.DrawEllipse(new Pen(Color.Orange, 2.0f), point.X, point.Y, 10f, 10f);
            Thread.Sleep(15);
            graphics1.DrawEllipse(new Pen(Color.Orange, 2.0f), point.X, point.Y, 15f, 15f);
            Thread.Sleep(15);
            graphics1.DrawEllipse(new Pen(Color.Orange, 2.0f), point.X, point.Y, 20f, 20f);
            Thread.Sleep(15);
            graphics1.Clear(Color.White);

        }

        public void DrawAirplane(int x, Pen pen)
        {
            //рисуется мишень
            graphics.DrawRectangle(new Pen(Color.DarkOliveGreen, 2.0f), target);
            //рисуется самолёт
            graphics.DrawRectangle(pen, new Rectangle(x, 50, 100, 50));
        }

        public void DrawBomb(int x, int y, Graphics graphicsBomb, Pen pen)
        {
            
            graphicsBomb.DrawRectangle(pen, new Rectangle(x, y, 20, 20));
        }

        public void RunBomb(int position)
        {
            Pen whitePen = new Pen(Color.White, 1.0f);
            Pen redPen = new Pen(Color.Red, 1.0f);
            Graphics graphicsBomb = this.CreateGraphics();
            for(int i = 75; i < height; i++)
            {
                DrawBomb(position + (i - 1) / 2 , i - 1, graphicsBomb, whitePen);
                DrawBomb(position + i / 2, i, graphicsBomb, redPen);

                //если нашёл target, то
                if(Math.Abs(target.Location.X - (position + i / 2)) < 20 & Math.Abs(target.Location.Y- i) < 20)
                {
                    //x проходит
                    //     MessageBox.Show("Попал!");
                    Task.Run(() => DrawExplosion(target.Location));
                    graphicsBomb.Clear(Color.White);
                    graphicsBomb.DrawRectangle(new Pen(Color.White, 2.0f), target);
                    DrawBomb(position, i, graphicsBomb, whitePen);

                    Random random = new Random();
                    target.Location = new Point(random.Next(0, (int)width - 20), target.Location.Y);
                    return;
                }

                Thread.Sleep(12);
            }
        }

        public int GameStart(int num)
        {
            Pen whitePen = new Pen(Color.White, 2.0f);
            Pen blackPen = new Pen(Color.Black, 2.0f);
            int i = num;
            while (!token.IsCancellationRequested) { 
                while(i < width)
                {
                    if(token.IsCancellationRequested)
                    {
                        break;  
                    }

                    //graphics.Clear(Color.Gray);
                    DrawAirplane(i - 1, whitePen);
                    DrawAirplane(i, blackPen);
                    Thread.Sleep(12);
                    i++;
                }
                if (token.IsCancellationRequested)
                {
                    break;
                }
                i = -100;
            }
            return i;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            task = Task<int>.Run(() => GameStart(-100));
        }

        private  void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            cancelTokenSource.Cancel();

            
            int a = 3;
            if(e.KeyChar == 32)
            {
                task.Wait();
                int position = task.Result;
                //метод срать
                Task.Run(() => RunBomb(position));
                //
                
                cancelTokenSource = new CancellationTokenSource();
                token = cancelTokenSource.Token;

                task = Task.Run(() => GameStart(position));
              
            }
        }
    }
}
