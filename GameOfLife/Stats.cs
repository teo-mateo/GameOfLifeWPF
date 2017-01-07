using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
	public class Stats : ObservableObject
	{
		private int _totalAlive;
		public int TotalAlive
		{
			get { return _totalAlive; }
			set
			{
				_totalAlive = value;
				OnPropertyChanged("TotalAlive");
				OnPropertyChanged("TotalDead");
			}
		}
		public int TotalDead
		{
			get
			{
				return BoardSize - TotalAlive;
			}
		}
		public int BoardSize { get; set; }
		public Stats(int boardSize)
		{
			this.BoardSize = boardSize;
		}
	}
}
