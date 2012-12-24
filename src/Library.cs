using System;
using System.Collections.Generic;

namespace Chancy
{
	public static class Library
	{
		public static Event Sequence(this Event ev)
		{
			ev.MakeEventCollection();

			// Collect together all the younger siblings until we hit another flow modifier
			Queue<Event> seqEvents = new Queue<Event>();
			ev.Started += (args) =>
			{
				Event next = ev.Next;
				while(next != null)
				{
					Type[] md = next.GetSubscriberAttributes();
					//if(Array.Exists(md, s => s.Contains("<Compound>")))
					//{
					//	break;
					//}
					next = next.Next;
				}
			};

			return ev.Extend();
		}

		public static Event Compound(this Event ev)
		{
			ev.MakeEventCollection();

			ev.Started += (args) =>
			{
				Event next = ev.Next;
				while(next != null)
				{
				}
			};

			return ev.Extend();
		}

		public static Event Loop (this Event ev)
		{
			ev.MakeEventCollection();

			ev.Started += (args) =>
			{
			};

			return ev.Extend();
		}

		//
	}
}

