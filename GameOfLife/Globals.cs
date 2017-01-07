using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GameOfLife
{
	public class G
	{
		public static uint CELL_SIZE = 15;
		public static Random Rnd = new Random((int)DateTime.Now.Ticks);
		public static Brush PickBrush()
		{
			Brush result = Brushes.Transparent;
			Type brushesType = typeof(Brushes);
			PropertyInfo[] properties = brushesType.GetProperties();

			int random = Rnd.Next(properties.Length);
			result = (Brush)properties[random].GetValue(null, null);
			return result;
		}
	}
}
