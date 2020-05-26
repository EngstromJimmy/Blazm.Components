"use strict";

window.resizeTable = resizeTable;

function resizeTable(id, isGrouped) {
    var container = document.getElementById("tablecontainer" + id);
    var table = document.getElementById("table" + id);
    var row = 0;
    if (isGrouped) {
        row = 1;
    }

    //Make all columns "visible" to be able to check their client width so we can recalculate which columns that should be visible
    table.classList.add("countableClientWidth");

    var celloffsets = Array.from(table.rows[row].cells).map(function (c) {
        return c.offsetWidth;
    });
    var tablewidth = table.clientWidth;

    table.classList.remove("countableClientWidth");

    return {
        tableClientWidth: tablewidth,
        containerClientWidth: container.clientWidth,
        columns: celloffsets
    };
}

//<OpenInNewWindow>
window.NavigationManagerExtensions = {};
window.NavigationManagerExtensions.openInNewWindow = function (url, message) {
    var newwindow = window.open('', '_blank');
    newwindow.document.write(message);
    newwindow.location.href = url;
};
//</OpenInNewWindow>

//<saveAsFile>
window.JSRuntimeExtensions = {};
window.JSRuntimeExtensions.saveAsFile = function (filename, type, bytesBase64) {
    if (navigator.msSaveBlob) {
        //Download document in Edge browser
        var data = window.atob(bytesBase64);
        var bytes = new Uint8Array(data.length);
        for (var i = 0; i < data.length; i++) {
            bytes[i] = data.charCodeAt(i);
        }
        var blob = new Blob([bytes.buffer], { type: type });
        navigator.msSaveBlob(blob, filename);
    } else {
        var link = document.createElement('a');
        link.download = filename;
        link.href = "data:" + type + ";base64," + bytesBase64;
        document.body.appendChild(link); // Needed for Firefox
        link.click();
        document.body.removeChild(link);
    }
};
//</saveAsFile>

