using System.Collections.Generic;

namespace MonoDevelop.Inspector.Mac
{
    internal interface ITabView : INativeObject
    {
        List<IViewWrapper> Pages { get; }
        void Remove(IViewWrapper viewWrapper);
        void Add(IViewWrapper viewWrapper);
    }
}
