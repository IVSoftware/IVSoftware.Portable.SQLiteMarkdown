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

        public Enum RunFSM<TFsm>(TFsm @enum) => StateRunner.RunFSM(@enum);

        public Enum RunTokenRing<TFsm>(TFsm @enum) => StateRunner.RunTokenRing(@enum);

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
