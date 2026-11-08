using Talabat.APIs.Dtos;

namespace Talabat.APIs.Helpers
{
    public class Pagination<T>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PagesCount { get; set; }
        public int CounOfData { get; set; }
        public IReadOnlyList<T> Data { get; set; }
        public Pagination(int pageIndex,int pageSize,int count,int counOfData, IReadOnlyList<T> data)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            Data = data;
            PagesCount = count;
            CounOfData = counOfData;
        }
    }
}
