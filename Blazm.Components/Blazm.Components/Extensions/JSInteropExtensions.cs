using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.JSInterop
{
    public static class JSInteropExtensions
    {
        public static async Task SaveAsFileAsync(this IJSRuntime js, string filename, byte[] data, string type = "application/octet-stream")
        {
            var savefilemodule = await js.InvokeAsync<IJSObjectReference>("import", "./_content/Blazm.Components/scripts/SaveAsFile.js");
            await savefilemodule.InvokeVoidAsync("SaveAsFile", filename, data, "application/vnd.ms-excel");
        }   
    }
}
