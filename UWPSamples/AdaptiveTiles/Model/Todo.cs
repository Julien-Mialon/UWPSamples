using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveTiles.Model
{
	public class Todo
	{
		public string Title { get; set; }

		public string Description { get; set; }

		public State State { get; set; }
	}
}
