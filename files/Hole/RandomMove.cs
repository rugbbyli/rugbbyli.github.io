using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMove : MonoBehaviour {

	public float Speed;
	public RectTransform Border;

	private Vector2 m_dir;
	private RectTransform m_rtf;

	// Use this for initialization
	void Start () {
		m_rtf = GetComponent<RectTransform> ();
		resetDir ();
	}
	
	// Update is called once per frame
	void Update () {
		var mov = Time.deltaTime * Speed;
//		transform.position = new Vector3 (transform.position.x + m_dir.x * mov, transform.position.y + m_dir.y * mov, transform.position.z);
//
		m_rtf.anchoredPosition = m_dir * mov + m_rtf.anchoredPosition;

		if (m_rtf.anchoredPosition.x + m_rtf.sizeDelta.x/2 > Border.rect.xMax || m_rtf.anchoredPosition.x - m_rtf.sizeDelta.x/2 < Border.rect.xMin)
			m_dir.x = -m_dir.x;
		if (m_rtf.anchoredPosition.y + m_rtf.sizeDelta.y/2 > Border.rect.yMax || m_rtf.anchoredPosition.y - m_rtf.sizeDelta.y/2 < Border.rect.yMin)
			m_dir.y = -m_dir.y;
	}

	void resetDir()
	{
		m_dir = new Vector2 (Random.Range (-1f, 1f), Random.Range (-1f, 1f)).normalized;
	}
}
