using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FocusEffect : MonoBehaviour {
	private Hole m_hole;
	private HoleImage m_holeImg;

	[SerializeField]
	private RectTransform FocusTarget;

    [System.NonSerialized]
    private RectTransform m_RectTransform;
    public RectTransform rectTransform
    {
        get { return m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>()); }
    }

	void Awake()
	{
		m_hole = GetComponent<Hole> ();
		m_holeImg = GetComponentInChildren<HoleImage> ();
	}

	void LateUpdate()
	{
		if (FocusTarget != null) {
			rectTransform.position = FocusTarget.position;
			rectTransform.sizeDelta = FocusTarget.sizeDelta;

			m_holeImg.transform.position = FocusTarget.root.position;
		}
	}
}
