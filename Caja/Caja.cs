namespace Caja
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public class Caja
    {
        public Caja()
        {
            this.Clientes = new Queue<Cliente>();
        }

        public string Id { get; set; }

        private Queue<Cliente> Clientes { get; set; }

        private readonly string connString = @"Server =.\SQLEXPRESS; Database = caja; Trusted_Connection = True;";

        private readonly MongoClient client = new MongoClient("mongodb://127.0.0.1:27017");

        public void Atender()
        {
            if (this.Clientes.Any())
            {
                Console.WriteLine("----- ATENCION -------");
                var primerCliente = this.Clientes.Dequeue();
                Console.WriteLine("Cliente: " + primerCliente.Nombre);
                Console.WriteLine("RUT: " + primerCliente.Id);
                decimal suma = 0;
                foreach (var mercaderia in primerCliente.Mercaderias)
                {
                    suma += mercaderia.Precio;
                }

                Console.WriteLine("Total: " + suma);
            }
            else
            {
                Console.WriteLine("----- CAJA DESOCUPADA -------");
            }
        }

        public void CargarDatosClientes(List<string> lineasClientes, List<string> lineasMercaderias)
        {
            for (int i = 1; i < lineasClientes.Count; i++)

            {
                var lineaCliente = lineasClientes[i];
                Cliente cliente = new Cliente
                {
                    Id = int.Parse(lineaCliente.Split(',')[0]),
                    Nombre = lineaCliente.Split(',')[1]
                };
                this.Clientes.Enqueue(cliente);
                try
                {
                    using (SqlConnection conn = new SqlConnection(this.connString))
                    {
                        string query = @"INSERT INTO client(id, first_name) values('"+cliente.Id +"','"+cliente.Nombre+"');";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        conn.Open();
                        SqlDataReader dr = cmd.ExecuteReader();
                        dr.Close();
                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }
            }

            for (int i = 1; i < lineasMercaderias.Count; i++)
            {
                var mercaderia = new Mercaderia();

                var lineaMercaderia = lineasMercaderias[i];
                var lineaSeparadaPorComillas = lineaMercaderia.Split(',');
                int product_id = int.Parse(lineaSeparadaPorComillas[0]);
                mercaderia.Nombre = lineaSeparadaPorComillas[1];
                mercaderia.Precio = decimal.Parse(lineaSeparadaPorComillas[3].Split('$')[1].Replace(".", ","));
                var clienteDeEstaMercaderia = int.Parse(lineaSeparadaPorComillas[2]);
                try
                {
                    using (SqlConnection conn = new SqlConnection(this.connString))
                    {
                        string query = @"INSERT INTO products(product_id, product_name,client_id,product_price) values('" + product_id + "','" + mercaderia.Nombre + "', '"+clienteDeEstaMercaderia+"','"+mercaderia.Precio+"');";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        conn.Open();
                        SqlDataReader dr = cmd.ExecuteReader();
                        dr.Close();
                        conn.Close();
                        Console.WriteLine("DATOS CARGADOS EXITOSAMENTE");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }

                foreach (var cliente in this.Clientes)
                {
                    if (cliente.Id == clienteDeEstaMercaderia)
                    {
                        cliente.Mercaderias.Add(mercaderia);
                    }
                }
            }
        }

        public void Actualizar_Precio(int product_id, decimal product_price)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(this.connString))
                {
                    string query = @"UPDATE products SET product_price= '"+product_price+"' WHERE product_id='"+product_id+"';";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    Console.WriteLine("SE ACTUALIZO EL PRODUCTO ID:"+product_id+"por el precio:"+product_price);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }

        public void Remover_Producto(int product_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(this.connString))
                {
                    string query = @"DELETE products WHERE product_id='"+product_id+"';";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    Console.WriteLine("SE ELIMINO EL PRODUCTO CON EL ID:"+product_id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }

        public void Insertar_Producto(int product_id, string product_name, int client_id, decimal product_price)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(this.connString))
                {
                    string query = @"INSERT INTO products(product_id, product_name, client_id, product_price) values('"+product_id+"','"+product_name+"','"+client_id+"','"+product_price+"');";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dr.Close();
                    conn.Close();
                    Console.WriteLine("PRODUCTO AGREGADO. DATOS : ID:"+product_id+"Product_name:"+product_name+"client_id:"+client_id+"product_price:"+product_name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:"+ex.Message);
            }
        }
        
        public void CargarDatosClientesMongoDB(List<string> lineasClientes, List<string> lineasMercaderias)
        {
            var contador = 0;
            for (int i = 1; i < lineasClientes.Count; i++)
            {  
                var lineaCliente = lineasClientes[i];
                var cliente = new Cliente
                {
                    Id = int.Parse(lineaCliente.Split(',')[0]),
                    Nombre = lineaCliente.Split(',')[1]
                };
                this.Clientes.Enqueue(cliente);
                try
                {
                    var db = this.client.GetDatabase("Caja");
                    var collection = db.GetCollection<Cliente>("cliente");
                    collection.InsertOneAsync(new Cliente { Id = cliente.Id, Nombre = cliente.Nombre });
                    contador++;
                    Console.WriteLine(contador);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }   
            }

            for (int i = 1; i < lineasMercaderias.Count; i++)
            {
                var mercaderia = new Mercaderia();

                var lineaMercaderia = lineasMercaderias[i];
                var lineaSeparadaPorComillas = lineaMercaderia.Split(',');
                int producto_id = int.Parse(lineaSeparadaPorComillas[0]);
                mercaderia.Nombre = lineaSeparadaPorComillas[1];
                mercaderia.Precio = decimal.Parse(lineaSeparadaPorComillas[3].Split('$')[1].Replace(".", ","));
                var clienteDeEstaMercaderia = int.Parse(lineaSeparadaPorComillas[2]);
                try
                {
                    var db = this.client.GetDatabase("Caja");
                    var collection = db.GetCollection<Mercaderia>("producto");
                    collection.InsertOneAsync(new Mercaderia { Id = producto_id, Nombre = mercaderia.Nombre, Precio = mercaderia.Precio });
                }
                
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }

                foreach (var cliente in this.Clientes)
                {
                    if (cliente.Id == clienteDeEstaMercaderia)
                    {
                        cliente.Mercaderias.Add(mercaderia);
                    }                       
                }
            }
        }

        public void Actualizar_Nombre_Cliente_MongoDB(string nombre_actual, string nombre_nuevo)
        {
            var db = this.client.GetDatabase("Caja");
            var collection_cliente = db.GetCollection<Cliente>("cliente");
            var buscar_nombre_actual = Builders<Cliente>.Filter.Eq("Nombre", nombre_actual);
            collection_cliente.UpdateOneAsync(buscar_nombre_actual,nombre_nuevo);
            
            
        }

        public void Eliminar_Producto_MongoDB(int id_producto)
        {
            var db = this.client.GetDatabase("Caja");
            var collection_producto = db.GetCollection<Mercaderia>("producto");
            var borrar_producto = Builders<Mercaderia>.Filter.Eq("_id", id_producto);
            collection_producto.DeleteOne(borrar_producto);
        }

        public void Insertar_Producto_MongoDB(int id_producto, string nombre_producto, decimal precio_producto)
        {
            var db = this.client.GetDatabase("Caja");
            var collection_producto = db.GetCollection<Mercaderia>("producto");
            collection_producto.InsertOneAsync(new Mercaderia { Id= id_producto, Nombre = nombre_producto, Precio = precio_producto });
        }
    }

}