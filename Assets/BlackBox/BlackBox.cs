using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackBox : MonoBehaviour {

	public static BlackBox Instance;

	void Awake() {
		Instance = this;
	}

	private StreamedProcessPool proc;

	public delegate void BlackBoxCallback(BlackBoxQuery query, float score, float echo);

	public BlackBoxCallback QueryResponse;

	Dictionary<int, BlackBoxQuery> callRegistry = new Dictionary<int, BlackBoxQuery>();

	public struct BlackBoxQuery {
		public int terminalA;
		public int terminalB;
		public int terminalC;
		public float drive;
		public float position;
		public int level;

		public BlackBoxQuery(bool init) { // default value constructor
			terminalA = 2;
			terminalB = 3;
			terminalC = 3;
			drive = 0.1f;
			position = 1.1f;
			level = 4;
		}

		public string ToString() {
			return terminalA
			       + "\n"
			       + terminalB
			       + "\n"
			       + terminalC
			       + "\n"
			       + drive
			       + "\n"
			       + position
			       + "\n"
			       + level;
		}

		/*
		public string ToString() {
			return terminalA
			       + ","
			       + terminalB
			       + ","
			       + terminalC
			       + ","
			       + drive
			       + ","
			       + position
			       + ","
			       + level;
		}
		*/
	}

	public void SendQuery( // simplified version with default terminals
		float drive = 0.1f,
		float position = 1.1f,
		int level = 4) {
		BlackBoxQuery query = new BlackBoxQuery(true);
		query.drive = drive;
		query.position = position;
		query.level = level;
		SendQuery(query);
	}

	public void SendQuery(
		int terminalA = 2,
		int terminalB = 3,
		int terminalC = 3,
		float drive = 0.1f,
		float position = 1.1f,
		int level = 4) {
		BlackBoxQuery query = new BlackBoxQuery(true);
		query.terminalA = terminalA;
		query.terminalB = terminalB;
		query.terminalC = terminalC;
		query.drive = drive;
		query.position = position;
		query.level = level;
		SendQuery(query);
	}

	public void SendQuery(BlackBoxQuery query) {
		if (
			0 < query.terminalA
			&& 0 < query.terminalB
			&& 0 < query.terminalC
			&& -1 <= query.position
			&& query.position <= 1
			&& -2 <= query.drive
			&& query.drive <= 2
			&& 0 <= query.level
			&& query.level <= 5
		) {
			int guid = proc.StdIn("" + query.ToString());
			callRegistry.Add(guid, query);
		} else {
			throw new Exception("Query value out of range!");
		}
	}

	void Start() {
		proc = GetComponent<StreamedProcessPool>();
		proc.StdOut = StdOut;
		proc.StdErr = (process, message) => { // just to suppress errors due to app quit
			return false;
		};
	}

	// Standard Output message handler - return true when we think process is ready to take more input
	bool StdOut(StreamedProcess proc, string message) {
		if ( string.IsNullOrEmpty(message) ) {
			return false;
		}
		message = message.Trim(); // cleanup response
		//Debug.Log("message "+message);

		if ( message.StartsWith("Enter terminalA") ) { //  process has sent message that it's ready to receive new input
			// we don't need to do anything with this - just return true
			return true;
		}

		float score, echo;
		if ( message.StartsWith("Score, Echo:") ) {
			// do something with received data

			string[] scoreecho = message.Split(':')[1].Split(',');
			score = float.Parse(scoreecho[0]);
			echo = float.Parse(scoreecho[1]);

			BlackBoxQuery query = callRegistry[proc.GUID]; // you can use the GUID to link sends to receives
			callRegistry.Remove(proc.GUID);

			if ( QueryResponse == null ) {
				Debug.Log(proc.index + " stdout " + query.ToString().Replace('\n', ',') + ", score " + score + " echo " + echo);
			} else {
				QueryResponse(query, score, echo);
			}
		} else {
			Debug.Log("some invalid message " + message);
		}

		// else, some invalid message

		return false; // this was not the last line of data
	}
	/*
	// if you are just expecting single line responses, it's ok for every line to return true
	bool StdOut(StreamedProcess proc, string message) {
		Debug.Log(proc.index + " stdout " + message);
		return true;
	}
	*/

}
