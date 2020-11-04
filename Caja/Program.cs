namespace Caja
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class Program
    {
        private static void Main(string[] args)
        {
            MongoDBAsync();
            Sqlserver();
        }

        private static void MongoDBAsync()
        {
            var ubicacionFicheroClientes = @"C:\Users\Basti\Downloads\Sigg\Sigg\Programacion\Programacion\Caja\Caja\clientes.csv";
            var ubicacionFicheroMercaderias = @"C:\Users\Basti\Downloads\Sigg\Sigg\Programacion\Programacion\Caja\Caja\mercaderias.csv";
            var lineasClientes = File.ReadAllLines(ubicacionFicheroClientes);
            var lineasMercaderias = File.ReadAllLines(ubicacionFicheroMercaderias);
            var caja = new Caja();
            caja.LoadClientDataMongoDB(lineasClientes.ToList(), lineasMercaderias.ToList());
            Task task = caja.Update_Info_Client_MongoAsync("Valeda", "Bastian");
            task.Wait();
            caja.Delete_Values_MongoDB(16);
            string precio = "662.76";
            caja.Insert_Values_MongoDB(16, "Rice Wine - Aji Mirin", 16, decimal.Parse(precio));
            Console.ReadLine();
        }

        private static void Sqlserver()
        {
            var ubicacionFicheroClientes = @"C:\Users\Basti\Downloads\Sigg\Sigg\Programacion\Programacion\Caja\Caja\clientes.csv";
            var ubicacionFicheroMercaderias = @"C:\Users\Basti\Downloads\Sigg\Sigg\Programacion\Programacion\Caja\Caja\mercaderias.csv";
            var lineasClientes = File.ReadAllLines(ubicacionFicheroClientes);
            var lineasMercaderias = File.ReadAllLines(ubicacionFicheroMercaderias);
            var caja = new Caja();
            caja.LoadClientData(lineasClientes.ToList(), lineasMercaderias.ToList());
            string precio = "662.76";
            precio.Replace(".", ",");
            caja.Insert_Product(16, "Rice Wine - Aji Mirin", 16, decimal.Parse(precio));
        }
    }
}
