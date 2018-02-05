using System;

namespace algochan.API
{
    public class Contest : IDisposable
    {
        public int id { get; set; }
        public string name { get; set; }
        public ContestType type { get; set; }
        public ContestPhase phase { get; set; }
        public bool frozen { get; set; }
        public int durationSeconds { get; set; }
        public int startTimeSeconds { get; set; }
        public int relativeTimeSeconds { get; set; }
        public DateTime time { get; set; }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Contest() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public enum ContestType
    {
        CF,
        IOI,
        ICPC
    }

    public enum ContestPhase
    {
        BEFORE,
        CODING,
        PENDING_SYSTEM_TEST,
        SYSTEM_TEST,
        FINISHED
    }
}