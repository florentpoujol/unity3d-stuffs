import UnityEngine

class CameraControl (MonoBehaviour): 
	def Update ():
		transform.position.z += Input.GetAxis( "Mouse ScrollWheel" ) * 5;
