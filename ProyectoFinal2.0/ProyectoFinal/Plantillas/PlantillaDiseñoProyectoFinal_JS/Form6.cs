using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PlantillaDiseñoProyectoFinal_JS.Conector;
using PlantillaDiseñoProyectoFinal_JS.Models;
using MySql.Data.MySqlClient;

namespace PlantillaDiseñoProyectoFinal_JS
{
    public partial class Form6 : Form
    {
        private Producto _currentProduct;
        private Conexion _dbConexion;

        public Form6()
        {
            InitializeComponent();
            menuStrip1.RenderMode = ToolStripRenderMode.System;
            ToolStripManager.VisualStylesEnabled = false;
            _dbConexion = new Conexion();

            // Configuración inicial de controles
            ConfigureInitialControls();
        }

        public Form6(int id, string nombre, string descripcion, decimal valor, int cantidad, string fabricante, Image imagen) : this()
        {
            SetProductDetails(id, nombre, descripcion, valor, cantidad, fabricante, imagen);
        }

        private void ConfigureInitialControls()
        {
            // Configurar el NumericUpDown
            num_canprod.Minimum = 1;
            num_canprod.Value = 1;

            // Ocultar controles inicialmente
            pb_carcom.Visible = false;
            num_canprod.Visible = false;
            lblmensajecompra.Visible = false;
        }

        public void SetProductDetails(int id, string nombre, string descripcion, decimal valor, int cantidad, string fabricante, Image imagen)
        {
            _currentProduct = new Producto
            {
                ProductoID = id,
                NombreProducto = nombre,
                DescripcionProducto = descripcion,
                ValorProducto = valor,
                CantidadProducto = cantidad,
                FabricanteProducto = fabricante,
                ImagenCargada = imagen
            };

            LoadProductDetails();
            CheckUserLoginStatus();
        }

        private void LoadProductDetails()
        {
            // Asignar valores a los controles de UI
            lblnombre.Text = _currentProduct.NombreProducto;
            lbldescripcion.Text = _currentProduct.DescripcionProducto;
            lblvalor.Text = _currentProduct.ValorProducto.ToString("C");
            lblfabricante.Text = _currentProduct.FabricanteProducto;

            // Configurar disponibilidad y stock
            ConfigureStockAvailability();

            // Cargar imagen del producto
            LoadProductImage();

            this.Text = $"Detalle de Producto: {_currentProduct.NombreProducto}";
        }

        private void ConfigureStockAvailability()
        {
            if (_currentProduct.CantidadProducto >= 1)
            {
                lblDisponibilidad.Text = "¡En Stock!";
                lblDisponibilidad.ForeColor = Color.Green;
                num_canprod.Visible = true;
                num_canprod.Maximum = _currentProduct.CantidadProducto;
                num_canprod.Value = 1;
            }
            else
            {
                lblDisponibilidad.Text = "¡Agotado!";
                lblDisponibilidad.ForeColor = Color.Red;
                num_canprod.Visible = false;
                pb_carcom.Visible = false;
                lblmensajecompra.Visible = false;
            }
        }

        private void LoadProductImage()
        {
            if (_currentProduct.ImagenCargada != null)
            {
                pbimagen.Image = _currentProduct.ImagenCargada;
                pbimagen.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else
            {
                string noImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "no_image.png");

                if (File.Exists(noImagePath))
                {
                    pbimagen.Image = Image.FromFile(noImagePath);
                    pbimagen.SizeMode = PictureBoxSizeMode.Zoom;
                }
                else
                {
                    pbimagen.Image = null;
                }
            }
        }

        private void CheckUserLoginStatus()
        {
            if (Program.LoggedInUser != null && _currentProduct.CantidadProducto > 0)
            {
                pb_carcom.Visible = true;
                lblmensajecompra.Visible = true;
            }
            else
            {
                pb_carcom.Visible = false;
                lblmensajecompra.Visible = false;
            }
        }

        private void Form6_Load(object sender, EventArgs e)
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

                case "Menu_Sesion":
                    this.Hide();
                    using (var formLogin = new Form4())
                    {
                        formLogin.ShowDialog();
                    }
                    // Actualizar estado después de login
                    CheckUserLoginStatus();
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
                    using (var formProductos = new Form5())
                    {
                        formProductos.ShowDialog();
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
                    item.ForeColor = Color.SaddleBrown;
                    break;
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pb_carcom_Click(object sender, EventArgs e)
        {
            if (Program.LoggedInUser == null)
            {
                MessageBox.Show("Debes iniciar sesión para añadir productos al carrito.", "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_currentProduct == null)
            {
                MessageBox.Show("No hay un producto seleccionado para añadir al carrito.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int quantityToAdd = (int)num_canprod.Value;

            if (quantityToAdd <= 0)
            {
                MessageBox.Show("La cantidad a añadir debe ser mayor que cero.", "Cantidad Inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Verificar stock actual en la base de datos
            if (!CheckStockAvailability(_currentProduct.ProductoID, quantityToAdd))
            {
                MessageBox.Show($"No hay suficiente stock disponible para '{_currentProduct.NombreProducto}'.", "Stock Insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Añadir al carrito
            if (Program.CurrentShoppingCart == null)
            {
                Program.CurrentShoppingCart = new ShoppingCart();
            }

            Program.CurrentShoppingCart.AddItem(_currentProduct, quantityToAdd);
        }

        private bool CheckStockAvailability(int productId, int requestedQuantity)
        {
            try
            {
                using (MySqlConnection connection = _dbConexion.Conectar())
                {
                    if (connection == null) return false;

                    string query = "SELECT can_prod FROM producto WHERE id_prod = @ProductId";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ProductId", productId);

                    connection.Open();
                    int currentStock = Convert.ToInt32(command.ExecuteScalar());

                    return currentStock >= requestedQuantity;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al verificar el stock: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            if (Program.LoggedInUser == null)
            {
                MessageBox.Show("Debes iniciar sesión para ver el carrito de compras.", "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.Hide();
            using (Form9 formCart = new Form9())
            {
                formCart.ShowDialog();
            }
            this.Show();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // Liberar recursos de la imagen si es necesario
            if (pbimagen.Image != null)
            {
                pbimagen.Image.Dispose();
            }
        }
    }
}