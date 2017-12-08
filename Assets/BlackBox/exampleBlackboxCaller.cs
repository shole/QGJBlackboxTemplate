using System.Collections;
using UnityEngine;

public class exampleBlackboxCaller : MonoBehaviour {

	public float callInterval = 0.5f;

	private ParticleSystem ps;
	
	void Start() {
		ps = GameObject.FindObjectOfType<ParticleSystem>();
		BlackBox.Instance.QueryResponse = BlackBoxCallback; // get the response after processing
		StartCoroutine(sendPoll());
	}

	IEnumerator sendPoll() { // simple poller
		while ( true ) {
			yield return new WaitForSeconds(callInterval);

			BlackBox.BlackBoxQuery query = new BlackBox.BlackBoxQuery(true);

			// just random values (within acceptable range)
			query.terminalA = Random.Range(1, 500);
			query.terminalB = Random.Range(1, 5000);
			query.terminalC = Random.Range(1, 5000);
			query.drive = Random.Range(-2f, 2f);
			query.position = Random.Range(-1f, 1f);
			query.level = Random.Range(0, 5);

			BlackBox.Instance.SendQuery(query);
		}
	}


	void BlackBoxCallback(BlackBox.BlackBoxQuery query, float score, float echo) {
		ParticleSystem.EmitParams p=new ParticleSystem.EmitParams();
		
		p.position=new Vector3(
			query.drive,
			(float)query.level / 2f,
			query.position * 2
		);
		p.startColor=new Color(
			score,
			echo,
			0f
		);
		p.startSize3D= new Vector3(
				query.terminalA / 500f,
				query.terminalB / 5000f,
				query.terminalC / 5000f
			)
			* 0.1f;
		
		ps.Emit(p,1);
		/*
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		
		go.transform.localScale = new Vector3(
			                          query.terminalA / 500f,
			                          query.terminalB / 5000f,
			                          query.terminalC / 5000f
		                          )
		                          * 0.1f;
		go.GetComponent<Renderer>().material.color = new Color(
			score,
			echo,
			0f
		);
		go.transform.position = new Vector3(
			query.drive,
			(float)query.level / 2f,
			query.position * 2
		);
		*/
	}
}
