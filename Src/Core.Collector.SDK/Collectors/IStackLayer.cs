using System.Threading;

namespace Collector.SDK.Collectors
{
    public interface IStackLayer
    {
        CancellationToken Token { get; }

        /// <summary>
        /// Set the cancellation token.
        /// </summary>
        void Kill();
    }
}
