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
            await js.InvokeAsync<object>("JSRuntimeExtensions.saveAsFile", filename, type, Convert.ToBase64String(data));
        }   
    }
}
