import UnityEngine

class GUIManager (MonoBehaviour): 

	displayHUD = true
	public playerCharacters as CubeCharacters // set in inspector
	public static drawPlayerWindow = true
	otherCubeCharacters as CubeCharacters
	drawCubeWindow = false
	
	displayMainMenu = false


	def Start ():
		// registering event
		//EventManager.cubeIsHoveredMethods += OnCubeIsHovered
		EventManager.playerTouchACubeMethods += OnPlayerTouchACube


	// ===============
	// Events

	/*def OnCubeIsHovered (cube as GameObject):
		otherCubeCharacters = cube.GetComponent[of CubeCharacters]()
		drawCubeWindow = true*/

	def OnPlayerTouchACube (cube as GameObject):
		otherCubeCharacters = cube.GetComponent[of CubeCharacters]()
		drawCubeWindow = true


	// ===============
	// GUI
	def Update ():
		if Input.GetButtonDown ("Escape"):
			if GameManager.player.active == false:
				GameManager.player.SetActiveRecursively(true)
			else:
				GameManager.player.active = false

			displayHUD = not displayHUD
			displayMainMenu = not displayMainMenu



	def OnGUI ():
		if displayHUD and drawPlayerWindow:
			GUILayout.Window (1, Rect (0, 0, 150, 0), PlayerCharacterWindow, "You")

		if displayHUD and drawCubeWindow:
			GUILayout.Window (2, Rect (160, 0, 150, 0), CubeCharacterWindow, otherCubeCharacters.cubeName)

		if displayHUD == false and displayMainMenu:
			GUILayout.Window (3, Rect (Screen.width/2-200, 20, 200, 0), MainMenuWindow, "MainMenu - Breeding Cubes")

	
	

	def PlayerCharacterWindow ():
		// get characters from the current cube
		if playerCharacters != null:
			for i in range(0, 4):
				key = GameManager.charactersNames[i]
				val = playerCharacters.characters[i]
				//print ("i="+i+" val="+val)
				if val != 0.0:
					GUILayout.Label (key+" "+val)

		/*if GUILayout.Button ("Save"):
			GameManager.Save ()

		if GUILayout.Button ("Kill player"):
			GameManager.OnPlayerKilled ()

		if GUILayout.Button ("Reload level"):
			PlayerPrefs.DeleteAll()
			EventManager.OnApplicationReload ()
			//Application.LoadLevel (Application.loadedLevel)
*/

	def CubeCharacterWindow ():
		// get characters from the current cube
		if otherCubeCharacters != null:
			for i in range(0, 4):
				key = GameManager.charactersNames[i]
				val = otherCubeCharacters.characters[i]
				if val > 0.0:
					GUILayout.Label (key+" "+val)

			if GUILayout.Button ("Close window"):
				drawCubeWindow = false

			if Vector3.Distance (otherCubeCharacters.gameObject.transform.position, playerCharacters.gameObject.transform.position) <= 1.6:
				if GUILayout.Button ("Breed with this cube"):
					drawPlayerWindow = false
					EventManager.OnPlayerProcreate (otherCubeCharacters.gameObject)
					drawCubeWindow = false


	def MainMenuWindow ():
		if GUILayout.Button ("Reload Level"):
			PlayerPrefs.DeleteAll ()
			EventManager.OnApplicationReload ()

		if GUILayout.Button ("Load Last CheckPoint"):
			//Application.LoadLevel (Application.loadedLevel)
			EventManager.OnApplicationReload ()

		if GUILayout.Button ("Quit Game"):
			Application.Quit ()
