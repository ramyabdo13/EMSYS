using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Vml;
using EMSYS.Models;

namespace EMSYS.Utils
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalItems { get; private set; }
        public string SearchMessage { get; private set; }
        public List<ColumnHeader> ColumnHeaders { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize, int total, List<ColumnHeader> headers, string searchMessage)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            PageSize = pageSize;
            TotalItems = total;
            ColumnHeaders = headers;
            SearchMessage = searchMessage;
            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;

        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize, int total, List<ColumnHeader> headers, string searchMessage)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize, total, headers, searchMessage);
        }
    }

}
