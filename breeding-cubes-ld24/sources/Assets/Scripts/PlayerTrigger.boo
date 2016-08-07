import UnityEngine

class PlayerTrigger (MonoBehaviour): 

	public playerController as PlayerController


	def OnTriggerEnter (collider as Collider):
		print ("player trigger hit : tag="+collider.tag+" name="+collider.gameObject.name)
		
		if collider.tag == "Cube":
			EventManager.OnPlayerTouchACube (collider.gameObject)

		elif collider.tag == "EnemyTop":
			Destroy (collider.gameObject.transform.parent.gameObject) // THAT'S INSANE ! How comme there is no method gameObject.FindParent() ? (Or just gameObject.parent ?)