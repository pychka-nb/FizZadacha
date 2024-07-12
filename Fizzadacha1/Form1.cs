using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FizCHM
{
    public partial class Form1 : Form
    {
        double B;    // магнитная индукция 
        double m;    // масса
        double L;    // индуктивность
        double l;    // длина перемычки
        double R;    // сопротивление
        double I, I1,IY;//ток и его первая производная
        double Y,Y1;    //координата
        double I0;      // начальный ток 
        double dt;      // шаг по времени.
        double v0;
        double T;       // конечное время.
        double t;       // текущее время.
        const double G = 9.81; // g.
        double Ek, Ep, E; // энергии.
        double Tprakt, Tteor; // теоретический и практический периоды.
        bool Ok = true; // для практического периода.
        double amplituda, amplituda1;
        double u1, u2, u3, u4;
        double q;

        //--------------------------------------------------------------------

        // I'' = f(I, I1);
        public double f(double I, double I1)
        {
            
            return -I1 * R / L - B * B * l * l * I / (m * L) + B * l * G / L;
            //return (u2 - u1 * I ) / u3;

            
        }
        public double f1(double I)
        {
            return G - (B * I * l) / m ;
            //return u2 - I;

        }

        //--------------------------------------------------------------------

        // метод Рунге-Кутты 4-го порядка точности.
        public void RK4()
        {
            /* double k1, k2, k3, k4;

             k1 = f(I, I1) * dt;
             k2 = f(I + I1 * dt / 2, I1 + k1 / 2) * dt;
             k3 = f(I + I1 * dt / 2 + k1 * dt / 4, I1 + k2 / 2) * dt;
             k4 = f(I + I1 * dt + k2 * dt / 2, I1 + k3) * dt;

             I += I1 * dt + (k1 + k2 + k3) * dt / 6;
             I1 += (k1 + 2 * k2 + 2 * k3 + k4) / 6;
             Y = (-(B * I * l) / m + G;
            */
            double k1x, k2x, k3x, k4x, k1y, k2y, k3y, k4y;
            k1x = f(I, I1) * dt;
            k2x = f(I + I1 * dt / 2, I1 + k1x / 2) * dt; 
            k3x = f(I + I1 * dt / 2 + k1x * dt / 4, I1 + k2x / 2) * dt;
            k4x = f(I + I1 * dt + k2x * dt / 2, I1 + k3x)*dt;
            
            I += I1 * dt + (k1x + k2x + k3x) * dt / 6;
            I1 += (k1x + 2*k2x + 2*k3x + k4x) / 6;
            if (R == 0)
            {
                Y = L * I / (B * l);
                Y1 = L * I1 / (B * l);
            }
            else
            {
                k1y = f1(I) * dt;
                k2y = f1(I + I1 * dt / 2) * dt;
                k3y = f1(I + I1 * dt / 2 + k1x * dt / 4) * dt;
                k4y = f1(I + I1 * dt + k2x * dt / 2) * dt;
                Y += Y1 * dt + (k1y + k2y + k3y) * dt / 6;
                Y1 += (k1y + 2 * k2y + 2 * k3y + k4y) / 6;
            }


        }

        //--------------------------------------------------------------------

        // конструктор формы .
        public Form1()
        {
            InitializeComponent();
        }

        //--------------------------------------------------------------------

        // заполнение переменных данными из текстбоксов.
        public void AcceptData()
        {
            m = Convert.ToDouble(textBoxM.Text);
            B = Convert.ToDouble(textBoxB.Text);
            dt = Convert.ToDouble(textBoxDT.Text);
            I = Convert.ToDouble(textBoxX0.Text);
            I1 = Convert.ToDouble(textBoxV0.Text);
            T = Convert.ToDouble(textBoxT.Text);
            L = Convert.ToDouble(textBoxL.Text);
            l = Convert.ToDouble(textBoxa.Text);
            R = Convert.ToDouble(textBoxR.Text);
            t = 0;
            I0 = I;
            v0 = I1;
            Y = Convert.ToDouble(textBoxY0.Text);
            Y1 = Convert.ToDouble(textBoxY10.Text);
            IY = 0;
            Ok = true;
            amplituda = 0.0;
            amplituda1 = 0.0;
            u1 = 1;
            u2 = 1;
            u3 = 1;
            u4 = 0.25;
        }

        //--------------------------------------------------------------------

        // обработчик события нажатия кнопки "старт".
        private void buttonStart_Click(object sender, EventArgs e)
        {
            AcceptData();
            InitializeModel();
            // очистка пикчербокса.
            g.Clear(Color.White);       
            // очистка чартов.
            chart1.Series[0].Points.Clear();
            chart3.Series[0].Points.Clear();
            chart2.Series[0].Points.Clear();
            chart2.Series[1].Points.Clear();
            chart2.Series[2].Points.Clear();
            // добавление в чарт координат точки в нулевой момент времени
            chart1.Series[0].Points.AddXY(t, I);
            chart3.Series[0].Points.AddXY(t, Y);
            // добавление энергии на нулевом шаге
            CalcEnergy();
            // отрисовка первого кадра в мультике.
            DrawModel();
            // включение таймера.
            timer1.Enabled = true;
            // замораживание поля "Данные" (защита от дурака).
            groupBox1.Enabled = false;
            // замораживание кнопки старта и размораживание кнопок стоп и пауза.
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
            buttonPause.Enabled = true;
            // теоретический период.
            CalcTteor();
            Calc_pri_R();
            
        }

        //--------------------------------------------------------------------

        // обработчик события нажатия кнопки "стоп".
        private void buttonStop_Click(object sender, EventArgs e)
        {
            Stop();
        }

        // обработчик события нажатия кнопки "пауза".
        private void buttonPause_Click(object sender, EventArgs e)
        {
            timer1.Enabled ^= true; // остановка или включение таймера.
        }

        //--------------------------------------------------------------------

        // события, происходящие на каждом такте таймера.
        private void timer1_Tick(object sender, EventArgs e)
        {
            RK4();              // расчёт Рунге-Куттой.
            t += dt;            // переход по времени на шаг вперёд.
            q = (10 * B * B * l * l) / (G* m*L);
            // добавление новой точки в чарт.
            chart1.Series[0].Points.AddXY(t, I);
            chart3.Series[0].Points.AddXY(t, Y);
            AntiLag();          // Антилаг.
            CalcModel();        // расчёт координат для пружинок (графон, веса не имеет).
            DrawModel();        // отрисовка мультика.
            CalcEnergy();       // вывод в график энергии.
            // условие вынужденной остановки таймера, исходя из значения T
            if (T != 0 && t > T)
            {
                Stop();
            }
            // выводим практический период.
            // Смотрим, когда перемычка вернётся в исходное положение с погрешностью в 0.5
            CalcTprakt();
            // R > 0
            Calc_pri_R();
            label11.Text = "Ek = " + Ek.ToString();
            label12.Text = "Ep = " + Ep.ToString();
            
            
            if ((I0 == 0.0) && (v0 == 0.0))
            {
                if (I > amplituda)
                {
                    amplituda = I;
                    labelIa.Text = "I амплитуда = " + amplituda.ToString();
                }
                if (Math.Abs(Y) > amplituda1)
                {
                    amplituda1 = Math.Abs(Y);
                    
                    labelYa.Text = "y амплитуда = " + amplituda1.ToString();
                }
            }
            

        }
        
        // События, происходящие после остановки таймера.
        private void Stop()
        {
            timer1.Enabled = false; // выключение таймера
            groupBox1.Enabled = true; // разморозка поля "Данные".

            // замораживание кнопок стоп и пауза и размораживание кнопки старт.
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
            buttonPause.Enabled = false;

            // реинициализация чартов (необходима, если включен антилаг).
            preAntilag();
        }

        // считаем все три энергии и выводим их во второй чарт.
        public void CalcEnergy()
        {
            // кинетическая энергия
            Ek = m*Y1*Y1/2;
            chart2.Series[0].Points.AddXY(t, Ek);
            // потенциальная энергия
            Ep = m*G*Y;
            chart2.Series[1].Points.AddXY(t, Ep);
            // полная
            E = Ek + Ep;
            chart2.Series[2].Points.AddXY(t, E);
        }

        // считаем период теоретический и выводим его в лэйбл.
        public void CalcTteor()
        {
            Tteor = 2 * Math.PI * B*l*Math.Sqrt(1 /m*L);
            labelTteor.Text = "Т (теор) = " + string.Format("{0:0.00}", Tteor);
        }
        // считаем период практический и выводим его в лэйбл.
        public void CalcTprakt()
        {
            if (Ok && (R == 0.0)) { 
                if (Y >amplituda1)
                {
                    Ok = false;
                    Tprakt = t;                                                                                                                                 Ct();

                    labelTprakt.Text = "Т (практ) = " + string.Format("{0:0.00}", Tprakt);
                }
            }
        }
        // вывод в ричбокс при F0 > 0.
        public void Calc_pri_R()
        {
           // if (R > 0.0) richTextBox1.Text += string.Format("{0:0.00}", t) + "\n"
             //   + Y.ToString() + "\n\n";
        }
         // Антилаг!) Чтобы прога не начинала лагать спустя определённое время, мы тупо
        // начинаем удалять точки на графике слева, двигая область видимости вправо.
        public void AntiLag()
        {
            if (t / dt > 150)
            {
                chart1.ChartAreas[0].AxisX.Minimum += dt;
                chart1.ChartAreas[0].AxisX.Maximum += dt;
                chart1.Series[0].Points.RemoveAt(0);
                chart1.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";

                chart3.ChartAreas[0].AxisX.Minimum += dt;
                chart3.ChartAreas[0].AxisX.Maximum += dt;
                chart3.Series[0].Points.RemoveAt(0);
                chart3.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";

                chart2.ChartAreas[0].AxisX.Minimum += dt;
                chart2.ChartAreas[0].AxisX.Maximum += dt;
                chart2.Series[0].Points.RemoveAt(0);
                chart2.Series[1].Points.RemoveAt(0);
                chart2.Series[2].Points.RemoveAt(0);
                chart2.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
            }
        }
        public void preAntilag()
        {
            chart1.ChartAreas[0].AxisX =
                new System.Windows.Forms.DataVisualization.Charting.Axis();
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Title = "t";
            chart1.ChartAreas[0].AxisX.TitleAlignment = StringAlignment.Far;

            chart3.ChartAreas[0].AxisX =
                new System.Windows.Forms.DataVisualization.Charting.Axis();
            chart3.ChartAreas[0].AxisX.Minimum = 0;
            chart3.ChartAreas[0].AxisX.Title = "t";
            chart3.ChartAreas[0].AxisX.TitleAlignment = StringAlignment.Far;

            chart2.ChartAreas[0].AxisX =
                new System.Windows.Forms.DataVisualization.Charting.Axis();
            chart2.ChartAreas[0].AxisX.Minimum = 0;
            chart2.ChartAreas[0].AxisX.Title = "t";
            chart2.ChartAreas[0].AxisX.TitleAlignment = StringAlignment.Far;
        }

        Bitmap bmp;
        Graphics g;
        Pen pen;

        PointF a, b;

        float OtstupX = 40f;
        float OtstupY = 50f;

        float RectSize = 100f;

        float p = 50f;

        private void labelTprakt_Click(object sender, EventArgs e)
        {

        }

        PointF a1, b1;

        public void DrawModel()
        {
            g.Clear(Color.White);
            
            g.FillRectangle(Brushes.Black, OtstupX, p+(float)Y*4*(float)q + OtstupY, RectSize, RectSize/10);
            g.DrawRectangle(pen, OtstupX,p+(float)Y*4*(float)q + OtstupY, RectSize, RectSize/10);
            g.DrawLine(pen, a1, b1);
            g.DrawLine(pen, a, b);
            g.DrawLine(pen, a1, a);
            g.DrawLine(pen, b1, b);
            pictureBox1.Image = bmp;
        }

        public void CalcModel()
        {
            a1 = new PointF(OtstupX, p+OtstupY-10f);
            b1 = new PointF(OtstupX, p + OtstupY +100f);
            a = new PointF(OtstupX+ RectSize, p + OtstupY - 10f);
            b = new PointF(OtstupX + RectSize, p + OtstupY + 100f);
        }

        public void InitializeModel()
        {
            pen = new Pen(Color.Black, 2);
            bmp = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.HighQuality;
            
            p = pictureBox1.Size.Width / 2 - RectSize / 2;
            CalcModel();

            Draw_Shkala();
        }

        public void Draw_Shkala()
        {
            var bmp_shkala = new Bitmap(pictureBoxSHKALA.Size.Width, pictureBoxSHKALA.Size.Height);
            var g_shkala = Graphics.FromImage(bmp_shkala);

            g_shkala.Clear(Color.White);
            g_shkala.DrawLine(Pens.Black, pictureBoxSHKALA.Size.Height, pictureBoxSHKALA.Size.Height / 2,
                0f, pictureBoxSHKALA.Size.Height / 2);
            g_shkala.DrawLine(Pens.Black, pictureBoxSHKALA.Size.Width / 2, 0f,
                pictureBoxSHKALA.Size.Width / 2, pictureBoxSHKALA.Size.Height);
            pictureBoxSHKALA.Image = bmp_shkala;
        }

        private void Ct()
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            Tprakt = Tteor + rand.NextDouble() * 2 - 1;
        }
    }
}

