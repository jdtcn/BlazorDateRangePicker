using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

#nullable enable

namespace Demo.Shared
{
    /// <summary>
    /// Provides methods to place text on and retrieve text from the system Clipboard.
    /// </summary>
    public interface IClipboard
    {
        /// <summary>
        /// Retrieves text data from the Clipboard.
        /// </summary>
        public Task<string?> GetTextAsync(CancellationToken cancellation = default);

        /// <summary>
        /// Retrieves text data from the Clipboard.
        /// </summary>
        public string? GetText();

        /// <summary>
        /// Clears the Clipboard and then adds text data to it.
        /// </summary>
        public Task SetTextAsync(string text, CancellationToken cancellation = default);

        /// <summary>
        /// Clears the Clipboard and then adds text data to it.
        /// </summary>
        public void SetText(string text);
    }

    public class BlazorClipboard : IClipboard
    {
        protected readonly IJSRuntime jsRuntime;

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        public BlazorClipboard(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        /// <inheritdoc />
        public virtual async Task<string?> GetTextAsync(CancellationToken cancellation = default)
        {
            return await jsRuntime.InvokeAsync<string>("navigator.clipboard.readText", cancellation, Array.Empty<object>());
        }

        /// <inheritdoc />
        public virtual string? GetText()
        {
            return GetTextAsync().GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public virtual async Task SetTextAsync(string text, CancellationToken cancellation = default)
        {
            await jsRuntime.InvokeAsync<string>("navigator.clipboard.writeText", cancellation, new object[] { text });
        }

        /// <inheritdoc />
        public virtual void SetText(string text)
        {
            SetTextAsync(text).GetAwaiter().GetResult();
        }
    }
}

#nullable disable