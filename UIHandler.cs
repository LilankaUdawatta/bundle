// Copyright 2016 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


// Handler for UI buttons on the scene.  Also performs some
// necessary setup (initializing the firebase app, etc) on
// startup.
public class UIHandler : MonoBehaviour {

	public Text emailText;
	public Text passwordText;
	public Text confirmPasswordText;
	public Text nameText;
	public Text loginPassword;
	public Text loginEmail;
	public GameObject LoginObject, SignUpObject,ScanUI;
	public Text SignUpErrorText, SignInErrorText;
	private bool emailError,passwordError;
	public Text UserNameProfile;

	protected Firebase.Auth.FirebaseAuth auth;
	protected Firebase.Auth.FirebaseAuth otherAuth;
	protected Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth =
		new Dictionary<string, Firebase.Auth.FirebaseUser>();

	//public GUISkin fb_GUISkin;
	private string logText = "";
	protected string email = "";
	protected string password = "";
	protected string displayName = "";
	protected string phoneNumber = "";
	protected string receivedCode = "";
	// Flag set when a token is being fetched.  This is used to avoid printing the token
	// in IdTokenChanged() when the user presses the get token button.
	private bool fetchingToken = false;
	// Enable / disable password input box.
	// NOTE: In some versions of Unity the password input box does not work in
	// iOS simulators.
	public bool usePasswordInput = false;
	//bool UIEnabled = true;

	const int kMaxLogSize = 16382;
	Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

	// When the app starts, check to make sure that we have
	// the required dependencies to use Firebase, and if not,
	// add them if possible.
	public virtual void Start() {
		Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
			dependencyStatus = task.Result;
			if (dependencyStatus == Firebase.DependencyStatus.Available) {
				InitializeFirebase();
			} else {
				Debug.LogError(
					"Could not resolve all Firebase dependencies: " + dependencyStatus);
			}
		});
		LogINScreen ();
		emailError = false;
		passwordError = false;
	}

	// Handle initialization of the necessary firebase modules:
	protected void InitializeFirebase() {
		DebugLog("Setting up Firebase Auth");
		auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
		auth.StateChanged += AuthStateChanged;
		auth.IdTokenChanged += IdTokenChanged;

		AuthStateChanged(this, null);
	}

	// Exit if escape (or back, on mobile) is pressed.
	protected virtual void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}
	}

	public void CreateUserAsync() {
		
		emailError = false;
		passwordError = false;

		email = emailText.text;
		if (Text.Equals(confirmPasswordText.text,passwordText.text)) {
			password = passwordText.text;
		} else {
			password = null;
			DebugLog ("Password Missmatch");
			SignUpErrorText.text = "Please make sure that your passwords match";
			passwordError = true;
		}
		DebugLog(String.Format("Attempting to create user {0}...", email));
		DisableUI();

		// This passes the current displayName through to HandleCreateUserAsync
		// so that it can be passed to UpdateUserProfile().  displayName will be
		// reset by AuthStateChanged() when the new user is created and signed in.

		//string newDisplayName = displayName; //Original Name
		string newDisplayName = nameText.text;

		auth.CreateUserWithEmailAndPasswordAsync(email, password)
			.ContinueWith((task) => {
				return HandleCreateUserAsync(task, newDisplayName: newDisplayName);
			}).Unwrap();
	}

	Task HandleCreateUserAsync(Task<Firebase.Auth.FirebaseUser> authTask, string newDisplayName = null) {
		EnableUI();
		if (LogTaskCompletion(authTask, "User Creation")) {
			if (auth.CurrentUser != null) {
				DebugLog(String.Format("User Info: {0}  {1}", auth.CurrentUser.Email, auth.CurrentUser.ProviderId));

				Firebase.Auth.FirebaseUser user = auth.CurrentUser;
				if (user != null) {
 				user.SendEmailVerificationAsync().ContinueWith(task => 
				{
   					if (task.IsCanceled) {
	     				Debug.LogError("SendEmailVerificationAsync was canceled.");
    	  				return;
	   				}

	    			if (task.IsFaulted) {
    					Debug.LogError("SendEmailVerificationAsync encountered an error: " + task.Exception);
        				return;
    				}

    				Debug.Log("Email sent successfully.");
 		 		});
			}

				return UpdateUserProfileAsync(newDisplayName: newDisplayName);
			}
		}
		// Nothing to update, so just return a completed Task.
		return Task.FromResult(0);
	}

	// Update the user's display name with the currently selected display name.
	public Task UpdateUserProfileAsync(string newDisplayName = null) {
		if (auth.CurrentUser == null) {
			DebugLog("Not signed in, unable to update user profile");
			return Task.FromResult(0);
		}
		displayName = newDisplayName ?? displayName;
		DebugLog("Updating user profile");
		DisableUI();
		return auth.CurrentUser.UpdateUserProfileAsync(new Firebase.Auth.UserProfile {
			DisplayName = nameText.text,
			PhotoUrl = auth.CurrentUser.PhotoUrl,
		}).ContinueWith(HandleUpdateUserProfile);
	}

	void HandleUpdateUserProfile(Task authTask) {
		EnableUI();
		if (LogTaskCompletion(authTask, "User profile")) {
			DisplayDetailedUserInfo(auth.CurrentUser, 1);
		}
	}

	public void SigninAsync() {

		email = emailText.text;
		//if (Text.Equals(confirmPasswordText.text,passwordText.text)) {
			password = passwordText.text;
		//} else {
		//	password = null;
		//	DebugLog ("Password Missmatch");
		//	passwordError = true;
			//password = loginPassword.text;
		//}
		
		DebugLog(String.Format("Attempting to sign in as {0}...", email));
		DisableUI();
		auth.SignInWithEmailAndPasswordAsync(email, password)
			.ContinueWith(HandleSigninResult);
	}

	// This is functionally equivalent to the Signin() function.  However, it
	// illustrates the use of Credentials, which can be aquired from many
	// different sources of authentication.
	public Task SigninWithCredentialAsync() {
		DebugLog(String.Format("Attempting to sign in as {0}...", email));
		DisableUI();
		Firebase.Auth.Credential cred = Firebase.Auth.EmailAuthProvider.GetCredential(email, password);
		return auth.SignInWithCredentialAsync(cred).ContinueWith(HandleSigninResult);
	}


	void HandleSigninResult(Task<Firebase.Auth.FirebaseUser> authTask) {
		EnableUI();
		LogTaskCompletion(authTask, "Sign-in");
	}



	void OnDestroy() {
		auth.StateChanged -= AuthStateChanged;
		auth.IdTokenChanged -= IdTokenChanged;
		auth = null;
	}

	void DisableUI() {
		//UIEnabled = false;
	}

	void EnableUI() {
		//UIEnabled = true;
	}

	// Output text to the debug log text field, as well as the console.
	public void DebugLog(string s) {
		Debug.Log(s);
		//if (String.Equals(s,"Firebase.FirebaseException: The email address is already in use by another account."))
		if(emailError)
		{
			if(passwordError)
			{
				SignUpErrorText.text = "Please make sure that your passwords match";
			}
			else{
				SignUpErrorText.text = "The email address is already in use by another account.";
			}
		}
	/*	else if(passwordError)
		{
				SignUpErrorText.text = "Please make sure that your passwords match";
		}*/
		
		logText += s + "\n";

		while (logText.Length > kMaxLogSize) {
			int index = logText.IndexOf("\n");
			logText = logText.Substring(index + 1);
		}
	}

	// Display user information.
	void DisplayUserInfo(Firebase.Auth.IUserInfo userInfo, int indentLevel) {
		string indent = new String(' ', indentLevel * 2);
		var userProperties = new Dictionary<string, string> {
			{"Display Name", userInfo.DisplayName},
			{"Email", userInfo.Email},
			{"Photo URL", userInfo.PhotoUrl != null ? userInfo.PhotoUrl.ToString() : null},
			{"Provider ID", userInfo.ProviderId},
			{"User ID", userInfo.UserId}
		};
		foreach (var property in userProperties) {
			if (!String.IsNullOrEmpty(property.Value)) {
				DebugLog(String.Format("{0}{1}: {2}", indent, property.Key, property.Value));
			}
		}
	}

	// Display a more detailed view of a FirebaseUser.
	void DisplayDetailedUserInfo(Firebase.Auth.FirebaseUser user, int indentLevel) {
		DisplayUserInfo(user, indentLevel);
		DebugLog("  Anonymous: " + user.IsAnonymous);
		DebugLog("  Email Verified: " + user.IsEmailVerified);
		var providerDataList = new List<Firebase.Auth.IUserInfo>(user.ProviderData);
		if (providerDataList.Count > 0) {
			DebugLog("  Provider Data:");
			foreach (var providerData in user.ProviderData) {
				DisplayUserInfo(providerData, indentLevel + 1);
			}
		}
	}

	// Track state changes of the auth object.
	void AuthStateChanged(object sender, System.EventArgs eventArgs) {
		Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
		Firebase.Auth.FirebaseUser user = null;
		if (senderAuth != null) userByAuth.TryGetValue(senderAuth.App.Name, out user);
		if (senderAuth == auth && senderAuth.CurrentUser != user) {
			bool signedIn = user != senderAuth.CurrentUser && senderAuth.CurrentUser != null;
			if (!signedIn && user != null) {
				DebugLog("Signed out " + user.UserId);
			}
			user = senderAuth.CurrentUser;
			userByAuth[senderAuth.App.Name] = user;
			if (signedIn) {
				DebugLog("Signed in " + user.UserId);
				displayName = user.DisplayName ?? "";
				DisplayDetailedUserInfo(user, 1);
			}
		}
	}

	// Track ID token changes.
	void IdTokenChanged(object sender, System.EventArgs eventArgs) {
		Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
		if (senderAuth == auth && senderAuth.CurrentUser != null && !fetchingToken) {
			senderAuth.CurrentUser.TokenAsync(false).ContinueWith(
				task => DebugLog(String.Format("Token[0:8] = {0}", task.Result.Substring(0, 8))));

/******************************************************************************************************************
 * 
 * */		// Activate Scanning
			ScanUIScreen ();
			//SceneManager.LoadScene("Scan");
			//signInUserAfterSignUp (); /////AAAADDDEEDD FOR AUTOSIGN after signup

/******************************************************************************************************************
 * 
 * */
		}
	}

	// Log the result of the specified task, returning true if the task
	// completed successfully, false otherwise.
	bool LogTaskCompletion(Task task, string operation) {
		bool complete = false;
		if (task.IsCanceled) {
			DebugLog(operation + " canceled.");
		} else if (task.IsFaulted) {
			DebugLog(operation + " encounted an error.");
			foreach (Exception exception in task.Exception.Flatten().InnerExceptions) {
				string authErrorCode = "";
				Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
				if (firebaseEx != null) {
					authErrorCode = String.Format("AuthError.{0}: ",
						((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString());
				}
				emailError = true;
				DebugLog(authErrorCode + exception.ToString());
			}
		} else if (task.IsCompleted) {
			DebugLog(operation + " completed");
			complete = true;

		}
		return complete;
	}

	
	public void ReloadUser() {
		if (auth.CurrentUser == null) {
			DebugLog("Not signed in, unable to reload user.");
			return;
		}
		DebugLog("Reload User Data");
		auth.CurrentUser.ReloadAsync().ContinueWith(HandleReloadUser);
	}

	void HandleReloadUser(Task authTask) {
		if (LogTaskCompletion(authTask, "Reload")) {
			DisplayDetailedUserInfo(auth.CurrentUser, 1);
		}
	}

	public void GetUserToken() {
		if (auth.CurrentUser == null) {
			DebugLog("Not signed in, unable to get token.");
			return;
		}
		DebugLog("Fetching user token");
		fetchingToken = true;
		auth.CurrentUser.TokenAsync(false).ContinueWith(HandleGetUserToken);
	}

	void HandleGetUserToken(Task<string> authTask) {
		fetchingToken = false;
		if (LogTaskCompletion(authTask, "User token fetch")) {
			DebugLog("Token = " + authTask.Result);
		}
	}

	void GetUserInfo() {
		if (auth.CurrentUser == null) {
			DebugLog("Not signed in, unable to get info.");
		} else {
			DebugLog("Current user info:");
			DisplayDetailedUserInfo(auth.CurrentUser, 1);
		}
	}

	public void SignOut() {
		DebugLog("Signing out.");
		auth.SignOut();
		LogINScreen ();
	}

	// Show the providers for the current email address.
	public void DisplayProvidersForEmail() {
		auth.FetchProvidersForEmailAsync(email).ContinueWith((authTask) => {
			if (LogTaskCompletion(authTask, "Fetch Providers")) {
				DebugLog(String.Format("Email Providers for '{0}':", email));
				foreach (string provider in authTask.Result) {
					DebugLog(provider);
				}
			}
		});
	}

	// Send a password reset email to the current email address.
	public void SendPasswordResetEmail() {
		auth.SendPasswordResetEmailAsync(email).ContinueWith((authTask) => {
			if (LogTaskCompletion(authTask, "Send Password Reset Email")) {
				DebugLog("Password reset email sent to " + email);
			}
		});
	}

	public void signInUserAfterSignUp()
	{
		email = emailText.text;
		password = passwordText.text;

		Firebase.Auth.Credential credential =
			Firebase.Auth.EmailAuthProvider.GetCredential(email, password);
		auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
			if (task.IsCanceled) {
				Debug.LogError("SignInWithCredentialAsync was canceled.");
				return;
			}
			if (task.IsFaulted) {
				Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
				return;
			}

			Firebase.Auth.FirebaseUser newUser = task.Result;
			Debug.LogFormat("User signed in successfully: {0} ({1})",
				newUser.DisplayName, newUser.UserId);

			// Activate Scanning
			ScanUIScreen ();
			//SceneManager.LoadScene("Scan");
		});
	}

	public void signInUser(){
		emailError = false;
		passwordError = false;

		email = loginEmail.text;
		password = loginPassword.text;
		auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
			if (task.IsCanceled) {
				Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
				return;
			}
			if (task.IsFaulted) {
				Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
				SignInErrorText.text = "Incorrect Email or Password";
				return;
			}

			Firebase.Auth.FirebaseUser newUser = task.Result;
			Debug.LogFormat("User signed in successfully: {0} ({1})",
				newUser.DisplayName, newUser.UserId);
		});
	}

	public void GetUserProfile()
	{
		Firebase.Auth.FirebaseUser user = auth.CurrentUser;
		if (user != null) {
 			string name = user.DisplayName;
			UserNameProfile.text = name.ToString();;
			Debug.Log(UserNameProfile);
 			//string email = user.Email;
 			//System.Uri photo_url = user.PhotoUrl;
 			// The user's Id, unique to the Firebase project.
	 		// Do NOT use this value to authenticate with your backend server, if you
			// have one; use User.TokenAsync() instead.
			string uid = user.UserId;
		}
	}



	public void LogINScreen ()
	{
		LoginObject.SetActive(true);
		SignUpObject.SetActive(false);
		ScanUI.SetActive(false);
		SignInErrorText.text = "";
	}

	public void SignUpScreen ()
	{
		SignUpObject.SetActive(true);
		LoginObject.SetActive(false);
		ScanUI.SetActive(false);
		SignUpErrorText.text = "";
		
	}

	public void ScanUIScreen ()
	{
		ScanUI.SetActive(true);
		SignUpObject.SetActive(false);
		LoginObject.SetActive(false);
	}
}

