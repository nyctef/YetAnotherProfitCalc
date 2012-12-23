using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YetAnotherProfitCalc
{
    public abstract class DisposableBase : IDisposable
    {
        private bool m_Disposed;

        public void Dispose()
        {
            if (!m_Disposed)
            {
                Dispose(true);
            }
        }

        ~DisposableBase()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            m_Disposed = true;
        }

        protected virtual string ObjectName { get { return GetType().Name; } }

        public void CheckDisposed()
        {
            if (m_Disposed) throw new ObjectDisposedException(ObjectName);
        }
    }
}
