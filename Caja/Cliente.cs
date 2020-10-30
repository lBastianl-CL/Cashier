using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Caja
{
    public class Cliente
    {
        public Cliente()
        {
            Mercaderias = new List<Mercaderia>();
        }

        public List<Mercaderia> Mercaderias { get; set; }

        public int RUT { get; set; }

        public string Nombre { get; set; }
    }
}
