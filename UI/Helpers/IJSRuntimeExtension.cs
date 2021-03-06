using Microsoft.JSInterop;

namespace UI.Helpers
{
    public static class IJSRuntimeExtension
    {
        public static async ValueTask SuccessAlert(this IJSRuntime JSRuntime, string title, string message)
        {
            await JSRuntime.InvokeVoidAsync("ShowToastr", "success", title, message);
        }

        public static async ValueTask ErrorAlert(this IJSRuntime JSRuntime, string title, string message)
        {
            await JSRuntime.InvokeVoidAsync("ShowToastr", "error", title, message);
        }

        public static async ValueTask SuccessSwal(this IJSRuntime JSRuntime, string title, string message)
        {
            await JSRuntime.InvokeVoidAsync("ShowSwal", "success", title, message);
        }

        public static async ValueTask ErrorSwal(this IJSRuntime JSRuntime, string title, string message)
        {
            await JSRuntime.InvokeVoidAsync("ShowSwal", "error", title, message);
        }
    }
}
