using UnityEngine;
using System.Collections;

public class AtomInteractions : ScriptableObject {

	public void OxygenCarbonInteraction(Transform selection, Transform target) {
		Vector3 axis = selection.parent.position - target.parent.position;	//rotate along the connected bonds
		Vector3 selectiondir;
		Vector3 targetdir;
		
		int selectionindex = selection.GetSiblingIndex();
		int targetindex = target.GetSiblingIndex();
		
		int selectionsiblingindex = (selectionindex + 1)%selection.parent.childCount;
		int targetsiblingindex = (targetindex + 1)%target.parent.childCount;
		
		Transform selectionsibling = selection.parent.GetChild(selectionsiblingindex);
		Transform targetsibling = target.parent.GetChild(targetsiblingindex);
		
		selectiondir = (selectionsibling.position - selectionsibling.parent.position);
		targetdir = (targetsibling.position - targetsibling.parent.position);
		Vector3 dir = (target.position-selection.position);
		Vector3 cross1 = Vector3.Cross(selectiondir, dir);
		Vector3 cross2= Vector3.Cross(targetdir, dir);
		
		float a = Vector3.Angle (cross1, cross2);

		float cangle = 109.4f/2;
		selection.parent.Rotate(axis, a, Space.World);
		selection.parent.Rotate(axis, cangle, Space.World);
	}

	public void CarbonCarbonInteraction(Transform selection, Transform target) {
		Vector3 axis = selection.parent.position - target.parent.position;	//rotate along the connected bonds
		Vector3 selectiondir;
		Vector3 targetdir;
		
		int selectionindex = selection.GetSiblingIndex();
		int targetindex = target.GetSiblingIndex();
		
		int selectionsiblingindex = (selectionindex + 1)%selection.parent.childCount;
		int targetsiblingindex = (targetindex + 1)%target.parent.childCount;
		
		Transform selectionsibling = selection.parent.GetChild(selectionsiblingindex);
		Transform targetsibling = target.parent.GetChild(targetsiblingindex);
		
		selectiondir = (selectionsibling.position - selectionsibling.parent.position);
		targetdir = (targetsibling.position - targetsibling.parent.position);
		Vector3 dir = (target.position-selection.position);
		Vector3 cross1 = Vector3.Cross(selectiondir, dir);
		Vector3 cross2= Vector3.Cross(targetdir, dir);
		
		float a = Vector3.Angle (cross1, cross2);
		
		int cangle = selection.parent.childCount == 4 ? 180 : 90;
		selection.parent.Rotate(axis, a, Space.World);
		selection.parent.Rotate(axis, cangle, Space.World);
	}
}
