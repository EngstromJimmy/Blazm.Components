using Blazm.Components.Extensions;
using BlazorPro.BlazorSize;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Blazm.Components
{

    public partial class BlazmGrid<TItem> : IGridContainer, IDisposable
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
        public EventCallback<int> CurrentPageChanged { get; set; }

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

        private bool useVirtualize = true;
        [Parameter]
        public bool UseVirtualize
        {
            get
            {
                //When using paging the use of Virtualize becomes obsolete so it is not possible to use them together.
                if (PageSize > 0)
                {
                    return false;
                }
                else
                {
                    return useVirtualize;
                }
            }
            set { useVirtualize = value; }

        }
        [Parameter]
        public bool Sortable
        {
            get;
            set;
        } = false;

        [Parameter]
        public string? SortField
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
        public string? GroupSortField
        {
            get;
            set;
        } = null;

        [Parameter]
        public System.ComponentModel.ListSortDirection GroupSortDirection
        {
            get; set;
        } = System.ComponentModel.ListSortDirection.Ascending;

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
        } = default!;

        [Parameter]
        public RenderFragment EmptyGridTemplate
        {
            get;
            set;
        } = default!;

        [Parameter]
        public RenderFragment NullGridTemplate
        {
            get;
            set;
        } = default!;

        [Parameter]
        public RenderFragment<object> GroupHeader
        {
            get;
            set;
        } = default!;

        [Parameter]
        public bool ShowFilter { get; set; }

        [Parameter]
        public bool HasData { get; set; }

        [Parameter]
        public Func<TItem, object>? GroupBy { get; set; } = null;

        [Parameter]
        public Func<TItem?, string>? RowCss { get; set; } = null;

        [Parameter]
        public Func<TItem?, bool>? RowShowDetails { get; set; } = null;

        [Parameter]
        public bool ShowDetails { get; set; } = true;

        [Parameter]
        public ItemsProviderDelegate<TItem>? ItemsProvider { get; set; } = null;

        [Parameter]
        public IEnumerable<TItem>? Data { get; set; }

        [Parameter]
        public List<TItem> SelectedData { get; set; } = new List<TItem>();

        [Parameter]
        public RenderFragment<TItem>? DetailTemplate
        {
            get;
            set;
        } = null;

        public bool IsFiltered { get { return Columns.Any(c => c.Filters.Any(f => !string.IsNullOrEmpty(f.FilterValue))); } }
        #endregion

        public async Task ClearFilters()
        {
            foreach (var c in Columns)
            {
                foreach (var f in c.Filters)
                {
                    f.FilterValue = null;
                }
            }
            await ApplyFilter();
        }

        [Inject] ResizeListener listener { get; set; } = default!;
        [Inject] IJSRuntime jsruntime { get; set; } = default!;

        public bool ShowAllColumns { get; set; }
        public List<IGridColumn> Columns { get; set; } = new List<IGridColumn>();
        public List<IGridColumn> AllColumns { get; set; } = new List<IGridColumn>();
        public List<int> ExpandedRows = new List<int>();
        string id = Guid.NewGuid().ToString();
        int ContainerClientWidth { get; set; }
        int TableClientWidth { get; set; }
        public Virtualize<TItem> Virtualize { get; set; } = default!;
        private IEnumerable<TItem> pagedData { get; set; } = new List<TItem>();

        protected override async Task OnParametersSetAsync()
        {
            if (ItemsProvider == null)
            {
                ItemsProvider = LoadData;
            }

            if (Data != null)
            {
                HasData = Data.Any();
                await RefreshDataAsync();
            }

            await base.OnParametersSetAsync();
        }

        public async Task RefreshDataAsync()
        {
            if (Virtualize != null)
            {
                await Virtualize.RefreshDataAsync();
            }
        }

        private IEnumerable<TItem> loadPagedData()
        {
            if (Data == null && pagedData != null)
            {
                pagedData = new List<TItem>();
            }

            if (Data != null)
            {
                pagedData = ApplyFilter(Data);

                if (SortField != null)
                {
                    var type = typeof(TItem).GetProperty(SortField)?.PropertyType;
                    if (SortDirection == ListSortDirection.Descending)
                    {
                        if (type == typeof(string))
                        {
                            pagedData = pagedData.OrderByDescending(x => (string?)x?.GetType().GetProperty(SortField)?.GetValue(x, null), StringComparer.InvariantCultureIgnoreCase).ToList();
                        }
                        else
                        {
                            pagedData = pagedData.OrderByDescending(x => x?.GetType().GetProperty(SortField)?.GetValue(x, null)).ToList();
                        }
                    }
                    else
                    {
                        if (type == typeof(string))
                        {
                            pagedData = pagedData.OrderBy(x => (string?)x?.GetType().GetProperty(SortField)?.GetValue(x, null), StringComparer.InvariantCultureIgnoreCase).ToList();
                        }
                        else
                        {
                            pagedData = pagedData.OrderBy(x => x?.GetType().GetProperty(SortField)?.GetValue(x, null)).ToList();
                        }
                    }
                }

                if (PageSize != 0 && !IsFiltered)
                {
                    pagedData = pagedData.Skip(PageSize * CurrentPage).Take(PageSize).ToList();
                }
                //StateHasChanged();
            }
            return pagedData;
        }

        [Parameter]
        public string Filter { get; set; } = "";
        private IEnumerable<TItem> ApplyFilter(IEnumerable<TItem> data)
        {
            var filtereddata = data;
            //AddFilter
            var param = Expression.Parameter(typeof(TItem));
            foreach (var column in Columns.Where(c => c.Field != null && c.CanFilter && c.Filters.Count > 0))
            {
                var columnType = typeof(TItem).GetProperty(column.Field)?.PropertyType;
                if (columnType != null)
                {
                    foreach (var f in column.Filters)
                    {
                        object? filtervalue = null;

                        try
                        {
                            if (!string.IsNullOrEmpty(Filter))
                            {
                                filtervalue = Convert.ChangeType(Filter, columnType);
                            }
                            else
                            {
                                columnType=  Nullable.GetUnderlyingType(columnType) ?? columnType;
								filtervalue = Convert.ChangeType(f.FilterValue, columnType);
                            }
                        }
                        catch
                        {
                            filtervalue = null;
                        }

                        if (!string.IsNullOrEmpty(filtervalue?.ToString()))
                        {
                            var filter = GetExpression<TItem>(column.Field, filtervalue, f.FilterType);
                            try
                            {
                                filtereddata = filtereddata.Where(filter.Compile());
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine(ex.Message);
                            }

                        }
                    }
                }
            }
            return filtereddata;
        }

        private async ValueTask<ItemsProviderResult<TItem>> LoadData(ItemsProviderRequest request)
        {
            loadPagedData();
            if (pagedData != null)
            {
                var totalRows = pagedData.Count();
                HasData = pagedData.Any();
                var numberofItems = Math.Min(request.Count, totalRows - request.StartIndex);

                return new ItemsProviderResult<TItem>(pagedData.Skip(request.StartIndex).Take(numberofItems), totalRows);
            }
            return new ItemsProviderResult<TItem>(new List<TItem>(), 0);
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

            if (Data != null)
            {

                var export = ApplyFilter(Data);

                if (SortField != null)
                {
                    var type = typeof(TItem).GetProperty(SortField)?.PropertyType;
                    if (SortDirection == ListSortDirection.Descending)
                    {
                        if (type == typeof(string))
                        {
                            export = export.OrderByDescending(x => (string?)x?.GetType().GetProperty(SortField)?.GetValue(x, null), StringComparer.InvariantCultureIgnoreCase).ToList();
                        }
                        else
                        {
                            export = export.OrderByDescending(x => x?.GetType().GetProperty(SortField)?.GetValue(x, null)).ToList();
                        }
                    }
                    else
                    {
                        if (type == typeof(string))
                        {
                            export = export.OrderBy(x => (string?)x?.GetType().GetProperty(SortField)?.GetValue(x, null), StringComparer.InvariantCultureIgnoreCase).ToList();
                        }
                        else
                        {
                            export = export.OrderBy(x => x?.GetType().GetProperty(SortField)?.GetValue(x, null)).ToList();
                        }
                    }
                }

                foreach (var r in export)
                {
                    IRow row = worksheet.CreateRow(rownum++);
                    cellnumber = 0;
                    foreach (var column in Columns.Where(d => d.Exportable))
                    {
                        ICell Cell = row.CreateCell(cellnumber++);

                        string stringValue = "";

                        if (column.Format == null)
                        {
                            stringValue = r?.GetType().GetProperty(column.Field)?.GetValue(r)?.ToString() ?? "";
                        }
                        else
                        {
                            stringValue = string.Format(column.Format, r?.GetType().GetProperty(column.Field)?.GetValue(r)?.ToString());
                        }

                        switch (r?.GetType().GetProperty(column.Field)?.PropertyType.Name)
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
            }
            MemoryStream ms = new MemoryStream();
            workbook.Write(ms,false);
            byte[] bytes = ms.ToArray();
            ms.Close();

            await jsruntime.SaveAsFileAsync(filename, bytes, "application/vnd.ms-excel");
        }

        public async Task SelectItemsAsync(List<TItem> items, object selected)
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

        public async Task SelectItemAsync(TItem item, object selected)
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
            await SelectedDataChanged.InvokeAsync(SelectedData);
            StateHasChanged();
        }

        public bool IsItemSelected(TItem item)
        {
            return SelectedData.Contains(item);
        }

        public string GetSignClass(IGridColumn column, string value)
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
                        if (double.TryParse(value, out var dvalue))
                        {
                            if (dvalue == 0d)
                            {
                                return "";
                            }
                            else
                            {
                                return column.ValuePositiveClass;
                            }
                        }
                        else
                        {
                            return "";
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
                await CurrentPageChanged.InvokeAsync(CurrentPage);
            }
            await RefreshDataAsync();
        }

        protected async Task NextPage()
        {

            if (Data != null && (CurrentPage * PageSize) + PageSize < Data.Count())
            {
                CurrentPage++;
                await CurrentPageChanged.InvokeAsync(CurrentPage);
            }
            await RefreshDataAsync();

        }

        public int VisibleColumns => Columns.Where(c => c.Visible).Count() + (ShowCheckbox ? 1 : 0) + ((DetailTemplate != null && ShowDetails) ? 1 : 0) + (Columns.Any(c => c.Visible == false) ? 1 : 0);

        void IGridContainer.AddColumn(IGridColumn column)
        {

            if (column.Field == null && column.Id == null)
            {
                if (GroupBy == null)
                {
                    column.Id = Guid.NewGuid().ToString();
                }
                else
                {
                    throw new Exception("Field and Id can not be null");
                }
            }

            if (column.Field != null)
            {
                column.Id = column.Field + column.Format?.GetHashCode();
            }

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

            if (firstRender)
            {
                await RefreshDataAsync();
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

        void IDisposable.Dispose() => listener.OnResized -= WindowResized;

        private bool resizing = false;
        private bool resizeAgain = false;

        async void WindowResized(object _, BrowserWindowSize window)
        {
            if (!resizing)
            {
                await ResizeGrid();
            }
            else
            {
                resizeAgain = true;
            }
        }

        IJSObjectReference resizemodule = default!;
        public async Task ResizeGrid()
        {
            if (Columns.Any(c => c.Priority > 0))
            {
                do
                {
                    try
                    {
                        resizing = true;
                        resizeAgain = false;
                        resizemodule = await jsruntime.InvokeAsync<IJSObjectReference>("import", "./_content/Blazm.Components/Scripts/ResizeTable.js");
                        ShowAllColumns = true;
                        await InvokeAsync(StateHasChanged);

                        var size = await resizemodule.InvokeAsync<TableSize>("ResizeTable", id, GroupBy != null);

                        ShowAllColumns = false;
                        await InvokeAsync(StateHasChanged);

                        if (size != null)
                        {
                            for (var i = 0; i < size?.Columns?.Length; i++)
                            {
                                var counter = i;
                                if (ShowCheckbox)
                                {
                                    //Don't count checkboxes
                                    counter = -1;
                                    if (counter <= 0)
                                    {
                                        continue;
                                    }
                                }
                                if (Columns.Any(c => !c.Visible) || (DetailTemplate != null && ShowDetails))
                                {
                                    //Don't count detail arrow
                                    counter = -1;
                                    if (counter <= 0)
                                    {
                                        continue;
                                    }
                                }
                                Columns[counter].ClientWidth = size.Columns[i];
                            }
                            TableClientWidth = size.TableClientWidth;
                            ContainerClientWidth = size.ContainerClientWidth;

                            await resizeTableAsync();

                            await InvokeAsync(StateHasChanged);
                        }
                    }
                    finally
                    {
                        resizing = false;
                    }
                }
                while (resizeAgain);
            }
        }

        public async Task ApplyFilter()
        {
            StateHasChanged();
            await RefreshDataAsync();
        }

        class TableSize
        {
            public int[] Columns { get; set; } = Array.Empty<int>();
            public int TableClientWidth { get; set; }
            public int ContainerClientWidth { get; set; }
        }

        static Expression<Func<T, bool>> GetExpression<T>(string propertyName, object filterValue, FilterType filterType)
        {
            var parameterExp = Expression.Parameter(typeof(T));
            var propertyExp = Expression.Property(parameterExp, propertyName);
            var someValue = Expression.Constant(filterValue, propertyExp.Type);

            if ((filterType == FilterType.Equal && propertyExp.Type == typeof(string)) || propertyExp.Type == typeof(string))
            {
                MethodInfo method = typeof(Extensions.ObjectExtensions).GetMethod("ContainsExt", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { propertyExp.Type, typeof(string) }, null);
                var containsMethodExp = Expression.Call(null, method, propertyExp, someValue);
                //NullCheck
                var nullCheck = Expression.NotEqual(propertyExp, Expression.Constant(null, typeof(object)));
                var containsMethodExpwithnullcheck = Expression.AndAlso(nullCheck, containsMethodExp);

                return Expression.Lambda<Func<T, bool>>(containsMethodExpwithnullcheck, parameterExp);

            }
            else if ((filterType == FilterType.Equal && (propertyExp.Type == typeof(DateTime) || propertyExp.Type == typeof(DateTime?))))
            {
                const string dateFormat = "yyyy-MM-dd HH:mm";
                var parsedDate = DateTime.ParseExact(((DateTime)filterValue).ToString(dateFormat), dateFormat, CultureInfo.InvariantCulture);

                var dayStart = new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day, 0, 0, 0, 0);
                var dayEnd = new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day, 23, 59, 59, 999);

                var left = Expression.Lambda<Func<T, bool>>(Expression.GreaterThanOrEqual(propertyExp, Expression.Constant(dayStart,propertyExp.Type)), parameterExp);
                var right = Expression.Lambda<Func<T, bool>>(Expression.LessThanOrEqual(propertyExp, Expression.Constant(dayEnd, propertyExp.Type)), parameterExp);

                return left.And(right);
            }
            else if ((filterType == FilterType.Equal && propertyExp.Type != typeof(string)))
            {
                var body = Expression.Equal(propertyExp, someValue);
                return Expression.Lambda<Func<T, bool>>(body, parameterExp);
            }
            else if (filterType == FilterType.GreaterThanOrEqual)
            {
                var body = Expression.GreaterThanOrEqual(propertyExp, someValue);
                return Expression.Lambda<Func<T, bool>>(body, parameterExp);
            }
            else
            {
                var body = Expression.LessThanOrEqual(propertyExp, someValue);
                return Expression.Lambda<Func<T, bool>>(body, parameterExp);

            }
        }
    }
}
