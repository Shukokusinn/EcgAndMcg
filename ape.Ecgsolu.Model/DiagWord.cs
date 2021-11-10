using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ape.EcgSolu.Model
{
    public class DiagWord
    {
        public Guid Id { get; set; }
        public string Word { get; set; }
        public string Category { get; set; }
        public int UseCount { get; set; }
    }
}
