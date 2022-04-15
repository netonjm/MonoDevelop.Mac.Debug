using System.Collections.Generic;
using VisualStudio.ViewInspector.Abstractions;

namespace VisualStudio.ViewInspector.Mac.Abstractions
{
    internal interface ITabView : INativeObject
    {
        List<IView> Pages { get; }
        void Remove(IView viewWrapper);
        void Add(IView viewWrapper);
    }
}
