using System;

namespace Chancy
{
	public static class Library
	{
		public static Event Sequence (this Event ev)
		{
			return ev;
		}

		public static Event Compound(this Event ev)
		{
			return ev;
		}

		public static Event Loop (this Event ev)
		{
			return ev;
		}

		//
	}
}

