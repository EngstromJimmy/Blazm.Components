using System.Threading.Tasks;

namespace Blazm.Components
{
    internal interface IGridContainer
    {
        void AddColumn(IGridColumn column);

        void RemoveColumn(IGridColumn column);

        Task Sort(IGridColumn column);
        string? SortField
        {
            get;
            set;
        }

        bool Sortable
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
