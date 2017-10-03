using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace BSK
{
    public partial class Propagacja : Form
    {
        public Propagacja()
        {
            InitializeComponent();
        }


        private void Window_Focus(object sender, /*MouseButton*/System.Windows.Forms.MouseEventArgs e)
        {
            this.Focus();
        }

        private void KeyPressing(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != ','))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == ',') && ((sender as TextBox).Text.IndexOf(',') > -1))
            {
                e.Handled = true;
            }
        }

        private void LostFocusFrequency(object sender, EventArgs e)
        {
            float check = float.Parse(textBox1.Text);

            if (check > 2000) textBox1.Text = "2000";
            else if (check < 800) textBox1.Text = "800";
        }

        private void LostFocusHeight1(object sender, EventArgs e)
        {
            float check = float.Parse(textBox4.Text);

            if (check > 50) textBox4.Text = "50";
            else if (check < 4) textBox4.Text = "4";
        }

        private void LostFocusHeight2(object sender, EventArgs e)
        {
            float check = float.Parse(textBox2.Text);

            if (check > 3) textBox2.Text = "3";
            else if (check < 1) textBox2.Text = "1";
        }

        private void LostFocusDistance(object sender, EventArgs e)
        {
            float check = float.Parse(textBox8.Text);

            if (check > 5) textBox8.Text = "5";
            else if (check < 0.02) textBox8.Text = "0.02";
        }


        private void LostFocusPhi(object sender, EventArgs e)
        {
            float check = float.Parse(textBox7.Text);

            if (check > 90) textBox7.Text = "90";
        }

        //W przypadku gdy nieszczęśnik wciśnie przycisk
        private void button1_Click(object sender, EventArgs e)
        {

            if (textBox1.Text.Equals("") || textBox2.Text.Equals("") || textBox3.Text.Equals("") || textBox4.Text.Equals("") || textBox5.Text.Equals("") || textBox6.Text.Equals("") || textBox7.Text.Equals("") || textBox8.Text.Equals("")) 
            {
                MessageBox.Show("Co najmniej jedną z kolumn jest pusta", "Błąd");
                return;
            }

            float Frequency = float.Parse(textBox1.Text);
            float Height1 = float.Parse(textBox4.Text);
            float Height2 = float.Parse(textBox2.Text);
            float BuildDist = float.Parse(textBox3.Text); if (BuildDist == 0) { MessageBox.Show("Odległość między budynkami równa zero", "Błąd"); return; }
            float HeightObstacle = float.Parse(textBox5.Text); if (HeightObstacle == 0) { MessageBox.Show("Średnia wysokość przeszkody równa zero", "Błąd"); return; }
            float StreetWidth = float.Parse(textBox6.Text); if (StreetWidth == 0) { MessageBox.Show("Średnia szerokość ulicy równa zero", "Błąd"); return; }
            float Distance = float.Parse(textBox8.Text);
            float Phi = float.Parse(textBox7.Text);
            bool LOS = checkBox1.Checked;
            string Environment = comboBox1.Text;

            float Intercept = PropagateLkm(Frequency, Height1, Height2, Distance, LOS, Phi, BuildDist, StreetWidth, HeightObstacle, Environment);
            float Slope = PropagateLaw(Height1, LOS, HeightObstacle);

            float Propagation = (float)(Intercept + 10 * Slope * Math.Log10(Distance));

            Wykresy Okno = new Wykresy(Frequency, Height1, Height2, LOS, Phi, BuildDist, StreetWidth, HeightObstacle, Environment, Propagation);
            Okno.Show();
        }

        public static float PropagateLkm(float Frequency, float Height1, float Height2, float Distance, bool LOS, float Phi, float BuildDist, float StreetWidth, float HeightObstacle, string Environment)
        {
            float frspl, delhm, delhb, oril, rtsl, bshl = 0, kaya = 0, kayf;
            float result; //Obliczona propagacja, trzeba sprawdzić jeszcze jeden warunek zanim się ją zwróci

            if (LOS) //zamienić na true z CheckBox1 (LOS)
            {
                return (float)(42.64 + 20 * Math.Log10(Frequency));
            }
            else//NLOS
            {
                frspl = (float)(32.45 + 20 * Math.Log10(Frequency)); //frspl
                delhm = (float)(HeightObstacle - Height2); //delhm
                delhb = (float)(Height1 - HeightObstacle); //delhb

                //oril
                if (Phi >= 0 && Phi < 35) oril = (float)(-10 + 0.354 * Phi);
                else if (Phi >= 35 && Phi < 55) oril = (float)(2.5 + 0.075 * (Phi - 35));
                else /*Phi >= 55*/oril = (float)(4 - 0.114 * (Phi - 55));

                rtsl = (float)(-16.9 - 10 * Math.Log10(StreetWidth) + 10 * Math.Log10(Frequency) + oril + 20 * Math.Log10(delhm));

                //bshl + kaya
                if (delhb > 0)
                {
                    bshl = (float)(-18 * Math.Log10(1 + delhb));
                    kaya = 54;
                }
                else if (delhb < 0 && Distance >= 0.5)
                {
                    bshl = 0; //przypisane na górze, przy deklaracji
                    kaya = (float)(54 + 0.8 * Math.Abs(delhb));
                }
                else if (delhb < 0 && Distance < 0.5)
                {
                    bshl = 0;
                    kaya = (float)(54 + 0.8 * Math.Abs(delhb) * (Distance / 0.5));
                }

                if (Environment.Equals("Duże miasto")) kayf = (float)(-4 + 1.5 * ((Frequency/925) - 1));
                else /*Inne*/ kayf = (float)(-4 + 0.7 * ((Frequency / 925) - 1));

                //propagacja
                result = (float)(frspl + rtsl + bshl + kaya + kayf * Math.Log10(Frequency) - 9 * Math.Log10(BuildDist));

                if (result > frspl) return result;
                else return frspl;
            }
            
        }
        public static float PropagateLaw(float Height1, bool LOS, float HeightObstacle)
        {
            if (LOS)
            {
                return (float)2.6;
            }
            else
            {
                float delhb = Height1 - HeightObstacle;
                if (delhb > 0) return (float)3.8;
                else return (float)(3.8 + 1.5 * (Math.Abs(delhb) / HeightObstacle));
            }
        }
    }
}
