#pragma checksum "C:\GitRepos\szakdoga\UI\Pages\Counter.razor" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "2e008b98366c155a5321b3e92fc6360b8037ff0b"
// <auto-generated/>
#pragma warning disable 1591
namespace UI.Pages
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
#nullable restore
#line 1 "C:\GitRepos\szakdoga\UI\_Imports.razor"
using System.Net.Http;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\GitRepos\szakdoga\UI\_Imports.razor"
using Microsoft.AspNetCore.Authorization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "C:\GitRepos\szakdoga\UI\_Imports.razor"
using Microsoft.AspNetCore.Components.Authorization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "C:\GitRepos\szakdoga\UI\_Imports.razor"
using Microsoft.AspNetCore.Components.Forms;

#line default
#line hidden
#nullable disable
#nullable restore
#line 5 "C:\GitRepos\szakdoga\UI\_Imports.razor"
using Microsoft.AspNetCore.Components.Routing;

#line default
#line hidden
#nullable disable
#nullable restore
#line 6 "C:\GitRepos\szakdoga\UI\_Imports.razor"
using Microsoft.AspNetCore.Components.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 7 "C:\GitRepos\szakdoga\UI\_Imports.razor"
using Microsoft.AspNetCore.Components.Web.Virtualization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 8 "C:\GitRepos\szakdoga\UI\_Imports.razor"
using Microsoft.JSInterop;

#line default
#line hidden
#nullable disable
#nullable restore
#line 9 "C:\GitRepos\szakdoga\UI\_Imports.razor"
using UI;

#line default
#line hidden
#nullable disable
#nullable restore
#line 10 "C:\GitRepos\szakdoga\UI\_Imports.razor"
using UI.Shared;

#line default
#line hidden
#nullable disable
    [Microsoft.AspNetCore.Components.RouteAttribute("/counter")]
    public partial class Counter : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenComponent<Microsoft.AspNetCore.Components.Web.PageTitle>(0);
            __builder.AddAttribute(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)((__builder2) => {
                __builder2.AddContent(2, "Counter");
            }
            ));
            __builder.CloseComponent();
            __builder.AddMarkupContent(3, "\r\n\r\n");
            __builder.AddMarkupContent(4, "<h1>Counter</h1>\r\n\r\n");
            __builder.OpenElement(5, "p");
            __builder.AddAttribute(6, "role", "status");
            __builder.AddContent(7, "Current count: ");
#nullable restore
#line (7,34)-(7,46) 24 "C:\GitRepos\szakdoga\UI\Pages\Counter.razor"
__builder.AddContent(8, currentCount);

#line default
#line hidden
#nullable disable
            __builder.CloseElement();
            __builder.AddMarkupContent(9, "\r\n\r\n");
            __builder.OpenElement(10, "button");
            __builder.AddAttribute(11, "class", "btn btn-primary");
            __builder.AddAttribute(12, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, 
#nullable restore
#line 9 "C:\GitRepos\szakdoga\UI\Pages\Counter.razor"
                                          IncrementCount

#line default
#line hidden
#nullable disable
            ));
            __builder.AddContent(13, "Click me");
            __builder.CloseElement();
        }
        #pragma warning restore 1998
#nullable restore
#line 11 "C:\GitRepos\szakdoga\UI\Pages\Counter.razor"
       
    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }

#line default
#line hidden
#nullable disable
    }
}
#pragma warning restore 1591
