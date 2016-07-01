using UnityEngine;
using System.Collections;

public class AtomCreation : MonoBehaviour {

	public Transform prefab;
	private Vector3 curPosition;
	private Vector3 curScreenSpace;
	private Vector3 screenSpace;
	private Vector3 offset;
	private Transform clone;
	
	public void OnMouseDown() {

		curScreenSpace = new Vector3(Input.mousePosition.x, 
		                             Input.mousePosition.y, 
		                             transform.position.z+15f);
		curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace);

		clone = (Transform)Instantiate(prefab, curPosition, prefab.transform.rotation);
		clone.tag = "Atom";
		clone.name = prefab.name;
		screenSpace = Camera.main.WorldToScreenPoint(clone.transform.position);
		offset = clone.transform.root.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y, screenSpace.z));
		UIScript.select = clone;


	}

	public void OnMouseUp() {
		if (clone.GetComponent<Atom>().destroy) {
			clone.GetComponent<Atom>().OnDestroy();
			return;
		}
		Camera.main.GetComponent<GameLogic>().CreateMolecule(clone);
		UIScript.select = null;
	}

	public void OnMouseDrag() {
		curScreenSpace = new Vector3(Input.mousePosition.x, 
		                             Input.mousePosition.y, 
		                             transform.position.z+15f);
		curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace)+offset;
		clone.position = curPosition;
	}
}
