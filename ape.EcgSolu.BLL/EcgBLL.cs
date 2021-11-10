//-------------------------------------------------------------------
//Ecgs表的业务逻辑层
//-------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ape.EcgSolu.Model;
using ape.EcgSolu.IDAL;
using ape.EcgSolu.DAL;

namespace ape.EcgSolu.BLL
{
    public class EcgBLL
    {
        EcgDAL ecgDAL = new EcgDAL();

        public List<Ecg> GetAllList()
        {
            return ecgDAL.GetAllList();
        }

        public List<Ecg> GetPagedList(int pageIndex, int pageSize)
        {
            return ecgDAL.GetPagedList(pageIndex, pageSize);
        }

        public Ecg GetById(Guid id)
        {
            return ecgDAL.GetById(id);
        }

        public bool InsertEcg(Ecg ecgEntity)
        {
            return ecgDAL.Insert(ecgEntity);
        }

        public int GetRowsCount()
        {
            return ecgDAL.GetRowsCount();
        }

        public bool UpdateResult(Ecg ecgEntity)
        {
            return ecgDAL.UpdateResult(ecgEntity);
        }

        public bool DeleteByid(Guid id)
        {
            return ecgDAL.DeleteByid(id);
        }

        public List<Ecg> GetPagedQueryList(int pageIndex, int pageSize, QueryArg queryArg)
        {
            return ecgDAL.GetPagedQueryList(pageIndex, pageSize, queryArg);
        }
    }
}
