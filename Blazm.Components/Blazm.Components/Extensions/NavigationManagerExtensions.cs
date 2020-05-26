using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Components
{
    public static class NavigationManagerExtensions
    {
        public static async Task NavigateToNewWindowAsync(this NavigationManager navigator, IJSRuntime jsRuntime, string url, string content)
        {
            await jsRuntime.InvokeAsync<object>("NavigationManagerExtensions.openInNewWindow", url, content);
        }
    }
}
