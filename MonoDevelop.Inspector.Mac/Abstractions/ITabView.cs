using System.Collections.Generic;

namespace MonoDevelop.Inspector.Mac.Abstractions
{
    internal interface ITabView : INativeObject
    {
        List<IView> Pages { get; }
        void Remove(IView viewWrapper);
        void Add(IView viewWrapper);
    }
}
