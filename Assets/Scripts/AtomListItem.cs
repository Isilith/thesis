using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AtomListItem {
	public Transform core = null;
	public List<Transform> bondedAtoms = new List<Transform>();

	public AtomListItem(Transform p, List<Transform> bonds) {
		core = p;
		bondedAtoms = bonds;
	}

	public string GetSymbol() {
		return core.GetComponent<Atom>().symbol;
	}
}
