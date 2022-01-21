using System;

namespace MapEditor.Grpc
{
    public class EventArgs<T> : EventArgs
    {
        public T Value
        {
            get;
            set;
        }

        public EventArgs()
            : this(default(T))
        {
        }

        public EventArgs(T t)
            : base()
        {
            Value = t;
        }
    }
}
