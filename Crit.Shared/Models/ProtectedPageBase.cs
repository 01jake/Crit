using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Crit.Shared.Models
{
    public abstract class ProtectedPageBase : ComponentBase
    {
        [Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        protected bool IsLoading { get; set; } = true;
        protected string? ErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated != true)
            {
                IsLoading = false;
                return;
            }

            await LoadProtectedDataSafeAsync();
        }

        protected async Task LoadProtectedDataSafeAsync()
        {
            try
            {
                ErrorMessage = null;
                IsLoading = true;

                await LoadDataAsync();

                if (ShouldRetryInitialLoad())
                {
                    await Task.Delay(300);
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected virtual bool ShouldRetryInitialLoad() => false;

        protected abstract Task LoadDataAsync();
    }
}
