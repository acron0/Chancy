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
        private static Event CollectRelevantEvents(Action<Event> addFn, Event ev, Action extraOps = null)
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

            return returnEvent;
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

            CollectRelevantEvents(seqEvents.Enqueue, ev, nextEvent);

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
            CollectRelevantEvents(listEvents.Add, ev); ;

			return ev.Extend();
		}

		public static Event Loop (this Event ev)
		{
            ev.MakeEventCollection();
            Queue<Event> seqEvents = new Queue<Event>();
            CollectRelevantEvents(seqEvents.Enqueue, ev);

			return ev.Extend();
		}

		//
	}
}

