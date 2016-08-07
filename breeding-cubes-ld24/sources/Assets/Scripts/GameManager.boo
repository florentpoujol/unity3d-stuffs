import UnityEngine
import System.Collections
import System.Collections.Generic

class GameManager (MonoBehaviour): 

	public mainCamera as Camera
	public static player as GameObject
	public heartPrefab as GameObject
	

	public static charactersNames = ("move", "jump", "push", "activate")

	def Start ():
		DontDestroyOnLoad (transform.gameObject)
		//EventManager.leftMouseButtonClickedMethods += OnLeftMouseButtonClicked
		EventManager.playerProcreateMethods += OnPlayerProcreate
		EventManager.applicationReloadMethods += OnApplicationReload

		player = GameObject.Find ("Player")

		if not PlayerPrefs.HasKey ("positionx"): // the first time the player enter the scene
			Save ()
		else: // the scene is loaded but some infos are saved, load them  
			Load ()

		if PlayerPrefs.GetInt ("CombatDone") == 1:
			GameObject.Find ("PushableCubeAfterCombat").transform.position = Vector3 (100,8,0)





		/*ray = mainCamera.ScreenPointToRay (Input.mousePosition);
		hit = RaycastHit();
		
		if Physics.Raycast (ray, hit):
			hitObject = hit.collider.gameObject

			// check if the object is a cube
			if hitObject.tag == "Cube":
				EventManager.OnCubeIsHovered (hitObject)*/



	def OnPlayerProcreate (cube as GameObject):
		print ("launch procreate")

		StartCoroutine (DoProcreate (cube))
		print ("launch2 procreate")


	def DoProcreate (cube as GameObject) as IEnumerator:
		print ("start procreate")
		heart = Instantiate (heartPrefab)
		heart.transform.position = player.transform.position

		playerCharacters = player.GetComponent[of CubeCharacters]().characters
		otherCharacters = cube.GetComponent[of CubeCharacters]().characters
		//newCharacters = Dictionary[of string, single]();

		yield WaitForSeconds (1)

		for i in range(0, 4):
			if otherCharacters[i] > playerCharacters[i]:
				playerCharacters[i] = otherCharacters[i]


		// change the color of the player
		playerColor = player.renderer.material.color
		cubeColor = cube.renderer.material.color
		player.renderer.material.color.r = (playerColor.r + cubeColor.r)/2
		player.renderer.material.color.g = (playerColor.g + cubeColor.g)/2
		player.renderer.material.color.b = (playerColor.b + cubeColor.b)/2
		//print (playerColor+" "+player.renderer.material.color)

		yield WaitForSeconds (0.5)

		GUIManager.drawPlayerWindow = true
		Save ()
		Destroy (cube)
		Destroy (heart)
		print ("end procreate")



	public static def OnPlayerKilled ():
		EventManager.OnApplicationReload ()

	
	def OnApplicationReload ():
		print ("OnApplicationReload")
		Application.LoadLevel (0)

	public static def Save ():
		PlayerPrefs.SetFloat ("positionx", player.transform.position.x)
		PlayerPrefs.SetFloat ("positiony", player.transform.position.y)
		PlayerPrefs.SetFloat ("positionz", player.transform.position.z)
		
		characters = player.GetComponent[of CubeCharacters]().characters
		PlayerPrefs.SetFloat ("move", characters[0])
		PlayerPrefs.SetFloat ("jump", characters[1])
		PlayerPrefs.SetFloat ("push", characters[2])
		PlayerPrefs.SetFloat ("activate", characters[3])

		PlayerPrefs.SetFloat ("red", player.renderer.material.color.r)
		PlayerPrefs.SetFloat ("green", player.renderer.material.color.g)
		PlayerPrefs.SetFloat ("blue", player.renderer.material.color.b)

		PlayerPrefs.Save ()

	/*public static def Save (position as bool):
		if position:
			PlayerPrefs.SetFloat ("positionx", player.transform.position.x)
			PlayerPrefs.SetFloat ("positiony", player.transform.position.y)
			PlayerPrefs.SetFloat ("positionz", player.transform.position.z)

		characters = player.GetComponent[of CubeCharacters]().characters
		PlayerPrefs.SetFloat ("move", characters[0])
		PlayerPrefs.SetFloat ("jump", characters[1])
		PlayerPrefs.SetFloat ("push", characters[2])
		PlayerPrefs.SetFloat ("activate", characters[3])
		PlayerPrefs.Save ()

	public static def Save ():
		Save (true)


	def SaveDefaultValues ():
		PlayerPrefs.SetFloat ("positionx", player.transform.position.x)
		PlayerPrefs.SetFloat ("positiony", player.transform.position.y)
		PlayerPrefs.SetFloat ("positionz", player.transform.position.z)

		PlayerPrefs.SetFloat ("move", 1)
		PlayerPrefs.SetFloat ("jump", 1)
		PlayerPrefs.SetFloat ("push", 0)
		PlayerPrefs.SetFloat ("activate", 0)*/



	public static def Load ():
		if PlayerPrefs.HasKey ("positionx"):
			x = PlayerPrefs.GetFloat ("positionx")
			y = PlayerPrefs.GetFloat ("positiony")
			z = PlayerPrefs.GetFloat ("positionz")
			player.transform.position = Vector3 (x, y, z)

			playerCharacters = player.GetComponent[of CubeCharacters]()
			playerCharacters.characters[0] = PlayerPrefs.GetFloat ("move")
			playerCharacters.characters[1] = PlayerPrefs.GetFloat ("jump")
			playerCharacters.characters[2] = PlayerPrefs.GetFloat ("push")
			playerCharacters.characters[3] = PlayerPrefs.GetFloat ("activate")

			player.renderer.material.color.r = PlayerPrefs.GetFloat ("red")
			player.renderer.material.color.g = PlayerPrefs.GetFloat ("green")
			player.renderer.material.color.b = PlayerPrefs.GetFloat ("blue")

			//player.SetActiveRecursively (true)

		else:
			Debug.LogWarning ("GameManager.Load() called but not data is saved !")


	public static def ResetCube ():
		player.transform.position = Vector3 (0, 5, 0)

		playerCharacters = player.GetComponent[of CubeCharacters]()
		playerCharacters.characters[0] = 1.0
		playerCharacters.characters[1] = 1.0
		playerCharacters.characters[2] = 0
		playerCharacters.characters[3] = 0

		player.renderer.material.color = Color.red
