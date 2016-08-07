import UnityEngine

class PlayerController (MonoBehaviour): 

	playerCharacters as CubeCharacters // characters of the player cube
	isOnGround as bool // does the player touch the ground ?
	

	def Start ():
		playerCharacters = gameObject.GetComponent[of CubeCharacters]()
		
		
	def Update ():

		isOnGround = Physics.Raycast (transform.position, -Vector3.up, 0.6);

		// moving left and right
		if Input.GetButton ("Horizontal"):

			translation = Input.GetAxis ("Horizontal") // return between -1 and 1, but  even with a key, it takes time to reach -1 or 1
			if translation != 0.0:
				translation = translation / Mathf.Abs (translation) // here translation is 1 or -1
				translation = translation * playerCharacters.characters[0] * 0.1
				transform.Translate (translation, 0, 0, Space.World)

		// jump
		if Input.GetButtonDown ("Vertical"):
			if isOnGround :
		        rigidbody.AddForce (Vector3 (0, 8 * playerCharacters.characters[1], 0), ForceMode.Impulse) 


	def OnTriggerEnter (collider as Collider):
		//print ("player hit : tag="+collider.tag+" name="+collider.gameObject.name+" name="+gameObject.name)
		
		if collider.tag == "Cube":
			EventManager.OnPlayerTouchACube (collider.gameObject)

		elif collider.tag == "EnemyTopTrigger":
			//print ("destroy")
			Destroy (collider.transform.parent.gameObject)
		elif collider.tag == "Enemy":
			//GameManager.OnPlayerKilled ()
			Application.LoadLevel (Application.loadedLevel)

		elif collider.tag == "Pushable":
			if playerCharacters.characters[2] == 1:
				collider.rigidbody.mass = 1


