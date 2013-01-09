using System;
using System.Collections.Generic;

namespace Chancy
{
	public static class Controller
	{
		#region Variables

		/// <summary
		/// New events are added to this list before being appended to the update list.
		/// </summary>
		static private List<Event> _pendingAddPrimary;

		/// <summary
		/// New events are added to this list before being appended to the update list.
		/// </summary>
		static private List<Event> _pendingAddSecondary;

		/// <summary>
		/// The current events.
		/// </summary>
		static private List<Event> _currentEvents;

		/// <summary
		/// Pending removal list.
		/// </summary>
		static private List<Event> _pendingRemove;

		/// <summary>
		/// Indicates whether we're current in an update or not.
		/// </summary>
		static private bool _lockAdd;

		#endregion

		#region Methods

		/// <summary>
		/// Static c'tor
		/// </summary>
		static Controller ()
		{
			_pendingAddPrimary = new List<Event> ();
			_pendingAddSecondary = new List<Event> ();
			_currentEvents = new List<Event> ();
			_pendingRemove = new List<Event> ();
			_lockAdd = false;
		}

		/// <summary>
		/// Updates all the events with the specified deltaTime.
		/// </summary>
		/// <param name='deltaTime'>
		/// Delta time since last update.
		/// </param>
		static public void Update (float deltaTime)
		{
			_lockAdd = true;

			///

			foreach (Event e in _pendingAddPrimary) {
				e.StartEvent ();
				_currentEvents.Add (e);
			}
			_pendingAddPrimary.Clear ();

			///

			_lockAdd = false;

			///

			foreach (Event e in _pendingAddSecondary) {
				e.StartEvent ();
				_currentEvents.Add (e);
			}
			_pendingAddSecondary.Clear (); 

			///

			foreach (Event e in _currentEvents) {
				if (e.UpdateEvent (deltaTime))
				{
					if(e.Next != null)
						e.Next.StartEx(false, true);

					_pendingRemove.Add (e);
				}
			}

			///

			foreach (Event e in _pendingRemove) {
				e.EndEvent ();
				_currentEvents.Remove (e);
			}
			_pendingRemove.Clear ();
		}

		/// <summary>
		/// Adds an event to the 'pending add' list.
		/// </summary>
		/// <param name='newEvent'>
		/// The new event we're adding.
		/// </param>
		static internal void AddEvent (Event newEvent, bool startImmediately)
		{
			if(startImmediately)
			{
				newEvent.StartEvent ();
				_currentEvents.Add (newEvent);
			}
			else if (!_lockAdd)
			{
				_pendingAddPrimary.Add (newEvent);
			}
			else
			{
				_pendingAddSecondary.Add (newEvent);
			}
		}

		/// <summary>
		/// Determines whether this instance is pending the specified ev.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance is pending the specified ev; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='ev'>
		/// If set to <c>true</c> ev.
		/// </param>
		static internal bool IsPending(Event ev)
		{
			return _pendingAddPrimary.Contains(ev) || _pendingAddSecondary.Contains(ev);
		}

		#endregion
	}
}

