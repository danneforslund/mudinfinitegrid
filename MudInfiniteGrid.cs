using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Extends MudDataGrid to provide infinite scrolling capabilities without virtualization.
/// Make sure to include mudinfinitegrid.js in your project.
/// </summary>
/// <typeparam name="T">The type of data item.</typeparam>
public class MudInfiniteGrid<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : MudDataGrid<T>, IAsyncDisposable {
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

    [Parameter] public bool Infinite { get; set; } = true;

    private DotNetObjectReference<MudInfiniteGrid<T>>? _dotNetRef;
    private readonly Guid _gridId = Guid.NewGuid();

    private bool loading;
    private int currentPage;
    private bool endReached;
    private int lastLoadedPage = -1;
    private readonly List<T> _loadedItems = [];
    private Func<GridState<T>, Task<GridData<T>>>? originalServerData;

    protected override void OnInitialized() {
        base.OnInitialized();
        if (Infinite) {
            this.UserAttributes.Add("id", _gridId);
            originalServerData = ServerData;
            this.ServerData = HandleInfiniteLoad;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        await base.OnAfterRenderAsync(firstRender);
        if (!Infinite) return;
        
        if (firstRender) {
            _dotNetRef = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("scrollHelper.attachScrollListener", _gridId.ToString(), _dotNetRef);
        }
    }

    private async Task<GridData<T>> HandleInfiniteLoad(GridState<T> state) {
        if (lastLoadedPage == currentPage) {
            //Reload triggered outside of scroll event (i.e. filter/sort). Clear _loadedItems to not show old data;
            _loadedItems.Clear();
            currentPage = 0;
            endReached = false;
        }

        var fakeState = new GridState<T> {
            Page = currentPage,
            PageSize = this.RowsPerPage,
        };

        loading = true;
        var data = await originalServerData!(fakeState);
        loading = false;
        
        if (!data.Items.Any()) {
            endReached = true;
        } else {
            _loadedItems.AddRange(data.Items);
            lastLoadedPage = currentPage;
        }

        return new() {
            Items = _loadedItems,
            TotalItems = _loadedItems.Count
        };
    }

    [JSInvokable]
    public async Task NotifyScrollBottomAsync() {
        if (endReached || loading) return;

        currentPage++;
        await ReloadServerData();
    }

    public async ValueTask DisposeAsync() {
        if (_dotNetRef is not null) {
            await JSRuntime.InvokeVoidAsync("scrollHelper.removeScrollListener", _gridId.ToString());
            _dotNetRef.Dispose();
        }
    }
}
