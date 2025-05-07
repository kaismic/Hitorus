namespace Hitorus.Data.Events
{
    public enum AdvancedCollectionChangedAction {
        AddSingle,
        AddRange,
        RemoveSingle,
        RemoveRange, // continuous range of items
        RemoveMultiple, // discontinuous (could be continuous)
        Reset,
    }

    public class AdvancedCollectionChangedEventArgs<T> : EventArgs
    {
        public AdvancedCollectionChangedAction Action { get; }
        public T? NewItem { get; }
        public T? OldItem { get; }
        public IEnumerable<T>? NewItems { get; }
        public IEnumerable<T>? OldItems { get; }
        public int StartIndex { get; } = -1;
        public int Count { get; } = -1;

        private AdvancedCollectionChangedEventArgs(AdvancedCollectionChangedAction action) {
            Action = action;
        }

        public AdvancedCollectionChangedEventArgs(AdvancedCollectionChangedAction action, T item) : this(action) {
            switch (action) {
                case AdvancedCollectionChangedAction.AddSingle:
                    NewItem = item;
                    break;
                case AdvancedCollectionChangedAction.RemoveSingle:
                    OldItem = item;
                    break;
                default:
                    throw new ArgumentException($"The 'action' parameter must be either {nameof(AdvancedCollectionChangedAction.AddSingle)} or {nameof(AdvancedCollectionChangedAction.RemoveSingle)} when providing only a single item.", nameof(action));
            }
        }
        
        public AdvancedCollectionChangedEventArgs(AdvancedCollectionChangedAction action, int index) : this(action) {
            switch (action) {
                case AdvancedCollectionChangedAction.RemoveSingle:
                    StartIndex = index;
                    break;
                default:
                    throw new ArgumentException($"The 'action' parameter must be {nameof(AdvancedCollectionChangedAction.RemoveSingle)} when providing only {nameof(index)}.", nameof(action));
            }
        }

        public AdvancedCollectionChangedEventArgs(AdvancedCollectionChangedAction action, IEnumerable<T> items) : this(action) {
            switch (action) {
                case AdvancedCollectionChangedAction.AddRange or AdvancedCollectionChangedAction.Reset:
                    NewItems = items;
                    break;
                case AdvancedCollectionChangedAction.RemoveMultiple:
                    OldItems = items;
                    break;
                default:
                    throw new ArgumentException($"The 'action' parameter must be {nameof(AdvancedCollectionChangedAction.AddRange)}, {nameof(AdvancedCollectionChangedAction.RemoveMultiple)} or {nameof(AdvancedCollectionChangedAction.Reset)} when providing only {nameof(items)}.", nameof(action));
            }
        }

        public AdvancedCollectionChangedEventArgs(AdvancedCollectionChangedAction action, int startIndex, int count) : this(action) {
            switch (action) {
                case AdvancedCollectionChangedAction.RemoveRange:
                    StartIndex = startIndex;
                    Count = count;
                    break;
                default:
                    throw new ArgumentException($"The 'action' parameter must be {nameof(AdvancedCollectionChangedAction.RemoveRange)} when providing {nameof(startIndex)} and {nameof(count)}.", nameof(action));
            }
        }
    }
}
