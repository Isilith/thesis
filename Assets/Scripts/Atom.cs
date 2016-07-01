using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Atom : MonoBehaviour {

	public class Connection {
		public int type;
		public Transform atom;
		public Connection(int t, Transform a) {
			type = t;
			atom = a;
		}
	}

	public Transform singlebond;
	public Transform doublebond;
	public Transform triplebond;

	public Transform singlebak;
	public Transform doublebak;
	public Transform triplebak;

	//Atom attributes
	public string atomname;
	public string symbol;
	public int maxOuterElectrons;
	public int outerElectrons;
	public int freeElectrons;
	public int atomnum;
	public float weight;
	public int group;
	public int period;
	public float negativity;
	public List<Vector3> bondPositions = new List<Vector3>();
	public List<Connection> attached = new List<Connection>();

	//Position attributes
	private Vector3 curPosition;
	private Vector3 curScreenSpace;
	private Vector3 screenSpace;
	private Vector3 offset;
	private Vector3 originalPosition;

	private List<List<Vector3>> positions = new List<List<Vector3>>();

	public bool destroy = false;
	public RectTransform trash = null;
	public bool NotInUse = true;

	private void Create(string n, string s, int maxE, int outerE, int freeE,
	                    int atomN, float w, int g, int p, float neg)
	{
		atomname = n;
		symbol = s;
		maxOuterElectrons = maxE;
		outerElectrons = outerE;	
		freeElectrons = freeE;														
		atomnum = atomN;
		weight = w;
		group = g;
		period = p;
		negativity = neg;
		bondPositions = positions[freeE-1];
	}
	
	private void Initialize() {	
		Helper help = new Helper();
		help.Start();
		positions.Add (help.one);
		positions.Add (help.two);
		positions.Add (help.three);
		positions.Add (help.four);

		//Initialize UI components
		Canvas c = GameObject.Find ("Canvas").GetComponent<Canvas>();
		RectTransform bottompanel = (RectTransform)c.transform.FindChild("BottomPanel");
		trash = (RectTransform)bottompanel.FindChild("TrashPanel");
	}
	
	private void CreateBonds() {
		foreach(Vector3 pos in bondPositions) {
			Transform bond = (Transform)Instantiate(singlebond, transform.position, Quaternion.identity);
			bond.parent = transform;
			bond.localPosition = pos;
			bond.name = "AtomBond";
			bond.LookAt(transform);
			bond.Rotate (90f,0f,0f);
			bond.GetComponent<Bond>().originalPositions.Add(pos);
			bond.GetComponent<Bond>().position = pos;
		}
	}
	
	private Vector3 CalculatePosition() {
		Vector3 ret = Vector3.zero;
		for (int i=0; i<transform.childCount; i++) {
			ret += transform.GetChild(i).localPosition;
		}
		ret = transform.childCount > 0 ? -ret/transform.childCount : Vector3.up*0.5f;
		//Ret is now the average of the remaining bonds


		if (transform.name == "Oxygen") {
			//if bond is single bond we can reflect the position
			if (transform.childCount > 0) {

			}
		}
		return ret;
	}

	public Transform GetPrefabOfType(int t) {
		Transform prefab = null;
		singlebond = singlebak;
		doublebond = doublebak;
		triplebond = triplebak;
		switch(t) {
		case 0:
			Debug.Log ("Type is 0");
			break;
		case 1: 
			prefab = singlebond;
			break;
		case 2:
			prefab = doublebond;
			break;
		case 3:
			prefab = triplebond;
			break;
		}
		return prefab;
	}

	private Transform GetNextFreeBond() {
		Transform ret = null;
		Transform child = null;
		for (int i=0; i<transform.childCount; i++) {
			child = transform.GetChild(i);
			if (child.GetComponent<Bond>().attachedTo == null) {
				ret = child;
				break;
			}
		}
		return ret;
	}
	
	public Transform GetClosestFreeBond(Transform target) {
		Transform ret = null;
		float minDistance = Mathf.Infinity;
		for (int i=0; i<transform.childCount; i++) {
			float distance = Vector3.Distance(transform.GetChild (i).position, target.position);
			if (distance < minDistance) {
				if (transform.GetChild(i).GetComponent<Bond>().attachedTo == null) {
					minDistance = distance;
					ret = transform.GetChild(i);
				}
			}
		}
		return ret;
	}

	//TODO fix oxygen atom. 22.12. what's broken with oxygen?
	public Transform ChangeBondType(int type, Transform target) {
		int destroyed = 0;
		Transform prefab = GetPrefabOfType(type);

		//if type was 0
		if (prefab == null) return null;

		Transform bond = null;	//this bond will be used for attachment
		if (type == 1) {
			//bond = GetNextFreeBond();
			bond = GetClosestFreeBond(target);
			bond.SetAsLastSibling();
		}
		//TODO check that this destroys the right bonds, it might already
		if (type > 1) {
			bond = (Transform)Instantiate (prefab, transform.position, Quaternion.identity);
			for (int i=0; i<bondPositions.Count; i++) {
				if (destroyed == type) 
					break;
				bond.GetComponent<Bond>().originalPositions.Add (transform.GetChild(i).localPosition);
				DestroyImmediate(transform.GetChild(i).gameObject);
				destroyed++;
				i--;
			}
			Vector3 newpos = CalculatePosition().normalized*0.5f;
			bond.parent = transform;
			bond.name = "AtomBond";
			bond.localPosition = newpos;
			bond.LookAt(transform);
			bond.Rotate (90f,0f,0f);
			bond.GetComponent<Bond>().position = newpos;
		}

		return bond;
	}

	public void UpdateElectrons(int type) {
		freeElectrons -= type;
		outerElectrons += type;
	}

	/* Initialize variables and create the right atom depending on the prefab name */
	private void Start() {
		Initialize();
		if (gameObject.name == "Carbon") {
			Create("Carbon", "C", 8, 4, 4, 6, 12.01f, 14, 2, 2.55f);
			CreateBonds();
		}
		else if (gameObject.name == "Nitrogen") {
			Create("Nitrogen", "N", 8, 5, 3, 7, 14.01f, 15, 2, 3.0f);
			//Add bonds to the created atom
			CreateBonds();
			for (int i=0; i<3; i++) {
				Vector3 pos = transform.GetChild(i).localPosition;
				transform.GetChild(i).localPosition = new Vector3(pos.x,pos.y,0.197599f);	//huhhuh ??
				transform.GetChild(i).LookAt(transform);
				transform.GetChild(i).Rotate(90f,0f,0f);
			}

		}
		else if (gameObject.name == "Oxygen") {
			Create("Oxygen", "O", 8, 6, 2, 8, 16.0f, 16, 2, 3.5f);
			CreateBonds();
		}
		else if (gameObject.name == "Hydrogen") {
			Create("Hydrogen", "H", 2, 1, 1, 1, 1.008f, 1, 1, 2.1f);
			CreateBonds();
		}
	}

	public void OnMouseDown() {
		if (CameraMovement.reset) {
			Reset();
		}
		if (!CameraMovement.rotating) {
			screenSpace = Camera.main.WorldToScreenPoint(transform.root.position);
			offset = transform.root.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y, screenSpace.z));

			originalPosition = transform.position;
		}
		UIScript.select = transform;
	}

	//TODO remove connections on destroy, isn't this done? dunno
	public void OnDestroy() {
		int destroyCount = 0;
		//destroy = false;	//we delete the atom anyway so this might be redundant
		Transform parent = transform.root;
		//if transform is a part of a molecule we destroy the whole molecule
		if (parent.name == "Molecule") {
			foreach(Transform child in parent) {
				UIScript.allAtoms.Remove(child);
				//if atom is held over the trash can icon we change it's color
				if (trash != null) 
					trash.GetComponent<Image>().color = new Color(.7f,.7f,.7f);

				Destroy(child.gameObject);
				destroyCount++;
			}
			Destroy(parent.gameObject);
			//else we are dealing with only one atom
		} else {
			UIScript.allAtoms.Remove(transform);
			if (trash != null) 
				trash.GetComponent<Image>().color = new Color(.7f,.7f,.7f);
			Destroy(gameObject);
		}
		GameObject.Find ("Canvas").GetComponent<UIScript>().Uusi ();
	}

	public void OnMouseUp() {
		if (destroy) {
			OnDestroy();
			return;
		}
		UIScript.select = null;
		//if (transform.parent == null)	//TODO korjaa toimimaan molekyylillä kanssa
		Camera.main.GetComponent<GameLogic>().CreateMolecule(transform);	
	}

	private void RemoveConnection(Transform target, Transform from) {
		foreach(Connection con in from.GetComponent<Atom>().attached) {
			if (con.atom == target) {
				from.GetComponent<Atom>().attached.Remove (con);
				return;
			}
		}
	}

	/* Returns the original bonds when atom is detached
	 * @param target this is the atom the transform was attached to
	 */
	private void ReturnOriginalBonds(Transform target) {
		//Go through all of the bonds in transform
		for(int i=0; i<transform.childCount; i++) {
			Transform child = transform.GetChild(i);
			Bond childBond = child.GetComponent<Bond>();

			//if bond is attached to target
			if (childBond.attachedTo == target) {
				Transform clone = null;

				//originalPositions contain the position/s for its original positions
				//note: double bond will have two vectors in the list and triple will have three
				foreach(Vector3 pos in childBond.originalPositions) {
					singlebond = singlebak;
					clone = (Transform)Instantiate (singlebond, transform.position, Quaternion.identity);

					clone.parent = transform;
					clone.localPosition = pos;
					clone.name = "AtomBond";

					clone.GetComponent<Bond>().originalPositions.Add(pos);

					clone.LookAt(transform);
					clone.Rotate (90f,0f,0f);
					clone.SetAsFirstSibling();
					childBond.attachedTo = null;
				}
				//return bonds in target
				foreach(Vector3 pos in target.GetComponent<Bond>().originalPositions) {
					clone = (Transform)Instantiate (singlebond, target.parent.position, Quaternion.identity);
					
					clone.parent = target.parent;
					clone.localPosition = pos;
					clone.name = "AtomBond";

					clone.GetComponent<Bond>().originalPositions.Add (pos);
					
					clone.LookAt(target.parent);
					clone.Rotate (90f,0f,0f);
					clone.SetAsFirstSibling();
				}
				RemoveConnection(transform, target.parent);
				RemoveConnection(target.parent, transform);
				Destroy(child.gameObject);
				Destroy(target.gameObject);
			}
		}
	}

	private void Reset() {
		CameraMovement.reset = false;
		//Prevents the atom from rotating for no reason when it's a part of a molecule
		//it happens because Distance isn't the right way to reset...
		if (transform.root.name != "Molecule") {
			NotInUse = true;
		}
		if (Vector3.Distance(originalPosition, transform.position) > 0.05f) {
			originalPosition = transform.position;
			//Go through all of the bonds in current
			//TODO change parent(?) and fix the atoms left in molecule
			transform.parent = null;
			for(int i=0; i<transform.childCount; i++) {
				//Get a child from current atom
				Transform child = transform.GetChild(i);
				Bond bond = child.GetComponent<Bond>();
				
				//we want to detach all connections here
				Transform target = bond.attachedTo;
				if (target != null) {
					Atom targetAtom = target.parent.GetComponent<Atom>();
					
					UpdateElectrons(-bond.type);
					targetAtom.UpdateElectrons(-bond.type);
					
					ReturnOriginalBonds (target);
					
					if (targetAtom.attached.Count == 0)
						targetAtom.NotInUse = true;
				}
			}
		}
	}

	public void OnMouseDrag() {
		if (!CameraMovement.rotating) {
			curScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z);    
			curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace)+offset;
			transform.root.position = curPosition;
		}
	}

} //EOF
