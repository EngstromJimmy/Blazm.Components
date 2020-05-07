window.resizeTable = resizeTable;


function resizeTable(id, isGrouped) {
    var container = document.getElementById("tablecontainer" + id)
    var table = document.getElementById("table" + id)
    var row = 0;
    if (isGrouped) {
        row = 1;
    }

    //Make all columns "visible" to be able to check their client width so we can recalculate which columns that should be visible
    table.classList.add("countableClientWidth");

    var celloffsets = Array.from(table.rows[row].cells).map(c => c.offsetWidth);
    var tablewidth = table.clientWidth;

    table.classList.remove("countableClientWidth");


    return {
        tableClientWidth: tablewidth,
        containerClientWidth: container.clientWidth,
        columns: celloffsets
    }
}
