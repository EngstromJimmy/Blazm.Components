using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Components
{
    public static class NavigationManagerExtensions
    {
        static IJSObjectReference windowmodule;

        public static async Task NavigateToNewWindowAsync(this NavigationManager navigator, IJSRuntime jsRuntime, string url, string content)
        {
            windowmodule = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Blazm.Components/scripts/OpenInNewWindow.js");
            await windowmodule.InvokeAsync<object>("OpenInNewWindow", url, content);
        }
    }
}
