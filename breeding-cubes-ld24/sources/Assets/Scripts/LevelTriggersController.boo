import UnityEngine

class LevelTriggersController (MonoBehaviour): 

	public triggerName as string

	public controlsText as TextMesh
	public heartRenderer as Renderer

	def OnTriggerEnter (collider as Collider):
		
		if collider.tag == "Player":
			if triggerName == "Start":
				controlsText.text = "Move with Right/Left arrows.\nJump with Up arrow.\nEscape for main menu."

			elif triggerName == "Enemies":
				controlsText.text = "Beware of enemies !\nKill them like Mario."

			elif triggerName == "Jump":
				controlsText.text = "Try to jump to the other side !"

			elif triggerName == "JumpFailed":
				controlsText.text = "You can't jump that far or that hight\nbecause your are not evolve enought."

			elif triggerName == "TouchTheCube":
				controlsText.text = "Don't fear, touch the cube..."

			elif triggerName == "CubeTouched":
				controlsText.text = "This cube has better characters than you !\nStay close to breed with him\nand create a new, evolved cube !"

			elif triggerName == "CombatReady":
				controlsText.text = "Ready ..."

			elif triggerName == "CombatCover":
				controlsText.text = "Get cover !"

			elif triggerName == "Combat":
				Destroy (GameObject.Find ("Platform"))
				controlsText.text = "Fight !"

			elif triggerName == "Push":
				controlsText.text = "This cube said that he can push those crates ..."

			elif triggerName == "EndGame":
				controlsText.text = "The End...\nThanks for playing 'Breeeding Cubes' which\nis my entry for Ludum Dare 24.\n\nReach me at @Lion_2 \nor http://www.florent-poujol.fr/en."
				heartRenderer.enabled = true



			Destroy (self)
