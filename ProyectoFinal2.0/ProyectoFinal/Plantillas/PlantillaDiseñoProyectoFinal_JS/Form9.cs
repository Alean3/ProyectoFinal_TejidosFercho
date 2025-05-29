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

namespace PlantillaDiseñoProyectoFinal_JS
{
    public partial class Form9 : Form
    {
        public Form9()
        {
            InitializeComponent();
            SetupCartDataGridView();
            LoadCartItems();

            menuStrip1.RenderMode = ToolStripRenderMode.System;
            ToolStripManager.VisualStylesEnabled = false;

            btnvaciar.Click += btnClearCart_Click;
            btnpago.Click += btnCheckout_Click;

            dataGridViewCart.CellValueChanged += dataGridViewCart_CellValueChanged;
            dataGridViewCart.CellValidating += dataGridViewCart_CellValidating;
            dataGridViewCart.CellContentClick += dataGridViewCart_CellContentClick; 
        }

        private void SetupCartDataGridView()
        {
            dataGridViewCart.Columns.Clear();
            dataGridViewCart.AutoGenerateColumns = false;

            // Nombre del Producto
            DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn();
            colName.DataPropertyName = "NombreProducto";
            colName.HeaderText = "Producto";
            colName.ReadOnly = true;
            colName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCart.Columns.Add(colName);

            // Cantidad (Editable)
            DataGridViewTextBoxColumn colQuantity = new DataGridViewTextBoxColumn();
            colQuantity.DataPropertyName = "Cantidad";
            colQuantity.HeaderText = "Cantidad";
            colQuantity.Name = "colQuantity"; // Dale un nombre para referenciarlo
            colQuantity.Width = 80;
            dataGridViewCart.Columns.Add(colQuantity);

            // Precio Unitario
            DataGridViewTextBoxColumn colUnitPrice = new DataGridViewTextBoxColumn();
            colUnitPrice.DataPropertyName = "ValorProducto";
            colUnitPrice.HeaderText = "Precio Unit.";
            colUnitPrice.ReadOnly = true;
            colUnitPrice.DefaultCellStyle.Format = "C";
            colUnitPrice.Width = 100;
            dataGridViewCart.Columns.Add(colUnitPrice);

            // Subtotal
            DataGridViewTextBoxColumn colSubtotal = new DataGridViewTextBoxColumn();
            colSubtotal.DataPropertyName = "Subtotal";
            colSubtotal.HeaderText = "Subtotal";
            colSubtotal.ReadOnly = true;
            colSubtotal.DefaultCellStyle.Format = "C";
            colSubtotal.Width = 100;
            dataGridViewCart.Columns.Add(colSubtotal);

            // Botón de Acción: Eliminar
            DataGridViewButtonColumn colRemove = new DataGridViewButtonColumn();
            colRemove.HeaderText = "Acción";
            colRemove.Text = "Eliminar";
            colRemove.Name = "colRemove";
            colRemove.UseColumnTextForButtonValue = true;
            colRemove.Width = 80;
            dataGridViewCart.Columns.Add(colRemove);

            // Columna de Imagen (Opcional)
            DataGridViewImageColumn colImage = new DataGridViewImageColumn();
            colImage.DataPropertyName = "ImagenProducto";
            colImage.HeaderText = "Imagen";
            colImage.ImageLayout = DataGridViewImageCellLayout.Zoom;
            colImage.Width = 70;
            colImage.ReadOnly = true;
            dataGridViewCart.Columns.Add(colImage);

            dataGridViewCart.RowTemplate.Height = 70; // Ajustar la altura de las filas para las imágenes
        }

        private void LoadCartItems()
        {
            if (Program.CurrentShoppingCart != null)
            {
                dataGridViewCart.DataSource = null; 
                dataGridViewCart.DataSource = Program.CurrentShoppingCart.Items;
                UpdateTotalAmount();
            }
        }

        private void UpdateTotalAmount()
        {
            if (Program.CurrentShoppingCart == null)
            {
                lblsubtotal.Text = "Subtotal: $0.00";
                lbliva.Text = "IVA (19%): $0.00";
                lbltotalapagar.Text = "Total a Pagar: $0.00";
                return; 
            }

            decimal subtotal = Program.CurrentShoppingCart.TotalAmount;
            decimal ivaRate = 0.19m; 
            decimal ivaAmount = subtotal * ivaRate;
            decimal totalToPay = subtotal + ivaAmount;

            lblsubtotal.Text = $"Subtotal: {subtotal:C}"; 
            lbliva.Text = $"IVA (19%): {ivaAmount:C}";
            lbltotalapagar.Text = $"Total a Pagar: {totalToPay:C}";
        }

        private void btnClearCart_Click(object sender, EventArgs e)
        {
            Program.CurrentShoppingCart.ClearCart();
            LoadCartItems(); 
        }

        private void dataGridViewCart_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Verificar si se hizo clic en el botón "Eliminar"
            if (e.ColumnIndex == dataGridViewCart.Columns["colRemove"].Index && e.RowIndex >= 0)
            {
                var itemToRemove = dataGridViewCart.Rows[e.RowIndex].DataBoundItem as CartItem;
                if (itemToRemove != null)
                {
                    Program.CurrentShoppingCart.RemoveItem(itemToRemove.ProductoID);
                    LoadCartItems(); // Actualizar la cuadrícula
                }
            }
        }

        private void dataGridViewCart_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // Validar cambios de cantidad
            if (dataGridViewCart.Columns[e.ColumnIndex].Name == "colQuantity")
            {
                if (!int.TryParse(e.FormattedValue.ToString(), out int newQuantity) || newQuantity <= 0)
                {
                    e.Cancel = true;
                    MessageBox.Show("La cantidad debe ser un número entero positivo.", "Cantidad Inválida", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridViewCart_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Actualizar la cantidad en el carrito cuando el valor de la celda cambia
            if (dataGridViewCart.Columns[e.ColumnIndex].Name == "colQuantity" && e.RowIndex >= 0)
            {
                var item = dataGridViewCart.Rows[e.RowIndex].DataBoundItem as CartItem;
                if (item != null && int.TryParse(dataGridViewCart.Rows[e.RowIndex].Cells["colQuantity"].Value.ToString(), out int newQuantity))
                {
                    Program.CurrentShoppingCart.UpdateItemQuantity(item.ProductoID, newQuantity);
                    LoadCartItems(); 
                }
            }
        }

        private void btnCheckout_Click(object sender, EventArgs e)
        {
            if (Program.CurrentShoppingCart.Items.Any())
            {
                this.Hide();
                using (Form10 formCheckout = new Form10(Program.CurrentShoppingCart))
                {
                    formCheckout.ShowDialog();
                }
                this.Show();

                // Después del pago, si es exitoso, vaciar el carrito
                if (Program.CurrentShoppingCart.Items.Count == 0) // Asumiendo que el pago lo vacía
                {
                    LoadCartItems();
                }
            }
            else
            {
                MessageBox.Show("Su carrito está vacío. Por favor, añada productos antes de proceder.", "Carrito Vacío", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Form9_Load(object sender, EventArgs e)
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

        private void btnvaciar_Click(object sender, EventArgs e)
        {

        }

        private void btnpago_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
