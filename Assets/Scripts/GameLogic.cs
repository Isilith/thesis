using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour {

	public List<Transform> allAtoms = new List<Transform>();
	public List<GameObject> objects = new List<GameObject>();
	public List<Transform> atoms = new List<Transform>();
	private List<Transform> bonds = new List<Transform>();

	private int scanDistance = 2;

	void Update() {
		UIScript.allAtoms = allAtoms;
	}
	

	public void CreateMolecule(Transform selection) {
		objects.Clear ();
		allAtoms.Clear ();
		objects.AddRange(GameObject.FindGameObjectsWithTag("Atom"));	//is there a way to skip this?
		foreach(GameObject o in objects) {
			allAtoms.Add (o.transform);
		}

		atoms = FindAtoms(selection);
		bonds = CheckForBonds(selection, atoms);
		//kinda useless to check modulo but w/e
		if (bonds.Count > 0 && bonds.Count%2 == 0)
			Attach (bonds);
	}

	//Returns all of the atoms that are close to selection
	private List<Transform> FindAtoms(Transform selection) {
		List<Transform> ret = new List<Transform>();

		foreach(Transform atom in allAtoms) {
			if (atom == selection) continue;
			float distance = Vector3.Distance(selection.position, atom.position);
			if (distance < scanDistance) {
				Atom script = atom.GetComponent<Atom>();
				if (script.freeElectrons > 0)
					ret.Add(atom);
			}
		}

		return ret;
	}

	//Return the bond type for the two atoms
	private int CalculateBond(Atom current, Atom target) {
		int max = Mathf.Min (3, Mathf.Min(current.freeElectrons, target.freeElectrons));
		int ret = UIScript.type <= max ? UIScript.type : max;
		return ret;
	}
	

	//Returns the bonds of the two atoms that will be used for attaching them
	//also updates atom stats
	//TODO move this to be done before each rotation
	//	   now it reserves the bonds for no reason if atom is close enough when looking for bonds
	//     but not on the second rotation round
	private List<Transform> CheckForBonds(Transform selection, List<Transform> atoms) {
		List<Transform> ret = new List<Transform>();
		if (atoms.Count > 0) {	//change to look for free bond pairs or something
			Atom selectionAtom = selection.GetComponent<Atom>();
			foreach(Transform atom in atoms) {
				Atom targetAtom = atom.GetComponent<Atom>();
				int type = CalculateBond (selectionAtom, targetAtom);
			
				if (type > 0) {
					selectionAtom.attached.Add(new Atom.Connection(type,atom));
					targetAtom.attached.Add(new Atom.Connection(type,selection));

					Transform rettemp = selectionAtom.ChangeBondType(type, atom);
					if (rettemp != null) {
						ret.Add (rettemp);	//selection bond
						rettemp.GetComponent<Bond>().type = type;
						selectionAtom.UpdateElectrons(type);
					}
					rettemp = targetAtom.ChangeBondType(type, selection);
					if (rettemp != null) {
						ret.Add (rettemp);	//target bond
						rettemp.GetComponent<Bond>().type = type;
						targetAtom.UpdateElectrons(type);
					}
				}
			}
		}
		return ret;
	}

	//TODO delete empty gameobjects
	//TODO update gameobject position when atom is detached
	private void MakeMolecule(Transform bond1, Transform bond2) {
		//Create a Molecule gameobject or add to an existing one
		//if target atom is a part of a Molecule add selection to it
		//else create a new Molecule gameobject and add them both
		//to it
		if (bond2.root.name == "Molecule") {
			bond1.root.parent = bond2.root;
			GameObject newmole = new GameObject("Molecule");
			Vector3 pos = Vector3.zero;
			for (int j=0; j<bond2.root.childCount; j++) {
				pos += bond2.root.GetChild(j).position;
			}
			pos = pos/(bond2.root.childCount);
			newmole.transform.position = pos;
			Transform temp = bond2.root;
			int childcount = temp.childCount;
			int k = 0;
			while ( k < childcount ) {
				temp.GetChild(0).parent = newmole.transform;
				k++;
			}
			Destroy (temp.gameObject);	//delete the old Molecule object
		}
		//this happens when we add two separate atoms
		else {
			GameObject mole = new GameObject("Molecule");
			Vector3 pos = (bond1.position + bond2.position)/2;
			mole.transform.position = pos;
			bond1.root.parent = mole.transform;
			bond2.root.parent = mole.transform;
		}
	}

	//this is used for testing only
	private void Multiply(Vector3 a, Vector3 b, Vector3 c) {
		Vector3 m = new Vector3(a.x*b.x*c.x, a.y*b.y*c.y, a.z*b.z*c.z);
		Debug.Log (m);
	}

	private void AtomInteractions(Transform selection, Transform target) {
		AtomInteractions ai = (AtomInteractions)ScriptableObject.CreateInstance("AtomInteractions");
		if (selection.parent.name == "Carbon") {
		    if (target.parent.name == "Carbon") {
				ai.CarbonCarbonInteraction(selection, target);
			}
			if (target.parent.name == "Oxygen") {
				//don't know yet if this works or if it needs to be
				//CarbonOxygenInteraction instead
				ai.OxygenCarbonInteraction(selection, target);
			}
		}
		if (selection.parent.name == "Oxygen") {
			if (target.parent.name == "Carbon") {
				ai.OxygenCarbonInteraction(selection, target);
			}
		}
	}

	//Attaches the atoms
	//TODO rotating doesn't work after the first round ( when bonds.Count > 2 )
	       //can we fix it by making sure the atoms are always rotated properly,
		   //or is it better to just look for the bond pairs after each attachment?
	private void Attach (List<Transform> bonds) {
		//Make sure there's at least one pair
		if (bonds.Count >= 2) {
			//Go through all of the pairs
			for (int i=0; i<bonds.Count; i+=2) {
				Transform selection = bonds[i];
				Transform target = bonds[i+1];

				//Get the direction vectors for the bonds
				Vector3 dir1 = (selection.position - selection.parent.position).normalized;
				Vector3 dir2 = (target.position - target.parent.position).normalized;

				//Calculate the axis of rotation and angle and rotate the atom bonds to face each other
				Vector3 axis = Vector3.Cross(dir1, -dir2);
				float angle = Vector3.Angle(dir1, -dir2);
				if (axis == Vector3.zero) {
					axis = Vector3.Cross (selection.position, selection.parent.position);
				}
				selection.parent.Rotate(axis,angle,Space.World);

				//this doesn't work with hydrogen+anything, there's a small gap between the atoms
				//distance is fugged up, now less so but still is
				float distance = Vector3.Distance(selection.position, selection.parent.position) + Vector3.Distance (target.position, target.parent.position);
				selection.parent.position = target.parent.position;
				selection.parent.position += dir2*(distance+1);

				//Align atoms depending on things
				AtomInteractions(selection, target);

				//Rotate again so the double/triple bonds would align
				//TODO fix, at least O=C was broken, seems to be broken with oxygen
				if (target.childCount >= 2) {
					dir1 = target.GetChild(0).position - target.position;
					dir2 = selection.GetChild(0).position - selection.position;

					axis = selection.parent.position - target.parent.position;

					if (axis == Vector3.zero) {
						axis = Vector3.Cross (selection.position, selection.parent.position);
					}

					angle = Vector3.Angle(dir1, dir2);

					int max = 360/target.childCount;	//max angle that we need to rotate
					angle = angle > max ? angle-max : angle;
					selection.Rotate (axis,angle,Space.World);
				}

				selection.GetComponent<Bond>().attachedTo = target;
				target.GetComponent<Bond>().attachedTo = selection;

				//Debug.Log (Vector3.Distance(selection.position, selection.parent.position));
				//Debug.Log (Vector3.Distance (target.position, target.parent.position));

				MakeMolecule(selection, target);
			}
		}
		GameObject.Find ("Canvas").GetComponent<UIScript>().Uusi ();
		//UIScript.Uusi();

	}
}
