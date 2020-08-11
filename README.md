# How to use Blazm

<a href="https://www.nuget.org/packages/Blazm.Components/"><img src="https://img.shields.io/nuget/v/Blazm.Components"></a>

I promised myself that I wouldn't build another component library.
But for different reasons I had to build a Grid component (and some other components), and why not share that with others?
So here it is, my component library.

The company me and my wife run is called Azm Dev (Awesome dev).
Blazor + Azm == Blazm and it will make you site blazm (blossom).
I know.. I like puns.


##  Components

BlazmGrid

**Forecast sample using Blazm**
``` HTML
<BlazmGrid Data="forecasts" PageSize="10" Sortable="true" SortField="@nameof(WeatherForecast.Date)">
    <GridColumns>
        <GridColumn Field="@nameof(WeatherForecast.Date)" Format="{0:d}"  Priority="3"/>
        <GridColumn Field="@nameof(WeatherForecast.TemperatureC)" HighlightSign="true" Priority="1" Class="alignRight" />
        <GridColumn Field="@nameof(WeatherForecast.TemperatureF)" Priority="2" Class="alignRight"/>
        <GridColumn Field="@nameof(WeatherForecast.Summary)"  Priority="0"/>
    </GridColumns>
</BlazmGrid>
```

## How to get started

### Prerequisites

At this point the library is using [Bootstrap](https://getbootstrap.com/).  
This might change in the future but for now you will need Bootstrap.  
It is also using [FontAwesome](https://fontawesome.com/) so make sure you reference that as well.

### In Startup.cs

1. Add ```services.AddGrid();```  
This adds the necessary things for the grid to be responsive.

### In your .Razor file

1. Add ```using Blazm.Components```
2. Add your grid

``` html
<BlazmGrid Data="TestData" PageSize="10">
    <GridColumns>
        <GridColumn Field="@nameof(TestClass.String)" Title="A string from property"></GridColumn>
        <GridColumn Field="@nameof(TestClass.Double)"></GridColumn>
        <GridColumn Field="@nameof(TestClass.Date)"></GridColumn>
    </GridColumns>
</BlazmGrid>
```

### Add Script and Styles

**In Pages/_Host.cshtml**

1. Inside the head tag add ``` <link href="_content/Blazm.Components/css/styles.min.css" rel="stylesheet" /> ```
2. After the script tag (towards the bottom of the page) referring to blazor.server.js.
Add 
``` html 
<script src="_content/Blazm.Components/scripts/jsinterop.min.js"></script>
<script src="_content/BlazorPro.BlazorSize/blazorSize.min.js"></script>
```

**In program.cs**
1. Add ```webBuilder.UseStaticWebAssets();```

## Grid Features

### Paging

By specifying a number in the ``` PageSize ``` property the grid will add next and previous buttons and page the data.

### Sorting

To sort the grid use 
```SortField="@nameof(TestClass.Double)"``` to sort your grid.
If you want to enable the user to sort the grid use ``` Sortable="true" ```.
Set the default sort direction by setting the ```SortDirection``` parameter

This works with grouped lists as well.
To sort the grouping you can use ```GroupSortField="@nameof(TestClass.Double)"```

### Checkboxes

Use ```ShowCheckbox="true"``` to show checkboxes on every row.
Use ```@bind-SelectedData="SelectedTestData"``` to get the selected items.

### Grouping

To add grouping set the data as usual (without grouping) and set the GroupBy-parameter.

``` csharp
GroupBy="d=>d.Group"
```

### Group header

If you group your grid you can add a group header.
It will add a table row with the content in the template.
Context is the group key.

``` csharp
<GroupHeader>
    <h5>@context</h5>
<GroupHeader>
```

### Footer

Footers are available in grouped segments and for the whole table.
In the GroupFooterTemplate the ```context```-variable is a List<T> containing all the data in that group.

``` csharp
<GroupFooterTemplate>
    var h = context as List<TestClass>;
    @h.Sum(r => r.Double).ToString("N0")
</GroupFooterTemplate>
```

Will sum all the Double-field in that group.

FooterTemplate will get a List<T> containing all the rows in the table.

``` csharp
<FooterTemplate>
    var h = context as List<TestClass>;
    @h.Sum(r => r.Double).ToString("N0")
</FooterTemplate>
```

Will sum ALL the double-fields in the entire table.

``` csharp
<GridColumn Field="@nameof(TestClass.Double)" Format="{0:N2}">
    <GroupFooterTemplate>
        @{
            var h = context as List<TestClass>;
            @h.Sum(r => r.Double).ToString("N0")
        }
    </GroupFooterTemplate>
    <FooterTemplate>
        @{
            var h = context as List<TestClass>;
            @h.Sum(r => r.Double).ToString("N0")
        }
    </FooterTemplate>
</GridColumn>
```

### Empty Grid Template
By supplying an ```EmptyGridTemplate``` you can specify if there should be any text shown it the grid is empty.
``` html
<EmptyGridTemplate>
    No data found
</EmptyGridTemplate>
```

## Column features

### Column header

The title of the column can be specified by using the title property but can also be specified using the Display-attribute

``` csharp
public class TestClass
{
    public string String { get; set; }
    [Display(Name = "Comes from attribute")]
    public double Double { get; set; }
    public DateTime Date { get; set; }
}
```

### Format

To format the data, use the Format-property.

``` html
<GridColumn Field="@nameof(TestClass.Double)" Format="{0:N2}"></GridColumn>
```

### Style

There are two ways of styling content.
```Class``` will set the class attribute on the header and data.
```HeaderClass``` will set the class attribute on the header.
```DataClass``` will set the class attribute on the data.
If you want to align both data and header to the right add the class property.

``` html
<GridColumn  ... Class="alignRight" ... />
```

### HighlightSign

To highlight the sign on a column set the HighlightSign to true.
This will set the class .negative if the value is negative.
It will set the class .positive if the value is positive and leave it alone of the value is zero.
Use ```ValueNegativeClass``` and ```ValuePositiveClass``` to specify the classes used (if you want to).

### Priority (Responsive)

By adding Priority to the columns the grid will become responsive and hide columns in the order of priority.
Priority 0 is the highest priority which means the column will always be visible.
It uses [BlazorPro.BlazorSize](https://github.com/EdCharbeneau/BlazorSize) by Ed Charbeneau to detect resize on the client.
If a column is hidden, an icon will appear and the row will become expandable (to show the removed columns).
The icon is customizable using ```ExpandTemplate``` and ```CollapseTemplate```

### Command columns

To add a column that is not connected to any field (like command buttons for example) just add a GridColumn without the Field-property.

``` html
<GridColumn>
    <Template>
        @{
            var item = context as WeatherForecast;
        }
        <button class="btn btn-danger" @onclick="@(async () => { await Delete(item); })"><i class="far fa-trash-alt"></i></button>
    </Template>
</GridColumn>
```

### Export to Excel

The build-in support for exporting data to Excel will not us templated columns (it will render the value not the template).
All columns will be exported by default.
Add ```Exportable=false``` to exclude columns from the export.

``` html
<button class="btn btn-success" @onclick="@(async ()=> { await MyGrid.ExportDataAsync("Weather.xlsx","Export","yyyy-mm-dd"); })">Export data to Excel</button>
    
<BlazmGrid @ref="MyGrid" .......>
//Code removed for brevity

```
and in the code section add
``` csharp
private BlazmGrid<WeatherForecast> MyGrid;
```

### Auto generate columns

If you want to auto generate columns you can do that by setting ```AutoGenerateColumns="true"```
The grid will then create columns for all the properties on your data object.

``` html
<BlazmGrid Data="forecasts" AutoGenerateColumns="true">
</BlazmGrid>
```

### dynamic

You can use dynamic (ExpandoObject) with the grid.
If you for example have a DataTable, convert it to dynamic and it will work with the grid.