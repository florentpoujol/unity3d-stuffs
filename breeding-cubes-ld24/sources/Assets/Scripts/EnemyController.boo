import UnityEngine

class EnemyController (MonoBehaviour): 
	public enum Direction:
		Left
		Right

	public currentDir = Direction.Left
	public speed = 1.0


	def Start ():
		currentDir = Direction.Right if Random.value >= 0.5
		speed = 2 + Random.value*1.5
		

	
	def Update ():
		translation = speed * Time.deltaTime

		if currentDir == Direction.Left:
			translation = -translation

		transform.Translate (translation, 0, 0, Space.World)


	def OnTriggerEnter (collider as Collider):
		//print ("enemy hit : tag="+collider.tag+" name="+collider.gameObject.name)

		if collider.tag == "EnemyWall" or collider.tag == "Pushable":
			
			if currentDir == Direction.Left:
				currentDir = Direction.Right
			else:
				currentDir = Direction.Left

		
