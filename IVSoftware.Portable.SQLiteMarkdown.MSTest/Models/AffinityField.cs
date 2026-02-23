using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.Models
{
    class ObservableAffinityField<T>
        : ObservableCollection<T>
        , IAffinityField
        where T : IAffinityItem
    {
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (!Equals(_isRunning, value))
                {
                    _isRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        bool _isRunning = false;
        private int _busy = 0;

        public DateTimeOffset AffinityEpochTimeSink
        {
            get => _affinityEpochTimeSink;
            set
            {
                if (!Equals(_affinityEpochTimeSink, value))
                {
                    _affinityEpochTimeSink = value;
                    if (Interlocked.Exchange(ref _busy, 1) == 0)
                    {
                        _ = ExecAffinityFsm();
                    }
                    else
                    {   /* G T K */
                        // N O O P 
                        // Pulse is discarded by design.
                    }
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Reconciles all Affinity items against the current injected epoch snapshot.
        /// </summary>
        /// <remarks>
        /// This method interprets the latest AffinityEpochTimeSink value and applies
        /// constraint semantics across the field. Typical responsibilities include:
        /// 
        /// - Recomputing Available windows for fixed epochs.
        /// - Adjusting Remaining balances when IsRunning is true.
        /// - Detecting scarcity (Available &lt; Remaining).
        /// - Applying deterministic, opt-in arbitration policies when compression
        ///   among competing items is required.
        ///
        /// Pulses are not queued. If a reconciliation is already in progress,
        /// intermediate snapshots are discarded and only the most recent epoch
        /// is considered authoritative.
        /// </remarks>
        public virtual async Task ExecAffinityFsm()
        {
            // Attempt non-queued reconciliation
            try
            {
                AffinityFsm state = AffinityFsm.PlaceFixed;
                
                while(!Equals(ReservedAffinityStateMachine.Idle, (ReservedAffinityStateMachine)AffinityFsmState))
                {
                    state = (AffinityFsm) await ExecAffinityState(state);
                }
            }
            finally
            {
                Volatile.Write(ref _busy, 0);
            }
        }

        public Enum AffinityFsmState { get; protected set; } = ReservedAffinityStateMachine.Idle;

        public virtual async Task<Enum> ExecAffinityState(Enum state)
        {
            // PLACEHOLDER - ToDo
            return ReservedAffinityStateMachine.Idle;
        }

        private DateTimeOffset _affinityEpochTimeSink = default;



        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        // We do not care about INPC in the BC which is mainly Count semantics.
        public new event PropertyChangedEventHandler? PropertyChanged;
    }
}
