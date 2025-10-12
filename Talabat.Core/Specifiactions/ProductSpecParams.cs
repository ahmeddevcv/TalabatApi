using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Specifiactions
{
    public class ProductSpecParams
    {
        //public int PageSize { get; set; }
        //public int PageIndex { get; set; }
        private const int MaxPageSize= 10;
        private int pageSize=5;//I put default values not to hsve Error
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = value > 10 ? MaxPageSize : value; }
        }
        public int PageIndex { get; set; } = 1;


        //sorting
        public string? Sort {  get; set; }
        public int? BrandId { get; set; }
        public int? TypeId { get; set; }

        //searching
        private string? search;
        public string? Search
        {
            get { return search; }
            set { search = value.ToLower(); }
        }


    }
}
