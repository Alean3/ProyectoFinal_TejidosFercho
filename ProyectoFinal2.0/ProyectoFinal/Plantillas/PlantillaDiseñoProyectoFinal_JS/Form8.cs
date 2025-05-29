using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlantillaDiseñoProyectoFinal_JS
{
    public partial class Form8 : Form
    {
        public Form8()
        {
            InitializeComponent();
            menuStrip1.RenderMode = ToolStripRenderMode.System;
            ToolStripManager.VisualStylesEnabled = false;
        }

        private void Form8_Load(object sender, EventArgs e)
        {
            this.Location = new Point(0, 0);
            EventosPersonalizados(menuStrip1.Items);
        }

        private void EventosPersonalizados(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    menuItem.MouseEnter += MenuItem_MouseEnter;
                    menuItem.MouseLeave += MenuItem_MouseLeave;
                    menuItem.Click += MenuItem_Click;

                    if (menuItem.HasDropDownItems)
                    {
                        EventosPersonalizados(menuItem.DropDownItems);
                    }
                }
            }
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {

            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            switch (item.Name)
            {
                case "Menu_Inicio":
                    this.Hide();
                    using (var formInicio = new Form1())
                    {
                        formInicio.ShowDialog();
                    }
                    this.Show();
                    break;

                case string ListaQuieSom when ListaQuieSom == "mision" || ListaQuieSom == "vision" || ListaQuieSom == "objetivos":
                    this.Hide();
                    using (var formNosotros = new Form2())
                    {
                        formNosotros.ShowDialog();
                    }
                    this.Show();
                    break;

                case "Menu_Sesion":
                    this.Hide();
                    using (var formLogin = new Form4())
                    {
                        formLogin.ShowDialog();
                    }
                    this.Show();
                    break;

                case "Menu_Registro":
                    this.Hide();
                    using (var formRegistro = new Form3())
                    {
                        formRegistro.ShowDialog();
                    }
                    this.Show();
                    break;

                case "Menu_Producto":
                    this.Hide();
                    using (var formRegistro = new Form5())
                    {
                        formRegistro.ShowDialog();
                    }
                    this.Show();
                    break;

                case "Contacto":
                    this.Hide();
                    using (var formContacto = new Form7())
                    {
                        formContacto.ShowDialog();
                    }
                    this.Show();
                    break;
            }
        }

        private void MenuItem_MouseEnter(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item == null) return;

            switch (item.Tag?.ToString())
            {
                case "Grupo1":
                    item.BackColor = Color.SaddleBrown;
                    item.ForeColor = Color.PapayaWhip;
                    break;
                case "Grupo2":
                    item.BackColor = Color.PapayaWhip;
                    item.ForeColor = Color.SaddleBrown;
                    break;
                default:

                    item.BackColor = Color.SaddleBrown;
                    item.ForeColor = Color.PapayaWhip;
                    break;
            }
        }

        private void MenuItem_MouseLeave(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item == null) return;

            switch (item.Tag?.ToString())
            {
                case "Grupo1":
                    item.BackColor = Color.PapayaWhip;
                    item.ForeColor = Color.SaddleBrown;
                    break;
                case "Grupo2":
                    item.BackColor = Color.SaddleBrown;
                    item.ForeColor = Color.PapayaWhip;
                    break;
                default:
                    item.BackColor = Color.PapayaWhip;
                    item.BackColor = Color.SaddleBrown;
                    break;
            }
        }
    }
}
