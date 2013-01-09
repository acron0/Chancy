using System;
using System.Collections.Generic;

namespace Chancy
{
	public static class Library
	{
		public static Event Sequence (this Event ev, Action<Event> action)
		{
			Event seq = Event.Create();
			action(seq);

			ev.Started += (args) =>
			{
				seq.StartEx(true, true);
			};

			ev.Updated += (args) =>
			{
				if (seq.IsRunningSingular) {
					return false;
				} else if (seq.Next != null) {
					seq = seq.Next;
					seq.StartEx (true, true);
					return false;
				} else {
					return true;
				}
			};

			return ev.Extend();
		}

		public static Event Compound (this Event ev, Action<Event> action)
		{
			List<Event> events = new List<Event> ();
			Event comp = Event.Create();
			action(comp);

			ev.Started += (args) => 
			{
				Event e = comp;
				do
				{
					events.Add(e);
					e.StartEx(true, true);
					Event t = e.Next;
					e.Detach();
					e = t;

				}while(e != null);
			};

			ev.Updated += (args) => 
			{
				return events.TrueForAll(e=>!e.IsRunningSingular);
			};

			return ev.Extend ();
		}

		public static Event Loop (this Event ev, int times, Action<Event> action)
		{
			Queue<Event> seqEvents = new Queue<Event> ();

			//ev.Updated += (args) => 
			//{
			//	ret
			//};

			return ev.Extend ();
		}
	}
}

