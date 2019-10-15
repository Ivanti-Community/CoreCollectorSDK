using System;

namespace Collector.SDK.Tests.Mocks
{
    public class MockDisposableSingleton : IDisposable
    {
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MockDisposableSingleton()
        {
            Dispose(false);
        }
    }
}
