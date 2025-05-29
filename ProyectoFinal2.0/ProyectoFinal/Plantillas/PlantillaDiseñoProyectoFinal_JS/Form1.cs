using MySql.Data.MySqlClient;
using PlantillaDiseñoProyectoFinal_JS.Conector;
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
    public partial class Form1 : Form
    {
        private List<Image> Carrusel = new List<Image>();
        private int IndiceActual = 0;
        private Timer Temporizador = new Timer();
        private Conexion dbConexion;
        public Form1()
        {
            InitializeComponent();

            dbConexion = new Conexion();
            InitializeDatabase();

            menuStrip1.RenderMode = ToolStripRenderMode.System;
            ToolStripManager.VisualStylesEnabled = false;

            Carrusel.Add(Image.FromFile("Carrusel1.png"));
            Carrusel.Add(Image.FromFile("Carrusel2.png"));
            Carrusel.Add(Image.FromFile("Carrusel3.png"));

            Carrusel_Interactivo.SizeMode = PictureBoxSizeMode.StretchImage;
            Carrusel_Interactivo.Image = Carrusel[IndiceActual];

            Temporizador.Interval = 2500;
            Temporizador.Tick += CambioAutomatico;
            Temporizador.Start();
        }

        private void InitializeDatabase()
        {
            using (MySqlConnection connection = dbConexion.Conectar())
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("Conexión a la base de datos MySQL abierta exitosamente.");

                    // Lista de tablas a crear en el orden de sus dependencias (sin FK primero)
                    string[] tablascreadas = { "producto", "usuario", "pedido", "detalle_pedido", "carrito_temporal" };

                    foreach (string tabla in tablascreadas)
                    {
                        using (MySqlCommand cmd = connection.CreateCommand())
                        {
                            string createTableSql = "";

                            if (tabla == "producto")
                            {
                                createTableSql = @"
                            CREATE TABLE `proyectofinal`.`producto` (
                                `id_prod` INT NOT NULL AUTO_INCREMENT,  
                                `nom_prod` VARCHAR(255) NOT NULL,  
                                `des_prod` TEXT NOT NULL,   
                                `val_prod` DECIMAL(10,2) NOT NULL,   
                                `can_prod` INT NOT NULL,   
                                `img_prod` VARCHAR(255) NOT NULL,   
                                `fab_prod` VARCHAR(45) NOT NULL,   
                                PRIMARY KEY (`id_prod`)
                            )";
                            }
                            else if (tabla == "usuario")
                            {
                                createTableSql = @"
                            CREATE TABLE `proyectofinal`.`usuario` (
                                `id_usu` INT NOT NULL AUTO_INCREMENT,
                                `user_usu` VARCHAR(50) NOT NULL UNIQUE,      
                                `nom_usu` VARCHAR(100) NOT NULL,             
                                `ape_usu` VARCHAR(100) NOT NULL,           
                                `dir_usu` VARCHAR(255) NOT NULL,     
                                `tel_usu` VARCHAR(20),             
                                `ciu_usu` VARCHAR(100),              
                                `emai_usu` VARCHAR(255) NOT NULL UNIQUE,     
                                `con_usu` VARCHAR(255) NOT NULL,     
                                PRIMARY KEY (`id_usu`)
                            )";
                            }
                            else if (tabla == "pedido")
                            {
                                createTableSql = @"
                            CREATE TABLE `proyectofinal`.`pedido` (
                                `id_ped` INT NOT NULL AUTO_INCREMENT,
                                `id_usu` INT,                          
                                `fec_ped` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                `est_ped` VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
                                `tot_ped` DECIMAL(10,2) NOT NULL,
                                `dir_env` VARCHAR(255) NOT NULL,
                                `ciu_env` VARCHAR(100) NOT NULL,
                                `te_env` VARCHAR(20),
                                `cor_env` VARCHAR(255) NOT NULL,
                                PRIMARY KEY (`id_ped`),
                                FOREIGN KEY (`id_usu`) REFERENCES `proyectofinal`.`usuario`(`id_usu`)
                                    ON UPDATE CASCADE ON DELETE SET NULL
                            )";
                            }
                            else if (tabla == "detalle_pedido")
                            {
                                createTableSql = @"
                            CREATE TABLE `proyectofinal`.`detalle_pedido` (
                                `id_det` INT NOT NULL AUTO_INCREMENT,
                                `id_ped` INT NOT NULL,                   
                                `id_prod` INT NOT NULL,                  
                                `can_prod_det` INT NOT NULL,
                                `precuni_det` DECIMAL(10,2) NOT NULL,  
                                PRIMARY KEY (`id_det`),
                                FOREIGN KEY (`id_ped`) REFERENCES `proyectofinal`.`pedido`(`id_ped`)
                                    ON UPDATE CASCADE ON DELETE CASCADE,
                                FOREIGN KEY (`id_prod`) REFERENCES `proyectofinal`.`producto`(`id_prod`)
                                    ON UPDATE CASCADE ON DELETE RESTRICT
                            )";
                            }
                            else if (tabla == "carrito_temporal")
                            {
                                createTableSql = @"
                            CREATE TABLE `proyectofinal`.`carrito_temporal` (
                                `id_ses` VARCHAR(255) NOT NULL,
                                `id_prod` INT NOT NULL,
                                `can_prod_ses` INT NOT NULL,
                                `fec_cre` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                `fec_act` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                                PRIMARY KEY (`id_ses`, `id_prod`),  
                                FOREIGN KEY (`id_prod`) REFERENCES `proyectofinal`.`producto`(`id_prod`)
                                    ON UPDATE CASCADE ON DELETE CASCADE
                            )";
                            }

                            if (!string.IsNullOrEmpty(createTableSql))
                            {
                                cmd.CommandText = createTableSql;
                                try
                                {
                                    cmd.ExecuteNonQuery();
                                    Console.WriteLine($"Tabla '{tabla}' creada exitosamente.");
                                }
                                catch (MySqlException ex)
                                {
                                    // MySQL error code for "table already exists" is 1050
                                    if (ex.Number == 1050)
                                    {
                                        Console.WriteLine($"La tabla '{tabla}' ya existe.");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Error al crear la tabla '{tabla}': {ex.Message}");
                                        throw; // Relanza otras excepciones de MySQL
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Advertencia: No se encontró la definición SQL para la tabla '{tabla}'.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al inicializar la base de datos: {ex.Message}");
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Location = new Point(0, 0);

            EventosPersonalizados(menuStrip1.Items);
        }
        private void CambioAutomatico(object sender, EventArgs e)
        {
            IndiceActual = (IndiceActual + 1) % Carrusel.Count;
            Carrusel_Interactivo.Image = Carrusel[IndiceActual];
        }
        private void btnsiguiente_Click(object sender, EventArgs e)
        {
            IndiceActual = (IndiceActual + 1) % Carrusel.Count;
            Carrusel_Interactivo.Image = Carrusel[IndiceActual];
        }

        private void btnatras_Click(object sender, EventArgs e)
        {
            IndiceActual = (IndiceActual - 1 + Carrusel.Count) % Carrusel.Count;
            Carrusel_Interactivo.Image = Carrusel[IndiceActual];
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
        
    }
}
