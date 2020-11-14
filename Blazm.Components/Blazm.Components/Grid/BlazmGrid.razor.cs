using BlazorPro.BlazorSize;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using NPOI.HSSF.Record.PivotTable;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Blazm.Components
{
    public partial class BlazmGrid<TItem>: IGridContainer,IDisposable
    {
        #region Parameters
        [Parameter]
        public bool AutoGenerateColumns { get; set; } = false;

        [Parameter]
        public string NextText { get; set; } = "Next";

        [Parameter]
        public string PreviousText { get; set; } = "Previous";
        [Parameter]
        public EventCallback<List<TItem>> SelectedDataChanged { get; set; }

        [Parameter]
        public int PageSize
        {
            get;
            set;
        } = 0;

        [Parameter]
        public int CurrentPage
        {
            get;
            set;
        } = 0;

        [Parameter]
        public bool ShowCheckbox
        {
            get;
            set;
        } = false;

        [Parameter]
        public bool ShowGroupFooter
        {
            get;
            set;
        } = false;

        [Parameter]
        public bool ShowFooter
        {
            get;
            set;
        } = false;

        [Parameter]
        public bool Sortable
        {
            get;
            set;
        } = false;

        [Parameter]
        public string SortField
        {
            get;
            set;
        } = null;

        [Parameter]
        public bool ShowPageCounter
        {
            get;
            set;
        } = true;


        [Parameter]
        public string GroupSortField
        {
            get;
            set;
        } = null;

        [Parameter]
        public System.ComponentModel.ListSortDirection SortDirection
        {
            get; set;
        } = System.ComponentModel.ListSortDirection.Ascending;

        [Parameter]
        public RenderFragment GridColumns
        {
            get;
            set;
        }


        [Parameter]
        public RenderFragment EmptyGridTemplate
        {
            get;
            set;
        }

        [Parameter]
        public RenderFragment NullGridTemplate
        {
            get;
            set;
        }




        [Parameter]
        public RenderFragment<object> GroupHeader
        {
            get;
            set;
        }


        [Parameter]
        public Func<TItem, object> GroupBy { get; set; } = null;

        [Parameter]
        public ItemsProviderDelegate<TItem> ItemsProvider { get; set; }

        [Parameter]
        public IEnumerable<TItem>? Data { get; set; }

        [Parameter]
        public List<TItem> SelectedData { get; set; } = new List<TItem>();

        #endregion

        [Inject] ResizeListener listener { get; set; }
        [Inject] IJSRuntime jsruntime { get; set; }
        private List<IGridColumn> Columns { get; set; } = new List<IGridColumn>();
        private List<IGridColumn> AllColumns { get; set; } = new List<IGridColumn>();
        private List<int> ExpandedRows = new List<int>();
        string id = Guid.NewGuid().ToString();
        int ContainerClientWidth { get; set; }
        int TableClientWidth { get; set; }
        private Virtualize<TItem> virtualize { get; set; }
        private IEnumerable<TItem> pagedData { get; set; }

        protected override async  Task OnParametersSetAsync()
        {
            await RefreshDataAsync();
               
            await base.OnParametersSetAsync();
        }


        public async Task  RefreshDataAsync()
        {
            if (virtualize != null && Data != null)
            {
                await virtualize.RefreshDataAsync();
            }
        }

        private void loadPagedData()
        {
            if (Data != null)
            {
                pagedData = Data;

                if (SortField != null)
                {
                    if (SortDirection == ListSortDirection.Descending)
                    {
                        pagedData = pagedData.OrderByDescending(x => x.GetType().GetProperty(SortField).GetValue(x, null)).ToList();
                    }
                    else
                    {
                        pagedData = pagedData.OrderBy(x => x.GetType().GetProperty(SortField).GetValue(x, null)).ToList();
                    }
                }

                if (PageSize != 0)
                {
                    pagedData = pagedData.Skip(PageSize * CurrentPage).Take(PageSize).ToList();
                }
            }
        }

        private async ValueTask<ItemsProviderResult<TItem>> LoadData(ItemsProviderRequest request)
        {
            loadPagedData();
            int totalRows = 0;
            if (pagedData != null)
            {
                totalRows = pagedData.Count();
            }
            var numberofItems = Math.Min(request.Count, totalRows - request.StartIndex);

            return new ItemsProviderResult<TItem>(pagedData.Skip(request.StartIndex).Take(numberofItems), totalRows);
        }


        public async Task ExportDataAsync(string filename, string sheetname = "sheet1", string dateformat = "MM/dd/yyyy HH:mm:ss")
        {
            IWorkbook workbook = new XSSFWorkbook();

            var newDataFormat = workbook.CreateDataFormat();
            var datestyle = workbook.CreateCellStyle();
            datestyle.DataFormat = newDataFormat.GetFormat(dateformat);

            ISheet worksheet = workbook.CreateSheet(sheetname);

            int rownum = 0;

            IRow hrow = worksheet.CreateRow(rownum++);
            int cellnumber = 0;
            foreach (var c in Columns.Where(d => d.Exportable))
            {
                ICell Cell = hrow.CreateCell(cellnumber++);
                Cell.SetCellValue(c.Title);
            }


            foreach (var r in Data)
            {
                IRow row = worksheet.CreateRow(rownum++);
                cellnumber = 0;
                foreach (var column in Columns.Where(d => d.Exportable))
                {
                    ICell Cell = row.CreateCell(cellnumber++);

                    string stringValue = "";

                    if (column.Format == null)
                    {
                        stringValue = r.GetType().GetProperty(column.Field).GetValue(r)?.ToString();
                    }
                    else
                    {
                        stringValue = string.Format(column.Format, r.GetType().GetProperty(column.Field)?.GetValue(r)?.ToString());
                    }

                    switch (r.GetType().GetProperty(column.Field).PropertyType.Name)
                    {

                        case "Int32":
                        case "Int64":
                        case "Int16":
                        case "Decimal":
                        case "Double":
                            Cell.SetCellValue(Convert.ToDouble(stringValue));
                            break;
                        case "DateTime":
                            Cell.CellStyle = datestyle;
                            Cell.SetCellValue(Convert.ToDateTime(stringValue));
                            break;
                        default:
                            Cell.SetCellValue(stringValue);
                            break;
                    }
                }
            }

            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            byte[] bytes = ms.ToArray();
            ms.Close();

            await jsruntime.SaveAsFileAsync(filename, bytes, "application/vnd.ms-excel");
        }


        private async Task selectItemsAsync(List<TItem> items, object selected)
        {
            foreach (var item in items)
            {
                if ((bool)selected)
                {
                    if (!SelectedData.Contains(item))
                    {
                        SelectedData.Add(item);
                    }
                }
                else
                {
                    SelectedData.Remove(item);
                }
            }
            await SelectedDataChanged.InvokeAsync(SelectedData);
            StateHasChanged();
        }

        private async Task selectItemAsync(TItem item, object selected)
        {
            if ((bool)selected)
            {
                SelectedData.Add(item);
            }
            else
            {
                SelectedData.Remove(item);
            }
            await SelectedDataChanged.InvokeAsync(SelectedData);
            StateHasChanged();
        }

        private bool isItemSelected(TItem item)
        {
            return SelectedData.Contains(item);
        }

       

        private string getSignClass(IGridColumn column, string value)
        {
            if (column.HighlightSign)
            {
                try
                {
                    if (value.StartsWith("-"))
                    {
                        return column.ValueNegativeClass;
                    }
                    else
                    {
                        var d = Convert.ToDouble(value);
                        if (d == 0d)
                        {
                            return "";
                        }
                        else
                        {
                            return column.ValuePositiveClass;
                        }
                    }
                }
                catch { }
            }
            return "";
        }


        protected void SetPage(int pageNumber)
        {
            CurrentPage = pageNumber;
        }

        protected async Task PreviousPage()
        {
            if (CurrentPage > 0)
            {
                CurrentPage--;
            }
            await RefreshDataAsync();
        }

        protected async Task  NextPage()
        {
            if ((CurrentPage * PageSize) + PageSize < Data.Count())
            {
                CurrentPage++;
            }
            await RefreshDataAsync();
        }
        
        void IGridContainer.AddColumn(IGridColumn column)
        {

            column.Id = column.Field + column.Template?.GetHashCode() + column.Format?.GetHashCode();

            var col = Columns.FirstOrDefault(c => c.Id == column.Id);
            column.Type = typeof(TItem);
            if (col == null)
            {
                Columns.Add(column);
            }
            //If the grid is grouped we need access to all columns (one set per group)
            AllColumns.Add(column);

            StateHasChanged();
        }

        Task IGridContainer.Sort(IGridColumn column)
        {
            return InvokeAsync(async () =>
            {
                if (Sortable)
                {
                    if (SortField == column.Field && column.CanSort)
                    {
                        if (SortDirection == ListSortDirection.Descending)
                        {
                            SortDirection = ListSortDirection.Ascending;
                        }
                        else
                        {
                            SortDirection = ListSortDirection.Descending;
                        }
                    }
                    else
                    {
                        SortField = column.Field;
                    }
                    await RefreshDataAsync();
                    StateHasChanged();
                }
            });
        }

        void IGridContainer.RemoveColumn(IGridColumn column)
        {
            Columns.Remove(column);
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await RefreshDataAsync();
            if (firstRender)
            {
                listener.OnResized += WindowResized;
            }
            await base.OnAfterRenderAsync(firstRender);
        }

#pragma warning disable 1998
        private async Task resizeTableAsync()
        {
            int reduceby = 0;
            Columns.ForEach((c) => c.Visible = true);

            foreach (var c in Columns.Where(c => c.Priority > 0).OrderByDescending(c => c.Priority))
            {
                if (TableClientWidth - reduceby <= ContainerClientWidth)
                {
                    break;
                }
                foreach (var col in AllColumns.Where(a => a.Id == c.Id))
                {
                    //update all references (needed if grouped)
                    col.Visible = false;
                }
                reduceby += c.ClientWidth;
            }

            StateHasChanged();
        }
#pragma warning restore 1998


        void IDisposable.Dispose()
        {
            listener.OnResized -= WindowResized;
        }

        async void WindowResized(object _, BrowserWindowSize window)
        {
            await ResizeGrid();
        }


        IJSObjectReference resizemodule;
        public async Task ResizeGrid()
        {
            resizemodule = await jsruntime.InvokeAsync<IJSObjectReference>("import", "./_content/Blazm.Components/scripts/ResizeTable.js");
            var size = await resizemodule.InvokeAsync<TableSize>("ResizeTable", id, GroupBy != null);

            if (size != null)
            {
                for (var i = 0; i < size?.Columns?.Length; i++)
                {
                    var counter = i;
                    if (ShowCheckbox || Columns.Any(c => !c.Visible))
                    {
                        //Skip the first one
                        counter = i - 1;
                        if (i == 0)
                        {
                            continue;
                        }
                    }
                    Columns[counter].ClientWidth = size.Columns[i];
                }
                TableClientWidth = size.TableClientWidth;
                ContainerClientWidth = size.ContainerClientWidth;

                await resizeTableAsync();

                StateHasChanged();
            }
        }

        class TableSize
        {
            public int[] Columns { get; set; }
            public int TableClientWidth { get; set; }
            public int ContainerClientWidth { get; set; }
        }
    }
}
