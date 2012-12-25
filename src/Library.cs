using System;
using System.Collections.Generic;

namespace Chancy
{
	public static class Library
	{
        /// <summary>
        /// Creates a Started delegate for event collections.
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        private static void SetUpEventCollectionDelegates(Action<Event> addFn, Event ev, Action extraOps = null)
        {
            Queue<Event> seqEvents = new Queue<Event>();
            Event returnEvent = null;
            ev.Started += (args) =>
            {
                Event next = ev.Next;
                while (next != null)
                {
                    if (next.IsEventCollection)
                    {
                        returnEvent = next;
                        next = null;
                    }
                    else
                    {
                        addFn(next);
                        next = next.Next;
                    }
                }

                if (extraOps != null)
                    extraOps();
            };

            ev.Ended += (args) =>
                {
                    if (returnEvent != null)
                        returnEvent.Start(true);
				};
        }

		public static Event Sequence(this Event ev)
		{
			ev.MakeEventCollection();
            Queue<Event> seqEvents = new Queue<Event>();
            Event top = null;
            Action nextEvent = () =>
            {
                top = seqEvents.Dequeue();
                top.Start(true);
            };

            SetUpEventCollectionDelegates(seqEvents.Enqueue, ev, nextEvent);

            ev.Updated += (args) =>
                {
                    if(top.IsRunningSingular)
                    {
                        return false;
                    }
                    else if(seqEvents.Count != 0)
                    {
                        nextEvent();
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                };

			return ev.Extend();
		}

		public static Event Compound(this Event ev)
		{
            ev.MakeEventCollection();
            List<Event> listEvents = new List<Event>();
			Action allStart = () =>
            {
                listEvents.ForEach(e => e.Start (true));
            };

            SetUpEventCollectionDelegates(listEvents.Add, ev, allStart);

			ev.Updated += (args) => 
			{
				return listEvents.TrueForAll(e => !e.IsRunningSingular);
			};

			return ev.Extend();
		}

		public static Event Loop (this Event ev, int times)
		{
            ev.MakeEventCollection();
            Queue<Event> seqEvents = new Queue<Event>();
            SetUpEventCollectionDelegates(seqEvents.Enqueue, ev);

			//ev.Updated += (args) => 
			//{
			//	ret
			//};

			return ev.Extend();
		}

		//
	}
}

