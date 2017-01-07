using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
	public class GenericEventArgs<T> : EventArgs
	{
		public T Payload { get; set; }
		public GenericEventArgs(T payload) : base() { this.Payload = payload; }
	}
}
