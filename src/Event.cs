using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

			public EventUpdateArgs (float deltaTime, float totalTime)
			{
				TotalTime = totalTime;
				DeltaTime = deltaTime;
			}
		};

		public class EventEndArgs
		{
		};

		public delegate void EventStartDelegate (EventStartArgs args);

		public delegate bool EventUpdateDelegate (EventUpdateArgs args);

		public delegate void EventEndDelegate (EventEndArgs args);

		/// <summary>
		/// Occurs when the Event starts.
		/// </summary>
		private event EventStartDelegate _started;

		public  event EventStartDelegate Started {
			add {
				if (_started == null)
					_started += value;
				else
					throw new Exception ("Events can only contain a single delegate for each state. Please use Event.Extend() to add a new Event to the stack");
			}

			remove {
				_started -= value;
			}
		}

		/// <summary>
		/// Occurs when update. Return 'true' to indicate that the Event is over.
		/// </summary>
		private event EventUpdateDelegate _updated;

		public  event EventUpdateDelegate Updated {
			add {
				if (_updated == null)
					_updated += value;
				else
					throw new Exception ("Events can only contain a single delegate for each state. Please use Event.Extend() to add a new Event to the stack");
			}

			remove {
				_updated -= value;
			}
		}

		/// <summary>
		/// Occurs when the Event ends.
		/// </summary>
		private event EventEndDelegate _ended;

		public  event EventEndDelegate Ended {
			add {
				if (_ended == null)
					_ended += value;
				else
					throw new Exception ("Events can only contain a single delegate for each state. Please use Event.Extend() to add a new Event to the stack");
			}

			remove {
				_ended -= value;
			}
		}		

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
		public bool IsRunning { get { return IsRunningSingular || AnySibling (e => e.IsRunningSingular || Controller.IsPending(e) );  } }

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
		static public Event Create ()
		{
			return new Event();
		}

		/// <summary>
		/// Extends this instance by creating a new Event and setting the sibl.
		/// </summary>
		public Event Extend ()
		{
			Event newEvent = new Event ();
			Next = newEvent;
			newEvent.Previous = this;
			return newEvent;
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		public Event Start ()
		{
			return StartEx(false, false);
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		internal Event StartEx (bool startImmediately, bool ignorePrevious )
		{
			if (IsRunningSingular)
				return this;

			// We ONLY want to start the top of the stack            
			if (Previous != null && !ignorePrevious) {
				Previous.Start ();
			} else {
				Controller.AddEvent (this, startImmediately);
				IsRunningSingular = true;
			}

			return this;
		}

		/// <summary>
		/// Starts the event.
		/// </summary>
		internal void StartEvent ()
		{
			_totalRunningTime = 0.0f;

			if (_started != null)
				_started (new EventStartArgs ());
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
		internal bool UpdateEvent (float dt)
		{
			float lastTTR = _totalRunningTime;
			_totalRunningTime += dt;

			if (_updated != null)
				return _updated (new EventUpdateArgs (lastTTR == 0.0f ? 0.0f : dt, lastTTR));
			else
				return true;
		}

		/// <summary>
		/// Ends the event.
		/// </summary>
		internal void EndEvent ()
		{
			if (_ended != null)
				_ended (new EventEndArgs ());

			IsRunningSingular = false;
		}

		/// <summary>
		/// Detach this event.
		/// </summary>
		internal void Detach()
		{
			Next = Previous = null;
		}

		/// <summary>
		/// Returns true if any sibling matches predicate
		/// </summary>
		/// <param name="match"></param>
		/// <returns></returns>
		internal bool AnySibling (Func<Event, bool> match)
		{
			if (!match (this)) {
				Event sibling = Previous;
				while (sibling != null) {
					if (match (sibling))
						return true;
					sibling = sibling.Previous;
				}

				sibling = Next;
				while (sibling != null) {
					if (match (sibling))
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

