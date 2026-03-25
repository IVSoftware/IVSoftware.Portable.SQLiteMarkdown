using IVSoftware.Portable.StateMachine;
using System;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    partial class ModeledMarkdownContext<T> : IStateRunner
    {
        private StateRunnerMMDC<T> StateRunner
        {
            get
            {
                if (_stateRunner is null)
                {
                    _stateRunner = new StateRunnerMMDC<T>(this);
                }
                return _stateRunner;
            }
        }
        StateRunnerMMDC<T>? _stateRunner = null;

        protected AuthorityEpochProvider AuthorityEpochProvider => StateRunner.AuthorityProvider;

        /// <summary>
        /// Identifies provenance of INCC.
        /// </summary>
        /// <remarks>
        /// Acts as an authority monitor and circularity guard for DDX between collections.
        /// </remarks>
        public IDisposable BeginCollectionChangeAuthority(CollectionChangeAuthority authority) => AuthorityEpochProvider.GetToken(authority);

        public Enum RunFSM<TFsm>(object? context = null) => StateRunner.RunFSM<TFsm>(context);

        public Enum RunTokenRing<TFsm>(object? context = null) => StateRunner.RunTokenRing<TFsm>(context);

        Enum IStateRunner.ExecState(Enum state, object? context) => StateRunner.ExecState(state, context);
    }
    class StateRunnerMMDC<T> : StateRunner where T : new()
    {
        public StateRunnerMMDC(ModeledMarkdownContext<T> mmdc) => MMDC = mmdc;

        public object MMDC { get; }

        public override Enum ExecState(Enum state, object? context)
        {
            return base.ExecState(state, context);
        }
    }
}
