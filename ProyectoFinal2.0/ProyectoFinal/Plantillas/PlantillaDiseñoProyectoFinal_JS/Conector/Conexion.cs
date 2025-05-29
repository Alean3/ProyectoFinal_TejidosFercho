using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace PlantillaDiseñoProyectoFinal_JS.Conector
{
    public class Conexion
    {
        public MySqlConnection Conectar()
        {
            string connectionString = "Server=localhost;Port=3306;Database=proyectofinal;Uid=root;Pwd=Prueba123;";
            MySqlConnection conexion = new MySqlConnection(connectionString);
            return conexion;
        }
    }
}