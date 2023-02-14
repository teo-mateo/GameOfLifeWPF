using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace GameOfLife
{
    public class Cell
    {
        Action<string> _logger;
        public int X;
        public int Y;

        public Rectangle Rect;
        private CellState _state;
        public CellState State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        public CellState NextState { get; set; }

        public Cell[] Neighbours;

        public Cell(int x, int y, Action<string> logger)
        {
            X = x;
            Y = y;
            _logger = logger;

            Rect = LifeCanvas.RectPool.Pop();
            Rect.SetValue(Canvas.LeftProperty, (double)(X * G.CELL_SIZE + X * 1));
            Rect.SetValue(Canvas.TopProperty, (double)(Y * G.CELL_SIZE + Y * 1));
            Rect.Tag = this;
            RandomizeState();
        }

        public bool ComputeNextState()
        {
            bool changed = false;
            if (State == CellState.Alive)
            {
                int aliveCount = Neighbours.Count(n => n.State == CellState.Alive);
                if (aliveCount < 2)
                {
                    NextState = CellState.Dead; //death by loneliness
                    changed = true;
                }
                else if (aliveCount > 3)
                {
                    NextState = CellState.Dead; //death by overpopulation
                    changed = true;
                }
                else if (aliveCount == 2 || aliveCount == 3) // live on; explicit for clarity
                {
                    NextState = CellState.Alive;
                }
            }
            else if (State == CellState.Dead)
            {
                if (Neighbours.Count(n => n.State == CellState.Alive) == 3)
                {
                    NextState = CellState.Alive; //born again
                    changed = true;
                }
                else
                {
                    NextState = CellState.Dead;
                }
            }
            return changed;
        }

        public void RandomizeState()
        {
            State = G.Rnd.NextDouble() > 0.7f ? CellState.Alive : CellState.Dead;
            NextState = State;
        }
    }
}
