using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Helper {

	private Vector3 up = Vector3.up*0.5f;
	private Vector3 two1 = new Vector3( 1.2903544f, 1f, 0f).normalized*0.5f;
	private Vector3 two2 = new Vector3(-1.2903544f, 1f, 0f).normalized*0.5f;
	private Vector3 three1 = new Vector3( 0f,  Mathf.Sqrt(3)/3, 0.0f).normalized*0.5f;
	private Vector3 three2 = new Vector3(-1f, -Mathf.Sqrt(3)/3, 0.0f).normalized*0.5f;
	private Vector3 three3 = new Vector3( 1f, -Mathf.Sqrt(3)/3, 0.0f).normalized*0.5f;
	private Vector3 four1 = new Vector3( 1f,0f,-1/Mathf.Sqrt(2)).normalized*0.5f;
	private Vector3 four2 = new Vector3(-1f,0f,-1/Mathf.Sqrt(2)).normalized*0.5f;
	private Vector3 four3 = new Vector3(0f, 1f,1/Mathf.Sqrt(2)).normalized*0.5f;
	private Vector3 four4 = new Vector3(0f,-1f,1/Mathf.Sqrt(2)).normalized*0.5f;
	public List<Vector3> one = new List<Vector3>(1);
	public List<Vector3> two = new List<Vector3>(2);
	public List<Vector3> three = new List<Vector3>(3);
	public List<Vector3> four = new List<Vector3>(4);

	public void Start() {
		one.Add(up);
		two.Add (two1);
		two.Add (two2);
		three.Add(three1);
		three.Add(three2);
		three.Add(three3);
		four.Add (four1);
		four.Add (four2);
		four.Add (four3);
		four.Add (four4);
	}
}
