using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshSwapper : MonoBehaviour 
{
	static int InstanceCount = 0;
	
	
	public struct ResetItem
	{
		public ResetItem(Object o, float d)
		{
			item = o;
			duration = d;
		}
		public Object item;
		public float duration;
		
		new public int GetHashCode()
		{
			return item.GetHashCode();
		}
	}
	
	public MeshFilter[] meshFilters;
	public int[] meshIndex;
	public Mesh[] meshes;
	public Animation anim;
	List<ResetItem> resetQueue;
	// Use this for initialization
	void Awake()
	{
		InstanceCount++;
	}
	void Start () 
	{
		if (InstanceCount > 1)
		{
			InstanceCount--;
			Destroy(this);
		}
		Reset ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		for(int i = 0; i < resetQueue.Count; i++)
		{
			ResetItem ri = resetQueue[i];
			ri.duration = resetQueue[i].duration - Time.deltaTime;
			resetQueue[i]= ri;
		}
		resetQueue.RemoveAll(ExpiredRenderer);
	}
	
	public void Reset()
	{
		anim 		= animation;
		meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
		meshes 		= (Mesh[])Resources.FindObjectsOfTypeAll(typeof(Mesh));
		List<int> tempIndex = new List<int>();
		resetQueue	= new List<ResetItem>();
		
		foreach(MeshFilter mf in meshFilters)
		{
			tempIndex.Add (0);
			foreach (Mesh m in meshes)
			{
				if(mf.mesh == m)
				{
					break;
				}
				tempIndex[tempIndex.Count-1]++;
			}
		}
		meshIndex = tempIndex.ToArray();
	}
	
	void OnGUI()
	{
		Rect buttonRect = new Rect(0,0,100,20);
		
		foreach (AnimationState s in anim)
		{
			if (GUI.Button(buttonRect, s.clip.name))
			{
				anim.Play(s.clip.name);
				anim.wrapMode = WrapMode.Loop;
			}
			buttonRect = BumpButton(buttonRect);
		}
		buttonRect = BumpButton(buttonRect);
		
		for (int i = 0; i < meshFilters.Length; i++)
		{
			MeshFilter mf = meshFilters[i];
			if (GUI.Button(buttonRect, mf.name))
			{
				meshIndex[i]++;
				if (meshIndex[i] >= meshes.Length)
				{
					meshIndex[i] = 0;
				}
				mf.mesh = meshes[meshIndex[i]];
				
				if (mf.renderer && !resetQueue.Contains(new ResetItem(mf.renderer,0)))
				{
					resetQueue.Add(new ResetItem(mf.renderer,1.0f));
					foreach (Material m in mf.renderer.materials)
					{
						m.color = Color.blue;
					}
				}
			}
			buttonRect = BumpButton(buttonRect);
		}
	}
	
	Rect BumpButton(Rect buttonRect)
	{
		buttonRect.y += 20;
		if (buttonRect.y > Screen.height)
		{
			buttonRect.y = 0;
			buttonRect.x += 100;
		}
		return buttonRect;
	}
	
	static bool ExpiredRenderer(ResetItem ri)
	{
		if (ri.duration <= 0)
		{
			MeshRenderer r = (MeshRenderer)ri.item;
			foreach (Material m in r.materials)
			{
				m.color = Color.white;
			}
			return true;
		}
		return false;
	}
}
