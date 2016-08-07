import UnityEngine

class HeartController (MonoBehaviour): 
	def Update ():
		transform.Translate (0, 1.5 * Time.deltaTime, 0)
		transform.localScale += Vector3 (0.01, 0.01, 0.01)
		