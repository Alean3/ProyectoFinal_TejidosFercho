using System;
using System.Collections.Generic;
using System.ComponentModel;
using MySql.Data.MySqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PlantillaDiseñoProyectoFinal_JS.Conector;
using BCrypt.Net; // Esto es para encriptar las contraseñas

namespace PlantillaDiseñoProyectoFinal_JS
{

    public partial class Form3 : Form
    {
        private Conexion dbConexion;
        public Form3()
        {
            InitializeComponent();
            menuStrip1.RenderMode = ToolStripRenderMode.System;
            ToolStripManager.VisualStylesEnabled = false;

            txtContrasena.PasswordChar = '*';
            txtConfContrasena.PasswordChar = '*';
        }

        private void Form3_Load(object sender, EventArgs e)
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
                    using (var formRegistro = new Form7())
                    {
                        formRegistro.ShowDialog();
                    }
                    this.Show();
                    break;
            }
        }

        private void NewForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Show();
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

        private void btn_registrar_Click(object sender, EventArgs e)
        {
        if (ValidarCampos())
            {
                MessageBox.Show("Lo sentimos, estos campos son obligatorios. Por favor asegurese de rellenarlos", "Error: Campo Vacío", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

        try
        {
          Conexion conexionBD = new Conexion();
                
            using (MySqlConnection conexion = conexionBD.Conectar())
            {
                conexion.Open();

                    string contrasenaEncriptada = BCrypt.Net.BCrypt.HashPassword(txtContrasena.Text); // Genera un hash aleatorio como método de encriptación

                    string query = @"INSERT INTO usuario (user_usu, nom_usu, ape_usu, dir_usu, tel_usu, ciu_usu, emai_usu, con_usu) 
                                         VALUES (@usuario, @nombre, @apellido, @direccion, @telefono, @ciudad, @email, @contrasena)";
                using (MySqlCommand comando = new MySqlCommand(query, conexion))
                {
                    // Asignamos valores desde los TextBox
                    comando.Parameters.AddWithValue("@usuario", txtUsuario.Text);
                    comando.Parameters.AddWithValue("@nombre", txtNombre.Text);
                    comando.Parameters.AddWithValue("@apellido", txtApellido.Text);
                    comando.Parameters.AddWithValue("@direccion", txtDireccion.Text);
                    comando.Parameters.AddWithValue("@telefono", txtTelefono.Text);
                    comando.Parameters.AddWithValue("@ciudad", txtCiudad.Text);
                    comando.Parameters.AddWithValue("@email", txtEmail.Text);
                    comando.Parameters.AddWithValue("@contrasena", contrasenaEncriptada);

                    // Ejecutamos el comando
                    int filasAfectadas = comando.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        Confirmacion();  // Limpiamos campos y mostramos mensaje
                    }
                }
            }
        }

            catch (MySqlException ex)
                {
                    // Si hay duplicados (usuario o email repetido)
                    if (ex.Number == 1062)
                    {
                        MessageBox.Show("¡Usuario o correo ya registrados!", "Error",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Error en la base de datos: {ex.Message}", "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error inesperado: {ex.Message}", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                }
        private bool ValidarCampos()
        {
            return string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtApellido.Text) || string.IsNullOrWhiteSpace(txtDireccion.Text) || string.IsNullOrWhiteSpace(txtTelefono.Text) || string.IsNullOrWhiteSpace(txtCiudad.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtContrasena.Text) || string.IsNullOrWhiteSpace(txtConfContrasena.Text);
        }

        private void Confirmacion()
        {
          MessageBox.Show("Su registro fue éxitoso, bienvenido a Tejidos el Fercho", "Registrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtUsuario.Text = "";
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtDireccion.Text = "";
            txtTelefono.Text = "";
            txtCiudad.Text = "";
            txtEmail.Text = "";
            txtContrasena.Text = "";
            txtConfContrasena.Text = "";

            this.Hide();

            using (var formMensaje = new Form4())
            {
                formMensaje.ShowDialog();
            }

            Form4 redirigir = new Form4();
            redirigir.Show();
            this.Hide();
        }
    }
}
