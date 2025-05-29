using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlantillaDiseñoProyectoFinal_JS.Models
{
    public class Producto
    {
        public int ProductoID { get; set; } // id_prod
        public string NombreProducto { get; set; } // nom_prod
        public string DescripcionProducto { get; set; } // des_prod
        public decimal ValorProducto { get; set; } // val_prod
        public int CantidadProducto { get; set; } // can_prod
        public string NombreImagen { get; set; } // img_prod (el nombre del archivo de imagen)
        public string FabricanteProducto { get; set; } // fab_prod

        public Image ImagenCargada { get; set; }
    }

    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string UsuarioNombre { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string DireccionEnvio { get; set; }
        public string Ciudad { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
}

    public class CartItem
    {
        public int ProductoID { get; set; }
        public string NombreProducto { get; set; }
        public decimal ValorProducto { get; set; }
        public int Cantidad { get; set; }
        public Image ImagenProducto { get; set; } 
        
        public decimal Subtotal => Cantidad * ValorProducto;
    }

    public class ShoppingCart
    {
        private List<CartItem> _items;

        public ShoppingCart()
        {
            _items = new List<CartItem>();
        }

        public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

        public decimal TotalAmount => _items.Sum(item => item.Subtotal);

        public void AddItem(Producto product, int quantity)
        {
            if (quantity <= 0) return;

            var existingItem = _items.FirstOrDefault(item => item.ProductoID == product.ProductoID);

            if (existingItem != null)
            {
                // Verificar si añadir más excedería el stock disponible
                if (existingItem.Cantidad + quantity > product.CantidadProducto)
                {
                    MessageBox.Show($"No hay suficiente stock para añadir {quantity} unidades de '{product.NombreProducto}'. Stock disponible: {product.CantidadProducto - existingItem.Cantidad}.", "Stock Insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                existingItem.Cantidad += quantity;
            }
            else
            {
                // Verificar stock inicial para un nuevo artículo
                if (quantity > product.CantidadProducto)
                {
                    MessageBox.Show($"No hay suficiente stock para añadir {quantity} unidades de '{product.NombreProducto}'. Stock disponible: {product.CantidadProducto}.", "Stock Insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _items.Add(new CartItem
                {
                    ProductoID = product.ProductoID,
                    NombreProducto = product.NombreProducto,
                    ValorProducto = product.ValorProducto,
                    Cantidad = quantity,
                    ImagenProducto = product.ImagenCargada, // Usar la imagen ya cargada
                });
            }
            MessageBox.Show($"'{product.NombreProducto}' x {quantity} añadido(s) al carrito.", "Producto Añadido", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void UpdateItemQuantity(int productId, int newQuantity)
        {
            var item = _items.FirstOrDefault(i => i.ProductoID == productId);
            if (item != null)
            {
                if (newQuantity <= 0)
                {
                    RemoveItem(productId);
                }
                else
                {
                    item.Cantidad = newQuantity;
                }
            }
        }

        public void RemoveItem(int productId)
        {
            _items.RemoveAll(item => item.ProductoID == productId);
            MessageBox.Show("Producto eliminado del carrito.", "Producto Eliminado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ClearCart()
        {
            _items.Clear();
            MessageBox.Show("El carrito ha sido vaciado.", "Carrito Vaciado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
