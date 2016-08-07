import UnityEngine

class EnemyTrigger (MonoBehaviour): 

	public parentController as EnemyController

	

	def OnTriggerEnter (collider as Collider):
		print ("enemy trigger hit : tag="+collider.tag+" name="+collider.gameObject.name)

		if collider.tag == "EnemyWall" : // might be the ground
			//currentDir = parentController.currentDir

			if parentController.currentDir == parentController.Direction.Left:
				parentController.currentDir = parentController.Direction.Right
			else:
				parentController.currentDir = parentController.Direction.Left

		elif collider.tag == "Player":
			EventManager.OnPlayerKilled ()