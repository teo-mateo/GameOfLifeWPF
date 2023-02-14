using System;

namespace GameOfLife
{
    public class GenericEventArgs<T> : EventArgs
    {
        public T Payload { get; set; }
        public GenericEventArgs(T payload) : base() { this.Payload = payload; }
    }
}
