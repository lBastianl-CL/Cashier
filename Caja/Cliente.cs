namespace Caja
{
    using System.Collections.Generic;

    public class Cliente
    {
        public Cliente()
        {
            this.Mercaderias = new List<Mercaderia>();
        }

        public List<Mercaderia> Mercaderias { get; set; }

        public int Id { get; set; }

        public string Nombre { get; set; } 
    }
}
