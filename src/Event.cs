using System;

namespace Chancy
{
	public class Event
	{
		#region Events/Delegates

		public class EventStartArgs
		{
		};

		public class EventUpdateArgs
		{
			public float TotalTime { get; private set; }
			public float DeltaTime { get; private set; }
			public EventUpdateArgs(float deltaTime, float totalTime)
			{
				TotalTime = totalTime;
				DeltaTime = deltaTime;
			}
		};

		public class EventEndArgs
		{
		};

		public delegate void EventStartDelegate(EventStartArgs args);
		public delegate bool EventUpdateDelegate(EventUpdateArgs args);
		public delegate void EventEndDelegate(EventEndArgs args);

		/// <summary>
		/// Occurs when the Event starts.
		/// </summary>
		public event EventStartDelegate Started;

		/// <summary>
		/// Occurs when update. Return 'true' to indicate that the Event is over.
		/// </summary>
		public event EventUpdateDelegate Updated;

		/// <summary>
		/// Occurs when the Event ends.
		/// </summary>
		public event EventEndDelegate Ended;

		#endregion

		#region Properties

		/// <summary>
		/// The next Event in the sequence.
		/// </summary>
		public Event Next { get; private set; }

		/// <summary>
		/// The previous Event in the sequence.
		/// </summary>
		public Event Previous { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this Event is running.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is running; otherwise, <c>false</c>.
		/// </value>
        public bool IsRunning { get { return IsRunningSingular || AnySibling(e => e.IsRunningSingular); } }

        /// <summary>
        /// Internal: Indicates whether this individual event is running (IsRunning is the whole event stack).
        /// </summary>
        /// <value>
        /// <c>true</c> if this singular instance is running; otherwise, <c>false</c>.
        /// </value>
        internal bool IsRunningSingular { get; private set; }

		#endregion

		#region Variables

		/// <summary>
		/// The total running time for this event.
		/// </summary>
		private float _totalRunningTime;

        /// <summary>
        /// Internal boolean for IsRunning property.
        /// </summary>
        private bool _isRunning;

		#endregion

		#region Methods

		/// <summary>
		/// Initializes a new instance of the <see cref="Chancy.Event"/> class.
		/// </summary>
		private Event ()
		{
			Next = Previous = null;
		}

		/// <summary>
		/// Public c'tor.	
		/// </summary>
		static public Event Create()
		{
			Event newEvent = new Event();
			Controller.AddEvent(newEvent);
			return newEvent;
		}

		/// <summary>
		/// Extends this instance by creating a new Event and setting the sibl.
		/// </summary>
		public Event Extend()
		{
			Event newEvent = new Event();
			Next = newEvent;
			newEvent.Previous = this;
			return newEvent;
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		public Event Start ()
		{
            // go both ways - return here because once we hit the bottom event we'll climb all the way back up.
            if (Next != null && !Next.IsRunning)
            {
                Next.Start();
                return this;
            }

            IsRunningSingular = true;
			if(Previous != null)
				Previous.Start ();
            return this;
		}

		/// <summary>
		/// Starts the event.
		/// </summary>
		internal void StartEvent()
		{
			_totalRunningTime = 0.0f;

			if(Started != null)
				Started(new EventStartArgs());
		}

		/// <summary>
		/// Updates the event.
		/// </summary>
		/// <returns>
		/// True, if the event has finished.
		/// </returns>
		/// <param name='dt'>
		/// The delta time since the last update.
		/// </param>
		internal bool UpdateEvent(float dt)
		{
			float lastTTR = _totalRunningTime;
			_totalRunningTime += dt;

			if(Updated != null)
				return Updated(new EventUpdateArgs(lastTTR == 0.0f ? 0.0f : dt, lastTTR));
			else
				return true;
		}

		/// <summary>
		/// Ends the event.
		/// </summary>
		internal void EndEvent()
		{
            IsRunningSingular = false;
			if(Ended != null)
				Ended(new EventEndArgs());
		}

        /// <summary>
        /// Returns true if any sibling matches predicate
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        internal bool AnySibling(Func<Event, bool> match)
        {
            if(!match(this))
            {
                Event sibling = Previous;
                while (sibling != null)
                {
                    if (match(sibling))
                        return true;
                    sibling = sibling.Previous;
                }

                sibling = Next;
                while (sibling != null)
                {
                    if (match(sibling))
                        return true;
                    sibling = sibling.Next;
                }

                return false;
            }

            return true;
        }

		#endregion
	}
}

