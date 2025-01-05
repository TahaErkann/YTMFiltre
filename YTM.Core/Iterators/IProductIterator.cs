using YTM.Core.Entities;

namespace YTM.Core.Iterators
{
    public interface IProductIterator
    {
        bool HasNext();
        Product Next();
        void Reset();
    }
} 