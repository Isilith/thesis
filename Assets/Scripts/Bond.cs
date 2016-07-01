using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * Type determines the size of the originalPositions list
 */
public class Bond : MonoBehaviour {

	public List<Vector3> originalPositions;	//list for storing the orignal positions of the deleted bond/s
	public Vector3 position;				//current position - useless?
	public Transform attachedTo = null;			//transform of the atom it's attached to
	public int type = 0;						//1 single, 2 double, 3 triple bond

}
