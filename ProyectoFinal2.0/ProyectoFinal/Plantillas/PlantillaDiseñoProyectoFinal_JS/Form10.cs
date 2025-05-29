using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PlantillaDiseñoProyectoFinal_JS.Models;
using PlantillaDiseñoProyectoFinal_JS.Conector;
using MySql.Data.MySqlClient;

namespace PlantillaDiseñoProyectoFinal_JS
{
    public partial class Form10 : Form
    {
        private ShoppingCart _cart;
        private Conexion _dbConexion;

        public Form10(ShoppingCart cart)
        {
            InitializeComponent();
            _cart = cart;
            _dbConexion = new Conexion();

            menuStrip1.RenderMode = ToolStripRenderMode.System;
            ToolStripManager.VisualStylesEnabled = false;

            DisplayOrderSummary();
            
            if (Program.LoggedInUser != null)
            {
                txtNombre.Text = Program.LoggedInUser.Nombres;
                txtApellido.Text = Program.LoggedInUser.Apellidos; // Asumiendo un txtApellido
                txtDireccion.Text = Program.LoggedInUser.DireccionEnvio;
                txtCiudad.Text = Program.LoggedInUser.Ciudad;
                txtTelefono.Text = Program.LoggedInUser.Telefono;

                txtNombre.ReadOnly = true;
                txtApellido.ReadOnly = true;
                txtDireccion.ReadOnly = true;
                txtCiudad.ReadOnly = true;
                txtTelefono.ReadOnly = true;
            }
            else
            {
                MessageBox.Show("Debe iniciar sesión para completar el pedido.", "Acceso Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close(); // Cierra el formulario de checkout si no está logueado
                return;
            }

            btnpagar.Click += btnPlaceOrder_Click; 
        }

        private void DisplayOrderSummary()
        {
            lblOrderSummary.Text = "Resumen del Pedido:\n"; 
            foreach (var item in _cart.Items)
            {
                lblOrderSummary.Text += $"{item.NombreProducto} x {item.Cantidad} - {item.Subtotal:C}\n";
            }
            lblOrderSummary.Text += $"\nTotal a Pagar: {_cart.TotalAmount:C}";
        }

        private void btnPlaceOrder_Click(object sender, EventArgs e)
        {
            if (Program.LoggedInUser == null)
            {
                MessageBox.Show("Debes iniciar sesión para completar un pedido.", "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_cart.Items.Any())
            {
                MessageBox.Show("El carrito está vacío. No se puede realizar un pedido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validación básica para la información de envío 
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtDireccion.Text) ||
                string.IsNullOrWhiteSpace(txtCiudad.Text))
            {
                MessageBox.Show("Por favor, complete toda la información de envío requerida.", "Campos Requeridos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var connection = _dbConexion.Conectar())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1) INSERT del pedido
                        const string insertOrderSql = @"
                            INSERT INTO pedido 
                                (id_usu, fec_ped, est_ped, tot_ped,
                                 dir_env, ciu_env, te_env, cor_env)
                            VALUES 
                                (@IdUsuario, @FechaPedido, @EstadoPedido, @TotalPedido,
                                 @DireccionEnvio, @CiudadEnvio, @TelefonoEnvio, @CorreoEnvio);";

                        long orderId;
                        using (var orderCmd = new MySqlCommand(insertOrderSql, connection, transaction))
                        {
                            orderCmd.Parameters.AddWithValue("@IdUsuario", Program.LoggedInUser.IdUsuario);
                            orderCmd.Parameters.AddWithValue("@FechaPedido", DateTime.Now);
                            orderCmd.Parameters.AddWithValue("@EstadoPedido", "Pendiente");
                            orderCmd.Parameters.AddWithValue("@TotalPedido", _cart.TotalAmount);
                            orderCmd.Parameters.AddWithValue("@DireccionEnvio", txtDireccion.Text);
                            orderCmd.Parameters.AddWithValue("@CiudadEnvio", txtCiudad.Text);
                            orderCmd.Parameters.AddWithValue("@TelefonoEnvio", txtTelefono.Text);
                            orderCmd.Parameters.AddWithValue("@CorreoEnvio", Program.LoggedInUser.Correo);

                            orderCmd.ExecuteNonQuery();
                            orderId = orderCmd.LastInsertedId;
                        }

                        // 2) Detalles y actualización de stock
                        const string insertDetailSql = @"
                                INSERT INTO detalle_pedido 
                                    (id_ped, id_prod, can_prod_det, precuni_det)
                                VALUES 
                                    (@IdPedido, @IdProd, @Cantidad, @PrecioUnitario);";
                        const string updateStockSql = @"
                            UPDATE producto 
                            SET can_prod = can_prod - @Cantidad
                            WHERE id_prod = @IdProd AND can_prod >= @Cantidad;";

                        foreach (var item in _cart.Items)
                        {
                            using (var detailCmd = new MySqlCommand(insertDetailSql, connection, transaction))
                            {
                                detailCmd.Parameters.AddWithValue("@IdPedido", orderId);
                                detailCmd.Parameters.AddWithValue("@IdProd", item.ProductoID);
                                detailCmd.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                                detailCmd.Parameters.AddWithValue("@PrecioUnitario", item.ValorProducto);
                                detailCmd.ExecuteNonQuery();
                            }

                            using (var stockCmd = new MySqlCommand(updateStockSql, connection, transaction))
                            {
                                stockCmd.Parameters.AddWithValue("@IdProd", item.ProductoID);
                                stockCmd.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                                if (stockCmd.ExecuteNonQuery() == 0)
                                    throw new Exception($"Stock insuficiente para '{item.NombreProducto}'.");
                            }
                        }

                        transaction.Commit();
                        _cart.ClearCart();
                        MessageBox.Show("¡Pedido realizado con éxito!", "Confirmación",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        if (ex is MySqlException me)
                            MessageBox.Show($"Error DB: {me.Message}\nSQLSTATE: {me.SqlState}\nCódigo: {me.Number}",
                                            "Error de Pedido", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        else
                            MessageBox.Show($"Error: {ex.Message}", "Error de Pedido",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void Form10_Load(object sender, EventArgs e)
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
