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
using MySql.Data.MySqlClient;
using System.Data.Common;
using PlantillaDiseñoProyectoFinal_JS.Models;

namespace PlantillaDiseñoProyectoFinal_JS
{
    
    public partial class Form5 : Form
    {
        private Image defaultNoProductImage = null;
        private Conexion dbConexion;

        private readonly List<string> tiposTejidosDisponibles = new List<string>
        {
            "Bufanda", "Gorro", "Amigurumi", "Bolso", "Guante", "Collar", "Pulsera"
        };

        private readonly List<string> coloresDisponibles = new List<string>
        {
            "Negro", "Blanco", "Beige", "Verde", "Azul", "Rojo", "Amarillo", "Anaranjado", "Rosado", 
            "Combinado"
        };

        public Form5()
        {
            InitializeComponent();

            dbConexion = new Conexion();

            menuStrip1.RenderMode = ToolStripRenderMode.System;
            ToolStripManager.VisualStylesEnabled = false;

            LoadDefaultImage();

            SetupDataGridViewColumns();
            CargarFiltrosIniciales();
            CargarProductosDesdeMySQL();

            this.dataGridView1.CellDoubleClick += new DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick_1);

            btnAplicarFiltros.Click += btnAplicarFiltros_Click;
            btnLimpiarFiltros.Click += btnLimpiarFiltros_Click;

        }

        private void Form5_Load(object sender, EventArgs e)
        {
            this.Location = new Point(0, 0);
            EventosPersonalizados(menuStrip1.Items);
        }

        private void LoadDefaultImage()
        {
            string defaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "no_image_available.jpg"); 
            if (File.Exists(defaultImagePath))
            {
                try
                {
                    using (var fs = new FileStream(defaultImagePath, FileMode.Open, FileAccess.Read))
                    {
                        defaultNoProductImage = Image.FromStream(fs);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar la imagen por defecto: {ex.Message}", "Error de Imagen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    defaultNoProductImage = null; 
                }
            }
            else
            {
                MessageBox.Show($"Imagen por defecto no encontrada: {defaultImagePath}. Asegúrate de que el archivo existe y su 'Copy to Output Directory' esté configurado.", "Advertencia de Imagen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                defaultNoProductImage = null; 
            }
        }

        private void CargarFiltrosIniciales()
        {
            
            clbTiposTejidos.Items.Clear();
            foreach (string tipo in tiposTejidosDisponibles)
            {
                clbTiposTejidos.Items.Add(tipo);
            }

            clbColores.Items.Clear();
            foreach (string color in coloresDisponibles)
            {
                clbColores.Items.Add(color);
            }
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

                case "Menu_Registro":
                    this.Hide();
                    using (var formRegistro = new Form3())
                    {
                        formRegistro.ShowDialog();
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

        private void SetupDataGridViewColumns()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.AutoGenerateColumns = false; 

            // Columna de ID
            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
            colId.DataPropertyName = "ProductoID"; 
            colId.HeaderText = "ID Producto";
            colId.Name = "colID";
            colId.Width = 1;
            colId.ReadOnly = true;
            dataGridView1.Columns.Add(colId);

            // Columna de Nombre del Producto
            DataGridViewTextBoxColumn colNombre = new DataGridViewTextBoxColumn();
            colNombre.DataPropertyName = "NombreProducto";
            colNombre.HeaderText = "Producto";
            colNombre.Name = "colNombre";
            colNombre.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colNombre.ReadOnly = true;
            dataGridView1.Columns.Add(colNombre);

            // Columna de Descripción
            DataGridViewTextBoxColumn colDescripcion = new DataGridViewTextBoxColumn();
            colDescripcion.DataPropertyName = "DescripcionProducto";
            colDescripcion.HeaderText = "Descripción";
            colDescripcion.Name = "colDescripcion";
            colDescripcion.Width = 200; 
            colDescripcion.ReadOnly = true;
            dataGridView1.Columns.Add(colDescripcion);

            // Columna de Valor
            DataGridViewTextBoxColumn colValor = new DataGridViewTextBoxColumn();
            colValor.DataPropertyName = "ValorProducto";
            colValor.HeaderText = "Valor";
            colValor.Name = "colValor";
            colValor.Width = 80;
            colValor.ReadOnly = true;
            colValor.DefaultCellStyle.Format = "C"; // Formato de moneda para la visualización
            dataGridView1.Columns.Add(colValor);

            // Columna de Cantidad
            DataGridViewTextBoxColumn colCantidad = new DataGridViewTextBoxColumn();
            colCantidad.DataPropertyName = "CantidadProducto";
            colCantidad.HeaderText = "Cantidad";
            colCantidad.Name = "colCantidad";
            colCantidad.Width = 70;
            colCantidad.ReadOnly = true;
            dataGridView1.Columns.Add(colCantidad);

            // Columna de Fabricante
            DataGridViewTextBoxColumn colFabricante = new DataGridViewTextBoxColumn();
            colFabricante.DataPropertyName = "FabricanteProducto"; 
            colFabricante.HeaderText = "Fabricante";
            colFabricante.Name = "colFabricante";
            colFabricante.Width = 120;
            colFabricante.ReadOnly = true;
            dataGridView1.Columns.Add(colFabricante);

            // Columna de Imagen
            DataGridViewImageColumn colImagen = new DataGridViewImageColumn();
            colImagen.DataPropertyName = "ImagenCargada"; 
            colImagen.HeaderText = "Foto";
            colImagen.Name = "colImagenProducto";
            colImagen.ImageLayout = DataGridViewImageCellLayout.Zoom; // Ajusta la imagen al tamaño de la celda
            colImagen.Width = 100;
            colImagen.ReadOnly = true;
            dataGridView1.Columns.Add(colImagen);

            dataGridView1.RowTemplate.Height = 80; // Ajusta la altura de las filas para las imágenes
        }
        private void CargarProductosDesdeMySQL(List<string> palabrasClaveTejido = null, string palabraClaveColor = null, decimal? precioMin = null, decimal? precioMax = null)
        {
            List<Producto> listaProductos = new List<Producto>();

            using (MySqlConnection connection = dbConexion.Conectar())
            {
                try
                {
                    connection.Open();
                    StringBuilder queryBuilder = new StringBuilder(@"
                        SELECT
                            id_prod,
                            nom_prod,
                            des_prod,
                            val_prod,
                            can_prod,
                            img_prod,
                            fab_prod
                        FROM
                            producto
                        WHERE 1=1 "); 

                    MySqlCommand command = new MySqlCommand();
                    command.Connection = connection;

                    // Filtrar por Tipo de Tejido (palabras clave en nombre o descripción)
                    if (palabrasClaveTejido != null && palabrasClaveTejido.Any())
                    {
                        queryBuilder.Append(" AND (");
                        for (int i = 0; i < palabrasClaveTejido.Count; i++)
                        {
                            string paramName = $"@Tejido{i}";
                            queryBuilder.Append($"nom_prod LIKE {paramName} OR des_prod LIKE {paramName}");
                            command.Parameters.AddWithValue(paramName, $"%{palabrasClaveTejido[i]}%");
                            if (i < palabrasClaveTejido.Count - 1)
                            {
                                queryBuilder.Append(" OR ");
                            }
                        }
                        queryBuilder.Append(")");
                    }

                    // Filtrar por Color (palabra clave en nombre o descripción)
                    if (!string.IsNullOrEmpty(palabraClaveColor) && palabraClaveColor != "Todos")
                    {
                        queryBuilder.Append(" AND (nom_prod LIKE @Color OR des_prod LIKE @Color)");
                        command.Parameters.AddWithValue("@Color", $"%{palabraClaveColor}%");
                    }

                    // Filtrar por Rango de Precios
                    if (precioMin.HasValue)
                    {
                        queryBuilder.Append(" AND val_prod >= @PrecioMin");
                        command.Parameters.AddWithValue("@PrecioMin", precioMin.Value);
                    }
                    if (precioMax.HasValue)
                    {
                        queryBuilder.Append(" AND val_prod <= @PrecioMax");
                        command.Parameters.AddWithValue("@PrecioMax", precioMax.Value);
                    }

                    queryBuilder.Append(" ORDER BY nom_prod;"); // Ordenar los resultados
                    command.CommandText = queryBuilder.ToString();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Producto prod = new Producto();
                            prod.ProductoID = reader.GetInt32("id_prod");
                            prod.NombreProducto = reader.GetString("nom_prod");
                            prod.DescripcionProducto = reader.GetString("des_prod");
                            prod.ValorProducto = reader.GetDecimal("val_prod");
                            prod.CantidadProducto = reader.GetInt32("can_prod");
                            prod.NombreImagen = reader.GetString("img_prod");
                            prod.FabricanteProducto = reader.GetString("fab_prod");

                            // Cargar la imagen desde el archivo
                            string imageFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", prod.NombreImagen);
                            Image loadedImage = defaultNoProductImage;

                            if (File.Exists(imageFullPath))
                            {
                                try
                                {
                                    using (var fs = new FileStream(imageFullPath, FileMode.Open, FileAccess.Read))
                                    {
                                        loadedImage = Image.FromStream(fs);
                                    }
                                }
                                catch (Exception imgEx)
                                {
                                    MessageBox.Show($"Error al cargar imagen '{prod.NombreImagen}': {imgEx.Message}", "Error de Imagen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                            prod.ImagenCargada = loadedImage;

                            listaProductos.Add(prod);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Error al cargar productos desde MySQL: {ex.Message}\nSQLSTATE: {ex.SqlState}\nCódigo de Error: {ex.Number}", "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocurrió un error inesperado al cargar productos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            dataGridView1.DataSource = listaProductos;
        }

        private void btnAplicarFiltros_Click(object sender, EventArgs e)
        {
            // 1. Obtener palabras clave de tipos de tejido seleccionadas
            List<string> palabrasClaveTejido = new List<string>();
            foreach (var item in clbTiposTejidos.CheckedItems)
            {
                if (item is string tipo)
                {
                    palabrasClaveTejido.Add(tipo);
                }
            }

            // 2. Obtener palabra clave de color seleccionada
            string palabraClaveColor = clbColores.SelectedItem?.ToString();

            // 3. Obtener rango de precios
            decimal? precioMin = null;
            decimal? precioMax = null;

            if (!string.IsNullOrWhiteSpace(txtPrecioMin.Text) && decimal.TryParse(txtPrecioMin.Text, out decimal minVal))
            {
                precioMin = minVal;
            }
            if (!string.IsNullOrWhiteSpace(txtPrecioMax.Text) && decimal.TryParse(txtPrecioMax.Text, out decimal maxVal))
            {
                precioMax = maxVal;
            }

            // Validaciones adicionales para precios
            if (precioMin.HasValue && precioMax.HasValue && precioMin > precioMax)
            {
                MessageBox.Show("El precio mínimo no puede ser mayor que el precio máximo.", "Error de Filtro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Llamar a CargarProductosDesdeMySQL con los filtros
            CargarProductosDesdeMySQL(palabrasClaveTejido, palabraClaveColor, precioMin, precioMax);
        }

        private void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            // Limpiar selecciones de los controles de filtro
            foreach (int i in clbTiposTejidos.CheckedIndices)
            {
                clbTiposTejidos.SetItemChecked(i, false);
            }
            clbColores.SelectedIndex = 0; // Seleccionar "Todos"
            txtPrecioMin.Clear();
            txtPrecioMax.Clear();

            // Recargar todos los productos (sin filtros)
            CargarProductosDesdeMySQL();
        }

        private void dataGridView1_CellDoubleClick_1(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dataGridView1.Rows[e.RowIndex];
                try
                {
                    int idProducto = Convert.ToInt32(row.Cells["colID"].Value);
                    string nombreProducto = row.Cells["colNombre"].Value?.ToString();
                    string descripcionProducto = row.Cells["colDescripcion"].Value?.ToString();
                    decimal valorProducto = Convert.ToDecimal(row.Cells["colValor"].Value);
                    int cantidadProducto = Convert.ToInt32(row.Cells["colCantidad"].Value); 
                    string fabricanteProducto = row.Cells["colFabricante"].Value?.ToString();
                    Image imagenProducto = row.Cells["colImagenProducto"].Value as Image; // Obtiene la imagen ya cargada

                    using (Form6 formDetalle = new Form6(idProducto, nombreProducto, descripcionProducto, valorProducto, cantidadProducto, fabricanteProducto, imagenProducto))
                    {
                        this.Hide();
                        formDetalle.ShowDialog(); // Mostrar como diálogo para que sea modal
                        this.Show(); // Mostrar Form5 de nuevo cuando Form6 se cierre
                    }
                }
                catch (FormatException ex)
                {
                    MessageBox.Show($"Error de formato en los datos: {ex.Message}. Asegúrate de que los valores numéricos de la base de datos sean correctos.", "Error de Conversión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (NullReferenceException ex)
                {
                    MessageBox.Show($"Error inesperado de referencia nula: {ex.Message}. Una celda o columna podría estar vacía o no existir. Verifica que todas las columnas de la DB marcadas como NOT NULL realmente contengan datos.", "Error de Referencia Nula", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocurrió un error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

            if (Program.LoggedInUser == null)
            {
                MessageBox.Show("Necesitas iniciar sesión para ver el carrito. Por favor, inicia sesión para continuar.", "Acceso Requerido", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Hide();

                using (Form4 formLogin = new Form4())
                {
                    formLogin.ShowDialog(); 
                }

                this.Show(); // Mostrar Form5 de nuevo
            }
            else
            {
                this.Hide();
                using (Form9 formCart = new Form9())
                {
                    formCart.ShowDialog();
                }
                this.Show();
            }
        }
    }
}

