using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ape.EcgSolu.IDAL
{
    public interface IDAL<T> where T:class
    {
        List<T> GetAllList();
        List<T> GetPagedList(int pageIndex, int pageSize);
        T GetById(Guid id);
        bool Insert(T entity);
        bool Update(T entity);
        bool DeleteByid(Guid id);
        int GetRowsCount();
    }
}
