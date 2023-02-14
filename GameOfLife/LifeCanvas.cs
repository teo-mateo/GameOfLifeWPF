using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GameOfLife
{

	public class LifeCanvas : Canvas
	{
		public event EventHandler<GenericEventArgs<string>> LogEvent;
		private void OnLog(string payload)
		{
			if (LogEvent != null)
			{
				LogEvent(this, new GenericEventArgs<string>(payload));
			}
		}

		private bool _busy;
		public static Stack<Rectangle> RectPool = new Stack<Rectangle>();

		public Cell[,] Cells;
		public Stats Stats { get; set; }

		Brush _cAlive = new SolidColorBrush(Colors.LightGreen);
		Brush _cDying = new SolidColorBrush(Colors.Salmon);
		Brush _cBorn = new SolidColorBrush(Colors.Pink);

		public Controller Controller { get; private set; }

		internal void ComputeAllNextStates()
		{
			foreach (var cell in Cells)
				cell.ComputeNextState();
		}

		public uint CellWidth
		{
			get
			{
				return (uint)GetValue(CellWidthProperty);
			}
			set
			{
				SetValue(CellWidthProperty, value);
				Width = CellWidth * G.CELL_SIZE;
			}
		}
		public uint CellHeight
		{
			get
			{
				return (uint)GetValue(CellHeightProperty);
			}
			set
			{
				SetValue(CellHeightProperty, value);

			}
		}

		#region DepProps
		public static DependencyProperty CellWidthProperty = DependencyProperty.Register("CellWidth", typeof(uint), typeof(LifeCanvas), new PropertyMetadata()
		{
			PropertyChangedCallback = (d, e) =>
			{
				((LifeCanvas)d).Width = ((uint)e.NewValue) * G.CELL_SIZE + (uint)e.NewValue;
				((LifeCanvas)d).GenerateCells();
			}
		});


		public static DependencyProperty CellHeightProperty = DependencyProperty.Register("CellHeight", typeof(uint), typeof(LifeCanvas), new PropertyMetadata()
		{
			PropertyChangedCallback = (d, e) =>
			{
				((LifeCanvas)d).Height = ((uint)e.NewValue) * G.CELL_SIZE + (uint)e.NewValue;
				((LifeCanvas)d).GenerateCells();
			}
		});
		#endregion

		public LifeCanvas() : base()
		{
			Controller = new GameOfLife.Controller(this);
			Stats = new GameOfLife.Stats((int)(this.CellHeight * this.CellWidth));

			RectPool = new Stack<Rectangle>();
			for (int i = 0; i < 100000; i++)
			{
				Rectangle r = new Rectangle()
				{
					RadiusX = 2,
					RadiusY = 2,
					Width = G.CELL_SIZE,
					Height = G.CELL_SIZE
				};
				r.MouseEnter += (s, e) =>
				{
					r.Stroke = Brushes.Red;
				};

				r.MouseDown += (s, e) =>
				{
					var cell = (Cell)r.Tag;
					cell.State = (cell.State == CellState.Alive) ? CellState.Dead : CellState.Alive;
					ComputeAllNextStates();
					UpdateAllUI();
				};

				r.MouseLeave += (s, e) => r.Stroke = Brushes.Transparent;
				RectPool.Push(r);
			}

			base.Loaded += (s, e) => GenerateCells();
		}

		private void GenerateCells()
		{

			if (CellWidth == 0 || CellHeight == 0)
				return;

			Stopwatch sw = new Stopwatch();
			sw.Start();

			//clear
			foreach (var r in this.Children.OfType<Rectangle>())
			{
				LifeCanvas.RectPool.Push(r);
			}
			this.Children.Clear();

			Debug.WriteLine("Took " + sw.ElapsedMilliseconds + "ms to clear board");
			sw.Restart();


			#region axis labels, lines
			for (int q = 0; q < CellWidth; q++)
			{
				TextBlock tb = new TextBlock() { FontSize = 9, Foreground = Brushes.White };
				tb.Text = q.ToString();
				Canvas.SetLeft(tb, 2 + (G.CELL_SIZE * q + q));
				Canvas.SetTop(tb, -14);
				this.Children.Add(tb);

				Line ln = new Line()
				{
					X1 = q * G.CELL_SIZE + q,
					X2 = q * G.CELL_SIZE + q,
					Y1 = -5,
					Y2 = G.CELL_SIZE * CellHeight + CellHeight + 5,
					Stroke = Brushes.AliceBlue,
					Opacity = 0.3
				};
				this.Children.Add(ln);
			}

			for (int w = 0; w < CellHeight; w++)
			{
				TextBlock tb = new TextBlock() { FontSize = 9, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Right };
				tb.Text = w.ToString();
				Canvas.SetTop(tb, (G.CELL_SIZE * w + w));
				Canvas.SetLeft(tb, -10);
				this.Children.Add(tb);

				Line ln = new Line()
				{
					Y1 = w * G.CELL_SIZE + w,
					Y2 = w * G.CELL_SIZE + w,
					X1 = -5,
					X2 = G.CELL_SIZE * CellWidth + CellWidth + 5,
					Stroke = Brushes.AliceBlue,
					Opacity = 0.3
				};
				this.Children.Add(ln);
			}
			#endregion

			Cells = new Cell[CellWidth, CellHeight];
			for (int i = 0; i < CellWidth; i++)
			{
				for (int j = 0; j < CellHeight; j++)
				{
					Cells[i, j] = new Cell(i, j, l => OnLog(l));
					this.Children.Add(Cells[i, j].Rect);
				}
			}

			Debug.WriteLine("Took " + sw.ElapsedMilliseconds + "ms to fill board");
			sw.Restart();

			foreach (var cell in Cells)
			{
				int[][] positions;

				var xs = new int[] { cell.X - 1, cell.X, cell.X + 1 };
				var ys = new int[] { cell.Y - 1, cell.Y, cell.Y + 1 };

				positions =
					(from x in xs
					 from y in ys
					 where (
					 x >= 0 && y >= 0
					 &&
					 (x != cell.X || y != cell.Y)
					 &&
					 (x < this.CellWidth && y < this.CellHeight))
					 select new int[] { x, y }).ToArray();

				cell.Neighbours = positions.Select(p => Cells[p[0], p[1]]).ToArray();
				cell.ComputeNextState();
			}

			Debug.WriteLine("Took " + sw.ElapsedMilliseconds + "ms to determine each cell neighbours");
			sw.Stop();
		}
		public void Regen()
		{
			Controller.Pause();
			GenerateCells();
			UpdateAllUI();
		}

		public void Update()
		{
			if (_busy)
			{
				Debug.WriteLine("Busy, skipping update.");
				return;
			}

			List<Cell> toUpdate = new List<GameOfLife.Cell>();
			foreach (var cell in Cells)
			{
				if (cell.NextState != cell.State)
				{
					toUpdate.Add(cell);
					cell.State = cell.NextState;
				}
			}
			ComputeAllNextStates();
			UpdateUI(toUpdate.ToArray());
		}

		public void UpdateAllUI()
		{
			List<Cell> toUpdate = new List<GameOfLife.Cell>();
			foreach (var cell in Cells)
				toUpdate.Add(cell);

			UpdateUI(toUpdate.ToArray());
		}

		public void UpdateUI(params Cell[] toUpdate)
		{
			//in case of shutting down
			if (Application.Current == null)
				return;

			Application.Current.Dispatcher.InvokeAsync(() =>
			{
				try
				{
					_busy = true;
					foreach (var cell in toUpdate)
					{
						if (cell.State == CellState.Alive && cell.NextState == CellState.Alive)
						{
							cell.Rect.Fill = _cAlive;
							cell.Rect.Opacity = 1.0;
						}
						else if (cell.State == CellState.Alive && cell.NextState == CellState.Dead)
						{
							cell.Rect.Fill = _cDying;
							cell.Rect.Opacity = 0.9;
						}
						else if (cell.State == CellState.Dead && cell.NextState == CellState.Alive)
						{
							cell.Rect.Fill = _cBorn;
							cell.Rect.Opacity = 0.2;
						}
						else
						{
							cell.Rect.Fill = Brushes.Transparent;
						}

					}
				}
				finally
				{
					_busy = false;
				}
			});
		}

		public void UpdateStats()
		{
			int alive = 0;
			foreach (var cell in Cells)
				alive += (cell.State == CellState.Alive) ? 1 : 0;

			this.Stats.TotalAlive = alive;
		}

		internal void KillAll()
		{
			foreach (var cell in Cells)
			{
				if (cell.State == CellState.Alive)
					cell.State = CellState.Dead;
				if (cell.NextState == CellState.Alive)
					cell.NextState = CellState.Dead;
			}

			Application.Current.Dispatcher.Invoke(UpdateAllUI);
		}
	}





}
