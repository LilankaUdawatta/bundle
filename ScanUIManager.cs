using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanUIManager : MonoBehaviour {

	public GameObject menuButton;
	public GameObject ScreeSpaceCloseProfileButton, ProfileButton, SupportButton, AboutUsButton, SignoutButton, CloseButton;
	//Menu Animation

	public void DissableBoolAnimator(Animator anim)
	{
		anim.SetBool ("IsDisplayed", false);
		ScreeSpaceCloseProfileButton.SetActive(false);
		Invoke("MenuButtonEnable", 0.5f);
		ScanUIButtonsDisable();
	}

	public void EnableBoolAnimator(Animator anim)
	{
		anim.SetBool ("IsDisplayed", true);
		menuButton.SetActive(false);
		ScreeSpaceCloseProfileButton.SetActive(true);
		Invoke("ScanUIButtonsEnable", 0.5f);
	}

	private void ScanUIButtonsEnable()
	{
		ProfileButton.SetActive(true);
		SupportButton.SetActive(true);
		AboutUsButton.SetActive(true);
		SignoutButton.SetActive(true);
		CloseButton.SetActive(true);
	}

	private void ScanUIButtonsDisable()
	{
		ProfileButton.SetActive(false);
		SupportButton.SetActive(false);
		AboutUsButton.SetActive(false);
		SignoutButton.SetActive(false);
		CloseButton.SetActive(false);
	}

	private void MenuButtonEnable()
	{
		menuButton.SetActive(true);		
	}

	public void ExitGame()
	{
		Application.Quit ();
	}
}
