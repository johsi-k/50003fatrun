using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Parent of the PlusPopUpController to ensure that pop up animation is always above the character's head
public class PlusPopUpParent : MonoBehaviour {
	public Animator animator;
	private Text popUpText;

	void OnEnable()
	{
		AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
		Destroy(gameObject, clipInfo[0].clip.length);
		popUpText = animator.GetComponent<Text>();
	}

	public void SetText(string text)
	{
		popUpText.text = text;
	}

	public void SetColor(Color color)
	{
		popUpText.color = color;
	}
}
