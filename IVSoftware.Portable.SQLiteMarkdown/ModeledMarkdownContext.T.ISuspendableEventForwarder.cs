
using IVSoftware.Portable.Xml.Linq.Collections;
using System;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public partial class ModeledMarkdownContext<T> : ISuspendableEventForwarder
    {
        public bool IsDisposing => ((ISuspendableEventForwarder)CanonicalSupersetProtected).IsDisposing;

        Enum ISuspendableEventForwarder.Authority => ((ISuspendableEventForwarder)CanonicalSupersetProtected).Authority;

        public bool IsEventTypeAllowed<T1>()
        {
            return ((ISuspendableEventForwarder)CanonicalSupersetProtected).IsEventTypeAllowed<T1>();
        }

        public IDisposable SuspendEvents()
        {
            return ((ISuspendableEventForwarder)CanonicalSupersetProtected).SuspendEvents();
        }

        public IDisposable SuspendEvents(params Type[] eventTypes)
        {
            return ((ISuspendableEventForwarder)CanonicalSupersetProtected).SuspendEvents(eventTypes);
        }

        public IDisposable SuspendEvents(Enum stdAuthority, params Type[] eventTypes)
        {
            return ((ISuspendableEventForwarder)CanonicalSupersetProtected).SuspendEvents(stdAuthority, eventTypes);
        }

        public IDisposable SuspendEvents<T1>(Enum? stdAuthority = null)
        {
            return ((ISuspendableEventForwarder)CanonicalSupersetProtected).SuspendEvents<T1>(stdAuthority);
        }
    }
}