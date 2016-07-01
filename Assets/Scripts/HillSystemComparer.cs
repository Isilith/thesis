using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HillSystemComparer : IComparer<AtomListItem> {
	public int Compare(AtomListItem first, AtomListItem second) {
		if (first != null && second != null) {
			string first_name = first.core.name;
			string second_name = second.core.name;
			//If we compare carbon/hydrogen to carbon/hydrogen
			if ((first_name == "Carbon" || first_name == "Hydrogen") &&
			    (second_name == "Carbon" || second_name == "Hydrogen"))
			{
				return first_name.CompareTo(second_name);
			}
			//We one of the instances is carbon/hydrogen and the other is not
			if (first_name == "Carbon" || first_name == "Hydrogen" &&
			    second_name != "Carbon" && second_name != "Hydrogen")
			{
				return -1;
			}
			if (second_name == "Carbon" || second_name == "Hydrogen" &&
			    first_name != "Carbon" && first_name != "Hydrogen")
			{
				return 1;
			}
		}

		if (first == null && second == null)
			return 0;

		if (first != null)
			return -1;

		return 1;
	}
}
