using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ape.EcgSolu.Model;
using ape.EcgSolu.DAL;

namespace ape.EcgSolu.BLL
{
    public class DiagWordBLL
    {
        public List<DiagWord> GetDiagWord()
        {
            return new DiagWordDAL().GetDiagWord();
        }

        public void InitWord()
        {
            new DiagWordDAL().InitWord();
        }
    }
}
