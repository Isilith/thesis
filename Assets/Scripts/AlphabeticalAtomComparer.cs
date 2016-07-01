using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AlphabeticalComparer : IComparer<AtomListItem> {

	public int Compare(AtomListItem first, AtomListItem second) {
		if (first != null && second != null) {
			return first.core.name.CompareTo(second.core.name);
		}
		if (first == null && second == null) {
			//both are null so can't compare them and they are equal
			return 0;
		}
		//only the first instance is not null, so prefer that
		if (first != null) {
			return -1;
		}
		//only the second instance is not null, so prefer that
		return 1;
	}
}
