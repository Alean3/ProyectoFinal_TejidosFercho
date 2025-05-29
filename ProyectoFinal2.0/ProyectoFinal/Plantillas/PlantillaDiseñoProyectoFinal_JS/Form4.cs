using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BCrypt.Net;
using MySql.Data.MySqlClient;
using PlantillaDiseñoProyectoFinal_JS.Conector;
using PlantillaDiseñoProyectoFinal_JS.Models;

namespace PlantillaDiseñoProyectoFinal_JS
{
    public partial class Form4 : Form
    {
        private Conexion dbConexion;

        public Form4()
        {
            InitializeComponent();
            dbConexion = new Conexion();

            menuStrip1.RenderMode = ToolStripRenderMode.System;
            ToolStripManager.VisualStylesEnabled = false;
            RegistroLink.Click += MenuItem_Click;

            txtContrasena.PasswordChar = '*';
        }

        private void Form4_Load(object sender, EventArgs e)
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
            string nombreControl = "";

            
            if (sender is ToolStripMenuItem menuItem)
            {
                nombreControl = menuItem.Name;
            }
            else if (sender is LinkLabel linkLabel)
            {
                nombreControl = linkLabel.Name;
            }

            switch (nombreControl)
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

                case string Registros when Registros == "Menu_Registro" || Registros == "RegistroLink":
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

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtContrasena.Text))
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
                    string query = @"SELECT id_usu, con_usu FROM usuario WHERE user_usu = @usuarioOEmail OR emai_usu = @usuarioOEmail";

                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                       comando.Parameters.AddWithValue("@usuarioOEmail", txtUsuario.Text);
                       using (MySqlDataReader lector = comando.ExecuteReader())
                       {
                            if (lector.Read())
                            {
                                // Verificar contraseña
                                string hashGuardado = lector["con_usu"].ToString();
                                bool contraseñaCorrecta = BCrypt.Net.BCrypt.Verify(txtContrasena.Text, hashGuardado);

                                if (contraseñaCorrecta)
                                {
                                    Usuario usuarioLogueado = ObtenerDatosUsuarioDesdeBD(txtUsuario.Text);
                                    Program.LoggedInUser = usuarioLogueado;

                                    MessageBox.Show("Usted ha iniciado sesión, puede continuar.", "Login Exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    this.Hide();

                                    using (var formMensaje = new Form1())
                                    {
                                        formMensaje.ShowDialog();
                                    }

                                    Form1 redirigir = new Form1();
                                    redirigir.Show();
                                    this.Hide();
                                }
                                else
                                {
                                    MessageBox.Show("Su contraseña es incorrecta, por favor verifique", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("El usuario que usted ingresó no se encuentra registrado, por favor verifique", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                       }
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Manejar errores específicos de MySQL
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("No se puede conectar al servidor", "Error de conexión",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case 1045:
                        MessageBox.Show("Credenciales de base de datos inválidas", "Error",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        MessageBox.Show($"Error de MySQL: {ex.Message}", "Error",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Usuario ObtenerDatosUsuarioDesdeBD(string nombreUsuario)
        {
            Usuario usuario = null;
            using (MySqlConnection connection = dbConexion.Conectar()) // _dbConexion es tu instancia de Conexion
            {
                if (connection == null) return null;

                string query = "SELECT id_usu, user_usu, nom_usu, ape_usu, dir_usu, tel_usu, ciu_usu, emai_usu FROM usuario WHERE user_usu = @UsuarioNombre";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@UsuarioNombre", nombreUsuario);

                try
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                IdUsuario = reader.GetInt32("id_usu"),
                                UsuarioNombre = reader.GetString("user_usu"),
                                Nombres = reader.GetString("nom_usu"),
                                Apellidos = reader.GetString("ape_usu"),
                                DireccionEnvio = reader.GetString("dir_usu"),
                                Telefono = reader.IsDBNull(reader.GetOrdinal("tel_usu")) ? null : reader.GetString("tel_usu"),
                                Ciudad = reader.IsDBNull(reader.GetOrdinal("ciu_usu")) ? null : reader.GetString("ciu_usu"),
                                Correo = reader.GetString("emai_usu")
                            };
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Error al cargar datos del usuario: {ex.Message}", "Error de BD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return usuario;
        }

        private void RegistroLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();

            using (var formMensaje = new Form3())
            {
                formMensaje.ShowDialog();
            }

            Form3 redirigir = new Form3();
            redirigir.Show();
            this.Hide();
        }
    }
}
