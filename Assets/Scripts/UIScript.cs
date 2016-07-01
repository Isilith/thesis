using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class UIScript : MonoBehaviour {

	public Text formula = null;
	public RectTransform topPanel = null;
	public RectTransform trash;
	

	public static Transform select = null;	//atom being dragged
	public static List<Transform> allAtoms = new List<Transform>();
	private int oldcount = 0;	//used to determine when we need to update
	public static int type = 1;

	public void TypeChanged(int index) {
		type = index;
	}

	private List<Atom.Connection> GetBondedAtoms(Transform parent) {
		return parent.GetComponent<Atom>().attached;
	}


	private int FindAtomInList(Transform target, List<Transform> list) {
		for (int i=0; i<list.Count; i++) {
			if (list[i] == target) {
				return i;
			}
		}
		return -1;
	}

	private Bond FindBondTo(Transform source, Transform target) {
		for (int i=0; i<source.childCount; i++) {
			if (source.GetChild(i).GetComponent<Bond>().attachedTo == target)
				return source.GetChild (i).GetComponent<Bond>();
		}
		//Debug.Log ("FindBondTo("+source+" ,"+target+") returned null.");
		return null;
	}

	//https://www.cas.org/training/stneasytips/subinforformula1.html for special cases
	public void Uusi() {
		//Debug.Log ("Uusi");
		List<AtomListItem> atomList = new List<AtomListItem>();
		bool hillSystemOK = false;

		foreach(Transform atom in allAtoms) {
			//one iteration
			List<Transform> list = new List<Transform>();
			for(int i=0; i<atom.childCount; i++) {
				Transform child = atom.GetChild(i);
				Bond childBond = child.GetComponent<Bond>();
				if (childBond.attachedTo != null) {
					//childBond.attachedTo returns a bond-gameobject so we need its parent
					list.Add(childBond.attachedTo.parent);
				}
			}
			atomList.Add(new AtomListItem(atom, list));
			//Check list for carbon
			if (atom.name == "Carbon") {
				//We can use Hill system
				hillSystemOK = true;
			}
		}
		//TODO check the logic here, it's faulty
		//     need to find the functional groups, then remove them from the formula,
		//     arrange the rest and add the groups back
		/*if (hillSystemOK) {
			//move carbon and hydrogen to the front and sort the rest
			atomList.Sort (new HillSystemComparer());
		}
		else {
			//Sort alphabetically
			atomList.Sort (new AlphabeticalComparer());
		}*/

		bool alcohol = false;
		bool carboxyl = false;
		Transform fgrouptransform = null;
		List<AtomListItem> temp = new List<AtomListItem>();
		//Look for any functional groups and move them to the end of the list
		for (int a=0; a<atomList.Count; a++) {
			string s = "";
			string core = atomList[a].core.GetComponent<Atom>().symbol;
			string bonded = "";
			for (int i=0; i<atomList[a].bondedAtoms.Count; i++) {
				bonded += atomList[a].bondedAtoms[i].GetComponent<Atom>().symbol;
			}
			s = core + bonded;
			Debug.Log (s);

			//don't think this can even be OHC but just in case
			if (s == "OCH" || s == "OHC") {
				for (int j=0; j<atomList[a].bondedAtoms.Count; j++) {
					if (atomList[a].bondedAtoms[j].name == "Carbon") {
						fgrouptransform = atomList[a].bondedAtoms[j];
						break;
					}
				}
				temp = ArrangeAlcohol(atomList, a, s);
				alcohol = true;
			}
		}
		//if we have alcohol we need to check for a double bond oxygen
		if (alcohol) {
			for (int a=0; a<atomList.Count; a++) {
				string s = "";
				string core = atomList[a].core.GetComponent<Atom>().symbol;
				string bonded = "";
				for (int i=0; i<atomList[a].bondedAtoms.Count; i++) {
					bonded += atomList[a].bondedAtoms[i].GetComponent<Atom>().symbol;
				}
				s = core + bonded;
				//Debug.Log (s);
				//TODO also check that the COOH-group is attached to hydrocarbon
				if (s == "OC") {
					if (fgrouptransform == atomList[a].bondedAtoms[0]) {
						Debug.Log ("Joo");
						Bond b = atomList[a].core.GetChild(0).GetComponent<Bond>();
						if (b.type == 2) {
							//we have an OH-group and a double bond oxygen
							carboxyl = true;
							alcohol = false;
							temp = ArrangeCarboxyl(atomList, a, s);
						}
					} else {Debug.Log ("Ei");}
				}
			}
		}

		for (int i=0; i<temp.Count; i++) {
			atomList.Remove(temp[i]);
		}
		if (hillSystemOK) {
			//move carbon and hydrogen to the front and sort the rest
			atomList.Sort (new HillSystemComparer());
		}
		else {
			//Sort alphabetically
			atomList.Sort (new AlphabeticalComparer());
		}
		//TODO combine the letters
		//for debugging purposes
		string text = "";
		foreach(AtomListItem item in atomList) {
			text += item.core.GetComponent<Atom>().symbol;
		}
		text = CompressFormula(text);

		atomList.AddRange(temp);
		for(int i=0; i<temp.Count; i++) {
			text += temp[i].core.GetComponent<Atom>().symbol;
		}

		formula.text = text;
	}

	private void MoveListElementToEnd(List<AtomListItem> list, int i) {
		AtomListItem removed = list.ElementAt(i);
		list.RemoveAt(i);
		list.Insert(list.Count, removed);
	}

	//a : index of double bond oxygen
	//s : not used
	private List<AtomListItem> ArrangeCarboxyl(List<AtomListItem> list, int a, string s) {
		//CHOOH -> H-COOH
		//C -> O(double) -> O-H
		//a = oxygen index
		List<AtomListItem> ret = new List<AtomListItem>();
		Transform carbon_transform = list[a].bondedAtoms[0];
		AtomListItem oxygen_d = list[a];
		AtomListItem carbon = null;
		foreach(AtomListItem item in list) {
			if (item.core == carbon_transform) {
				carbon = item;
				break;
			}
		}
		list.Remove(oxygen_d);
		list.Remove(carbon);

		//TODO make sure that these get the right items (they don't right now)
		AtomListItem hydrogen = list.Last();
		list.Remove(list.Last());
		AtomListItem oxygen_s = list.Last();
		list.Remove (list.Last());

		ret.Add(carbon);
		ret.Add (oxygen_d);
		ret.Add (oxygen_s);
		ret.Add(hydrogen);
		list.Remove (oxygen_d);
		list.Remove (carbon);
		return ret;
	}

	//arranges list so that the OH-group is at the end
	private List<AtomListItem> ArrangeAlcohol(List<AtomListItem> list, int a, string s) {
		List<AtomListItem> ret = new List<AtomListItem>();
		//list[a] = oxygen
		AtomListItem oxygen = list[a];

		Transform hydrogen_transform = oxygen.bondedAtoms[1];
		AtomListItem hydrogen = null;
		foreach(AtomListItem item in list) {
			if (item.core == hydrogen_transform) {
				hydrogen = item;
				break;
			}
		}
		ret.Add (oxygen);
		ret.Add (hydrogen);
		//list.Remove (oxygen);
		//list.Remove (hydrogen);
		return ret;
	}

	private void MoleculeFormula() {
		string text = "";
		Uusi ();
		List<Atom.Connection> bonded = new List<Atom.Connection>();
		bool[] visited = new bool[allAtoms.Count];	//used to track which atoms in the molecule have been visited

		//Run through all of the atoms in the game
		for (int i=0; i<allAtoms.Count; i++) {
			Transform atom = allAtoms[i];
			Atom script = atom.GetComponent<Atom>();

			//if atom is hydrogen we take the atom it's attached to
			if (script != null && script.symbol == "H") {	//TODO do we need to check if atom has been visited?
				if (script.attached.Count > 0)
					atom = script.attached[0].atom;
			}

			//Get the atom index in our list and add it as visited
			int ind = FindAtomInList(atom, allAtoms);
			if (ind != -1 && !visited[ind]) {
				text += atom.GetComponent<Atom>().symbol;
				visited[ind] = true;
			}

			//Get atoms that have a bond with atom
			bonded = GetBondedAtoms(atom);
			foreach (Atom.Connection a in bonded) {
				if (a.atom != null) {
					//Get the atom index in our list and add it as visited
					ind = FindAtomInList(a.atom, allAtoms);
					if (ind != -1 && !visited[ind]) {
						//if atom has things attached to it we skip it and check it later
						if (a.atom.GetComponent<Atom>().attached.Count > 1)
							continue;
						text += a.atom.GetComponent<Atom>().symbol;
						visited[ind] = true;
					}
				}
			}
		}
		//text = FormulateMore(text);
		text = CompressFormula(text);
		formula.text = text;
		//AndMore(text);
	}


	/*
	 * Compress the generated formula to a more familiar form. This needs to
	 * be further modified to get the final form using FindFunctionalGroups()
	 * Example:
	 * CompressForumula("CHHOHCHHH") === "CH2OHCH3"
	 */
	private string CompressFormula(string text) {
		string ret = "";	//compressed text
		int count = 1;		//counter for consecutive letters
		if (text.Length > 0) {
			//Go through each character in text
			foreach(char ch in text) {
				/* if ret has something in it and the character is not the 
				 * same as the last one was we add the letter to the 'ret' string
				 */
				if (ret.Length > 0 && ch != ret.Last()) {
					string add = count > 1 ? ""+count+ch : ""+ch;
					ret = ret.Insert(ret.Length, add);
					count = 1;
				}
				/* if we are at the first character of the text
				 * we simply add the character to the 'ret' string
				 */
				else if (ret.Length == 0) {
					ret = ret.Insert(0, ""+ch);
				}
				/* if the character is the same as last one was
				 * we increase the counter for that letter and move
				 * on to the next
				 */
				else if (ch == ret.Last()) {
					count++;
				}
			}
			// handle the last letter in the 'ret'
			string end = count > 1 ? ""+count : "";
			ret = ret.Insert(ret.Length, end);
		}
		return ret;
	}

	private string AndMore(string text) {
		string ret = "";
		//int count = 1;
		if (text.Length > 0) {
		}
		return ret;
	}

	/*
	 * Generate the final form for molecule formula
	 * Example:
	 * text - CH2OHCH3
	 * ret - CH2CH3OH
	 * 
	 * TODO write a separate search function for each
	 * functional group instead of just looking for strings 
	 * from text
	 * 
	 * e.g. oxygen that has a hydrogen atom, attached to carbon = alcohol
	 *      carbon with a double bond oxygen and alcohol = aldehyd?
	 *      etc. etc.
	 */
	private string FormulateMore(string text) {
		string ret = text;
		string[] findses = {"OH"};
		//change to look for an actual OH from the molecule
		foreach(string s in findses) {
			int ind = text.LastIndexOf(s);
			if (ind > -1) {
				string sub = ret.Remove(ind, s.Length);
				sub = sub.Insert(sub.Length, s);
				ret = sub;
			}
		}
		return ret;
	}


	void Update() {
		//Update formula when atom count changes
		if (oldcount != allAtoms.Count) {
			oldcount = allAtoms.Count;
			//MoleculeFormula();
			//Uusi ();
		}

		//dragging something
		if (select != null) {
			Rect box = new Rect(Screen.width-trash.rect.width,
			                    Screen.height-trash.rect.height,
			                    trash.rect.width,
			                    trash.rect.height);
			Vector3 pos = Camera.main.WorldToScreenPoint(select.position);    
			pos.y = Screen.height - pos.y;

			//If dragged atom is over the trash can, set destroy to true
			if (box.Contains(pos)) {
				select.GetComponent<Atom>().destroy = true;
				trash.GetComponent<Image>().color = new Color(0.7f,0f,0f);
			} else {
				select.GetComponent<Atom>().destroy = false;
				trash.GetComponent<Image>().color = new Color(.7f,.7f,.7f);
			}
		}
	}


}
