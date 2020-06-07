using Assets.Scripts.Enums;
using System;

namespace Assets.Scripts
{
    class DictionaryChangedEventArgs<TKey, TValue> : EventArgs
    {
        public DictionaryActionType ActionType { get; set; }
        public TKey Key { get; set; }
        public TValue Value { get; set; }

        public DictionaryChangedEventArgs(TKey key, TValue value, DictionaryActionType actionType)
        {
            Key = key;
            Value = value;
            ActionType = actionType;
        }
    }
}
