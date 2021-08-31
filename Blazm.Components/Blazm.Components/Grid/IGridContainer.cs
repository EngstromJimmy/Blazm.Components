using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blazm.Components
{
    internal interface IGridContainer
    {
        void AddColumn(IGridColumn column);

        void RemoveColumn(IGridColumn column);

        Task Sort(IGridColumn column);

        Task ApplyFilter();
        string SortField
        {
            get;
            set;
        }

        bool UseVirtualize
        {
            get;
            set;
        }

        bool Sortable
        {
            get;
            set;
        }

        bool ShowFilter
        {
            get;
            set;
        } 


        System.ComponentModel.ListSortDirection SortDirection
        {
            get; set;
        }

    }
}
