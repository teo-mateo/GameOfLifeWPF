using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GameOfLife
{
	public class Controller : INotifyPropertyChanged
	{
		System.Threading.Timer _timer;

		private LifeCanvas _board;


        private bool _isRunning;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsRunning"));
            }
        }

		public Controller(LifeCanvas board)
		{
			_board = board;
            IsRunning = false;
            
			_timer = new System.Threading.Timer(o =>
			{
				_board.Update();
			}, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
		}

		public void Start()
		{
            IsRunning = true;
			_timer.Change(100, 50);
		}

		public void Pause()
		{
            IsRunning = false;
			_timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

		}

		public void Clear()
		{
			Pause();
			_board.KillAll();
		}

		public void Step()
		{
			_board.ComputeAllNextStates();
			_board.Update();
		}
	}

    
}
