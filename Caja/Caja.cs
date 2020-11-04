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
                Console.WriteLine("RUT: " + primerCliente.RUT);
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

        public void LoadClientData(List<string> lineasClientes, List<string> lineasMercaderias)
        {
            for (int i = 1; i < lineasClientes.Count; i++)

            {
                var lineaCliente = lineasClientes[i];
                Cliente cliente = new Cliente
                {
                    RUT = int.Parse(lineaCliente.Split(',')[0]),
                    Nombre = lineaCliente.Split(',')[1]
                };
                this.Clientes.Enqueue(cliente);
                try
                {
                    using (SqlConnection conn = new SqlConnection(this.connString))
                    {
                        string query = @"INSERT INTO client(id, first_name) values('"+cliente.RUT +"','"+cliente.Nombre+"');";
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
                    if (cliente.RUT == clienteDeEstaMercaderia)
                    {
                        cliente.Mercaderias.Add(mercaderia);
                    }
                }
            }
        }

        public void Update_price(int product_id, decimal product_price)
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

        public void Remove_product(int product_id)
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

        public void Insert_Product(int product_id, string product_name, int client_id, decimal product_price)
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
        
        public void LoadClientDataMongoDB(List<string> lineasClientes, List<string> lineasMercaderias)
        {
            var contador = 0;
            for (int i = 1; i < lineasClientes.Count; i++)
            {  
                var lineaCliente = lineasClientes[i];
                var cliente = new Cliente
                {
                    RUT = int.Parse(lineaCliente.Split(',')[0]),
                    Nombre = lineaCliente.Split(',')[1]
                };
                this.Clientes.Enqueue(cliente);
                try
                {
                    var db = this.client.GetDatabase("caja");
                    var collect = db.GetCollection<BsonDocument>("client");
                    var info = new BsonDocument
                    {
                        { "_id", cliente.RUT },
                        { "first_name", cliente.Nombre }
                    };
                    collect.InsertOneAsync(info);
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
                int product_id = int.Parse(lineaSeparadaPorComillas[0]);
                mercaderia.Nombre = lineaSeparadaPorComillas[1];
                mercaderia.Precio = decimal.Parse(lineaSeparadaPorComillas[3].Split('$')[1].Replace(".", ","));
                var clienteDeEstaMercaderia = int.Parse(lineaSeparadaPorComillas[2]);
                try
                {
                    var db = this.client.GetDatabase("caja");
                    var collect = db.GetCollection<BsonDocument>("products");
                    var info = new BsonDocument
                    {
                        { "_id", product_id },
                        { "product_name", mercaderia.Nombre },
                        { "client_id", clienteDeEstaMercaderia },
                        { "product_price", mercaderia.Precio }
                    };
                    collect.InsertOneAsync(info);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }

                foreach (var cliente in this.Clientes)
                {
                    if (cliente.RUT == clienteDeEstaMercaderia)
                    {
                        cliente.Mercaderias.Add(mercaderia);
                    }                       
                }
            }
        }

        public async Task Update_Info_Client_MongoAsync(string name_actual, string name_new)
        {
            var db = this.client.GetDatabase("caja");
            var collection = db.GetCollection<BsonDocument>("client");
            var result = await collection.FindOneAndUpdateAsync(Builders<BsonDocument>.Filter.Eq("first_name", name_actual), Builders<BsonDocument>.Update.Set("first_name", name_new));
            await collection.Find(new BsonDocument()).ForEachAsync(x => Console.WriteLine(x));
        }

        public void Delete_Values_MongoDB(int product_id)
        {
            var db = this.client.GetDatabase("caja");
            var collection = db.GetCollection<BsonDocument>("products");
            var delete = Builders<BsonDocument>.Filter.Eq("_id", product_id);
            collection.DeleteOne(delete);
        }

        public void Insert_Values_MongoDB(int product_id, string product_name, int client_id, decimal product_price)
        {
            var db = this.client.GetDatabase("caja");
            var collect = db.GetCollection<BsonDocument>("products");
            var info = new BsonDocument
            {
                { "_id", product_id },
                { "product_name", product_name },
                { "client_id", client_id },
                { "product_price", product_price }       
            };
            collect.InsertOneAsync(info);
        }
    }
}
