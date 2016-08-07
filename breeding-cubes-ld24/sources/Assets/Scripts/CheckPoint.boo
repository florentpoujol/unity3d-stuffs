import UnityEngine

class CheckPoint (MonoBehaviour): 

	public tname as string
	reachedOnce = false // has the check point been reached once

	def OnTriggerEnter (collider as Collider):
		if collider.tag == "Player" and reachedOnce == false:
			reachedOnce = true
			GameManager.Save ()

		if tname == "CombatDone":
			//print ("Combat done")
			PlayerPrefs.SetInt ("CombatDone", 1)
			