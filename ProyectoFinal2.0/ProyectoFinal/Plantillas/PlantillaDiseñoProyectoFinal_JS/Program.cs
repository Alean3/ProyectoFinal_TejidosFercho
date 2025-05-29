using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PlantillaDiseñoProyectoFinal_JS.Models;

namespace PlantillaDiseñoProyectoFinal_JS
{
    internal static class Program
    {
        public static ShoppingCart CurrentShoppingCart { get; set; }

        public static Usuario LoggedInUser { get; set; }

        public static void SetLoggedInUser(Usuario user)
        {
            LoggedInUser = user;
        }

        public static void ClearLoggedInUser()
        {
            LoggedInUser = null;
        }

        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            CurrentShoppingCart = new ShoppingCart();
            Application.Run(new Form1());
        }

    }
}
